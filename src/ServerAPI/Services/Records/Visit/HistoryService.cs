using System;
using System.Collections.Generic;
using System.Linq;
using ServerAPI.ViewModels;
using GenerateTables.Models;
using Microsoft.EntityFrameworkCore;
using ServerAPI.Utilities;

// ReSharper disable once CheckNamespace
namespace ServerAPI.Services
{
    public class HistoryService : IHistoryService
    {
        private readonly AAtims _context;
        private readonly ICommonService _commonService;
        public HistoryService(AAtims context, ICommonService commonService)
        {
            _context = context;
            _commonService = commonService;
        }

        public List<PrivilegesVm> GetVisitLocation(int facilityId)
        {
            List<PrivilegesVm> location = _context.Privileges
                .Where(s => s.InactiveFlag == 0 && s.ShowInVisitation && s.FacilityId == facilityId)
                .Select(w => new PrivilegesVm
                {
                    PrivilegesDescription = w.PrivilegeDescription,
                    PrivilegesId = w.PrivilegeId,
                    TrackingAllowRefusal = w.TrackingAllowRefusal==1
                    
                }).ToList();
            return location;
        }
        public List<VisitHistoryDetail> GetVisitHistoryDetails(SearchVisitHistoryVm searchValue)
        {
            List<LookupVm> lstLookup = _commonService.GetLookups(new[]{LookupConstants.RELATIONS});
            IQueryable<VisitToVisitor> visitToVisitors = _context.VisitToVisitor.AsNoTracking().Where(s =>
                (!searchValue.FacilityId.HasValue || s.Visit.RegFacilityId == searchValue.FacilityId) && 
                s.Visit.VisitorType.HasValue && (string.IsNullOrEmpty(searchValue.VisitorLast) ||
                 s.Visitor.PersonLastName.StartsWith(searchValue.VisitorLast)) &&
                (string.IsNullOrEmpty(searchValue.VisitorFirst) ||
                 s.Visitor.PersonFirstName.StartsWith(searchValue.VisitorFirst)) &&
                (string.IsNullOrEmpty(searchValue.VisitorIdType) || s.VisitorIdType != null &&
                 String.Equals(s.VisitorIdType.Trim(), searchValue.VisitorIdType.Trim(),
                     StringComparison.CurrentCultureIgnoreCase)) &&
                (string.IsNullOrEmpty(searchValue.VisitorIdState) || s.VisitorIdState != null &&
                 String.Equals(s.VisitorIdState.Trim(), searchValue.VisitorIdState.Trim(),
                     StringComparison.CurrentCultureIgnoreCase)) &&
                (string.IsNullOrEmpty(searchValue.VisitorIdNumber) ||
                 s.VisitorIdNumber.StartsWith(searchValue.VisitorIdNumber)) &&
                (string.IsNullOrEmpty(searchValue.VisitorNote) ||
                 s.Visit.Notes.StartsWith(searchValue.VisitorNote)) &&
                (string.IsNullOrEmpty(searchValue.InmateLastName) ||
                 s.Visit.Inmate.Person.PersonLastName.StartsWith(searchValue.InmateLastName)) &&
                (string.IsNullOrEmpty(searchValue.InmateFirstName) ||
                 s.Visit.Inmate.Person.PersonFirstName.StartsWith(searchValue.InmateFirstName)) &&
                (string.IsNullOrEmpty(searchValue.InmateNumber) ||
                 s.Visit.Inmate.InmateNumber.StartsWith(searchValue.InmateNumber)) &&
                (string.IsNullOrEmpty(searchValue.Classification)
                 || (s.Visit.Inmate.Incarceration.Any(inc => !inc.ReleaseOut.HasValue)
                     && s.Visit.Inmate.InmateClassification.InmateClassificationReason == searchValue.Classification)) &&
                (searchValue.BoothId == null ||(s.Visit.VisitBooth!=null && s.Visit.VisitBooth == searchValue.BoothId)) &&
                (searchValue.LocationId == null || (s.Visit.LocationId != null && s.Visit.LocationId == searchValue.LocationId))&&
                (!searchValue.DateFrom.HasValue || (s.Visit.StartDate.Date==searchValue.DateFrom.Value.Date)) &&
                (string.IsNullOrEmpty(searchValue.DenyReason) ||
                 s.Visit.VisitDenyReason.StartsWith(searchValue.DenyReason)) &&
                (string.IsNullOrEmpty(searchValue.CompleteReason) || s.Visit.CompleteVisitReason != null &&
                 String.Equals(s.Visit.CompleteVisitReason.Trim(), searchValue.CompleteReason.Trim(),
                     StringComparison.CurrentCultureIgnoreCase)) &&
                (!searchValue.IsDeleted || s.Visit.DeleteFlag) &&
                (!searchValue.IsDenied || s.Visit.VisitDenyFlag == 1) &&
                (!searchValue.IsComplete || s.Visit.CompleteVisitFlag) &&
                (!searchValue.CountAsVisit || s.Visit.CountAsVisit == 1));

            if (searchValue.LocationId != null && (searchValue.IsRefused || searchValue.RefusedReason != null))
            {
                int[] inmateId = visitToVisitors.Select(s => s.Visit.InmateId??0).ToArray();
                int[] inmate = InmateRefuseFilter(inmateId, searchValue);
                visitToVisitors = visitToVisitors
                    .Where(s => inmate.Any(inc=>inc== s.Visit.InmateId));
            }
            List<VisitHistoryDetail> lstDetails = visitToVisitors
               .Select(s => new VisitHistoryDetail
               {
                   VisitToVisitorId = s.VisitToVisitorId,
                   ScheduleId = s.ScheduleId,
                   PersonId = s.PersonId,
                   InmateId=s.Visit.InmateId,
                   VisitorLast = s.Visitor.PersonLastName,
                   VisitorFirst = s.Visitor.PersonFirstName,
                   VisitorMiddle = s.Visitor.PersonMiddleName,
                   InmateNumber = s.Visit.Inmate.InmateNumber,
                   InmateLastName = s.Visit.Inmate.Person.PersonLastName,
                   InmateFirstName = s.Visit.Inmate.Person.PersonFirstName,
                   Location = s.Visit.Location.PrivilegeDescription,
                   CompleteReason = s.Visit.CompleteVisitReason,
                   FromDate = s.Visit.StartDate,
                   CountAsVisit = s.Visit.CountAsVisit,
                   AdultCount = s.Visit.VisitAdditionAdultCount,
                   ChildCount = s.Visit.VisitAdditionChildCount,
                   VisitorType = s.Visit.VisitorType??0,
                   RelationshipId = s.Visitor.VisitorToInmate.FirstOrDefault(w => w.InmateId == s.Visit.InmateId).VisitorRelationship ?? 0
               }).ToList();

            lstDetails.ForEach(item =>
            {
                item.Relationship = lstLookup.SingleOrDefault(l =>
                    l.LookupIndex == item.RelationshipId && l.LookupType == LookupConstants.RELATIONS)?.LookupDescription;
            });
            
            return lstDetails;
        }
        private int[] InmateRefuseFilter(int[] inmateIds,SearchVisitHistoryVm searchValue)
        {
            List<InmateTrak> lstScheduleInmate = _context.InmateTrak.Where(s =>
                inmateIds.Any(w=>w==s.InmateId) && s.InmateTrakLocationId != null && s.InmateTrakLocationId == searchValue.LocationId &&
                (!searchValue.IsRefused || s.InmateRefused) &&
                (string.IsNullOrEmpty(searchValue.RefusedReason) || s.InmateRefusedReason != null &&
                 String.Equals(s.InmateRefusedReason.Trim(), searchValue.RefusedReason.Trim(),
                     StringComparison.CurrentCultureIgnoreCase))).ToList();
            int[] inmateId = lstScheduleInmate.Select(s => s.InmateId).ToArray();
            return inmateId;
        }
    }
}