﻿using GenerateTables.Models;
using ServerAPI.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using ServerAPI.ViewModels;

// ReSharper disable once CheckNamespace
namespace ServerAPI.Services
{
    [UsedImplicitly]
    public class ContactHistoryService : IContactHistoryService
    {
        private readonly AAtims _context;
        private readonly IPhotosService _photosService;

        public ContactHistoryService(AAtims context, IPhotosService photosService)
        {
            _context = context;
            _photosService = photosService;
        }
        #region Person Contacts
        public List<ContactHistoryDetailVm> GetContacts(ContactHistoryVm searchValue)
        {
            List<LookupVm> lookUpList = _context.Lookup.Where(w => w.LookupInactive == 0 &&
                (w.LookupType == LookupConstants.ARRTYPE ||
                    w.LookupType == LookupConstants.RELATIONS ||
                    w.LookupType == LookupConstants.DISCTYPE ||
                    w.LookupType == LookupConstants.GRIEVTYPE ||
                    w.LookupType == LookupConstants.VISREAS ||
                    w.LookupType== LookupConstants.CLASSGROUP)).Select(
                x => new LookupVm
                {
                    LookupIndex = x.LookupIndex,
                    LookupType = x.LookupType,
                    LookupDescription = x.LookupDescription
                }).ToList();
            List<ContactHistoryDetailVm> contactHistoryDetail = new List<ContactHistoryDetailVm>();

            searchValue.FilterDate = searchValue.ToDate?.Date.AddHours(23).AddMinutes(59).AddSeconds(59) ?? DateTime.Now;
            if (searchValue.IsHousingNumber)
            {
                GetHousingNumber(contactHistoryDetail, searchValue);
            }
            if (searchValue.IsHousingBed)
            {
                GetHousingBedNumber(contactHistoryDetail, searchValue);
            }
            if (searchValue.IsIntakeDate)
            {
                GetIntakeDate(contactHistoryDetail, searchValue, lookUpList);
            }
            if (searchValue.IsVisitorHistory && searchValue.InmateId > 0)
            {
                GetVisitorHistory(contactHistoryDetail, searchValue, lookUpList);
            }
            if (searchValue.IsLocation)
            {
                GetCheckOutLoc(contactHistoryDetail, searchValue);
            }
            if (searchValue.IsIncidentDate)
            {
                GetIncidentDate(contactHistoryDetail, searchValue, lookUpList);
            }
            if (searchValue.IsGrievance)
            {
                GetGrievance(contactHistoryDetail, searchValue, lookUpList);
            }
            if (searchValue.IsClassify)
            {
                GetClassify(contactHistoryDetail, searchValue);
            }
            if (searchValue.IsKeepSep && searchValue.InmateId > 0)
            {
                GetKeepSep(contactHistoryDetail, searchValue);
            }
            return contactHistoryDetail;
        }
        public List<ContactHistoryDetailVm> GetContactDetail(ContactHistoryModelVm modelVm)
        {
            IQueryable<Address> address = _context.Address;
            IQueryable<Incarceration> xref =
                _context.Incarceration.Where(s =>s.InmateId.HasValue && !s.ReleaseOut.HasValue);
            List<LookupVm> lookUpList = _context.Lookup.Where(w => w.LookupInactive == 0 && (w.LookupType == LookupConstants.ARRTYPE ||
                                                                    w.LookupType == LookupConstants.GRIEVTYPE ||
                                                                    w.LookupType == LookupConstants.VISREAS ||
                                                                    w.LookupType == LookupConstants.CLASSGROUP ||
                                                                    w.LookupType == LookupConstants.DISCTYPE)).Select(
                x => new LookupVm
                {
                    LookupIndex = x.LookupIndex,
                    LookupType = x.LookupType,
                    LookupDescription = x.LookupDescription
                }).ToList();
            modelVm.FilterDate = modelVm.SearchToDate?.Date.AddHours(23).AddMinutes(59).AddSeconds(59) ?? DateTime.Now;
            List<ContactHistoryDetailVm> contactDetails = new List<ContactHistoryDetailVm>();
            if (modelVm.Type == ContactConstants.HOUSINGNUMBER)
            {
                contactDetails = GetHousingNumberDetail(modelVm, address, xref);
            }
            if (modelVm.Type == ContactConstants.HOUSINGBEDNUMBER)
            {
                contactDetails = GetHousingBedNumberDetail(modelVm, address, xref);
            }
            if (modelVm.Type == ContactConstants.INTAKE)
            {
                contactDetails = GetIntakeDateDetail(modelVm, address, lookUpList, xref);
            }
            if (modelVm.Type == ContactConstants.VISIT)
            {
                contactDetails = GetVisitorHistoryDetail(modelVm, address, lookUpList, xref);
            }

            if (modelVm.Type == ContactConstants.CHECKOUTLOC)
            {
                contactDetails = GetCheckOutLocDetail(modelVm, address, xref);
            }
            if (modelVm.Type == ContactConstants.INCIDENT)
            {
                contactDetails = GetIncidentDateDetail(modelVm, address, lookUpList, xref);
            }
            if (modelVm.Type == ContactConstants.GRIEVANCE)
            {
                contactDetails = GetGrievanceDetail(modelVm, address, xref);
            }
            if (modelVm.Type == ContactConstants.JMSCLASSIFY)
            {
                contactDetails = GetClassifyDetail(modelVm, address, xref);
            }
            if (modelVm.Type == ContactConstants.JMSKEEPSEP && modelVm.SearchInmateId > 0)
            {
                contactDetails = GetKeepSepDetail(modelVm, address, xref);
            }
            return contactDetails;
        }
        private void GetHousingNumber(List<ContactHistoryDetailVm> contactHistoryDetail,
            ContactHistoryVm searchValue)
        {
            List<ContactHistoryDetailVm> housingNumDetail = _context.HousingUnitMoveHistory
                .Where(s => s.Inmate.PersonId == searchValue.PersonId && s.HousingUnitToId.HasValue
                      && (!searchValue.FromDate.HasValue || s.MoveDate <= searchValue.FilterDate &&
                          (s.MoveDateThru >= searchValue.FromDate.Value.Date ||
                           !s.MoveDateThru.HasValue)))
                .Select(x =>
                    new ContactHistoryDetailVm
                    {
                        Id = x.HousingUnitMoveHistoryId,
                        InmateId = x.Inmate.InmateId,
                        PersonId = x.Inmate.PersonId,
                        UnitId = x.HousingUnitTo.HousingUnitId,
                        FromDate = x.MoveDate,
                        ToDate = x.MoveDateThru,
                        Type = ContactConstants.HOUSINGNUMBER,
                        Detail = x.HousingUnitTo.HousingUnitId > 0 ? new ContactDetail
                        {
                            Detail1 = x.HousingUnitTo.HousingUnitLocation,
                            Detail2 = x.HousingUnitTo.HousingUnitNumber,
                            Detail3 = x.HousingUnitTo.HousingUnitBedNumber,
                            Detail4 = x.HousingUnitTo.HousingUnitBedLocation
                        } : new ContactDetail(),
                    }).ToList();
            housingNumDetail.ForEach(item =>
            {
                item.Count = GetHousingCount(item, searchValue);
            });
            contactHistoryDetail.AddRange(housingNumDetail);
        }
        private void GetHousingBedNumber(List<ContactHistoryDetailVm> contactHistoryDetail,
            ContactHistoryVm searchValue)
        {
            List<ContactHistoryDetailVm> housingNumDetail = _context.HousingUnitMoveHistory
                .Where(s => s.Inmate.PersonId == searchValue.PersonId && s.HousingUnitToId.HasValue
                      && (!searchValue.FromDate.HasValue || s.MoveDate <= searchValue.FilterDate &&
                          (s.MoveDateThru >= searchValue.FromDate.Value.Date || !s.MoveDateThru.HasValue)))
                .Select(x =>
                    new ContactHistoryDetailVm
                    {
                        Id = x.HousingUnitMoveHistoryId,
                        InmateId = x.Inmate.InmateId,
                        PersonId = x.Inmate.PersonId,
                        FromDate = x.MoveDate,
                        ToDate = x.MoveDateThru,
                        UnitId = x.HousingUnitTo.HousingUnitId,
                        Type = ContactConstants.HOUSINGBEDNUMBER,
                        Detail = x.HousingUnitTo.HousingUnitId > 0 ? new ContactDetail
                        {
                            Detail1 = x.HousingUnitTo.HousingUnitLocation,
                            Detail2 = x.HousingUnitTo.HousingUnitNumber,
                            Detail3 = x.HousingUnitTo.HousingUnitBedNumber,
                            Detail4 = x.HousingUnitTo.HousingUnitBedLocation
                        } : new ContactDetail(),
                    }).ToList();

            housingNumDetail.ForEach(item =>
            {
                item.Count = GetHousingCount(item, searchValue);
            });

            contactHistoryDetail.AddRange(housingNumDetail);
        }
        private void GetIntakeDate(List<ContactHistoryDetailVm> contactHistoryDetail,
            ContactHistoryVm searchValue, List<LookupVm> lookUpList)
        {
            IQueryable<IncarcerationArrestXref> xref = 
                _context.IncarcerationArrestXref.Where(s=>s.Arrest.InmateId.HasValue);
            List<ContactHistoryDetailVm> lstIncDetails = _context.Incarceration
                    .Where(x => x.Inmate.PersonId == searchValue.PersonId
                                && (!searchValue.FromDate.HasValue || x.DateIn <= searchValue.FilterDate &&
                                    (x.DateIn >= searchValue.FromDate.Value.Date ||
                                        !x.DateIn.HasValue))).Select(s => new ContactHistoryDetailVm
                                        {
                                            Id = s.IncarcerationId,
                                            PersonId = s.Inmate.PersonId,
                                            InmateId = s.InmateId ?? 0,
                                            FromDate = searchValue.AddHour > 0 ? s.DateIn.Value.AddHours(-searchValue.AddHour) : s.DateIn,
                                            ToDate = searchValue.AddHour > 0 ? s.DateIn.Value.AddHours(searchValue.AddHour) : s.DateIn,
                                            Type = ContactConstants.INTAKE,
                                            Detail = xref.Where(t => t.Arrest.InmateId == s.InmateId).
                                                Select(n => new ContactDetail
                                                {
                                                Detail1 = n.Arrest.ArrestBookingNo,
                                                Detail2 = lookUpList
                                                    .FirstOrDefault(l => l.LookupType == LookupConstants.ARRTYPE &&
                                                                         l.LookupIndex == Convert.ToInt32(n.Arrest.ArrestType))
                                                    .LookupDescription
                                                }).FirstOrDefault()
                                        }).ToList();
            lstIncDetails.ForEach(item =>
            {
                item.Count = GetIntakeCount(item, searchValue);
            });
            contactHistoryDetail.AddRange(lstIncDetails);
        }
        private void GetVisitorHistory(List<ContactHistoryDetailVm> contactHistoryDetail,
            ContactHistoryVm searchValue, List<LookupVm> lookUpList)
        {
            List<ContactHistoryDetailVm> schedule = _context.VisitToVisitor
                .Where(x => x.Visit.InmateId == searchValue.InmateId && (!searchValue.FromDate.HasValue ||
                             x.Visit.StartDate <= searchValue.FilterDate &&
                             (x.Visit.EndDate >= searchValue.FromDate.Value.Date ||
                              !x.Visit.EndDate.HasValue))).Select(x => new ContactHistoryDetailVm{
                    Id = x.Visit.LocationId ?? 0,
                    PersonId = x.Visit.Inmate.PersonId,
                    InmateId = x.Visit.Inmate.InmateId,
                    FromDate = x.Visit.StartDate,
                    ToDate = x.Visit.EndDate,
                    Type = ContactConstants.VISIT,
                    Detail = new ContactDetail
                    {
                        Detail1 = x.Visit.Location.PrivilegeDescription,
                        Detail3 = lookUpList.FirstOrDefault(l =>
                                l.LookupType == LookupConstants.VISREAS &&
                                Equals(l.LookupIndex, x.Visit.ReasonId))
                            .LookupDescription
                    }
                }).ToList();
            schedule.ForEach(item => item.Count = GetVisitorCount(item, searchValue));
            contactHistoryDetail.AddRange(schedule);
        }
        private void GetCheckOutLoc(List<ContactHistoryDetailVm> contactHistoryDetail,
            ContactHistoryVm searchValue)
        {
            List<ContactHistoryDetailVm> loc = _context.InmateTrak
                .Where(s => s.Inmate.InmateId == searchValue.InmateId &&
                            (!searchValue.FromDate.HasValue || s.InmateTrakDateOut <= searchValue.FilterDate &&
                             (s.InmateTrakDateIn >= searchValue.FromDate.Value.Date || !s.InmateTrakDateIn.HasValue)))
                .Select(x => new ContactHistoryDetailVm
                {
                    Id = x.InmateTrakLocationId??0,
                    PersonId = x.Inmate.PersonId,
                    InmateId = x.Inmate.InmateId,
                    FromDate = x.InmateTrakDateOut,
                    ToDate = x.InmateTrakDateIn,
                    Type = ContactConstants.CHECKOUTLOC,
                    Detail = new ContactDetail
                    {
                        Detail1 = x.InmateTrakLocation
                    }
                }).ToList();
            loc.ForEach(item => item.Count = GetCheckoutLocCount(item, searchValue));
            contactHistoryDetail.AddRange(loc);
        }
        private void GetIncidentDate(List<ContactHistoryDetailVm> contactHistoryDetail,
            ContactHistoryVm searchValue, List<LookupVm> lookUpList)
        {
            List<ContactHistoryDetailVm> discInmate = _context.DisciplinaryInmate
                .Where(x => x.Inmate.InmateId == searchValue.InmateId && (!searchValue.FromDate.HasValue ||
                  x.DisciplinaryIncident.DisciplinaryIncidentDate <= searchValue.FilterDate &&
                  x.DisciplinaryIncident.DisciplinaryIncidentDate >= searchValue.FromDate.Value.Date))
                .Select(x => new ContactHistoryDetailVm
                {
                    PersonId = x.Inmate.PersonId,
                    InmateId = x.Inmate.InmateId,
                    Id = x.DisciplinaryIncident.DisciplinaryIncidentId,
                    FromDate =x.DisciplinaryIncident.DisciplinaryIncidentDate,
                    ToDate = x.DisciplinaryIncident.DisciplinaryIncidentDate,
                    Detail = new ContactDetail
                    {
                        Detail1 = lookUpList.FirstOrDefault(l => l.LookupType == LookupConstants.DISCTYPE &&
                              Equals(l.LookupIndex, x.DisciplinaryIncident.DisciplinaryType))
                            .LookupDescription
                    },
                    Type = ContactConstants.INCIDENT
                }).ToList();
            discInmate.ForEach(item => { item.Count = GetIncidentCount(item, searchValue); });
            contactHistoryDetail.AddRange(discInmate);
        }
        private void GetGrievance(List<ContactHistoryDetailVm> contactHistoryDetail,
            ContactHistoryVm searchValue, List<LookupVm> lookUpList)
        {
            List<ContactHistoryDetailVm> housingNumDetail = _context.Grievance
                .Where(x => x.Inmate.PersonId == searchValue.PersonId && (!searchValue.FromDate.HasValue
                  || x.DateOccured <= searchValue.FilterDate &&
                  (x.DateOccured >= searchValue.FromDate.Value.Date ||
                   !x.DateOccured.HasValue))).Select(s => new ContactHistoryDetailVm
                {
                    Id = s.GrievanceId,
                    PersonId = s.Inmate.PersonId,
                    FromDate = s.DateOccured,
                    ToDate = s.DateOccured,
                    Detail = new ContactDetail
                    {
                        Detail1 = lookUpList
                            .SingleOrDefault(w =>
                                w.LookupType == LookupConstants.GRIEVTYPE && Equals(w.LookupIndex,
                                   s.GrievanceType))
                            .LookupDescription,
                    },
                    Type = ContactConstants.GRIEVANCE
                }).ToList();
            housingNumDetail.ForEach(item => { item.Count = GetGrievanceCount(item, searchValue); });
            contactHistoryDetail.AddRange(housingNumDetail);
        }
        private void GetClassify(List<ContactHistoryDetailVm> contactHistoryDetail,
            ContactHistoryVm searchValue)
        {
            List<ContactHistoryDetailVm> lstDetail = _context.InmateClassification.
                Where(x => x.InmateId == searchValue.InmateId &&
                           (!searchValue.FromDate.HasValue ||x.InmateDateAssigned <= searchValue.FilterDate && 
                            (x.InmateDateUnassigned >= searchValue.FromDate.Value.Date ||
                          !x.InmateDateUnassigned.HasValue)) && !string.IsNullOrEmpty(x.InmateClassificationReason))
                .Select(s => new ContactHistoryDetailVm
                {
                    PersonId = s.InmateNavigation.PersonId,
                    InmateId = s.InmateId,
                    FromDate = s.InmateDateAssigned,
                    ToDate = s.InmateDateUnassigned,
                    Detail = new ContactDetail
                    {
                        Detail1 = s.InmateClassificationReason
                    },
                    Type = ContactConstants.JMSCLASSIFY
                }).ToList();
            lstDetail.ForEach(item => { item.Count = GetClassifyCount(item, searchValue); });

            contactHistoryDetail.AddRange(lstDetail);
        }
        private void GetKeepSep(List<ContactHistoryDetailVm> contactHistoryDetail,
            ContactHistoryVm searchValue)
        {
            List<KeepSeparate> keepSeparates = _context.KeepSeparate.Where(ks =>
                ks.KeepSeparateInmate1Id == searchValue.InmateId ||
                ks.KeepSeparateInmate2Id == searchValue.InmateId).Select(s => new KeepSeparate
            {
                KeepSeparateType=s.KeepSeparateType,
                KeepSeparateInmate1Id = s.KeepSeparateInmate1Id,
                KeepSeparateInmate2Id=s.KeepSeparateInmate2Id
                }).ToList();
          
            int[] keepSepTo = keepSeparates.Select(s => s.KeepSeparateInmate1Id).ToArray();
            keepSepTo = keepSepTo.Concat(keepSeparates.Select(s => s.KeepSeparateInmate2Id).ToArray()).ToArray();
            keepSepTo = keepSepTo.Distinct().Where(s => s != searchValue.InmateId).ToArray();
        
            List<ContactHistoryDetailVm> keepSepToInc = _context.Incarceration
                .Where(s => keepSepTo.Any(a=>a==s.InmateId) && (!searchValue.FromDate.HasValue ||
                          s.DateIn <= searchValue.FilterDate && (s.ReleaseOut >= searchValue.FromDate.Value.Date ||
                              !s.ReleaseOut.HasValue)) &&
                            s.Inmate.Person.Address.Any())
                .Select(x => new ContactHistoryDetailVm
                {
                    FromDate = x.DateIn,
                    ToDate = x.ReleaseOut ?? DateTime.Now,
                    InmateId = x.Inmate.InmateId,
                    PersonId = x.Inmate.PersonId,
                    Type = ContactConstants.JMSKEEPSEP,
                    Detail = new ContactDetail
                    {
                        Detail1 = keepSeparates.FirstOrDefault(s => s.KeepSeparateInmate1Id == x.Inmate.InmateId ||
                         s.KeepSeparateInmate2Id == x.Inmate.InmateId).KeepSeparateType,
                        Detail2 = x.Inmate.InmateNumber
                    }
                }).ToList();

            keepSepToInc.ForEach(item => { item.Count = GetKeepSepCount(item, searchValue); });
            contactHistoryDetail.AddRange(keepSepToInc);
        }
        private int GetHousingCount(ContactHistoryDetailVm item, ContactHistoryVm searchValue)
        {
            DateTime? fromDate = item.FromDate?.Date.AddHours(item.FromDate.Value.Hour).
                AddMinutes(item.FromDate.Value.Minute).AddSeconds(0);
            DateTime toDate = item.ToDate?.Date.AddHours(item.ToDate.Value.Hour).
                                  AddMinutes(item.ToDate.Value.Minute).AddSeconds(0) ?? DateTime.Now;
            List<ContactHistoryDetailVm> housing = _context.HousingUnitMoveHistory.Where(s =>
                s.Inmate.PersonId != item.PersonId && s.HousingUnitToId.HasValue
                && (!searchValue.FromDate.HasValue || s.MoveDate <= searchValue.FilterDate &&
                    (s.MoveDateThru >= searchValue.FromDate.Value.Date ||
                        !s.MoveDateThru.HasValue)) && s.MoveDate <= toDate &&
                (s.MoveDateThru >= fromDate || !s.MoveDateThru.HasValue) &&
                s.Inmate.Person.Address.Any()).Select(x =>
                  new ContactHistoryDetailVm
                  {
                      FromDate = x.MoveDate,
                      ToDate = x.MoveDateThru,
                      Detail = x.HousingUnitTo.HousingUnitId > 0 ? new ContactDetail
                      {
                          Detail1 = x.HousingUnitTo.HousingUnitLocation,
                          Detail2 = x.HousingUnitTo.HousingUnitNumber,
                          Detail3 = x.HousingUnitTo.HousingUnitBedNumber,
                          Detail4 = x.HousingUnitTo.HousingUnitBedLocation
                      } : new ContactDetail(),
                  }).ToList();
            if (item.Type == ContactConstants.HOUSINGNUMBER)
            {
                housing = housing
                    .Where(x => !string.IsNullOrEmpty(x.Detail.Detail1) && !string.IsNullOrEmpty(x.Detail.Detail2) &&
                                !string.IsNullOrEmpty(item.Detail.Detail1) && !string.IsNullOrEmpty(item.Detail.Detail2)
                                && x.Detail.Detail1.Trim() == item.Detail.Detail1.Trim() &&
                                x.Detail.Detail2.Trim() == item.Detail.Detail2.Trim())
                    .ToList();
            }
           else 
            {
                housing = housing.Where(x =>
                    !string.IsNullOrEmpty(x.Detail.Detail3) && !string.IsNullOrEmpty(item.Detail.Detail3) &&
                    x.Detail.Detail3.Trim() == item.Detail.Detail3.Trim()).ToList();
            }
            return housing.Count;
        }
        private int GetIntakeCount(ContactHistoryDetailVm item, ContactHistoryVm searchValue)
        {
            DateTime? fromDate = item.FromDate?.Date.AddHours(item.FromDate.Value.Hour).
                AddMinutes(item.FromDate.Value.Minute).AddSeconds(0);
            List<ContactHistoryDetailVm> lstIncDetailVms = _context.Incarceration.Where(s =>
                    s.Inmate.PersonId != item.PersonId
                    && (!searchValue.FromDate.HasValue || s.DateIn <= searchValue.FilterDate &&
                        (s.DateIn >= searchValue.FromDate.Value.Date || !s.DateIn.HasValue)) && 
                    s.DateIn <= item.ToDate && s.DateIn >= fromDate &&
                    s.Inmate.Person.Address.Any())
                .Select(x =>
                    new ContactHistoryDetailVm
                    {
                        Id = x.IncarcerationId,
                        InmateId = x.Inmate.InmateId,
                        PersonId = x.Inmate.PersonId
                    }).ToList();
            return lstIncDetailVms.Count;
        }
        private int GetVisitorCount(ContactHistoryDetailVm item, ContactHistoryVm searchValue)
        {
            DateTime? fromDate = item.FromDate?.Date.AddHours(item.FromDate.Value.Hour).
                AddMinutes(item.FromDate.Value.Minute).AddSeconds(0);
            DateTime toDate = item.ToDate?.Date.AddHours(item.ToDate.Value.Hour).
                                  AddMinutes(item.ToDate.Value.Minute).AddSeconds(0) ?? DateTime.Now;
            //DateTime toDate = item.ToDate?.Date.AddHours(23).AddMinutes(59).AddSeconds(59) ?? DateTime.Now;
            List<ContactHistoryDetailVm> schedule = _context.VisitToVisitor
                .Where(x => x.Visit.InmateId != item.InmateId && x.Visit.StartDate!=null &&
                            x.Visit.EndDate.HasValue && x.Visit.LocationId.HasValue && x.Visit.LocationId==item.Id&&
                            (!searchValue.FromDate.HasValue ||
                        x.Visit.StartDate <= searchValue.FilterDate &&
                        (x.Visit.EndDate >= searchValue.FromDate.Value.Date ||
                            !x.Visit.EndDate.HasValue)) && x.Visit.StartDate <= toDate &&
                    (x.Visit.EndDate >= fromDate || !x.Visit.EndDate.HasValue) &&
                    x.Visit.Inmate.Person.Address.Any())
                .Select(x => new ContactHistoryDetailVm
                {
                    Id = x.Visit.LocationId ?? 0,
                    PersonId = x.Visit.Inmate.PersonId,
                    InmateId = x.Visit.Inmate.InmateId,
                    FromDate = x.Visit.StartDate,
                    ToDate = x.Visit.EndDate,
                    Type = ContactConstants.VISIT
                }).ToList();
            return schedule.Count;
        }
        private int GetCheckoutLocCount(ContactHistoryDetailVm item, ContactHistoryVm searchValue)
        {
            DateTime? toDate = item.ToDate ?? DateTime.Now;
            List<ContactHistoryDetailVm> loc = _context.InmateTrak
                .Where(s => s.Inmate.InmateId != searchValue.InmateId &&
                            (!searchValue.FromDate.HasValue || s.InmateTrakDateOut <= searchValue.FilterDate &&
                             (s.InmateTrakDateIn >= searchValue.FromDate.Value.Date || !s.InmateTrakDateIn.HasValue)) &&
                            (s.InmateTrakDateIn.HasValue || DateTime.Now >= item.FromDate) &&
                            (!s.InmateTrakDateIn.HasValue || s.InmateTrakDateIn >= item.FromDate) &&
                            s.InmateTrakDateOut <= toDate &&
                             s.InmateTrakLocationId==item.Id && s.Inmate.Person.Address.Any())
                .Select(x => new ContactHistoryDetailVm
                {
                    Id = x.InmateTrakLocationId ?? 0,
                    PersonId = x.Inmate.PersonId,
                    InmateId = x.Inmate.InmateId,
                    FromDate = x.InmateTrakDateOut,
                    ToDate = x.InmateTrakDateIn,
                    Type = ContactConstants.CHECKOUTLOC
                }).ToList();
            return loc.Count;
        }
        private int GetIncidentCount(ContactHistoryDetailVm item, ContactHistoryVm searchValue)
        {
            DateTime? fromDate = item.FromDate?.Date.AddHours(item.FromDate.Value.Hour).
                AddMinutes(item.FromDate.Value.Minute).AddSeconds(0);
            DateTime toDate = item.ToDate?.Date.AddHours(23).
                                  AddMinutes(59).AddSeconds(59) ?? DateTime.Now;
            List<ContactHistoryDetailVm> discInmate = _context.DisciplinaryInmate
                .Where(x => x.Inmate.InmateId != item.InmateId && (!searchValue.FromDate.HasValue ||
                          x.DisciplinaryIncident.DisciplinaryIncidentDate <= searchValue.FilterDate &&
                          (x.DisciplinaryIncident.DisciplinaryIncidentDate >= searchValue.FromDate.Value.Date)) &&
                            x.DisciplinaryIncident.DisciplinaryIncidentDate <= toDate &&
                            x.DisciplinaryIncident.DisciplinaryIncidentDate >= fromDate
                            && x.Inmate.Person.Address.Any())
                .Select(x => new ContactHistoryDetailVm
                {
                    PersonId = x.Inmate.PersonId,
                    InmateId = x.Inmate.InmateId,
                    Id = x.DisciplinaryIncident.DisciplinaryIncidentId,
                }).ToList();

            return discInmate.Count;
        }
        private int GetGrievanceCount(ContactHistoryDetailVm item, ContactHistoryVm searchValue)
        {
            DateTime? fromDate = item.FromDate?.Date.AddHours(item.FromDate.Value.Hour).
                AddMinutes(item.FromDate.Value.Minute).AddSeconds(0);
            DateTime toDate = item.ToDate?.Date.AddHours(23).
                                  AddMinutes(59).AddSeconds(59) ?? DateTime.Now;
            List<ContactHistoryDetailVm> grievanceList = _context.Grievance
                .Where(x => x.Inmate.PersonId != item.PersonId && (!searchValue.FromDate.HasValue ||
                        x.DateOccured <= searchValue.FilterDate &&
                        (x.DateOccured >= searchValue.FromDate.Value.Date ||
                            !x.DateOccured.HasValue)) &&
                    x.DateOccured <= toDate && x.DateOccured >= fromDate &&
                    x.Inmate.Person.Address.Any()).Select(
                    s => new ContactHistoryDetailVm
                    {
                        Id = s.GrievanceId,
                        PersonId = s.Inmate.PersonId,
                        InmateId = s.Inmate.InmateId,
                        FromDate = s.DateOccured,
                        ToDate = s.DateOccured
                    }).ToList();
            return grievanceList.Count;
        }
        private int GetClassifyCount(ContactHistoryDetailVm item, ContactHistoryVm searchValue)
        {
            DateTime toDate = item.ToDate?.Date.AddHours(item.ToDate.Value.Hour).
                                  AddMinutes(item.ToDate.Value.Minute).AddSeconds(0) ?? DateTime.Now;
            List<ContactHistoryDetailVm> lstInc= _context.InmateClassification.
                Where(x=>x.InmateId!=searchValue.InmateId &&
                       (!searchValue.FromDate.HasValue ||
                        x.InmateDateAssigned <= searchValue.FilterDate &&
                        (x.InmateDateUnassigned >= searchValue.FromDate.Value.Date ||
                         !x.InmateDateUnassigned.HasValue)) &&x.InmateDateAssigned <= toDate && 
                       (x.InmateDateUnassigned >=item.FromDate || !x.InmateDateUnassigned.HasValue) &&
                       x.InmateNavigation.Person.Address.Any() && !string.IsNullOrEmpty(x.InmateClassificationReason)&& 
                       x.InmateClassificationReason.Trim() == item.Detail.Detail1.Trim())
                    .Select(s => new ContactHistoryDetailVm
                      {
                          InmateId = s.InmateId,
                          FromDate = s.InmateDateAssigned,
                          ToDate = s.InmateDateUnassigned,
                          Detail = new ContactDetail
                          {
                              Detail1 = s.InmateClassificationReason
                          }
                    }).ToList();
            return lstInc.Count;
        }
        private int GetKeepSepCount(ContactHistoryDetailVm item, ContactHistoryVm searchValue)
        {
            DateTime? fromDate = item.FromDate?.Date.AddHours(item.FromDate.Value.Hour).
                AddMinutes(item.FromDate.Value.Minute).AddSeconds(0);
            DateTime toDate = item.ToDate?.Date.AddHours(item.ToDate.Value.Hour).
                                  AddMinutes(item.ToDate.Value.Minute).AddSeconds(0) ?? DateTime.Now;

            List<KeepSeparate> keepSeparates = _context.KeepSeparate.Where(ks =>
                ks.KeepSeparateInmate1Id == item.InmateId ||
                ks.KeepSeparateInmate2Id == item.InmateId).Select(s => new KeepSeparate
            {
                KeepSeparateType = s.KeepSeparateType,
                KeepSeparateInmate1Id = s.KeepSeparateInmate1Id,
                KeepSeparateInmate2Id = s.KeepSeparateInmate2Id
            }).ToList();
            int[] keepSepTo = keepSeparates.Select(s => s.KeepSeparateInmate1Id).ToArray();
            keepSepTo = keepSepTo.Concat(keepSeparates.Select(s => s.KeepSeparateInmate2Id).ToArray()).ToArray();
            keepSepTo = keepSepTo.Distinct().Where(s=>s!=item.InmateId).ToArray();
           
            List<ContactHistoryDetailVm> keepSepToInc = _context.Incarceration
                .Where(s => keepSepTo.Any(a=>a==s.Inmate.InmateId) && (!searchValue.FromDate.HasValue ||
                     s.DateIn <= searchValue.FilterDate && (s.ReleaseOut >= searchValue.FromDate.Value.Date ||
                         !s.ReleaseOut.HasValue)) && s.Inmate.Person.Address.Any())
                .Select(x => new ContactHistoryDetailVm
                {
                    FromDate = x.DateIn,
                    ToDate = x.ReleaseOut,
                    InmateId = x.Inmate.InmateId,
                    PersonId = x.Inmate.PersonId,
                    Type = ContactConstants.JMSKEEPSEP
                }).ToList();

            List<ContactHistoryDetailVm> lstInc = keepSepToInc.Where(x => x.FromDate <= toDate &&
                (x.ToDate >= fromDate ||
                    !x.ToDate.HasValue)).Select(x =>
                new ContactHistoryDetailVm
                {
                    PersonId = x.PersonId,
                    FromDate = x.FromDate,
                    ToDate = x.ToDate
                }).ToList();
            return lstInc.Count;
        }
        private List<ContactHistoryDetailVm> GetHousingNumberDetail(ContactHistoryModelVm modelVm, 
            IQueryable<Address> address, IQueryable<Incarceration> xref)
        {
            DateTime? fromDate = modelVm.FromDate?.Date.AddHours(modelVm.FromDate.Value.Hour).
                AddMinutes(modelVm.FromDate.Value.Minute).AddSeconds(0);
            DateTime toDate = modelVm.ToDate?.Date.AddHours(modelVm.ToDate.Value.Hour).
                                  AddMinutes(modelVm.ToDate.Value.Minute).AddSeconds(0) ?? DateTime.Now;
            List<ContactHistoryDetailVm> contactDetails = _context.HousingUnitMoveHistory.Where(s =>
                     s.Inmate.PersonId != modelVm.PersonId && s.HousingUnitToId.HasValue &&
                        (!modelVm.SearchFromDate.HasValue || s.MoveDate <= modelVm.FilterDate &&
                            (s.MoveDateThru >= modelVm.SearchFromDate.Value.Date ||
                                !s.MoveDateThru.HasValue)) && s.MoveDate <= toDate &&
                        (s.MoveDateThru >= fromDate || !s.MoveDateThru.HasValue) &&
                     s.Inmate.Person.Address.Any())
                    .Select(x =>
                        new ContactHistoryDetailVm
                        {
                            Id = x.HousingUnitMoveHistoryId,
                            InmateId = x.Inmate.InmateId,
                            PersonId = x.Inmate.PersonId,
                            InmateNumber = x.Inmate.InmateNumber,
                            FromDate = x.MoveDate,
                            ToDate = x.MoveDateThru,
                            UnitId = x.HousingUnitTo.HousingUnitId,
                            Type = modelVm.Type,
                            BookingStatus = xref.FirstOrDefault(s=>s.InmateId== x.Inmate.InmateId).ReleaseOut != null,
                            Detail = x.HousingUnitTo.HousingUnitId > 0 ? new ContactDetail
                            {
                                Detail1 = x.HousingUnitTo.HousingUnitLocation,
                                Detail2 = x.HousingUnitTo.HousingUnitNumber,
                                Detail3 = x.HousingUnitTo.HousingUnitBedNumber,
                                Detail4 = x.HousingUnitTo.HousingUnitBedLocation
                            } : new ContactDetail(),
                            AddressDetail = address.Where(t => t.PersonId == x.Inmate.PersonId)
                                .OrderByDescending(o => o.AddressId).
                                Select(n => new PersonAddressVm
                                {
                                    City = n.AddressCity,
                                    State = n.AddressState,
                                    Zip = n.AddressZip,
                                    Number = n.AddressNumber,
                                    Direction = n.AddressDirection,
                                    Street = n.AddressStreet,
                                    Suffix = n.AddressSuffix,
                                    DirectionSuffix = n.AddressDirectionSuffix,
                                    UnitType = n.AddressUnitType,
                                    UnitNo = n.AddressUnitNumber,
                                    Line2 = n.AddressLine2
                                }).FirstOrDefault()
                        }).ToList();

            contactDetails = contactDetails
            .Where(x => !string.IsNullOrEmpty(x.Detail.Detail1) && !string.IsNullOrEmpty(x.Detail.Detail2) &&
                        !string.IsNullOrEmpty(modelVm.Detail1) && !string.IsNullOrEmpty(modelVm.Detail2)
                        && x.Detail.Detail1.Trim() == modelVm.Detail1.Trim() && x.Detail.Detail2.Trim() == modelVm.Detail2.Trim())
            .ToList();
            contactDetails = GetPersonDetail(contactDetails, modelVm.Type);
            return contactDetails;
        }
        private List<ContactHistoryDetailVm> GetHousingBedNumberDetail(ContactHistoryModelVm modelVm,
            IQueryable<Address> address, IQueryable<Incarceration> xref)
        {
            DateTime? fromDate = modelVm.FromDate?.Date.AddHours(modelVm.FromDate.Value.Hour).
                AddMinutes(modelVm.FromDate.Value.Minute).AddSeconds(0);
            DateTime toDate = modelVm.ToDate?.Date.AddHours(modelVm.ToDate.Value.Hour).
                                  AddMinutes(modelVm.ToDate.Value.Minute).AddSeconds(0) ?? DateTime.Now;
            List<ContactHistoryDetailVm> contactDetails = _context.HousingUnitMoveHistory.Where(s =>
                       s.Inmate.PersonId != modelVm.PersonId && s.HousingUnitToId.HasValue &&
                       (!modelVm.SearchFromDate.HasValue || s.MoveDate <= modelVm.FilterDate &&
                           (s.MoveDateThru >= modelVm.SearchFromDate.Value.Date ||
                               !s.MoveDateThru.HasValue)) && s.MoveDate <= toDate &&
                       (s.MoveDateThru >= fromDate || !s.MoveDateThru.HasValue) &&
                       s.Inmate.Person.Address.Any())
                   .Select(x =>
                       new ContactHistoryDetailVm
                       {
                           Id = x.HousingUnitMoveHistoryId,
                           InmateId = x.Inmate.InmateId,
                           PersonId = x.Inmate.PersonId,
                           InmateNumber = x.Inmate.InmateNumber,
                           FromDate = x.MoveDate,
                           ToDate = x.MoveDateThru,
                           UnitId = x.HousingUnitTo.HousingUnitId,
                           Type = modelVm.Type,
                           BookingStatus = xref.FirstOrDefault(s => s.InmateId == x.Inmate.InmateId).ReleaseOut != null,
                           Detail = x.HousingUnitTo.HousingUnitId > 0 ? new ContactDetail
                           {
                               Detail1 = x.HousingUnitTo.HousingUnitLocation,
                               Detail2 = x.HousingUnitTo.HousingUnitNumber,
                               Detail3 = x.HousingUnitTo.HousingUnitBedNumber,
                               Detail4 = x.HousingUnitTo.HousingUnitBedLocation
                           } : new ContactDetail(),
                           AddressDetail = address.Where(t => t.PersonId == x.Inmate.PersonId)
                               .OrderByDescending(o => o.AddressId).
                               Select(n => new PersonAddressVm
                               {
                                   City = n.AddressCity,
                                   State = n.AddressState,
                                   Zip = n.AddressZip,
                                   Number = n.AddressNumber,
                                   Direction = n.AddressDirection,
                                   Street = n.AddressStreet,
                                   Suffix = n.AddressSuffix,
                                   DirectionSuffix = n.AddressDirectionSuffix,
                                   UnitType = n.AddressUnitType,
                                   UnitNo = n.AddressUnitNumber,
                                   Line2 = n.AddressLine2
                               }).FirstOrDefault()
                       }).ToList();
            contactDetails = contactDetails.Where(x =>
                !string.IsNullOrEmpty(x.Detail.Detail3) && !string.IsNullOrEmpty(modelVm.Detail3) &&
                x.Detail.Detail3.Trim() == modelVm.Detail3.Trim()).ToList();

            contactDetails = GetPersonDetail(contactDetails, modelVm.Type);
            return contactDetails;
        }
        private List<ContactHistoryDetailVm> GetIntakeDateDetail(ContactHistoryModelVm modelVm,
            IQueryable<Address> address, List<LookupVm> lookUpList, IQueryable<Incarceration> inc)
        {
            IQueryable<IncarcerationArrestXref> xref = _context.IncarcerationArrestXref.Where(s => s.Arrest.InmateId.HasValue);
            List<ContactHistoryDetailVm> lstIncDetailVms = _context.Incarceration.Where(s =>
                        s.Inmate.PersonId != modelVm.PersonId && (!modelVm.SearchFromDate.HasValue ||
                            s.DateIn <= modelVm.FilterDate &&
                            (s.DateIn >= modelVm.SearchFromDate.Value.Date || !s.DateIn.HasValue)) &&
                        s.DateIn <= modelVm.ToDate && s.DateIn >= modelVm.FromDate &&
                        s.Inmate.Person.Address.Any())
                    .Select(x =>
                        new ContactHistoryDetailVm
                        {
                            Id = x.IncarcerationId,
                            InmateId = x.Inmate.InmateId,
                            PersonId = x.Inmate.PersonId,
                            FromDate = x.DateIn,
                            ToDate = x.DateIn,
                            Type = modelVm.Type,
                            BookingStatus = inc.FirstOrDefault(s => s.InmateId == x.Inmate.InmateId).ReleaseOut != null,
                            AddressDetail = address.Where(t => t.PersonId == x.Inmate.PersonId)
                                .OrderByDescending(o => o.AddressId).
                                Select(n => new PersonAddressVm
                                {
                                    City = n.AddressCity,
                                    State = n.AddressState,
                                    Zip = n.AddressZip,
                                    Number = n.AddressNumber,
                                    Direction = n.AddressDirection,
                                    Street = n.AddressStreet,
                                    Suffix = n.AddressSuffix,
                                    DirectionSuffix = n.AddressDirectionSuffix,
                                    UnitType = n.AddressUnitType,
                                    UnitNo = n.AddressUnitNumber,
                                    Line2 = n.AddressLine2
                                }).FirstOrDefault(),
                            Detail = xref.Where(t => t.Arrest.InmateId == x.Inmate.InmateId).
                                Select(n => new ContactDetail
                                {
                                    Detail1 = n.Arrest.ArrestBookingNo,
                                    Detail2 = lookUpList
                                        .FirstOrDefault(l => l.LookupType == LookupConstants.ARRTYPE &&
                                                             l.LookupIndex == Convert.ToInt32(n.Arrest.ArrestType))
                                        .LookupDescription
                                }).FirstOrDefault()
                        }).ToList();
            List<ContactHistoryDetailVm> contactDetails = GetPersonDetail(lstIncDetailVms, modelVm.Type);
            return contactDetails;
        }
        private List<ContactHistoryDetailVm> GetVisitorHistoryDetail(ContactHistoryModelVm modelVm,
            IQueryable<Address> address, List<LookupVm> lookUpList, IQueryable<Incarceration> xref)
        {
            DateTime? fromDate = modelVm.FromDate?.Date.AddHours(modelVm.FromDate.Value.Hour).
                AddMinutes(modelVm.FromDate.Value.Minute).AddSeconds(0);
            DateTime toDate = modelVm.ToDate?.Date.AddHours(modelVm.ToDate.Value.Hour).
                                  AddMinutes(modelVm.ToDate.Value.Minute).AddSeconds(0) ?? DateTime.Now;
            List<ContactHistoryDetailVm> schedule = _context.VisitToVisitor
                   .Where(x => x.Visit.InmateId != modelVm.InmateId && x.Visit.LocationId==modelVm.Id 
                                && x.Visit.StartDate!=null &&
                               x.Visit.EndDate.HasValue && x.Visit.LocationId.HasValue 
                               && (!modelVm.SearchFromDate.HasValue ||
                           x.Visit.StartDate <= modelVm.FilterDate &&
                           (x.Visit.EndDate >= modelVm.SearchFromDate.Value.Date ||
                            !x.Visit.EndDate.HasValue)) && x.Visit.StartDate <= toDate &&
                       (x.Visit.EndDate >= fromDate || !x.Visit.EndDate.HasValue) &&
                       x.Visit.Inmate.Person.Address.Any())
                   .Select(x => new ContactHistoryDetailVm
                   {
                       Id = x.Visit.LocationId ?? 0,
                       PersonId = x.Visit.Inmate.PersonId,
                       InmateId = x.Visit.Inmate.InmateId,
                       FromDate = x.Visit.StartDate,
                       ToDate = x.Visit.EndDate,
                       Type = ContactConstants.VISIT,
                       BookingStatus = xref.FirstOrDefault(s => s.InmateId == x.Visit.Inmate.InmateId).ReleaseOut != null,
                       Detail = new ContactDetail
                       {
                           Detail1 = x.Visit.Location.PrivilegeDescription,
                           Detail3 = lookUpList.FirstOrDefault(l =>
                                   l.LookupType == LookupConstants.VISREAS &&
                                   Equals(l.LookupIndex, x.Visit.ReasonId))
                               .LookupDescription
                       },
                       AddressDetail = address.Where(t => t.PersonId == x.Visit.Inmate.PersonId)
                           .OrderByDescending(o => o.AddressId).
                           Select(n => new PersonAddressVm
                           {
                               City = n.AddressCity,
                               State = n.AddressState,
                               Zip = n.AddressZip,
                               Number = n.AddressNumber,
                               Direction = n.AddressDirection,
                               Street = n.AddressStreet,
                               Suffix = n.AddressSuffix,
                               DirectionSuffix = n.AddressDirectionSuffix,
                               UnitType = n.AddressUnitType,
                               UnitNo = n.AddressUnitNumber,
                               Line2 = n.AddressLine2
                           }).FirstOrDefault()
                   }).ToList();
            List<ContactHistoryDetailVm> contactDetails = GetPersonDetail(schedule, modelVm.Type);
            return contactDetails;
        }
        private List<ContactHistoryDetailVm> GetCheckOutLocDetail(ContactHistoryModelVm modelVm,
            IQueryable<Address> address, IQueryable<Incarceration> xref)
        {
            DateTime? toDate = modelVm.ToDate ?? DateTime.Now;
            List<ContactHistoryDetailVm> loc = _context.InmateTrak
                .Where(s => s.Inmate.InmateId != modelVm.InmateId &&
                            (!modelVm.SearchFromDate.HasValue || s.InmateTrakDateOut <= modelVm.FilterDate &&
                             (s.InmateTrakDateIn >= modelVm.SearchFromDate.Value.Date || !s.InmateTrakDateIn.HasValue)) &&
                            (s.InmateTrakDateIn.HasValue || DateTime.Now >= modelVm.FromDate) &&
                            (!s.InmateTrakDateIn.HasValue || s.InmateTrakDateIn >= modelVm.FromDate) &&
                            s.InmateTrakDateOut <= toDate &&
                            s.InmateTrakLocationId == modelVm.Id && s.Inmate.Person.Address.Any())
                .Select(x => new ContactHistoryDetailVm
                {
                    Id = x.InmateTrakId,
                    PersonId = x.Inmate.PersonId,
                    InmateId = x.Inmate.InmateId,
                    FromDate = x.InmateTrakDateOut,
                    ToDate = x.InmateTrakDateIn,
                    Type = ContactConstants.CHECKOUTLOC,
                    BookingStatus = xref.FirstOrDefault(s => s.InmateId == x.Inmate.InmateId).ReleaseOut != null,
                    Detail = new ContactDetail
                    {
                        Detail1 = x.InmateTrakLocation
                    },
                    AddressDetail = address.Where(t => t.PersonId == x.Inmate.PersonId)
                        .OrderByDescending(o => o.AddressId).
                        Select(n => new PersonAddressVm
                        {
                            City = n.AddressCity,
                            State = n.AddressState,
                            Zip = n.AddressZip,
                            Number = n.AddressNumber,
                            Direction = n.AddressDirection,
                            Street = n.AddressStreet,
                            Suffix = n.AddressSuffix,
                            DirectionSuffix = n.AddressDirectionSuffix,
                            UnitType = n.AddressUnitType,
                            UnitNo = n.AddressUnitNumber,
                            Line2 = n.AddressLine2
                        }).FirstOrDefault()
                }).ToList();
            List<ContactHistoryDetailVm> contactDetails = GetPersonDetail(loc, modelVm.Type);
            return contactDetails;
        }
        private List<ContactHistoryDetailVm> GetIncidentDateDetail(ContactHistoryModelVm modelVm,
            IQueryable<Address> address, List<LookupVm> lookUpList, IQueryable<Incarceration> xref)
        {
            DateTime? fromDate = modelVm.FromDate?.Date.AddHours(modelVm.FromDate.Value.Hour).
                AddMinutes(modelVm.FromDate.Value.Minute).AddSeconds(0);
             DateTime toDate = modelVm.ToDate?.Date.AddHours(23).
                                  AddMinutes(59).AddSeconds(59) ?? DateTime.Now;
            List<ContactHistoryDetailVm> discInmate = _context.DisciplinaryInmate
                    .Where(x => x.Inmate.InmateId != modelVm.InmateId && (!modelVm.SearchFromDate.HasValue ||
                      x.DisciplinaryIncident.DisciplinaryIncidentDate <= modelVm.FilterDate &&
                      (x.DisciplinaryIncident.DisciplinaryIncidentDate >= modelVm.SearchFromDate.Value.Date)) &&
                                x.DisciplinaryIncident.DisciplinaryIncidentDate<=toDate &&
                                x.DisciplinaryIncident.DisciplinaryIncidentDate >= fromDate
                      //(!x.DisciplinaryIncident.DisciplinaryIncidentDate.HasValue ||
                      // x.DisciplinaryIncident.DisciplinaryIncidentDate.Value.Date <= toDate.Date &&
                      // x.DisciplinaryIncident.DisciplinaryIncidentDate.Value.Hour <= toDate.Hour &&
                      // x.DisciplinaryIncident.DisciplinaryIncidentDate.Value.Minute <= toDate.Minute)
                      //&& (!x.DisciplinaryIncident.DisciplinaryIncidentDate.HasValue ||
                      //    x.DisciplinaryIncident.DisciplinaryIncidentDate.Value.Date >= fromDate.Value.Date &&
                      //    x.DisciplinaryIncident.DisciplinaryIncidentDate.Value.Hour >= fromDate.Value.Hour &&
                      //    x.DisciplinaryIncident.DisciplinaryIncidentDate.Value.Minute >= fromDate.Value.Minute)
                      && x.Inmate.Person.Address.Any())
                    .Select(x => new ContactHistoryDetailVm
                    {
                        PersonId = x.Inmate.PersonId,
                        InmateId = x.Inmate.InmateId,
                        Id = x.DisciplinaryIncident.DisciplinaryIncidentId,
                        FromDate = x.DisciplinaryIncident.DisciplinaryIncidentDate,
                        ToDate = x.DisciplinaryIncident.DisciplinaryIncidentDate,
                        Type = modelVm.Type,
                        BookingStatus = xref.FirstOrDefault(s => s.InmateId == x.Inmate.InmateId).ReleaseOut != null,
                        Detail = new ContactDetail
                        {
                            Detail1 = lookUpList.FirstOrDefault(l => l.LookupType == LookupConstants.DISCTYPE &&
                                  Equals(l.LookupIndex, x.DisciplinaryIncident.DisciplinaryType))
                                .LookupDescription
                        },
                        AddressDetail = address.Where(t => t.PersonId == x.Inmate.PersonId)
                            .OrderByDescending(o => o.AddressId).
                            Select(n => new PersonAddressVm
                            {
                                City = n.AddressCity,
                                State = n.AddressState,
                                Zip = n.AddressZip,
                                Number = n.AddressNumber,
                                Direction = n.AddressDirection,
                                Street = n.AddressStreet,
                                Suffix = n.AddressSuffix,
                                DirectionSuffix = n.AddressDirectionSuffix,
                                UnitType = n.AddressUnitType,
                                UnitNo = n.AddressUnitNumber,
                                Line2 = n.AddressLine2
                            }).FirstOrDefault()
                    }).ToList();

            List<ContactHistoryDetailVm> contactDetails = GetPersonDetail(discInmate, modelVm.Type);
            return contactDetails;
        }
        private List<ContactHistoryDetailVm> GetGrievanceDetail(ContactHistoryModelVm modelVm,
            IQueryable<Address> address, IQueryable<Incarceration> xref)
        {
            DateTime? fromDate = modelVm.FromDate?.Date.AddHours(modelVm.FromDate.Value.Hour).
                AddMinutes(modelVm.FromDate.Value.Minute).AddSeconds(0);
            DateTime toDate = modelVm.ToDate?.Date.AddHours(23).
                                  AddMinutes(59).AddSeconds(59) ?? DateTime.Now;
            List<ContactHistoryDetailVm> grievanceList = _context.Grievance
                   .Where(x => x.Inmate.PersonId != modelVm.PersonId && (!modelVm.SearchFromDate.HasValue ||
                           x.DateOccured <= modelVm.FilterDate &&
                           (x.DateOccured >= modelVm.SearchFromDate.Value.Date ||
                               !x.DateOccured.HasValue)) && x.DateOccured <= toDate &&
                       x.DateOccured >= fromDate
                       && x.Inmate.Person.Address.Any())
                   .Select(
                       s => new ContactHistoryDetailVm
                       {
                           Id = s.GrievanceId,
                           InmateId = s.Inmate.InmateId,
                           PersonId = s.Inmate.PersonId,
                           FromDate = s.DateOccured,
                           ToDate = s.DateOccured,
                           Type = modelVm.Type,
                           BookingStatus = xref.FirstOrDefault(x => x.InmateId == s.Inmate.InmateId).ReleaseOut != null,
                           AddressDetail = address.Where(t => t.PersonId == s.Inmate.PersonId)
                               .OrderByDescending(o => o.AddressId).
                               Select(n => new PersonAddressVm
                               {
                                   City = n.AddressCity,
                                   State = n.AddressState,
                                   Zip = n.AddressZip,
                                   Number = n.AddressNumber,
                                   Direction = n.AddressDirection,
                                   Street = n.AddressStreet,
                                   Suffix = n.AddressSuffix,
                                   DirectionSuffix = n.AddressDirectionSuffix,
                                   UnitType = n.AddressUnitType,
                                   UnitNo = n.AddressUnitNumber,
                                   Line2 = n.AddressLine2
                               }).FirstOrDefault()
                       }).ToList();
            List<ContactHistoryDetailVm> contactDetails = GetPersonDetail(grievanceList, modelVm.Type);
            return contactDetails;
        }
        private List<ContactHistoryDetailVm> GetClassifyDetail(ContactHistoryModelVm modelVm,
            IQueryable<Address> address, IQueryable<Incarceration> xref)
        {
            DateTime toDate = modelVm.ToDate?.Date.AddHours(modelVm.ToDate.Value.Hour).
                                  AddMinutes(modelVm.ToDate.Value.Minute).AddSeconds(0) ?? DateTime.Now;
            List<ContactHistoryDetailVm> lstDetail= _context.InmateClassification.
                Where(x => x.InmateId != modelVm.InmateId &&(!modelVm.SearchFromDate.HasValue ||
                            x.InmateDateAssigned <= modelVm.FilterDate && 
                            (x.InmateDateUnassigned >= modelVm.SearchFromDate.Value.Date ||
                           !x.InmateDateUnassigned.HasValue)) && x.InmateDateAssigned <= toDate &&
                           (x.InmateDateUnassigned >= modelVm.FromDate || 
                            !x.InmateDateUnassigned.HasValue) && !string.IsNullOrEmpty(x.InmateClassificationReason)
                           && x.InmateClassificationReason==modelVm.Detail1 
                           && x.InmateNavigation.Person.Address.Any())
                .Select(s => new ContactHistoryDetailVm
                {
                    PersonId = s.InmateNavigation.PersonId,
                    InmateId = s.InmateId,
                    FromDate = s.InmateDateAssigned,
                    ToDate = s.InmateDateUnassigned,
                    Type = modelVm.Type,
                    BookingStatus = xref.FirstOrDefault(x => x.InmateId == s.InmateId).ReleaseOut != null,
                    Detail = new ContactDetail
                    {
                        Detail1 = s.InmateClassificationReason
                    },
                    AddressDetail = address.Where(t => t.PersonId == s.InmateNavigation.PersonId)
                        .OrderByDescending(o => o.AddressId).Select(n => new PersonAddressVm
                        {
                            City = n.AddressCity,
                            State = n.AddressState,
                            Zip = n.AddressZip,
                            Number = n.AddressNumber,
                            Direction = n.AddressDirection,
                            Street = n.AddressStreet,
                            Suffix = n.AddressSuffix,
                            DirectionSuffix = n.AddressDirectionSuffix,
                            UnitType = n.AddressUnitType,
                            UnitNo = n.AddressUnitNumber,
                            Line2 = n.AddressLine2
                        }).FirstOrDefault()
                }).ToList();
            List<ContactHistoryDetailVm> contactDetails = GetPersonDetail(lstDetail, modelVm.Type);
            return contactDetails;
        }
        private List<ContactHistoryDetailVm> GetKeepSepDetail(ContactHistoryModelVm modelVm,
            IQueryable<Address> address, IQueryable<Incarceration> xref)
        {
            DateTime? fromDate = modelVm.FromDate?.Date.AddHours(modelVm.FromDate.Value.Hour).
                AddMinutes(modelVm.FromDate.Value.Minute).AddSeconds(0);
            DateTime toDate = modelVm.ToDate?.Date.AddHours(modelVm.ToDate.Value.Hour).
                                  AddMinutes(modelVm.ToDate.Value.Minute).AddSeconds(0) ?? DateTime.Now;
            List<KeepSeparate> keepSeparates = _context.KeepSeparate.Where(ks =>
                ks.KeepSeparateInmate1Id == modelVm.InmateId ||
                ks.KeepSeparateInmate2Id == modelVm.InmateId).Select(s => new KeepSeparate
            {
                KeepSeparateType = s.KeepSeparateType,
                KeepSeparateInmate1Id = s.KeepSeparateInmate1Id,
                KeepSeparateInmate2Id = s.KeepSeparateInmate2Id
            }).ToList();
            int[] keepSepTo = keepSeparates.Select(s => s.KeepSeparateInmate1Id).ToArray();
            keepSepTo = keepSepTo.Concat(keepSeparates.Select(s => s.KeepSeparateInmate2Id).ToArray()).ToArray();
            keepSepTo = keepSepTo.Distinct().Where(s => s != modelVm.InmateId).ToArray();
            List<ContactHistoryDetailVm> keepSepToInc = _context.Incarceration
                .Where(s => keepSepTo.Any(a=>a==s.Inmate.InmateId) && (!modelVm.SearchFromDate.HasValue ||
                 s.DateIn <= modelVm.FilterDate && (s.ReleaseOut >= modelVm.SearchFromDate.Value.Date ||
                !s.ReleaseOut.HasValue)) && s.Inmate.Person.Address.Any())
                .Select(x => new ContactHistoryDetailVm
                {
                    FromDate = x.DateIn,
                    ToDate = x.ReleaseOut,
                    InmateId = x.Inmate.InmateId,
                    PersonId = x.Inmate.PersonId,
                    Type = ContactConstants.JMSKEEPSEP,
                    Detail = new ContactDetail
                    {
                       Detail1 = keepSeparates.FirstOrDefault(s => s.KeepSeparateInmate1Id == x.Inmate.InmateId ||
                       s.KeepSeparateInmate2Id == x.Inmate.InmateId).KeepSeparateType,
                       Detail2 = x.Inmate.InmateNumber
                    }
                }).ToList();

            List<ContactHistoryDetailVm> lstInc = keepSepToInc.Where(x => x.FromDate <= toDate &&
                (x.ToDate >= fromDate ||
                    !x.ToDate.HasValue)).Select(x =>
                new ContactHistoryDetailVm
                {
                    PersonId = x.PersonId,
                    InmateId = x.InmateId,
                    FromDate = x.FromDate,
                    ToDate = x.ToDate,
                    Type = ContactConstants.JMSKEEPSEP,
                    Detail = new ContactDetail
                    {
                        Detail1 = x.Detail.Detail1
                    },
                    BookingStatus = xref.FirstOrDefault(s => s.InmateId == x.InmateId)?.ReleaseOut != null,
                    AddressDetail = address.Where(t => t.PersonId == x.PersonId)
                        .OrderByDescending(o => o.AddressId).
                        Select(n => new PersonAddressVm
                        {
                            City = n.AddressCity,
                            State = n.AddressState,
                            Zip = n.AddressZip,
                            Number = n.AddressNumber,
                            Direction = n.AddressDirection,
                            Street = n.AddressStreet,
                            Suffix = n.AddressSuffix,
                            DirectionSuffix = n.AddressDirectionSuffix,
                            UnitType = n.AddressUnitType,
                            UnitNo = n.AddressUnitNumber,
                            Line2 = n.AddressLine2
                        }).FirstOrDefault()
                }).ToList();
            List<ContactHistoryDetailVm> contactDetails = GetPersonDetail(lstInc, modelVm.Type);
            return contactDetails;
        }
        private List<ContactHistoryDetailVm> GetPersonDetail(List<ContactHistoryDetailVm> lstDetail,
            string type)
        {
            int?[] personId = lstDetail.Select(x => x.PersonId).ToArray();
            string externalPath = _photosService.GetExternalPath();
            string path = _photosService.GetPath();
            List<Person> persons = _context.Person.Where(x => personId.Any(a=>a==x.PersonId)).Select(s => new Person
            {
                PersonId = s.PersonId,
                PersonLastName = s.PersonLastName,
                PersonFirstName = s.PersonFirstName,
                PersonMiddleName = s.PersonMiddleName,
                PersonEmail = s.PersonEmail,
                PersonPhone = s.PersonPhone,
                PersonBusinessPhone = s.PersonBusinessPhone,
                PersonDob = s.PersonDob
            }).ToList();
            List<Inmate> inmate = _context.Inmate.Where(x => personId.Any(a => a == x.PersonId)).ToList();
            List<Identifiers> identifiers = _context.Identifiers
                .Where(idf => idf.DeleteFlag == 0 && personId.Any(s => s == idf.PersonId) && 
                              !string.IsNullOrEmpty(idf.IdentifierType)  && idf.IdentifierType =="1"
                              ).OrderByDescending(s => s.IdentifierId).ToList();
            if (persons.Count > 0)
            {
                lstDetail.ForEach(p =>
                {
                    Person per = persons.FirstOrDefault(x => x.PersonId == p.PersonId);
                    if (per != null)
                    {
                        p.LastName = per.PersonLastName;
                        p.FirstName = per.PersonFirstName;
                        p.MiddleName = per.PersonMiddleName;
                        p.Type = type;
                        p.Email = per.PersonEmail;
                        p.HomePhone = per.PersonPhone;
                        p.Dob = per.PersonDob;
                        p.BusinessPhone = per.PersonBusinessPhone;
                        p.InmateNumber = inmate.FirstOrDefault(s => s.PersonId == per.PersonId)?.InmateNumber;
                        p.PhotographRelativePath = identifiers.Where(d => d.PersonId == per.PersonId).Select(x =>
                            x.PhotographRelativePath == null
                                ? externalPath + x.PhotographPath
                                : path + x.PhotographRelativePath).FirstOrDefault();
                        p.IdentifierId = identifiers.FirstOrDefault(i => i.PersonId == per.PersonId)?.IdentifierId ?? 0;
                    }
                });
            }
            return lstDetail;
        }
        #endregion

        #region Personnel Contacts
        public List<ContactHistoryDetailVm> GetPersonnelContacts(ContactHistoryVm searchValue)
        {
            List<LookupVm> lookUpList = _context.Lookup.Where(w => w.LookupInactive == 0 && (w.LookupType == LookupConstants.ARRTYPE ||
                                                                   w.LookupType == LookupConstants.RELATIONS ||
                                                                   w.LookupType == LookupConstants.DISCTYPE ||
                                                                   w.LookupType == LookupConstants.GRIEVTYPE ||
                                                                   w.LookupType == LookupConstants.DNADISPO ||
                                                                   w.LookupType == LookupConstants.TESTTYPE)).Select(
                x => new LookupVm
                {
                    LookupIndex = x.LookupIndex,
                    LookupType=x.LookupType,
                    LookupDescription = x.LookupDescription
                }).ToList();
            searchValue.FilterDate = searchValue.ToDate?.Date.AddHours(23).AddMinutes(59).AddSeconds(59) ?? DateTime.Now;
            List<ContactHistoryDetailVm> contactHistoryDetail = new List<ContactHistoryDetailVm>();
            if (searchValue.IsIntakeDate)
            {
                GetIntakePersonnel(contactHistoryDetail, searchValue, lookUpList);
            }
            if (searchValue.IsRelease)
            {
                GetReleasePersonnel(contactHistoryDetail, searchValue, lookUpList);
            }
            if (searchValue.IsHousingMove)
            {
                GetHousingPersonnel(contactHistoryDetail, searchValue);
            }
            if (searchValue.IsPhoto)
            {
                GetPhotoPersonnel(contactHistoryDetail, searchValue);
            }
            if (searchValue.IsClassify)
            {
                GetClassifyPersonnel(contactHistoryDetail, searchValue);
            }
            if (searchValue.IsJmsIncident)
            {
                GetIncidentPersonnel(contactHistoryDetail, searchValue, lookUpList);
            }
            if (searchValue.IsGrievance)
            {
                GetGrievancePersonnel(contactHistoryDetail, searchValue, lookUpList);
            }
            if (searchValue.IsFloorNote)
            {
                GetFloorPersonnel(contactHistoryDetail, searchValue);
            }
            if (searchValue.IsDna)
            {
                GetDnaPersonnel(contactHistoryDetail, searchValue, lookUpList);
            }
            if (searchValue.IsTesting)
            {
                GetTestingPersonnel(contactHistoryDetail, searchValue, lookUpList);
            }
            return contactHistoryDetail;
        }
        public List<ContactHistoryDetailVm> GetPersonnelDetail(ContactHistoryDetailVm item)
        {
            List<LookupVm> lookUpList = _context.Lookup.Where(w => w.LookupInactive == 0 
                && w.LookupType == LookupConstants.TESTTYPE)
                .Select(
                x => new LookupVm
                {
                    LookupIndex = x.LookupIndex,
                    LookupType = x.LookupType,
                    LookupDescription = x.LookupDescription
                }).ToList();
            List<ContactHistoryDetailVm> contactDetails = new List<ContactHistoryDetailVm>();
            if (item.Type == ContactConstants.INTAKE)
            {
                contactDetails = GetPersonnelIntakeDetail(item);
            }
            if (item.Type == ContactConstants.RELEASE)
            {
                contactDetails = GetReleasePersonnelDetail(item);
            }
            if (item.Type == ContactConstants.HOUSINGMOVE)
            {
                contactDetails = GetHousingPersonnelDetail(item);
            }
            if (item.Type == ContactConstants.PHOTO)
            {
                contactDetails = GetPhotoPersonnelDetail(item);
            }
            if (item.Type == ContactConstants.CLASSIFY)
            {
                contactDetails = GetClassifyPersonnelDetail(item);
            }
            if (item.Type == ContactConstants.INCIDENT)
            {
                contactDetails = GetIncidentPersonnelDetail(item);
            }
            if (item.Type == ContactConstants.GRIEVANCE)
            {
                contactDetails = GetGrievancePersonnelDetail(item);
            }
            if (item.Type == ContactConstants.FLOORNOTE)
            {
                contactDetails = GetFloorPersonnelDetail(item);
            }
            if (item.Type == ContactConstants.DNA)
            {
                contactDetails = GetDnaPersonnelDetail(item);
            }
            if (item.Type == ContactConstants.TESTING)
            {
                contactDetails = GetTestingPersonnelDetail(item, lookUpList);
            }
            return contactDetails;
        }
        private void GetIntakePersonnel(List<ContactHistoryDetailVm> contactHistoryDetail,
            ContactHistoryVm searchValue, List<LookupVm> lookUpList)
        {
            IQueryable<IncarcerationArrestXref> xref =
                _context.IncarcerationArrestXref.Where(s => s.Arrest.InmateId.HasValue);
            List<ContactHistoryDetailVm> incarceration = _context.Incarceration
                   .Where(x => x.Inmate.PersonId == searchValue.PersonId && (!searchValue.FromDate.HasValue ||
                       x.DateIn <= searchValue.FilterDate &&
                       x.DateIn >= searchValue.FromDate.Value.Date)).Select(x => new ContactHistoryDetailVm
                       {
                           Id = xref.FirstOrDefault(s=>s.IncarcerationId== x.IncarcerationId).ArrestId??0,
                           PersonId = x.Inmate.PersonId,
                           InmateId = x.Inmate.InmateId,
                           FromDate = x.DateIn,
                           Type = ContactConstants.INTAKE,
                           Detail = xref.Where(t => t.Arrest.InmateId == x.Inmate.InmateId).
                               Select(n => new ContactDetail
                               {
                                   Detail1 = n.Arrest.ArrestBookingNo,
                                   Detail2 = lookUpList
                                       .FirstOrDefault(l => l.LookupType == LookupConstants.ARRTYPE &&
                                                            l.LookupIndex == Convert.ToInt32(n.Arrest.ArrestType))
                                       .LookupDescription
                               }).FirstOrDefault()
                       }).ToList();
            incarceration.ForEach(item =>
            {
                item.Count = GetIntakePersonnelCount(item);
            });
            contactHistoryDetail.AddRange(incarceration);
        }
        private void GetReleasePersonnel(List<ContactHistoryDetailVm> contactHistoryDetail,
            ContactHistoryVm searchValue, List<LookupVm> lookUpList)
        {
            List<ContactHistoryDetailVm> incRef = _context.IncarcerationArrestXref
                .Where(x => x.Arrest.Inmate.PersonId == searchValue.PersonId && (!searchValue.FromDate.HasValue ||
                                                                                 x.ReleaseDate <= searchValue.FilterDate && x.ReleaseDate >= searchValue.FromDate.Value.Date))
                .Select(x => new ContactHistoryDetailVm
                {
                    Id = x.Arrest.ArrestId,
                    PersonId = x.Arrest.Inmate.PersonId,
                    FromDate = x.ReleaseDate,
                    Detail = new ContactDetail
                    {
                        Detail1 = x.Arrest.ArrestBookingNo,
                        Detail2 = lookUpList
                            .FirstOrDefault(l => l.LookupType == LookupConstants.ARRTYPE &&
                                                 l.LookupIndex == Convert.ToInt32(x.Arrest.ArrestType)).LookupDescription
                    },
                    Type = ContactConstants.RELEASE
                }).ToList();
            incRef.ForEach(item => { item.Count = GetReleasePersonnelCount(item); });
            contactHistoryDetail.AddRange(incRef);
        }
        private void GetHousingPersonnel(List<ContactHistoryDetailVm> contactHistoryDetail,
            ContactHistoryVm searchValue)
        {
            List<ContactHistoryDetailVm> historyDetail = _context.HousingUnitMoveHistory
                .Where(s => s.Inmate.PersonId == searchValue.PersonId && s.HousingUnitToId != null && (!searchValue.FromDate.HasValue ||
                    s.MoveDate <= searchValue.FilterDate && (s.MoveDateThru >= searchValue.FromDate.Value.Date ||
                     s.MoveDateThru == null)))
                .Select(x =>
                    new ContactHistoryDetailVm
                    {
                        Id = x.HousingUnitMoveHistoryId,
                        InmateId = x.Inmate.InmateId,
                        PersonId = x.Inmate.PersonId,
                        FromDate = x.MoveDate,
                        ToDate = x.MoveDateThru,
                        UnitId = x.HousingUnitTo.HousingUnitId,
                        Detail = x.HousingUnitTo.HousingUnitId > 0 ? new ContactDetail
                        {
                            Detail1 = x.HousingUnitTo.HousingUnitLocation,
                            Detail2 = x.HousingUnitTo.HousingUnitNumber,
                            Detail3 = x.HousingUnitTo.HousingUnitBedNumber,
                            Detail4 = x.HousingUnitTo.HousingUnitBedLocation
                        } : new ContactDetail(),
                        Type = ContactConstants.HOUSINGMOVE
                    }).ToList();

            historyDetail.ForEach(item =>
            {
                item.Count = GetHousingPersonnelCount(item);
            });
            contactHistoryDetail.AddRange(historyDetail);
        }
        private void GetPhotoPersonnel(List<ContactHistoryDetailVm> contactHistoryDetail,
            ContactHistoryVm searchValue)
        {
            List<ContactHistoryDetailVm> photo = _context.Identifiers.Where(x => x.PersonId == searchValue.PersonId
                             && (!searchValue.FromDate.HasValue || x.CreateDate <= searchValue.FilterDate
                                 && x.CreateDate >= searchValue.FromDate.Value.Date))
                .Select(x => new ContactHistoryDetailVm
                {
                    Id = x.IdentifierId,
                    Type = ContactConstants.PHOTO,
                    PersonId = x.PersonId,
                    FromDate = x.CreateDate,
                    Detail = new ContactDetail
                    {
                        Detail1 = ContactConstants.TAKENBY
                    }
                }).ToList();
            photo.ForEach(item => { item.Count = GetPhotoCount(item); });
            contactHistoryDetail.AddRange(photo);
        }
        private void GetClassifyPersonnel(List<ContactHistoryDetailVm> contactHistoryDetail,
            ContactHistoryVm searchValue)
        {
            List<ContactHistoryDetailVm> classify = _context.InmateClassification.Where(x =>
                    x.InmateNavigation.PersonId == searchValue.PersonId
                    && (!searchValue.FromDate.HasValue || x.CreateDate <= searchValue.FilterDate
                        && x.CreateDate >= searchValue.FromDate.Value.Date))
                .Select(x => new ContactHistoryDetailVm
                {
                    Id = x.InmateClassificationId,
                    Type = ContactConstants.CLASSIFY,
                    PersonId = x.InmateNavigation.PersonId,
                    FromDate = x.CreateDate,
                    Detail = new ContactDetail
                    {
                        Detail1 = x.InmateClassificationType,
                        Detail2 = x.InmateClassificationReason
                    }
                }).ToList();
            classify.ForEach(item => { item.Count = GetClassifyPersonnelCount(item); });
            contactHistoryDetail.AddRange(classify);
        }
        private void GetIncidentPersonnel(List<ContactHistoryDetailVm> contactHistoryDetail,
            ContactHistoryVm searchValue, List<LookupVm> lookUpList)
        {
            List<ContactHistoryDetailVm> discInmate = _context.DisciplinaryInmate
                .Where(x => x.Inmate.PersonId == searchValue.PersonId
                            && (!searchValue.FromDate.HasValue || x.DisciplinaryIncident.DisciplinaryIncidentDate <= searchValue.FilterDate
                                && x.DisciplinaryIncident.DisciplinaryIncidentDate >= searchValue.FromDate.Value.Date))
                .Select(x => new ContactHistoryDetailVm
                {
                    PersonId = x.Inmate.PersonId,
                    Id = x.DisciplinaryIncident.DisciplinaryIncidentId,
                    FromDate = x.DisciplinaryIncident.DisciplinaryIncidentDate,
                    Detail = new ContactDetail
                    {
                        Detail1 = lookUpList.FirstOrDefault(l => l.LookupType == LookupConstants.DISCTYPE &&
                               Equals(l.LookupIndex, x.DisciplinaryIncident.DisciplinaryType))
                            .LookupDescription
                    },
                    Type = ContactConstants.INCIDENT
                }).ToList();
            discInmate.ForEach(item => { item.Count = GetIncidentPersonnelCount(item); });
            contactHistoryDetail.AddRange(discInmate);
        }
        private void GetGrievancePersonnel(List<ContactHistoryDetailVm> contactHistoryDetail,
            ContactHistoryVm searchValue, List<LookupVm> lookUpList)
        {
            List<ContactHistoryDetailVm> grievance = _context.GrievanceInmate
                   .Where(x => x.Inmate.PersonId == searchValue.PersonId && (!searchValue.FromDate.HasValue ||
                       x.Grievance.DateOccured <= searchValue.FilterDate
                       && x.Grievance.DateOccured >= searchValue.FromDate.Value.Date)).Select(s =>
                       new ContactHistoryDetailVm
                       {
                           Id = s.Grievance.GrievanceId,
                           PersonId = s.Inmate.PersonId,
                           FromDate = s.Grievance.DateOccured,
                           Detail = new ContactDetail
                           {
                               Detail1 = lookUpList
                                   .SingleOrDefault(w =>
                                       w.LookupType == LookupConstants.GRIEVTYPE && Equals(w.LookupIndex,
                                           s.Grievance.GrievanceType))
                                   .LookupDescription,
                               Detail2 = s.Grievance.Department
                           },
                           Type = ContactConstants.GRIEVANCE
                       }).ToList();
            grievance.AddRange(_context.Grievance
                .Where(x => x.Inmate.PersonId == searchValue.PersonId && (!searchValue.FromDate.HasValue ||
                    x.DateOccured <= searchValue.FilterDate
                    && x.DateOccured >= searchValue.FromDate.Value.Date)).Select(s => new ContactHistoryDetailVm
                    {
                        Id = s.GrievanceId,
                        PersonId = s.Inmate.PersonId,
                        FromDate = s.DateOccured,
                        Detail = new ContactDetail
                        {
                            Detail1 = lookUpList
                            .SingleOrDefault(w =>
                                w.LookupType == LookupConstants.GRIEVTYPE &&
                                Equals(w.LookupIndex,s.GrievanceType))
                            .LookupDescription,
                            Detail2 = s.Department
                        },
                        Type = ContactConstants.GRIEVANCE
                    }).ToList());
            grievance.ForEach(item => { item.Count = GetGrievancePersonnelCount(item); });
            contactHistoryDetail.AddRange(grievance);
        }
        private void GetFloorPersonnel(List<ContactHistoryDetailVm> contactHistoryDetail,
            ContactHistoryVm searchValue)
        {
            List<ContactHistoryDetailVm> floor = _context.FloorNoteXref
                .Where(x => x.Inmate.PersonId == searchValue.PersonId && (!searchValue.FromDate.HasValue || 
                 x.FloorNote.FloorNoteDate <= searchValue.FilterDate
                 && x.FloorNote.FloorNoteDate >= searchValue.FromDate.Value.Date)).
                Select(s => new ContactHistoryDetailVm
                {
                    Id = s.FloorNote.FloorNoteId,
                    PersonId = s.Inmate.PersonId,
                    FromDate = s.FloorNote.FloorNoteDate,
                    Type = ContactConstants.FLOORNOTE,
                    Detail = new ContactDetail
                    {
                        Detail1 = s.FloorNote.FloorNoteLocation
                    }
                }).ToList();
            floor.ForEach(item => item.Count = GetFloorPersonnelCount(item));
            contactHistoryDetail.AddRange(floor);
        }
        private void GetDnaPersonnel(List<ContactHistoryDetailVm> contactHistoryDetail,
            ContactHistoryVm searchValue, List<LookupVm> lookUpList)
        {
            List<ContactHistoryDetailVm> dna = _context.PersonDna.Where(x => x.PersonId == searchValue.PersonId
                 && (!searchValue.FromDate.HasValue || x.DnaDateGathered <= searchValue.FilterDate
                     && x.DnaDateGathered >= searchValue.FromDate.Value.Date ||
                     x.DnaDateRequired <= searchValue.FilterDate
                     && x.DnaDateRequired >= searchValue.FromDate.Value.Date))
                .Select(x => new ContactHistoryDetailVm
                {
                    Id = x.PersonDnaId,
                    Type = ContactConstants.DNA,
                    PersonId = x.PersonId,
                    FromDate = x.DnaDateGathered ?? x.DnaDateRequired,
                    Detail = new ContactDetail
                    {
                        Detail1 = lookUpList
                            .FirstOrDefault(l =>
                                l.LookupType == LookupConstants.DNADISPO &&
                                Equals(l.LookupIndex, x.DnaDisposition)).LookupDescription
                    }
                }).ToList();
            dna.ForEach(item => { item.Count = GetDnaPersonnelCount(item); });
            contactHistoryDetail.AddRange(dna);
        }
        private void GetTestingPersonnel(List<ContactHistoryDetailVm> contactHistoryDetail,
            ContactHistoryVm searchValue, List<LookupVm> lookUpList)
        {
            List<ContactHistoryDetailVm> testing = _context.PersonTesting.Where(x => x.PersonId == searchValue.PersonId
                     && (!searchValue.FromDate.HasValue || x.TestingDateGathered <= searchValue.FilterDate
                         && x.TestingDateGathered >= searchValue.FromDate.Value.Date ||
                         x.TestingDateRequired <= searchValue.FilterDate && x.TestingDateRequired >=
                         searchValue.FromDate.Value.Date))
                .Select(x => new ContactHistoryDetailVm
                {
                    Id = x.PersonTestingId,
                    Type = ContactConstants.TESTING,
                    PersonId = x.PersonId,
                    FromDate = x.TestingDateGathered ?? x.TestingDateRequired,
                    Detail = new ContactDetail
                    {
                        Detail1 = lookUpList
                            .FirstOrDefault(l =>
                                l.LookupType == LookupConstants.TESTTYPE &&
                                Equals(l.LookupIndex,x.TestingType))
                            .LookupDescription
                    }
                }).ToList();
            testing.ForEach(item => { item.Count = GetTestingPersonnelCount(item); });
            contactHistoryDetail.AddRange(testing);
        }
        private int GetIntakePersonnelCount(ContactHistoryDetailVm item)
        {
            List<ContactHistoryDetailVm> person = _context.Personnel.Select(x => new ContactHistoryDetailVm
            {
                Detail1 = x.PersonNavigation.PersonLastName,
                PersonId = x.PersonId,
                PersonnelId = x.PersonnelId
            }).ToList();
            int?[] perId = person.Select(x => x.PersonnelId).ToArray();
            List<Arrest> count = _context.Arrest
                .Where(s => s.ArrestId == item.Id && perId.Contains(s.ArrestOfficerId)).ToList();
            count.AddRange(_context.Arrest
                .Where(s => s.ArrestId == item.Id && perId.Contains(s.ArrestTransportingOfficerId)).ToList());
            count.AddRange(_context.Arrest
                .Where(s => s.ArrestId == item.Id && perId.Contains(s.ArrestReceivingOfficerId)).ToList());
            count.AddRange(_context.Arrest
                .Where(s => s.ArrestId == item.Id && perId.Contains(s.ArrestSearchOfficerId)).ToList());
            count.AddRange(_context.Arrest
                .Where(s => s.ArrestId == item.Id && perId.Contains(s.ArrestBookingSupervisorId)).ToList());
            count.AddRange(_context.Arrest
                .Where(s => s.ArrestId == item.Id && perId.Contains(s.ArrestBookingOfficerId)).ToList());

            return count.Count;
        }
        private int GetReleasePersonnelCount(ContactHistoryDetailVm item)
        {
            List<ContactHistoryDetailVm> person = _context.Personnel.Select(x => new ContactHistoryDetailVm
            {
                Detail1 = x.PersonNavigation.PersonLastName,
                PersonId = x.PersonId,
                PersonnelId = x.PersonnelId,
            }).ToList();
            int?[] perId = person.Select(x => x.PersonnelId).ToArray();
            List<ContactHistoryDetailVm> contactDetails = _context.Arrest
                .Where(s => s.ArrestId == item.Id && perId.Contains(s.ArrestReleaseClearedId ?? 0)).Select(x => new ContactHistoryDetailVm
                {
                    Id = x.ArrestId
                }).ToList();
            contactDetails.AddRange(_context.IncarcerationArrestXref
                .Where(s => s.Arrest.ArrestId == item.Id && perId.Contains(s.ReleaseOfficerId)).Select(x => new ContactHistoryDetailVm
                {
                    Id = x.Arrest.ArrestId
                }).ToList());

            return contactDetails.Count;
        }
        private int GetHousingPersonnelCount(ContactHistoryDetailVm item)
        {
            List<ContactHistoryDetailVm> person = _context.Personnel.Select(x => new ContactHistoryDetailVm
            {
                Detail1 = x.PersonNavigation.PersonLastName,
                PersonId = x.PersonId,
                PersonnelId = x.PersonnelId
            }).ToList();
            int?[] perId = person.Select(x => x.PersonnelId).ToArray();
            List<HousingUnitMoveHistory> contactDetails = _context.HousingUnitMoveHistory
                .Where(s => s.HousingUnitMoveHistoryId == item.Id && perId.Contains(s.MoveOfficerId)).ToList();
            return contactDetails.Count;
        }
        private int GetPhotoCount(ContactHistoryDetailVm item)
        {
            List<ContactHistoryDetailVm> person = _context.Personnel.Select(x => new ContactHistoryDetailVm
            {
                Detail1 = x.PersonNavigation.PersonLastName,
                PersonId = x.PersonId,
                PersonnelId = x.PersonnelId
            }).ToList();
            int?[] perId = person.Select(x => x.PersonnelId).ToArray();
            List<Identifiers> photo = _context.Identifiers.Where(x => x.IdentifierId == item.Id && perId.Contains(x.PhotographTakenBy)).ToList();
            return photo.Count;
        }
        private int GetClassifyPersonnelCount(ContactHistoryDetailVm item)
        {
            List<ContactHistoryDetailVm> person = _context.Personnel.Select(x => new ContactHistoryDetailVm
            {
                Detail1 = x.PersonNavigation.PersonLastName,
                PersonId = x.PersonId,
                PersonnelId = x.PersonnelId
            }).ToList();
            int?[] perId = person.Select(x => x.PersonnelId).ToArray();
            List<InmateClassification> classify = _context.InmateClassification.Where(x => x.InmateClassificationId == item.Id && perId.Contains(x.ClassificationOfficerId)).ToList();
            return classify.Count;
        }
        private int GetIncidentPersonnelCount(ContactHistoryDetailVm item)
        {
            List<ContactHistoryDetailVm> person = _context.Personnel.Select(x => new ContactHistoryDetailVm
            {
                Detail1 = x.PersonNavigation.PersonLastName,
                PersonId = x.PersonId,
                PersonnelId = x.PersonnelId
            }).ToList();
            int?[] perId = person.Select(x => x.PersonnelId).ToArray();
            List<DisciplinaryInmate> classify = _context.DisciplinaryInmate.Where(x =>
                x.DisciplinaryIncident.DisciplinaryIncidentId == item.Id &&
                perId.Contains(x.DisciplinaryHearingOfficer2)).Select(x => new DisciplinaryInmate
                {
                    InmateId = x.Inmate.InmateId
                }).ToList();
            classify.AddRange(_context.DisciplinaryInmate.Where(x =>
               x.DisciplinaryIncident.DisciplinaryIncidentId == item.Id &&
               perId.Contains(x.DisciplinaryHearingOfficer1)).Select(x => new DisciplinaryInmate
               {
                   InmateId = x.Inmate.InmateId
               }).ToList());
            classify.AddRange(_context.DisciplinaryInmate.Where(x =>
                 x.DisciplinaryIncident.DisciplinaryIncidentId == item.Id &&
                 perId.Contains(x.DisciplinaryIncident.DisciplinaryOfficerId)).Select(x => new DisciplinaryInmate
                 {
                     InmateId = x.Inmate.InmateId
                 }).ToList());
            return classify.Count;
        }
        private int GetGrievancePersonnelCount(ContactHistoryDetailVm item)
        {
            List<ContactHistoryDetailVm> person = _context.Personnel.Select(x => new ContactHistoryDetailVm
            {
                Detail1 = x.PersonNavigation.PersonLastName,
                PersonId = x.PersonId,
                PersonnelId = x.PersonnelId
            }).ToList();
            int?[] perId = person.Select(x => x.PersonnelId).ToArray();
            List<GrievancePersonnel> grievance = _context.GrievancePersonnel.Where(x => x.Grievance.GrievanceId == item.Id && perId.Contains(x.PersonnelId)).ToList();
            return grievance.Count;
        }
        private int GetFloorPersonnelCount(ContactHistoryDetailVm item)
        {
            List<ContactHistoryDetailVm> person = _context.Personnel.Select(x => new ContactHistoryDetailVm
            {
                Detail1 = x.PersonNavigation.PersonLastName,
                PersonId = x.PersonId,
                PersonnelId = x.PersonnelId
            }).ToList();
            int?[] perId = person.Select(x => x.PersonnelId).ToArray();
            List<FloorNotes> floor = _context.FloorNotes.Where(x => x.FloorNoteId == item.Id && perId.Contains(x.FloorNoteOfficerId)).ToList();
            return floor.Count;
        }
        private int GetDnaPersonnelCount(ContactHistoryDetailVm item)
        {
            List<ContactHistoryDetailVm> person = _context.Personnel.Select(x => new ContactHistoryDetailVm
            {
                Detail1 = x.PersonNavigation.PersonLastName,
                PersonId = x.PersonId,
                PersonnelId = x.PersonnelId
            }).ToList();
            int?[] perId = person.Select(x => x.PersonnelId).ToArray();
            List<PersonDna> dna = _context.PersonDna.Where(x => x.PersonDnaId == item.Id && perId.Contains(x.PersonnelId)).ToList();
            return dna.Count;
        }
        private int GetTestingPersonnelCount(ContactHistoryDetailVm item)
        {
            List<ContactHistoryDetailVm> person = _context.Personnel.Select(x => new ContactHistoryDetailVm
            {
                Detail1 = x.PersonNavigation.PersonLastName,
                PersonId = x.PersonId,
                PersonnelId = x.PersonnelId
            }).ToList();
            int?[] perId = person.Select(x => x.PersonnelId).ToArray();
            List<PersonTesting> testings = _context.PersonTesting.Where(x => x.PersonTestingId == item.Id && perId.Contains(x.PersonnelId)).ToList();
            return testings.Count;
        }
        private List<ContactHistoryDetailVm> GetPersonnelIntakeDetail(ContactHistoryDetailVm item)
        {
            List<ContactHistoryDetailVm> person = _context.Personnel.Select(x => new ContactHistoryDetailVm
            {
                PersonId = x.PersonId,
                PersonnelId = x.PersonnelId,
                Detail = new ContactDetail
                {
                    Detail1 = x.PersonNavigation.PersonLastName,
                    Detail2 = x.OfficerBadgeNumber
                }
            }).ToList();
            int?[] perId = person.Select(x => x.PersonnelId).ToArray();
            List<ContactHistoryDetailVm> contactDetails = _context.Arrest
                .Where(s => s.ArrestId == item.Id  && perId.Contains(s.ArrestOfficerId)).Select(s =>
                    new ContactHistoryDetailVm
                    {
                        PersonId = s.ArrestOfficerId,
                        FromDate = s.ArrestDate,
                        Detail = new ContactDetail
                        {
                            Detail1 = ContactTypeEnum.Arresting.ToString(),
                            Detail2 = s.ArrestOfficerText,
                            Detail3 = s.ArrestBookingNo
                        },
                        Type = ContactConstants.INTAKE
                    }).ToList();
            contactDetails.AddRange(_context.Arrest
                .Where(s => s.ArrestId == item.Id && perId.Contains(s.ArrestTransportingOfficerId)).Select(s =>
                    new ContactHistoryDetailVm
                    {
                        PersonId = s.ArrestTransportingOfficerId,
                        FromDate = s.ArrestBookingDate,
                        Detail = new ContactDetail
                        {
                            Detail1 = ContactTypeEnum.Transport.ToString(),
                            Detail2 = s.ArrestTransportingOfficerText,
                            Detail3 = s.ArrestBookingNo
                        },
                        Type = ContactConstants.INTAKE
                    }).ToList());
            contactDetails.AddRange(_context.Arrest
                .Where(s => s.ArrestId == item.Id && perId.Contains(s.ArrestReceivingOfficerId)).Select(s =>
                    new ContactHistoryDetailVm
                    {
                        PersonId = s.ArrestReceivingOfficerId,
                        FromDate = s.ArrestBookingDate,
                        Detail = new ContactDetail
                        {
                            Detail1 = ContactTypeEnum.Receiving.ToString(),
                            Detail2 = s.ArrestBookingNo
                        },
                        Type = ContactConstants.INTAKE
                    }).ToList());
            contactDetails.AddRange(_context.Arrest
                .Where(s => s.ArrestId == item.Id && perId.Contains(s.ArrestSearchOfficerId)).Select(s =>
                    new ContactHistoryDetailVm
                    {
                        PersonId = s.ArrestSearchOfficerId,
                        FromDate = s.ArrestBookingDate,
                        Detail = new ContactDetail
                        {
                            Detail1 = ContactTypeEnum.Search.ToString(),
                            Detail2 = s.ArrestBookingNo
                        },
                        Type = ContactConstants.INTAKE
                    }).ToList());
            contactDetails.AddRange(_context.Arrest
                .Where(s => s.ArrestId == item.Id && perId.Contains(s.ArrestBookingSupervisorId)).Select(s =>
                    new ContactHistoryDetailVm
                    {
                        PersonId = s.ArrestBookingSupervisorId,
                        FromDate = s.ArrestBookingDate,
                        Detail = new ContactDetail
                        {
                            Detail1 = ContactTypeEnum.Supervisor.ToString(),
                            Detail2 = s.ArrestBookingNo
                        },
                        Type = ContactConstants.INTAKE
                    }).ToList());
            contactDetails.AddRange(_context.Arrest
                .Where(s => s.ArrestId == item.Id && perId.Contains(s.ArrestBookingOfficerId)).Select(s =>
                    new ContactHistoryDetailVm
                    {
                        PersonId = s.ArrestBookingOfficerId,
                        FromDate = s.ArrestBookingDate,
                        Detail = new ContactDetail
                        {
                            Detail1 = ContactTypeEnum.Booking.ToString(),
                            Detail2 = s.ArrestBookingNo
                        },
                        Type = ContactConstants.INTAKE
                    }).ToList());
            contactDetails = GetPersonnelDetail(contactDetails, item.Type);
            return contactDetails;
        }
        private List<ContactHistoryDetailVm> GetReleasePersonnelDetail(ContactHistoryDetailVm item)
        {
            List<ContactHistoryDetailVm> person = _context.Personnel.Select(x => new ContactHistoryDetailVm
            {
                Detail1 = x.PersonNavigation.PersonLastName,
                PersonId = x.PersonId,
                PersonnelId = x.PersonnelId,
            }).ToList();
            int?[] perId = person.Select(x => x.PersonnelId).ToArray();
            List<ContactHistoryDetailVm> contactDetails = _context.Arrest
                .Where(s => s.ArrestId == item.Id && perId.Contains(s.ArrestReleaseClearedId)).Select(s =>
                    new ContactHistoryDetailVm
                    {
                        PersonId = s.ArrestReleaseClearedId,
                        FromDate = s.ArrestReleaseClearedDate,
                        Detail = new ContactDetail
                        {
                            Detail1 = ContactConstants.CLEAREDBY,
                            Detail2 = s.ArrestBookingNo
                        },
                        Type = ContactConstants.RELEASE
                    }).ToList();
            contactDetails.AddRange(_context.IncarcerationArrestXref
                .Where(s => s.Arrest.ArrestId == item.Id && perId.Contains(s.ReleaseOfficerId)).Select(s =>
                    new ContactHistoryDetailVm
                    {
                        PersonId = s.ReleaseOfficerId,
                        FromDate = s.ReleaseDate,
                        Detail = new ContactDetail
                        {
                            Detail1 = ContactConstants.RELEASEDBY,
                            Detail2 = s.Arrest.ArrestBookingNo
                        },
                        Type = ContactConstants.RELEASE
                    }).ToList());
            contactDetails = GetPersonnelDetail(contactDetails, item.Type);
            return contactDetails;
        }
        private List<ContactHistoryDetailVm> GetHousingPersonnelDetail(ContactHistoryDetailVm item)
        {
            List<ContactHistoryDetailVm> person = _context.Personnel.Select(x => new ContactHistoryDetailVm
            {
                Detail = new ContactDetail
                {
                    Detail1 = x.PersonNavigation.PersonLastName
                },
                PersonId = x.PersonId,
                PersonnelId = x.PersonnelId
            }).ToList();
            int?[] perId = person.Select(x => x.PersonnelId).ToArray();
            List<ContactHistoryDetailVm> detail = _context.HousingUnitMoveHistory
                .Where(s => s.HousingUnitMoveHistoryId == item.Id && perId.Contains(s.MoveOfficerId)).Select(x => new ContactHistoryDetailVm
                {
                    PersonId = x.MoveOfficerId,
                    FromDate = x.MoveDate,
                    ToDate = x.MoveDateThru,
                    Detail = new ContactDetail
                    {
                        Detail1 = x.MoveReason
                    },
                    Type = item.Type
                }).ToList();
            List<ContactHistoryDetailVm> contactDetails = GetPersonnelDetail(detail, item.Type);
            return contactDetails;
        }
        private List<ContactHistoryDetailVm> GetPhotoPersonnelDetail(ContactHistoryDetailVm item)
        {
            List<ContactHistoryDetailVm> person = _context.Personnel.Select(x => new ContactHistoryDetailVm
            {
                Detail = new ContactDetail
                {
                    Detail1 = x.PersonNavigation.PersonLastName
                },
                PersonId = x.PersonId,
                PersonnelId = x.PersonnelId
            }).ToList();
            int?[] perId = person.Select(x => x.PersonnelId).ToArray();
            List<ContactHistoryDetailVm> photo = _context.Identifiers
                .Where(x => x.IdentifierId == item.Id && perId.Contains(x.PhotographTakenBy)).Select(x =>
                    new ContactHistoryDetailVm
                    {
                        FromDate = x.CreateDate,
                        Detail = new ContactDetail
                        {
                            Detail1 = ContactConstants.TAKENBY
                        },
                        PersonId = x.PhotographTakenBy,
                        Type = item.Type
                    }).ToList();
            List<ContactHistoryDetailVm> contactDetails = GetPersonnelDetail(photo, item.Type);
            return contactDetails;
        }
        private List<ContactHistoryDetailVm> GetClassifyPersonnelDetail(ContactHistoryDetailVm item)
        {
            List<ContactHistoryDetailVm> person = _context.Personnel.Select(x => new ContactHistoryDetailVm
            {
                Detail1 = x.PersonNavigation.PersonLastName,
                PersonId = x.PersonId,
                PersonnelId = x.PersonnelId
            }).ToList();
            int?[] perId = person.Select(x => x.PersonnelId).ToArray();
            List<ContactHistoryDetailVm> classify = _context.InmateClassification
                .Where(x => x.InmateClassificationId == item.Id && perId.Contains(x.ClassificationOfficerId))
                .Select(x => new ContactHistoryDetailVm
                {
                    PersonId = x.ClassificationOfficerId,
                    FromDate = x.CreateDate,
                    Detail = new ContactDetail
                    {
                        Detail1 = x.InmateClassificationType,
                        Detail2 = x.InmateClassificationReason
                    },
                    Type = item.Type
                }).ToList();
            List<ContactHistoryDetailVm> contactDetails = GetPersonnelDetail(classify, item.Type);
            return contactDetails;
        }
        private List<ContactHistoryDetailVm> GetIncidentPersonnelDetail(ContactHistoryDetailVm item)
        {
            List<ContactHistoryDetailVm> person = _context.Personnel.Select(x => new ContactHistoryDetailVm
            {
                Detail1 = x.PersonNavigation.PersonLastName,
                PersonId = x.PersonId,
                PersonnelId = x.PersonnelId
            }).ToList();
            int?[] perId = person.Select(x => x.PersonnelId).ToArray();
            List<ContactHistoryDetailVm> discInc = _context.DisciplinaryInmate.Where(x =>
                x.DisciplinaryIncident.DisciplinaryIncidentId == item.Id &&
                perId.Contains(x.DisciplinaryHearingOfficer2)).Select(x => new ContactHistoryDetailVm
                {
                    PersonId = x.DisciplinaryHearingOfficer2,
                    Type = item.Type,
                    FromDate = x.DisciplinaryHearingDate ?? x.DisciplinaryIncident.DisciplinaryIncidentDate,
                    Detail = new ContactDetail
                    {
                        Detail1 = ContactConstants.HEARING2
                    }
                }).ToList();
            discInc.AddRange(_context.DisciplinaryInmate.Where(x =>
                x.DisciplinaryIncident.DisciplinaryIncidentId == item.Id &&
                perId.Contains(x.DisciplinaryHearingOfficer1)).Select(x => new ContactHistoryDetailVm
                {
                    PersonId = x.DisciplinaryHearingOfficer1,
                    Type = item.Type,
                    FromDate = x.DisciplinaryHearingDate ?? x.DisciplinaryIncident.DisciplinaryIncidentDate,
                    Detail = new ContactDetail
                    {
                        Detail1 = ContactConstants.HEARING1
                    }
                }).ToList());
            discInc.AddRange(_context.DisciplinaryInmate.Where(x =>
                x.DisciplinaryIncident.DisciplinaryIncidentId == item.Id &&
                perId.Contains(x.DisciplinaryIncident.DisciplinaryOfficerId)).Select(x => new ContactHistoryDetailVm
                {
                    PersonId = x.DisciplinaryIncident.DisciplinaryOfficerId,
                    Type = item.Type,
                    FromDate = x.DisciplinaryHearingDate ?? x.DisciplinaryIncident.DisciplinaryIncidentDate,
                    Detail = new ContactDetail
                    {
                        Detail1 = ContactTypeEnum.Reporting.ToString()
                    }
                }).ToList());
            List<ContactHistoryDetailVm> contactDetails = GetPersonnelDetail(discInc, item.Type);
            return contactDetails;
        }
        private List<ContactHistoryDetailVm> GetGrievancePersonnelDetail(ContactHistoryDetailVm item)
        {
            List<LookupVm> lookup = _context.Lookup.Where(x => x.LookupType == LookupConstants.GRIEVTYPE).Select(
                s => new LookupVm
                {
                    LookupIndex = s.LookupIndex,
                    LookupDescription = s.LookupDescription
                }).ToList();
            List<ContactHistoryDetailVm> person = _context.Personnel.Select(x => new ContactHistoryDetailVm
            {
                Detail1 = x.PersonNavigation.PersonLastName,
                PersonId = x.PersonId,
                PersonnelId = x.PersonnelId
            }).ToList();
            int?[] perId = person.Select(x => x.PersonnelId).ToArray();
            List<ContactHistoryDetailVm> grievance = _context.GrievancePersonnel.Where(x => x.Grievance.GrievanceId == item.Id && perId.Contains(x.PersonnelId)).Select(x => new ContactHistoryDetailVm
            {
                PersonId = x.PersonnelId,
                Type = item.Type,
                FromDate = x.Grievance.DateOccured,
                Detail = new ContactDetail
                {
                    Detail1 = lookup
                        .SingleOrDefault(w => Equals(w.LookupIndex,x.Grievance.GrievanceType))
                        .LookupDescription,
                    Detail2 = x.Grievance.Department
                }
            }).ToList();
            List<ContactHistoryDetailVm> contactDetails = GetPersonnelDetail(grievance, item.Type);
            return contactDetails;
        }
        private List<ContactHistoryDetailVm> GetFloorPersonnelDetail(ContactHistoryDetailVm item)
        {
            List<ContactHistoryDetailVm> person = _context.Personnel.Select(x => new ContactHistoryDetailVm
            {
                Detail1 = x.PersonNavigation.PersonLastName,
                PersonId = x.PersonId,
                PersonnelId = x.PersonnelId
            }).ToList();
            int?[] perId = person.Select(x => x.PersonnelId).ToArray();
            List<ContactHistoryDetailVm> floor = _context.FloorNotes
                .Where(x => x.FloorNoteId == item.Id && perId.Contains(x.FloorNoteOfficerId)).Select(x =>
                    new ContactHistoryDetailVm
                    {
                        PersonId = x.FloorNoteOfficerId,
                        FromDate = x.FloorNoteDate,
                        Type = item.Type,
                        Detail = new ContactDetail
                        {
                            Detail1 = x.FloorNoteLocation
                        }
                    }).ToList();
            List<ContactHistoryDetailVm> contactDetails = GetPersonnelDetail(floor, item.Type);
            return contactDetails;
        }
        private List<ContactHistoryDetailVm> GetDnaPersonnelDetail(ContactHistoryDetailVm item)
        {
            List<ContactHistoryDetailVm> person = _context.Personnel.Select(x => new ContactHistoryDetailVm
            {
                Detail1 = x.PersonNavigation.PersonLastName,
                PersonId = x.PersonId,
                PersonnelId = x.PersonnelId
            }).ToList();
            int?[] perId = person.Select(x => x.PersonnelId).ToArray();
            List<ContactHistoryDetailVm> dna = _context.PersonDna
                .Where(x => x.PersonDnaId == item.Id && perId.Contains(x.PersonnelId)).Select(x =>
                    new ContactHistoryDetailVm
                    {
                        Id = x.PersonId,
                        Type = item.Type,
                        PersonId = x.PersonnelId,
                        FromDate = x.DnaDateGathered ?? x.DnaDateRequired,
                        Detail = new ContactDetail
                        {
                            Detail1 = x.DnaTestBy != null ? $"BY: {x.DnaTestBy}" : "NONE"
                        }
                    }).ToList();
            List<ContactHistoryDetailVm> contactDetails = GetPersonnelDetail(dna, item.Type);
            return contactDetails;
        }
        private List<ContactHistoryDetailVm> GetTestingPersonnelDetail(ContactHistoryDetailVm item, List<LookupVm> lookUpList)
        {
            List<ContactHistoryDetailVm> person = _context.Personnel.Select(x => new ContactHistoryDetailVm
            {
                Detail1 = x.PersonNavigation.PersonLastName,
                PersonId = x.PersonId,
                PersonnelId = x.PersonnelId
            }).ToList();
            int?[] perId = person.Select(x => x.PersonnelId).ToArray();
            List<ContactHistoryDetailVm> test = _context.PersonTesting
                .Where(x => x.PersonTestingId == item.Id && perId.Contains(x.PersonnelId)).Select(x =>
                    new ContactHistoryDetailVm
                    {
                        Id = x.PersonId,
                        Type = item.Type,
                        PersonId = x.PersonnelId,
                        FromDate = x.TestingDateGathered ?? x.TestingDateRequired,
                        Detail = new ContactDetail
                        {
                            Detail1 = lookUpList
                                .FirstOrDefault(l =>
                                    l.LookupType == LookupConstants.TESTTYPE &&
                                    Equals(l.LookupIndex, x.TestingType))
                                .LookupDescription
                        }
                    }).ToList();
            List<ContactHistoryDetailVm> contactDetails = GetPersonnelDetail(test, item.Type);
            return contactDetails;
        }
        private List<ContactHistoryDetailVm> GetPersonnelDetail(List<ContactHistoryDetailVm> lstDetail, string type)
        {
            int?[] personId = lstDetail.Select(x => x.PersonId).ToArray();
            List<Person> persons = _context.Personnel.Where(x => personId.Contains(x.PersonnelId)).Select(s => new Person
            {
                PersonId = s.PersonnelId,
                PersonNumber = s.OfficerBadgeNumber,
                PersonLastName = s.PersonNavigation.PersonLastName,
                PersonFirstName = s.PersonNavigation.PersonFirstName,
                PersonMiddleName = s.PersonNavigation.PersonMiddleName,
                PersonEmail = s.PersonNavigation.PersonEmail,
                PersonPhone = s.PersonNavigation.PersonPhone,
                PersonBusinessPhone = s.PersonNavigation.PersonBusinessPhone
            }).ToList();
            lstDetail.ForEach(p =>
            {
                Person per = persons.FirstOrDefault(x => x.PersonId == p.PersonId);
                if (per != null)
                {
                    p.LastName = per.PersonLastName;
                    p.FirstName = per.PersonFirstName;
                    p.MiddleName = per.PersonMiddleName;
                    p.Type = type;
                    p.Email = per.PersonEmail;
                    p.InmateNumber = per.PersonNumber;
                }
            });
            return lstDetail;
        }
        #endregion
    }
}
