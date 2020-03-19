using System;
using GenerateTables.Models;
using System.Collections.Generic;
using ServerAPI.ViewModels;
using System.Linq;

// ReSharper disable once CheckNamespace
namespace ServerAPI.Services
{
    public class ClassifyAlertsPrivilegeService : IClassifyAlertsPrivilegeService
    {
        private readonly AAtims _context;
        public ClassifyAlertsPrivilegeService(AAtims context)
        {
            _context = context;
        }

        public ClassifyAlertsPrivilegeVm GetClassifyPrivilegeDetails(ClassifyPrivilegeSearchVm search)
        {
            ClassifyAlertsPrivilegeVm classifyAlerts = new ClassifyAlertsPrivilegeVm
            {
                PrivilegeAlertDetail = _context.InmatePrivilegeXref
                    .Where(w => (search.OfficerId == 0 || w.PrivilegeOfficerId == search.OfficerId) && w.Inmate.InmateActive == 1 &&
                                w.Privilege.InactiveFlag != 1 && w.Inmate.FacilityId == search.FacilityId
                                && ((w.PrivilegeDate >= search.FromDate.Value.Date && w.PrivilegeDate.HasValue) ||
                                    (w.PrivilegeExpires >= search.ToDate.Value.Date
                                     && w.PrivilegeExpires.HasValue))&&
                                     (!search.ShowActive|| w.PrivilegeRemoveDatetime==null &&
                                     (w.PrivilegeExpires>=DateTime.Now || !w.PrivilegeExpires.HasValue)))                            
                    .Select(pl => new PrivilegeAlertVm
                    {
                        InmateId = pl.InmateId,
                        InmatePrivilegeXrefId = pl.InmatePrivilegeXrefId,
                        PrivilegeId = pl.PrivilegeId,
                        PrivilegeDescription = pl.Privilege.PrivilegeDescription,
                        PrivilegeType = pl.Privilege.PrivilegeType,
                        PrivilegeDate = pl.PrivilegeDate,
                        PrivilegeExpires = pl.PrivilegeExpires,
                        PrivilegeRemoveDateTime = pl.PrivilegeRemoveDatetime,
                        PrivilegeNote = pl.PrivilegeNote,
                        PrivilegeReviewFlag = pl.ReviewFlag,
                        PrivilegeReviewInterval = pl.ReviewInterval,
                        PrivilegeNextReview = pl.ReviewNext,
                        HousingUnitLocation = pl.Inmate.HousingUnitId > 0
                            ? pl.Inmate.HousingUnit.HousingUnitLocation
                            : null,
                        HousingUnitNumber =
                            pl.Inmate.HousingUnitId > 0 ? pl.Inmate.HousingUnit.HousingUnitNumber : null,
                        HousingUnitId = pl.Inmate.HousingUnitId ?? 0,
                        PersonInfoVm = new PersonInfoVm
                        {
                            PersonFirstName = pl.Inmate.Person.PersonFirstName,
                            PersonLastName = pl.Inmate.Person.PersonLastName,
                            PersonMiddleName = pl.Inmate.Person.PersonMiddleName,
                            PersonDob=pl.Inmate.Person.PersonDob,
                            InmateNumber = pl.Inmate.InmateNumber,
                        },
                        PrivilegeFlagList = pl.InmatePrivilegeXrefFlag.Select(fn => new PrivilegeFlagVm
                        {
                            PrivilegeFlagLookupId = fn.PrivilegeFlagLookupId, IsFlag = fn.FlagValue == 1
                        }).ToList(),
                    }).ToList()
            };
             
            classifyAlerts.PrivilegeGroupDetail = GetClassifyPrivilegeCount(classifyAlerts.PrivilegeAlertDetail);
            classifyAlerts.HousingGroupDetail = GetClassifyHousingCount(classifyAlerts.PrivilegeAlertDetail);

            return classifyAlerts;
        }
        private List<PrivilegeGroupFlagVm> GetClassifyPrivilegeCount(List<PrivilegeAlertVm> privilegeAlerts) => privilegeAlerts
            .GroupBy(g => new
            {
                g.PrivilegeDescription,
                g.PrivilegeId
            })
           .Select(pr => new PrivilegeGroupFlagVm
           {
               PrivilegeDescription = pr.Key.PrivilegeDescription,
               PrivilegeId = pr.Key.PrivilegeId,
               Count = pr.Count(),
               
           }).ToList();

        private List<PrivilegeGroupFlagVm> GetClassifyHousingCount(List<PrivilegeAlertVm> privilegeAlerts) =>
         privilegeAlerts.GroupBy(g => new
         {
             g.HousingUnitLocation,
             g.HousingUnitNumber,
             g.HousingUnitId
         }).Select(s => new PrivilegeGroupFlagVm
         {
             PrivilegeId = s.Key.HousingUnitId,
             HousingUnitLocation = s.Key.HousingUnitLocation,
             HousingUnitNumber = s.Key.HousingUnitNumber,
             Count = s.Count(),
            
         }).ToList();
    }
}












