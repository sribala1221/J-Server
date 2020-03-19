﻿using System;
using System.Collections.Generic;
using System.Linq;
using GenerateTables.Models;
using ServerAPI.Utilities;
using System.Linq.Dynamic.Core;

namespace ServerAPI.ViewModels
{
    public class IncidentViewerDetails
    {
        public List<KeyValuePair<int, string>> IncidentTypes { get; set; }
        public List<KeyValuePair<int, string>> CategorizationTypes { get; set; }
        public List<KeyValuePair<int, string>> DispInmateTypes { get; set; }
        public List<IncidentViewer> IncidentViewerList { get; set; }
        public int TotalRecords { get; set; }

        private IncidentFilters Filters { get; set; }
        private IQueryable<DisciplinaryIncident> Query { get; set; }
        private List<DisciplinaryControlLookup> DisciplinaryControlLookups { get; set; }

        public void CreateIncidentViewerTypes(AAtims context)
        {
            GetIncidentTypes(context);
            GetCategorizationTypes(context);
            GetDispInmateTypes(context);
        }

        private void GetIncidentTypes(AAtims context)
        {
            IncidentTypes = context.Lookup.Where(a => a.LookupType == LookupConstants.DISCTYPE)
                .OrderByDescending(a => a.LookupOrder)
                .ThenBy(a => a.LookupDescription)
                .Select(a =>
                    new KeyValuePair<int, string>(a.LookupIndex, a.LookupDescription))
                .ToList();
        }

        private void GetCategorizationTypes(AAtims context)
        {
            CategorizationTypes = context.Lookup.Where(a => a.LookupType == LookupConstants.INCCAT)
                .OrderByDescending(a => a.LookupOrder)
                .ThenBy(a => a.LookupDescription)
                .Select(a => new KeyValuePair<int, string>(a.LookupIndex, a.LookupDescription))
                .ToList();
        }

        private void GetDispInmateTypes(AAtims context)
        {
            DispInmateTypes = context.Lookup.Where(a => a.LookupType == LookupConstants.DISCINTYPE)
                .OrderByDescending(a => a.LookupOrder)
                .ThenBy(a => a.LookupDescription)
                .Select(a =>
                    new KeyValuePair<int, string>(a.LookupIndex, a.LookupDescription))
                .ToList();
        }

        public void SetTotalRecords()
        {
            TotalRecords = Query.Count();
        }

        public void SetFilters(IncidentFilters filters)
        {
            Filters = filters;
        }

        public IncidentViewerDetails SetQuery(AAtims context)
        {
            Query = context.DisciplinaryIncident
                .Where(a => a.FacilityId == Filters.FacilityId);
            return this;
        }

        public IncidentViewerDetails FilterIsHistory()
        {
            Query = !Filters.IsHistory ? Query.Where(w => w.DisciplinaryActive == 1) : Query;
            return this;
        }

        public IncidentViewerDetails FilterDeleteFlag()
        {
            Query = !Filters.DeleteFlag ? Query.Where(w => w.DisciplinaryInmate.Any(x => !x.DeleteFlag.HasValue || x.DeleteFlag == 0)) : Query;
            return this;
        }

        public IncidentViewerDetails FilterByCategory(int personnelId)
        {
            switch (Filters.FilterCategory)
            {
                case FilterCategory.ShowMyActiveIncidents:
                    Query = Query.Where(w => w.DisciplinaryOfficerId == personnelId);
                    break;
                case FilterCategory.ShowMyInvolvedPartyIncidents:
                    Query = Query.Where(w => w.DisciplinaryInmate.Any(x => x.PersonnelId == personnelId));
                    break;
            }
            return this;
        }

        public IncidentViewerDetails FilterByRestrictions()
        {
            switch (Filters.Restriction)
            {
                case Restrictions.SensitiveOnly:
                    Query = Query.Where(w => w.SensitiveMaterial);
                    break;
                case Restrictions.PreaOnly:
                    Query = Query.Where(w => w.PreaOnly);
                    break;
            }
            return this;
        }

        public IncidentViewerDetails FilterDisciplinaryType()
        {
            Query = Filters.DisciplinaryType > 0
                ? Query.Where(w => w.DisciplinaryType == Filters.DisciplinaryType)
                : Query;
            return this;
        }

        public IncidentViewerDetails FilterByHearing()
        {
            switch (Filters.Hearing)
            {
                case IncidentActiveConstants.APPROVED:
                    Query = Query
                        .Where(w => w.AllowHearingFlag == 1);
                    break;
                case IncidentActiveConstants.NOTAPPROVED:
                    Query = Query
                        .Where(w => !w.AllowHearingFlag.HasValue || w.AllowHearingFlag == 0);
                    break;
            }
            return this;
        }

        public IncidentViewerDetails FilterCategorization()
        {
            Query = Filters.Categorization > 0
                ? Query.Where(w => w.IncidentCategorizationIndex == Filters.Categorization)
                : Query;
            return this;
        }

        public IncidentViewerDetails FilterKeyword()
        {
            if (!string.IsNullOrEmpty(Filters.KeyWord))
            {
                Query = Query
                    .Where(w => !string.IsNullOrEmpty(w.DisciplinarySynopsis)
                                && w.DisciplinarySynopsis.Contains(Filters.KeyWord) ||
                                w.DisciplinaryIncidentNarrative.Any(x =>
                                    !string.IsNullOrEmpty(x.DisciplinaryIncidentNarrative1)
                                    && x.DisciplinaryIncidentNarrative1.Contains(Filters.KeyWord)) ||
                                w.DisciplinaryInmate.Any(x => !string.IsNullOrEmpty(x.DisciplinaryViolationDescription)
                                                              && x.DisciplinaryViolationDescription.Contains(Filters.KeyWord)) ||
                                w.DisciplinaryInmate.Any(x => !string.IsNullOrEmpty(x.DisciplinaryRecommendations) &&
                                                              x.DisciplinaryRecommendations.Contains(Filters.KeyWord)) ||
                                w.DisciplinaryInmate.Any(x => !string.IsNullOrEmpty(x.DisciplinaryReviewNotes)
                                                              && x.DisciplinaryReviewNotes.Contains(Filters.KeyWord)) ||
                                w.DisciplinaryInmate.Any(x => !string.IsNullOrEmpty(x.DisciplinarySanction)
                                                              && x.DisciplinarySanction.Contains(Filters.KeyWord)) ||
                                w.DisciplinaryInmate.Any(x => x.DisciplinaryFindingNotes.Contains(Filters.KeyWord)));
            }

            return this;
        }

        public IncidentViewerDetails FilterHours()
        {
            Query = Filters.Hours > 0
                ? Query.Where(w => w.DisciplinaryIncidentDate.HasValue &&
                                   w.DisciplinaryIncidentDate >= DateTime.Now.AddHours(-Filters.Hours) &&
                                   w.DisciplinaryIncidentDate <= DateTime.Now)
                : Query;
            return this;
        }

        public IncidentViewerDetails FilterByDate()
        {
            if (Filters.FromDate.HasValue)
            {
                Query = Query
                    .Where(w => w.DisciplinaryIncidentDate.HasValue &&
                                w.DisciplinaryIncidentDate.Value.Date >= Filters.FromDate.Value.Date);
            }
            if (Filters.ToDate.HasValue)
            {
                Query = Query
                    .Where(w => w.DisciplinaryIncidentDate.HasValue &&
                                w.DisciplinaryIncidentDate.Value.Date <= Filters.ToDate.Value.Date);
            }

            return this;
        }

        public IncidentViewerDetails FilterOfficer()
        {
            if (Filters.OfficerId > 0)
            {
                switch (Filters.OfficerType)
                {
                    case OfficerType.NARRATIVEBY:
                    case OfficerType.CREATEDBY:
                        Query = Query
                            .Where(w => w.DisciplinaryOfficerId == Filters.OfficerId);
                        break;
                    case OfficerType.REVIEWBY:
                        Query = Query
                            .Where(w => w.DisciplinaryInmate
                                .Any(x => x.DisciplinaryReviewOfficer == Filters.OfficerId));
                        break;
                    case OfficerType.HEARINGBY:
                        Query = Query
                            .Where(w => w.DisciplinaryInmate
                                .Any(x => x.DisciplinaryHearingOfficer1 == Filters.OfficerId));
                        break;
                    case OfficerType.COMPLETEBY:
                        Query = Query
                            .Where(w => w.DisciplinaryInmate
                                .Any(x => x.DisciplinaryReviewCompleteOfficer == Filters.OfficerId));
                        break;
                }
            }

            return this;
        }

        public IncidentViewerDetails FilterIncidentNumber()
        {
            Query = !string.IsNullOrEmpty(Filters.IncidentNumber)
                ? Query.Where(w => w.DisciplinaryNumber.Contains(Filters.IncidentNumber))
                : Query;
            return this;
        }

        public IncidentViewerDetails FilterIncidentDate()
        {
            if (Filters.IncidentDate.HasValue)
            {
                Query = Query
                    .Where(w => w.DisciplinaryIncidentDate.HasValue &&
                                w.DisciplinaryIncidentDate.Value.Date == Filters.IncidentDate.Value.Date);
            }
            return this;
        }

        public IncidentViewerDetails FilterLocation()
        {
            if (!string.IsNullOrEmpty(Filters.Location))
            {
                Query = Query
                    .Where(w => w.DisciplinaryHousingUnitLocation
                                    .Contains(Filters.Location) || w.DisciplinaryHousingUnitNumber
                                    .Contains(Filters.Location) || w.DisciplinaryHousingUnitBed
                                    .Contains(Filters.Location) || w.DisciplinaryLocation
                                    .Contains(Filters.Location) || w.DisciplinaryLocationOther
                                    .Contains(Filters.Location));
            }

            return this;
        }

        public IncidentViewerDetails Sort()
        {
            Query=Query.OrderBy(Filters.SortColumn + " " + Filters.SortOrder);
            return this;
        }

        public void SetIncidentViewerList(AAtims context)
        {
            SetDisciplinaryControlLookups(context);

            IncidentViewerList = Query.Select(a => new IncidentViewer
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
                DispFreeForm = a.DisciplinaryLocationOther,
                DispOtherLocationId = a.OtherLocationID,
                DispOtherLocationName = a.OtherLocation.LocationName,
                ReportDate = a.DisciplinaryReportDate,
                DisciplinaryTypeId = a.DisciplinaryType,
                SupervisorReviewFlag = a.DisciplinaryIncidentNarrative
                        .Select(s => s.SupervisorReviewFlag == 1).FirstOrDefault(),
                DisciplinaryType = context.Lookup.Where(x => x.LookupType == LookupConstants.DISCTYPE
                        && x.LookupIndex == a.DisciplinaryType)
                        .Select(x => x.LookupDescription).SingleOrDefault(),
                Personnel = new PersonVm
                {
                    PersonLastName = a.DisciplinaryOfficer.PersonNavigation.PersonLastName,
                    PersonFirstName = a.DisciplinaryOfficer.PersonNavigation.PersonFirstName,
                    OfficerBadgeNumber = a.DisciplinaryOfficer.OfficerBadgeNum
                },
                SupervisorAction = a.DisciplinarySupervisorAction,
                FacilityName = a.Facility.FacilityName,
                DispOfficerNarrativeFlag = a.DisciplinaryOfficerNarrativeFlag ?? false,
                ExpectedCount = a.ExpectedNarrativeCount ?? 0,
                RecordsCount = a.DisciplinaryIncidentNarrative.Count,
                DisciplinaryActive = a.DisciplinaryActive,
                SensitiveMaterial = a.SensitiveMaterial,
                PreaOnly = a.PreaOnly,
                InitialEntryFlag = a.InitialEntryFlag ?? false,
                Categorization = context.Lookup.Where(x => x.LookupType == LookupConstants.INCCAT
                        && x.LookupIndex == a.IncidentCategorizationIndex)
                        .Select(x => x.LookupDescription)
                        .SingleOrDefault(),
                DispIncidentFlag = a.DisciplinaryIncidentFlag.Select(x => x.IncidentFlagText).ToArray(),
                InvolvedPartyTypes = string.Join(", ", a.DisciplinaryInmate
                            .Where(x => !x.DeleteFlag.HasValue || x.DeleteFlag == 0)
                            .Select(x => x.DisciplinaryInmateType ?? 0)
                            .GroupBy(x => x).Select(x => context.Lookup
                                .FirstOrDefault(l => l.LookupType == LookupConstants.DISCINTYPE && l.LookupIndex == x.Key)
                                .LookupDescription + " = " + x.Count())),
                Violations = GetDispInmateViolations(a.DisciplinaryInmate
                            .Where(x => !x.DeleteFlag.HasValue || x.DeleteFlag == 0)
                            .SelectMany(x => x.DisciplinaryControlXref).ToList())
            }).Skip(Filters.Skip).Take(Filters.RowsPerPage).ToList();
        }

        private string GetDispInmateViolations(ICollection<DisciplinaryControlXref> disciplinaryInmates) =>
            string.Join(", ", disciplinaryInmates.Select(s => s.DisciplinaryControlViolationId)
                .GroupBy(a => a)
                .Select(s => "(" + s.Count() + ") " + DisciplinaryControlLookups
                                 .SingleOrDefault(x => x.DisciplinaryControlLookupId == s.Key)?.DisciplinaryControlLookupName));

        private void SetDisciplinaryControlLookups(AAtims context)
        {
            DisciplinaryControlLookups = context.DisciplinaryControlLookup
                .Where(w => !w.InactiveFlag.HasValue || w.InactiveFlag == 0 
                            && (w.DisciplinaryControlLookupType == (int)DisciplinaryLookup.DISCVIOL ||
                               w.DisciplinaryControlLookupType == (int)DisciplinaryLookup.DISCWAIV ||
                               w.DisciplinaryControlLookupType == (int)DisciplinaryLookup.DISCFIND ||
                               w.DisciplinaryControlLookupType == (int)DisciplinaryLookup.DISCPLEA ||
                               w.DisciplinaryControlLookupType == (int)DisciplinaryLookup.DISCSANC)).ToList();
        }

    }
}
