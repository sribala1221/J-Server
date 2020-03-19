using GenerateTables.Models;
using Microsoft.AspNetCore.Http;
using ServerAPI.Interfaces;
using ServerAPI.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using ServerAPI.Utilities;

namespace ServerAPI.Services
{
    public class HeadCountService : IHeadCountService
    {
        private readonly AAtims _context;
        private readonly ICommonService _commonService;
        private readonly ICellService _cellService;
        private readonly int _personnelId;
        private int? _maxHeadCountId;
        private string facilityAbbr;

        public HeadCountService(AAtims context, ICommonService commonService, ICellService cellService,
            IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _commonService = commonService;
            _cellService = cellService;
            _personnelId = Convert.ToInt32(httpContextAccessor.HttpContext.User.FindFirst("personnelId")?.Value);
        }

        //refresh the head count list
        public List<HeadCountVm> GetHeadCountList(ConsoleInputVm value)
        {
            if (value.ListConsoleHousingId.Count>0)
            {
                List<HeadCountVm> lstHeadCountHousingDetails = LoadHeadCountHousingList(value);
                return lstHeadCountHousingDetails;
            }
            else
            {
                List<HeadCountVm> lstHeadCountLocationDetails = LoadHeadCountLocationList(value);
                return lstHeadCountLocationDetails;
            }
        }

        public List<HeadCountVm> LoadHeadCountHousingList(ConsoleInputVm value)
        {

            IQueryable<HousingUnit> listHousingUnit = _context.HousingUnit
                .Where(w => w.FacilityId == value.FacilityId);

            IQueryable<Inmate> _inmateList = _context.Inmate
                .Where(w => w.InmateActive == 1
                            && w.FacilityId == value.FacilityId);

            IQueryable<HousingUnit> _listHousingUnit = listHousingUnit
                .Where(w => value.ListConsoleHousingId.Contains(w.HousingUnitListId)).Distinct();

            List<HeadCountVm> headCountHousingStartDetails = new List<HeadCountVm>();

            int headCount = _context.CellLogHeadcount.Count(w => w.FacilityId == value.FacilityId);

            if (headCount > 0)
            {
                List<int> housingIds = _listHousingUnit.Where(h => h.FacilityId == value.FacilityId)
                    .Select(h => h.HousingUnitListId).ToList();

                if (value.ListConsoleHousingId.Any(a => a == 0))
                {
                    housingIds.Add(0);
                }

                _maxHeadCountId =
                    _context.CellLogHeadcount.Where(w => w.FacilityId == value.FacilityId && !w.ClearedDate.HasValue)
                        .Select(f => f.CellLogHeadcountId)
                        .FirstOrDefault();

                headCountHousingStartDetails = _context.CellLog.Where(w =>
                        w.CellLogHeadcountId == _maxHeadCountId && w.FacilityId == value.FacilityId
                        && housingIds.Contains(w.HousingUnitListId ?? 0) && !w.CellLogLocationId.HasValue)
                        .OrderBy(o => o.HousingUnitListId)
                    .Select(c => new HeadCountVm
                    {
                        CellLogId = c.CellLogId,
                        HousingUnitListId = c.HousingUnitListId ?? 0,
                        HousingLocation = c.HousingUnitLocation,
                        HousingNumber = c.HousingUnitNumber,
                        FacilityId = value.FacilityId,
                        Assigned = c.CellLogAssigned ?? 0,
                        Out = c.CellLogCheckout ?? 0,
                        Actual = c.CellLogActual ?? 0,
                        Count = c.CellLogCount ?? 0,
                        UpdateDate = c.UpdateDate,
                        StartDate = c.CellLogHeadcount.StartDate,
                        CellLogSchdule = c.CellLogHeadcount.CellLogSchedule,
                        UpdatedBy= c.UpdateBy
                    }).ToList();

                if (headCountHousingStartDetails.Where(a => a.HousingUnitListId == 0).Count() == 0 
                    && value.ListConsoleHousingId.Any(a => a == 0))
                {
                    headCountHousingStartDetails.Add(new HeadCountVm
                    {
                        FacilityId =  value.FacilityId,
                        HousingUnitListId = 0,
                        HousingLocation = HousingConstants.NOHOUSING
                    });
                }

                headCountHousingStartDetails.ForEach(s =>
                    {
                        if (s.HousingUnitListId == 0)
                        {
                            int assigned = _inmateList
                                .Count(c => !c.HousingUnitId.HasValue && c.FacilityId == value.FacilityId);
                            s.Assigned = assigned;
                            int outCnt = _inmateList.Count(c => !c.HousingUnitId.HasValue
                                                                && c.InmateCurrentTrackId.HasValue &&
                                                                c.FacilityId == value.FacilityId);
                            s.Out = outCnt;
                            s.Actual = assigned - outCnt;
                        }

                    }); 
                _inmateList = _inmateList
                    .Where(w => w.HousingUnitId.HasValue
                                && value.ListConsoleHousingId.Contains(w.HousingUnit.HousingUnitListId));

                List<InmateSearchVm> inmateDetailsList = _inmateList
                    .Where(w => w.HousingUnitId.HasValue && w.FacilityId == value.FacilityId)
                    .Select(s => new InmateSearchVm
                    {
                        InmateId = s.InmateId,
                        InmateNumber = s.InmateNumber,
                        LocationId = s.InmateCurrentTrackId,
                        Location = s.InmateCurrentTrack,
                        HousingUnitId = s.HousingUnit.HousingUnitListId
                    }).ToList();

                if (value.FacilityId > 0)
                {
                    facilityAbbr = _context.Facility.Single(f => f.FacilityId == value.FacilityId).FacilityAbbr;
                }

                IEnumerable<KeyValuePair<int, string>> lstHousingLocation = _listHousingUnit
                                   .Where(s => !headCountHousingStartDetails.Select(h => h.HousingUnitListId).Contains(s.HousingUnitListId))
                                   .OrderBy(o => o.HousingUnitNumber)
                                   .Select(x => new KeyValuePair<int, string>(x.HousingUnitListId, x.HousingUnitNumber))
                                   .Distinct().ToList();

                List<HeadCountVm> headCountHousingDetails = lstHousingLocation
                    .OrderBy(o => o.Key)
                    .Select(s => new HeadCountVm
                    {
                        FacilityAbbr = facilityAbbr,
                        HousingNumber = s.Value,
                        HousingUnitListId = s.Key,
                        Assigned = inmateDetailsList
                                .Count(c => c.HousingUnitId == s.Key),
                        Out = inmateDetailsList
                                .Count(c => c.HousingUnitId == s.Key
                                            && c.LocationId.HasValue),
                        Actual = inmateDetailsList
                                .Count(c => c.HousingUnitId == s.Key)
                      //  UpdatedBy = s.Key
                    }).ToList();

                List<HousingUnitList> lstHousing = _context.HousingUnitList.Where(h => h.FacilityId == value.FacilityId).Select(
                    h => new HousingUnitList
                    {
                        HousingUnitListId = h.HousingUnitListId,
                        HousingUnitLocation = h.HousingUnitLocation,
                        FacilityId = h.FacilityId
                    }).ToList();

                headCountHousingDetails.Where(a => a.HousingUnitListId > 0).ToList().ForEach(item =>
                    {
                        item.HousingLocation = lstHousing.Single(h => h.HousingUnitListId == item.HousingUnitListId)
                                .HousingUnitLocation;
                    });

                headCountHousingStartDetails.AddRange(headCountHousingDetails);
            }

            return headCountHousingStartDetails.OrderBy(a => a.HousingUnitListId).ThenBy(a => a.HousingLocation)
                .ThenBy(a => a.HousingNumber).ToList();

        }

        public List<HeadCountVm> LoadHeadCountLocationList(ConsoleInputVm value)
        {
            List<HeadCountVm> headCountLocationDetails = new List<HeadCountVm>();

            headCountLocationDetails = _context.CellLog
                .Where(c => c.FacilityId == value.FacilityId && !c.CellLogHeadcount.ClearedDate.HasValue
                            && c.CellLogId > 0
                            && c.CellLogLocationId.HasValue
                            && (value.ListConsoleLocationId.Contains(c.CellLogLocationId ?? 0)
                                || value.ListConsoleMyLocationId.Contains(c.CellLogLocationId ?? 0)))
                .OrderBy(c => c.CellLogLocationNavigation.PrivilegeDescription)
                .Select(c => new HeadCountVm
                {
                    CellLogId = c.CellLogId,
                    CellLogDate = c.CellLogDate,
                    PrivilegesDescription = c.CellLogLocationNavigation.PrivilegeDescription,
                    PrivilegesId = c.CellLogLocationId,
                    Actual = c.CellLogActual ?? 0,
                    Count = c.CellLogCount ?? 0,
                    UpdateDate = c.UpdateDate,
                    StartDate = c.CellLogHeadcount.StartDate,
                    FacilityAbbr = c.Facility.FacilityAbbr,
                    FacilityId = value.FacilityId,
                    CellLogSchdule = c.CellLogHeadcount.CellLogSchedule,
                    UpdatedBy = c.UpdateBy
                }).ToList();

            List<HeadCountVm> lstHeadCountDetails = _context.Privileges
                .Where(s => !headCountLocationDetails.Select(h => h.PrivilegesId).Contains(s.PrivilegeId)
                            && (value.ListConsoleLocationId.Contains(s.PrivilegeId)
                                || value.ListConsoleMyLocationId.Contains(s.PrivilegeId)))
                .OrderBy(s => s.PrivilegeDescription)
                .Select(p => new HeadCountVm
                {
                    FacilityId = p.FacilityId ?? 0,
                    FacilityAbbr = p.Facility1.FacilityAbbr,
                    PrivilegesDescription = p.PrivilegeDescription,
                    PrivilegesId = p.PrivilegeId,
                    CellLogDate = DateTime.Now,
                    ClearedDate = DateTime.Now,
                    Actual = p.Inmate.Count(s => s.InmateCurrentTrackId == p.PrivilegeId)
                }).ToList();

            headCountLocationDetails.AddRange(lstHeadCountDetails);

            return headCountLocationDetails.Where(w=>w.Actual>0).ToList();
        }
    }
}