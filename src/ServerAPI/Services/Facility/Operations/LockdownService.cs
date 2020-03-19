using GenerateTables.Models;
using Microsoft.AspNetCore.Http;
using ServerAPI.Utilities;
using ServerAPI.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ServerAPI.Services
{
    public class LockdownService : ILockdownService
    {
        private readonly AAtims _context;
        private readonly int _personnelId;
        public LockdownService(AAtims context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _personnelId = Convert.ToInt32(httpContextAccessor.HttpContext.User
                .FindFirst(AuthDetailConstants.PERSONNELID)?.Value);
        }
        public List<LockdownVm> GetActiveLockdownDetails(int facilityId)
        {
            List<LockdownVm> lockdown = _context.HousingLockdown
                .Where(w => !w.DeleteFlag && DateTime.Now >= w.StartLockdown
                                       && DateTime.Now <= w.EndLockdown)
                .OrderByDescending(o => o.StartLockdown)
                .Select(s => new LockdownVm
                {
                    HousingLockdownId = s.HousingLockdownId,
                    StartDate = s.StartLockdown,
                    EndDate = s.EndLockdown,
                    Reason = s.LockdownReason,
                    Region = s.LockdownSource,
                    SourceId = s.SourceId,
                    DeleteFlag = s.DeleteFlag,
                    Note = s.LockdownNote,
                    CreateDate = s.CreateDate,
                    CreatedPersonnel = new PersonnelVm
                    {
                        PersonLastName = s.CreateByNavigation.PersonNavigation.PersonLastName,
                        OfficerBadgeNumber = s.CreateByNavigation.OfficerBadgeNum
                    },
                    UpdateDate = s.UpdateDate,
                    UpdatedPersonnel = new PersonnelVm
                    {
                        PersonLastName = s.UpdateByNavigation.PersonNavigation.PersonLastName,
                        OfficerBadgeNumber = s.UpdateByNavigation.OfficerBadgeNum
                    },
                    DeleteDate = s.DeleteDate,
                    DeletedPersonnel = new PersonnelVm
                    {
                        PersonLastName = s.DeleteByNavigation.PersonNavigation.PersonLastName,
                        OfficerBadgeNumber = s.DeleteByNavigation.OfficerBadgeNum
                    }
                }).ToList();

            lockdown.ForEach(f =>
            {
                switch (f.Region)
                {
                    case LockdownRegion.HousingGroup:
                        HousingGroupVm group = _context.HousingGroup.Where(w => w.HousingGroupId == f.SourceId)
                            .Select(s => new HousingGroupVm
                            {
                                GroupName = s.GroupName,
                                LocationString = s.Facility.FacilityAbbr,
                                FacilityId = s.FacilityId
                            }).FirstOrDefault();
                        if (!(group is null))
                        {
                            f.FacilityAbbr = @group.LocationString;
                            f.GroupName = @group.GroupName;
                            f.FacilityId = @group.FacilityId;
                        }

                        break;
                    case LockdownRegion.Facility:
                        f.FacilityAbbr = _context.Facility.FirstOrDefault(w => w.FacilityId == f.SourceId)?.FacilityAbbr;
                        f.FacilityId = f.SourceId;
                        break;
                    case LockdownRegion.Building:
                        HousingDetail housing = _context.HousingUnit.Where(w => w.HousingUnitListId == f.SourceId)
                            .Select(s => new HousingDetail
                            {
                                FacilityId = s.FacilityId,
                                FacilityAbbr = s.Facility.FacilityAbbr,
                                HousingUnitLocation = s.HousingUnitLocation
                            }).FirstOrDefault();
                        if (!(housing is null))
                        {
                            f.FacilityId = housing.FacilityId;
                            f.FacilityAbbr = housing.FacilityAbbr;
                            f.HousingUnitLocation = housing.HousingUnitLocation;
                        }

                        break;
                    case LockdownRegion.Pod:
                        HousingDetail podHousing = _context.HousingUnit.Where(w => w.HousingUnitListId == f.SourceId)
                            .Select(s => new HousingDetail
                            {
                                FacilityId = s.FacilityId,
                                FacilityAbbr = s.Facility.FacilityAbbr,
                                HousingUnitLocation = s.HousingUnitLocation,
                                HousingUnitListId = s.HousingUnitListId,
                                HousingUnitNumber = s.HousingUnitNumber
                            }).FirstOrDefault();
                        if (!(podHousing is null))
                        {
                            f.FacilityId = podHousing.FacilityId;
                            f.FacilityAbbr = podHousing.FacilityAbbr;
                            f.HousingUnitLocation = podHousing.HousingUnitLocation;
                            f.HousingUnitListId = podHousing.HousingUnitListId;
                            f.HousingUnitNumber = podHousing.HousingUnitNumber;
                        }

                        break;
                    case LockdownRegion.Cell:
                        HousingDetail cellHousing = _context.HousingUnit.Where(w => w.HousingUnitId == f.SourceId)
                            .Select(s => new HousingDetail
                            {
                                FacilityId = s.FacilityId,
                                FacilityAbbr = s.Facility.FacilityAbbr,
                                HousingUnitLocation = s.HousingUnitLocation,
                                HousingUnitListId = s.HousingUnitListId,
                                HousingUnitNumber = s.HousingUnitNumber,
                                HousingUnitBedNumber = s.HousingUnitBedNumber
                            }).FirstOrDefault();
                        if (!(cellHousing is null))
                        {
                            f.FacilityId = cellHousing.FacilityId;
                            f.FacilityAbbr = cellHousing.FacilityAbbr;
                            f.HousingUnitLocation = cellHousing.HousingUnitLocation;
                            f.HousingUnitListId = cellHousing.HousingUnitListId;
                            f.HousingUnitNumber = cellHousing.HousingUnitNumber;
                            f.HousingUnitBedNumber = cellHousing.HousingUnitBedNumber;
                        }

                        break;
                    case LockdownRegion.CellGroup:
                        HousingDetail cellGroupHousing = _context.HousingUnitListBedGroup
                            .Where(w => w.HousingUnitListBedGroupId == f.SourceId)
                            .Select(s => new HousingDetail
                            {
                                FacilityId = s.HousingUnitList.FacilityId,
                                FacilityAbbr = s.HousingUnitList.Facility.FacilityAbbr,
                                HousingUnitLocation = s.HousingUnitList.HousingUnitLocation,
                                HousingUnitListId = s.HousingUnitList.HousingUnitListId,
                                HousingUnitNumber = s.HousingUnitList.HousingUnitNumber,
                                HousingUnitBedGroupId = s.HousingUnitListBedGroupId,
                                HousingUnitBedLocation = s.BedGroupName

                            }).FirstOrDefault();
                        if (!(cellGroupHousing is null))
                        {
                            f.FacilityId = cellGroupHousing.FacilityId;
                            f.FacilityAbbr = cellGroupHousing.FacilityAbbr;
                            f.HousingUnitLocation = cellGroupHousing.HousingUnitLocation;
                            f.HousingUnitListId = cellGroupHousing.HousingUnitListId;
                            f.HousingUnitNumber = cellGroupHousing.HousingUnitNumber;
                            f.HousingUnitBedLocation = cellGroupHousing.HousingUnitBedLocation;
                        }

                        break;
                }
            });
            lockdown = lockdown.Where(w => w.FacilityId == facilityId).ToList();
            return lockdown;
        }

        public List<LockdownVm> GetLockdownHistoryDetails(DateTime? fromDate, DateTime? toDate, int facilityId)
        {
            IQueryable<HousingLockdown> housingLockdowns = _context.HousingLockdown;

            if (!(fromDate == null))
            {
                housingLockdowns = housingLockdowns.Where(w => w.StartLockdown.Date >= fromDate.Value.Date
                     && w.EndLockdown.Date <= toDate.Value.Date);
            }

            List<LockdownVm> lockdown = housingLockdowns
                .OrderByDescending(o => o.StartLockdown)
                .Select(s => new LockdownVm
                {
                    HousingLockdownId = s.HousingLockdownId,
                    StartDate = s.StartLockdown,
                    EndDate = s.EndLockdown,
                    Reason = s.LockdownReason,
                    Region = s.LockdownSource,
                    SourceId = s.SourceId,
                    DeleteFlag = s.DeleteFlag,
                    Note = s.LockdownNote,
                    CreateDate = s.CreateDate,
                    CreatedPersonnel = new PersonnelVm
                    {
                        PersonLastName = s.CreateByNavigation.PersonNavigation.PersonLastName,
                        OfficerBadgeNumber = s.CreateByNavigation.OfficerBadgeNum
                    },
                    UpdateDate = s.UpdateDate,
                    UpdatedPersonnel = new PersonnelVm
                    {
                        PersonLastName = s.UpdateByNavigation.PersonNavigation.PersonLastName,
                        OfficerBadgeNumber = s.UpdateByNavigation.OfficerBadgeNum
                    },
                    DeleteDate = s.DeleteDate,
                    DeletedPersonnel = new PersonnelVm
                    {
                        PersonLastName = s.DeleteByNavigation.PersonNavigation.PersonLastName,
                        OfficerBadgeNumber = s.DeleteByNavigation.OfficerBadgeNum
                    }
                }).ToList();

            if (fromDate is null)
            {
                lockdown = lockdown
                    .Take(100).ToList();
            }

            lockdown.ForEach(f =>
            {
                switch (f.Region)
                {
                    case LockdownRegion.HousingGroup:
                        HousingGroupVm group = _context.HousingGroup.Where(w => w.HousingGroupId == f.SourceId)
                            .Select(s => new HousingGroupVm
                            {
                                GroupName = s.GroupName,
                                LocationString = s.Facility.FacilityAbbr,
                                FacilityId = s.FacilityId
                            }).FirstOrDefault();
                        if (!(group is null))
                        {
                            f.FacilityAbbr = group.LocationString;
                            f.GroupName = group.GroupName;
                            f.FacilityId = group.FacilityId;
                        }

                        break;
                    case LockdownRegion.Facility:
                        f.FacilityAbbr = _context.Facility.FirstOrDefault(w => w.FacilityId == f.SourceId)?.FacilityAbbr;
                        break;
                    case LockdownRegion.Building:
                        HousingDetail housing = _context.HousingUnit.Where(w => w.HousingUnitListId == f.SourceId)
                            .Select(s => new HousingDetail
                            {
                                FacilityId = s.FacilityId,
                                FacilityAbbr = s.Facility.FacilityAbbr,
                                HousingUnitLocation = s.HousingUnitLocation
                            }).FirstOrDefault();
                        if (!(housing is null))
                        {
                            f.FacilityId = housing.FacilityId;
                            f.FacilityAbbr = housing.FacilityAbbr;
                            f.HousingUnitLocation = housing.HousingUnitLocation;
                        }

                        break;
                    case LockdownRegion.Pod:
                        HousingDetail podHousing = _context.HousingUnit.Where(w => w.HousingUnitListId == f.SourceId)
                            .Select(s => new HousingDetail
                            {
                                FacilityId = s.FacilityId,
                                FacilityAbbr = s.Facility.FacilityAbbr,
                                HousingUnitLocation = s.HousingUnitLocation,
                                HousingUnitListId = s.HousingUnitListId,
                                HousingUnitNumber = s.HousingUnitNumber
                            }).FirstOrDefault();
                        if (!(podHousing is null))
                        {
                            f.FacilityId = podHousing.FacilityId;
                            f.FacilityAbbr = podHousing.FacilityAbbr;
                            f.HousingUnitLocation = podHousing.HousingUnitLocation;
                            f.HousingUnitListId = podHousing.HousingUnitListId;
                            f.HousingUnitNumber = podHousing.HousingUnitNumber;
                        }

                        break;
                    case LockdownRegion.Cell:
                        HousingDetail cellHousing = _context.HousingUnit.Where(w => w.HousingUnitId == f.SourceId)
                            .Select(s => new HousingDetail
                            {
                                FacilityId = s.FacilityId,
                                FacilityAbbr = s.Facility.FacilityAbbr,
                                HousingUnitLocation = s.HousingUnitLocation,
                                HousingUnitListId = s.HousingUnitListId,
                                HousingUnitNumber = s.HousingUnitNumber,
                                HousingUnitBedNumber = s.HousingUnitBedNumber,
                                HousingUnitId = s.HousingUnitId
                            }).FirstOrDefault();
                        if (!(cellHousing is null))
                        {
                            f.FacilityId = cellHousing.FacilityId;
                            f.FacilityAbbr = cellHousing.FacilityAbbr;
                            f.HousingUnitLocation = cellHousing.HousingUnitLocation;
                            f.HousingUnitListId = cellHousing.HousingUnitListId;
                            f.HousingUnitNumber = cellHousing.HousingUnitNumber;
                            f.HousingUnitBedNumber = cellHousing.HousingUnitBedNumber;
                            f.HousingUnitId = cellHousing.HousingUnitId;
                        }

                        break;
                    case LockdownRegion.CellGroup:
                        HousingDetail cellGroupHousing = _context.HousingUnitListBedGroup.Where(w => w.HousingUnitListBedGroupId == f.SourceId)
                            .Select(s => new HousingDetail
                            {
                                FacilityId = s.HousingUnitList.FacilityId,
                                FacilityAbbr = s.HousingUnitList.Facility.FacilityAbbr,
                                HousingUnitLocation = s.HousingUnitList.HousingUnitLocation,
                                HousingUnitListId = s.HousingUnitList.HousingUnitListId,
                                HousingUnitNumber = s.HousingUnitList.HousingUnitNumber,
                                HousingUnitBedGroupId = s.HousingUnitListBedGroupId,
                                HousingUnitBedLocation = s.BedGroupName

                            }).FirstOrDefault();
                        if (!(cellGroupHousing is null))
                        {
                            f.FacilityId = cellGroupHousing.FacilityId;
                            f.FacilityAbbr = cellGroupHousing.FacilityAbbr;
                            f.HousingUnitLocation = cellGroupHousing.HousingUnitLocation;
                            f.HousingUnitListId = cellGroupHousing.HousingUnitListId;
                            f.HousingUnitNumber = cellGroupHousing.HousingUnitNumber;
                            f.HousingUnitBedLocation = cellGroupHousing.HousingUnitBedLocation;
                        }

                        break;
                }
            });
            lockdown = lockdown.Where(w => w.FacilityId == facilityId).ToList();
            return lockdown;
        }

        public LockdownDetailsVm GetLockdownEntryDetails(int housingLockdownId)
        {
            LockdownDetailsVm lockdownDetail = new LockdownDetailsVm();

            if (housingLockdownId > 0)
            {
                lockdownDetail.LockdownInfo = _context.HousingLockdown
                    .Where(w => w.HousingLockdownId == housingLockdownId)
                    .Select(s => new LockdownVm
                    {
                        HousingLockdownId = s.HousingLockdownId,
                        StartDate = s.StartLockdown,
                        EndDate = s.EndLockdown,
                        Reason = s.LockdownReason,
                        Note = s.LockdownNote,
                        SourceId = s.SourceId,
                        Region = s.LockdownSource,
                        DeleteFlag = s.DeleteFlag
                    }).FirstOrDefault();

                if (!(lockdownDetail.LockdownInfo is null))
                {
                    switch (lockdownDetail.LockdownInfo.Region)
                    {
                        case LockdownRegion.HousingGroup:
                            HousingGroupVm group = _context.HousingGroup.Where(w => w.HousingGroupId == lockdownDetail.LockdownInfo.SourceId)
                                .Select(s => new HousingGroupVm
                                {
                                    HousingGroupId = s.HousingGroupId,
                                    FacilityId = s.FacilityId
                                }).FirstOrDefault();
                            if (!(group is null))
                            {
                                lockdownDetail.LockdownInfo.HousingUnitGroupId = group.HousingGroupId;
                                lockdownDetail.LockdownInfo.FacilityId = group.FacilityId;
                            }

                            break;
                        case LockdownRegion.Facility:
                            lockdownDetail.LockdownInfo.FacilityId = lockdownDetail.LockdownInfo.SourceId;
                            break;
                        case LockdownRegion.Building:
                            HousingDetail housing = _context.HousingUnit
                                .Where(w => w.HousingUnitListId == lockdownDetail.LockdownInfo.SourceId)
                                .Select(s => new HousingDetail
                                {
                                    FacilityId = s.FacilityId,
                                    HousingUnitLocation = s.HousingUnitLocation,
                                    HousingUnitListId = s.HousingUnitListId
                                }).FirstOrDefault();
                            if (!(housing is null))
                            {
                                lockdownDetail.LockdownInfo.FacilityId = housing.FacilityId;
                                lockdownDetail.LockdownInfo.HousingUnitListId = housing.HousingUnitListId;
                                lockdownDetail.LockdownInfo.HousingUnitLocation = housing.HousingUnitLocation;
                            }

                            break;
                        case LockdownRegion.Pod:
                            HousingDetail podHousing = _context.HousingUnit
                                .Where(w => w.HousingUnitListId == lockdownDetail.LockdownInfo.SourceId)
                                .Select(s => new HousingDetail
                                {
                                    FacilityId = s.FacilityId,
                                    HousingUnitListId = s.HousingUnitListId,
                                    HousingUnitLocation = s.HousingUnitLocation
                                }).FirstOrDefault();
                            if (!(podHousing is null))
                            {
                                lockdownDetail.LockdownInfo.FacilityId = podHousing.FacilityId;
                                lockdownDetail.LockdownInfo.HousingUnitLocation = podHousing.HousingUnitLocation;
                                lockdownDetail.LockdownInfo.HousingUnitListId = podHousing.HousingUnitListId;
                            }

                            break;
                        case LockdownRegion.Cell:
                            HousingDetail cellHousing = _context.HousingUnit
                                .Where(w => w.HousingUnitId == lockdownDetail.LockdownInfo.SourceId)
                                .Select(s => new HousingDetail
                                {
                                    FacilityId = s.FacilityId,
                                    HousingUnitListId = s.HousingUnitListId,
                                    HousingUnitLocation = s.HousingUnitLocation,
                                    HousingUnitBedNumber = s.HousingUnitBedNumber,
                                    HousingUnitId = s.HousingUnitId
                                }).FirstOrDefault();
                            if (!(cellHousing is null))
                            {
                                lockdownDetail.LockdownInfo.FacilityId = cellHousing.FacilityId;
                                lockdownDetail.LockdownInfo.HousingUnitLocation = cellHousing.HousingUnitLocation;
                                lockdownDetail.LockdownInfo.HousingUnitListId = cellHousing.HousingUnitListId;
                                lockdownDetail.LockdownInfo.HousingUnitBedNumber = cellHousing.HousingUnitBedNumber;
                                lockdownDetail.LockdownInfo.HousingUnitId = cellHousing.HousingUnitId;
                            }

                            break;
                        case LockdownRegion.CellGroup:
                            HousingDetail cellGroupHousing = _context.HousingUnit
                                .Where(w => w.HousingUnitListBedGroupId == lockdownDetail.LockdownInfo.SourceId)
                                .Select(s => new HousingDetail
                                {
                                    FacilityId = s.FacilityId,
                                    HousingUnitListId = s.HousingUnitListId,
                                    HousingUnitLocation = s.HousingUnitLocation,
                                    HousingUnitBedGroupId = s.HousingUnitListBedGroupId ?? 0
                                }).FirstOrDefault();
                            if (!(cellGroupHousing is null))
                            {
                                lockdownDetail.LockdownInfo.FacilityId = cellGroupHousing.FacilityId;
                                lockdownDetail.LockdownInfo.HousingUnitLocation = cellGroupHousing.HousingUnitLocation;
                                lockdownDetail.LockdownInfo.HousingUnitListId = cellGroupHousing.HousingUnitListId;
                                lockdownDetail.LockdownInfo.HousingUnitBedGroupId =
                                    cellGroupHousing.HousingUnitBedGroupId;
                            }

                            break;
                    }
                }
            }

            lockdownDetail.FacilityList = _context.Facility.Where(w => w.DeleteFlag == 0)
                .Select(s => new FacilityVm
                {
                    FacilityId = s.FacilityId,
                    FacilityAbbr = s.FacilityAbbr
                }).ToList();

            lockdownDetail.BuildingList = _context.HousingUnit.GroupBy(g => new { g.HousingUnitLocation, g.FacilityId })
                .Select(s => new HousingDetail
                {
                    HousingUnitLocation = s.Key.HousingUnitLocation,
                    FacilityId = s.Key.FacilityId,
                    HousingUnitListId = s.Select(se => se.HousingUnitListId).FirstOrDefault()
                }).ToList();

            lockdownDetail.PodList = _context.HousingUnit.GroupBy(g => new { g.HousingUnitLocation, g.HousingUnitNumber, g.FacilityId })
                .Select(s => new HousingDetail
                {
                    HousingUnitLocation = s.Key.HousingUnitLocation,
                    HousingUnitNumber = s.Key.HousingUnitNumber,
                    FacilityId = s.Key.FacilityId,
                    HousingUnitListId = s.Select(se => se.HousingUnitListId).FirstOrDefault()
                }).ToList();

            lockdownDetail.CellList = _context.HousingUnit.GroupBy(g => new { g.HousingUnitListId, g.HousingUnitBedNumber, g.FacilityId })
                .Select(s => new HousingDetail
                {
                    HousingUnitBedNumber = s.Key.HousingUnitBedNumber,
                    FacilityId = s.Key.FacilityId,
                    HousingUnitId = s.Select(se => se.HousingUnitId).FirstOrDefault(),
                    HousingUnitListId = s.Select(se => se.HousingUnitListId).FirstOrDefault()
                }).ToList();

            lockdownDetail.CellGroupList = _context.HousingUnitListBedGroup.OrderBy(o => o.BedGroupName)
                .Select(s => new HousingDetail
                {
                    HousingUnitId = s.HousingUnitListBedGroupId,
                    HousingUnitListId = s.HousingUnitListId,
                    HousingUnitLocation = s.BedGroupName,
                    FacilityId = s.HousingUnitList.FacilityId,
                }).ToList();

            lockdownDetail.GroupList = _context.HousingGroup
                .Select(s => new HousingGroupVm
                {
                    HousingGroupId = s.HousingGroupId,
                    FacilityId = s.FacilityId,
                    GroupName = s.GroupName
                }).ToList();

            return lockdownDetail;
        }

        public async Task<int> InsertUpdateLockdown(LockdownVm value)
        {
            switch (value.Region)
            {
                case "Cell":
                    value.SourceId = value.HousingUnitId;
                    break;
                case "CellGroup":
                    value.SourceId = value.HousingUnitBedGroupId;
                    break;
                case "Pod":
                    value.SourceId = value.HousingUnitListId ?? 0;
                    break;
                case "HousingGroup":
                    value.SourceId = value.HousingUnitGroupId ?? 0;
                    break;
                case "Building":
                    value.SourceId = value.HousingUnitListId ?? 0;
                    break;
                case "Facility":
                    value.SourceId = value.FacilityId;
                    break;
            }
            if (value.HousingLockdownId == 0)
            {
                HousingLockdown lockdown = new HousingLockdown
                {
                    CreateDate = DateTime.Now,
                    CreateBy = _personnelId,
                    LockdownReason = value.Reason,
                    StartLockdown = value.StartDate,
                    EndLockdown = value.EndDate,
                    LockdownNote = value.Note,
                    SourceId = value.SourceId,
                    LockdownSource = value.Region
                };
                _context.HousingLockdown.Add(lockdown);
            }
            else
            {
                HousingLockdown updateLockdown = _context.HousingLockdown.Single(s => s.HousingLockdownId == value.HousingLockdownId);
                if (!(updateLockdown is null))
                {
                    updateLockdown.UpdateDate = DateTime.Now;
                    updateLockdown.UpdateBy = _personnelId;
                    updateLockdown.LockdownReason = value.Reason;
                    updateLockdown.StartLockdown = value.StartDate;
                    updateLockdown.EndLockdown = value.EndDate;
                    updateLockdown.LockdownNote = value.Note;
                    updateLockdown.SourceId = value.SourceId;
                    updateLockdown.LockdownSource = value.Region;
                }
            }
            return await _context.SaveChangesAsync();
        }

        public async Task<int> DeleteUndoLockdown(LockdownVm value)
        {
            HousingLockdown lockdown = _context.HousingLockdown.Single(w => w.HousingLockdownId == value.HousingLockdownId);
            if (!(lockdown is null))
            {
                lockdown.DeleteFlag = value.DeleteFlag;
                lockdown.DeleteDate = value.DeleteFlag ? DateTime.Now : default(DateTime?);
                lockdown.DeleteBy = value.DeleteFlag ? _personnelId : default(int?);
            }
            return await _context.SaveChangesAsync();
        }

        public List<LockdownVm> GetLockdownDetails(int regionId, LockDownType region, string housingInfo)
        {
            List<LockdownVm> lockDownDetails = new List<LockdownVm>();
            List<LockdownVm> housingLockdown = _context.HousingLockdown.Where(w =>
            w.StartLockdown <= DateTime.Now && w.EndLockdown >= DateTime.Now && !w.DeleteFlag)
            .Select(s => new LockdownVm
            {
                Region = s.LockdownSource,
                Note = s.LockdownNote,
                StartDate = s.StartLockdown,
                EndDate = s.EndLockdown,
                Reason = s.LockdownReason,
                HousingLockdownId = s.HousingLockdownId,
                SourceId = s.SourceId
            }).ToList();
            List<HousingUnit> housingUnit = _context.HousingUnit.ToList();
            List<HousingGroupAssign> housingGroupAssign = _context.HousingGroupAssign
                .Where(w => w.DeleteFlag == 0 && w.HousingUnitLocation != null).ToList();
            switch (region)
            {
                case LockDownType.Facility:
                    housingLockdown.ForEach(lockDown =>
                    {
                        if (lockDown.Region == LockDownType.Facility.ToString() && lockDown.SourceId == regionId)
                        {
                            lockDown.FacilityAbbr = _context.Facility.Find(lockDown.SourceId).FacilityAbbr;
                            lockDownDetails.Add(lockDown);
                        }
                    });
                    break;
                case LockDownType.Building:
                    HousingUnit buildingDetails = housingUnit.First(w => w.FacilityId == regionId && w.HousingUnitLocation == housingInfo);
                    housingLockdown.ForEach(lockDown =>
                    {
                        if (lockDown.Region == LockDownType.Facility.ToString() && lockDown.SourceId == buildingDetails.FacilityId)
                        {
                            lockDown.FacilityAbbr = _context.Facility.Find(lockDown.SourceId).FacilityAbbr;
                            lockDownDetails.Add(lockDown);
                        }
                        if (lockDown.Region == LockDownType.Building.ToString() && housingUnit.First(w =>
                        w.HousingUnitListId == lockDown.SourceId).HousingUnitLocation == buildingDetails.HousingUnitLocation)
                        {
                            lockDown.HousingUnitLocation = buildingDetails.HousingUnitLocation;
                            lockDownDetails.Add(lockDown);
                        }
                    });
                    break;
                case LockDownType.HousingGroup:
                    int housingUnitListId = housingGroupAssign.Where(w => w.HousingGroupId == regionId
                      && w.HousingUnitLocation != null && w.HousingUnitListId.HasValue)
                    .Select(s => s.HousingUnitListId ?? 0).FirstOrDefault();
                    if (housingUnitListId > 0)
                    {
                        HousingUnit groupDetails = housingUnit.First(f => f.HousingUnitListId == housingUnitListId);
                        housingLockdown.ForEach(lockDown =>
                        {
                            if (lockDown.Region == LockDownType.Facility.ToString() &&
                                lockDown.SourceId == groupDetails.FacilityId)
                            {
                                lockDown.FacilityAbbr = _context.Facility.Find(lockDown.SourceId).FacilityAbbr;
                                lockDownDetails.Add(lockDown);
                            }

                            if (lockDown.Region == LockDownType.HousingGroup.ToString() &&
                                lockDown.SourceId == regionId)
                            {
                                lockDown.GroupName = _context.HousingGroup.Find(lockDown.SourceId).GroupName;
                                lockDownDetails.Add(lockDown);
                            }
                        });
                    }
                    break;
                case LockDownType.Pod:
                    HousingUnit podDetails = housingUnit.First(f => f.HousingUnitListId == regionId);
                    housingLockdown.ForEach(lockDown =>
                    {
                        if (lockDown.Region == LockDownType.Facility.ToString() && lockDown.SourceId == podDetails.FacilityId)
                        {
                            lockDown.FacilityAbbr = _context.Facility.Find(lockDown.SourceId).FacilityAbbr;
                            lockDownDetails.Add(lockDown);
                        }
                        if (lockDown.Region == LockDownType.Building.ToString()
                        && housingUnit.First(f => f.HousingUnitListId == lockDown.SourceId).HousingUnitLocation == podDetails.HousingUnitLocation)
                        {
                            lockDown.HousingUnitLocation = podDetails.HousingUnitLocation;
                            lockDownDetails.Add(lockDown);
                        }
                        if (lockDown.Region == LockDownType.HousingGroup.ToString()
                        && lockDown.SourceId == housingGroupAssign.FirstOrDefault(f => f.HousingUnitListId == regionId)?.HousingGroupId)
                        {
                            lockDown.GroupName = _context.HousingGroup.Find(lockDown.SourceId).GroupName;
                            lockDownDetails.Add(lockDown);
                        }
                        if (lockDown.Region == LockDownType.Pod.ToString() && lockDown.SourceId == regionId)
                        {
                            lockDown.HousingUnitLocation = podDetails.HousingUnitLocation;
                            lockDown.HousingUnitNumber = podDetails.HousingUnitNumber;
                            lockDownDetails.Add(lockDown);
                        }
                    });
                    break;
                case LockDownType.CellGroup:
                    HousingUnit cellGroupDetails = housingUnit.First(f => f.HousingUnitListBedGroupId == regionId);
                    housingLockdown.ForEach(lockDown =>
                    {
                        if (lockDown.Region == LockDownType.Facility.ToString() && lockDown.SourceId == cellGroupDetails.FacilityId)
                        {
                            lockDown.FacilityAbbr = _context.Facility.Find(lockDown.SourceId).FacilityAbbr;
                            lockDownDetails.Add(lockDown);
                        }
                        if (lockDown.Region == LockDownType.Building.ToString()
                        && housingUnit.First(f => f.HousingUnitListId == lockDown.SourceId).HousingUnitLocation == cellGroupDetails.HousingUnitLocation)
                        {
                            lockDown.HousingUnitLocation = cellGroupDetails.HousingUnitLocation;
                            lockDownDetails.Add(lockDown);
                        }
                        if (lockDown.Region == LockDownType.HousingGroup.ToString()
                        && lockDown.SourceId == housingGroupAssign.FirstOrDefault(f => f.HousingUnitListId == cellGroupDetails.HousingUnitListId)?.HousingGroupId)
                        {
                            lockDown.GroupName = _context.HousingGroup.Find(lockDown.SourceId).GroupName;
                            lockDownDetails.Add(lockDown);
                        }
                        if (lockDown.Region == LockDownType.Pod.ToString() && lockDown.SourceId == cellGroupDetails.HousingUnitListId)
                        {
                            lockDown.HousingUnitLocation = cellGroupDetails.HousingUnitLocation;
                            lockDown.HousingUnitNumber = cellGroupDetails.HousingUnitNumber;
                            lockDownDetails.Add(lockDown);
                        }
                        if (lockDown.Region == LockDownType.CellGroup.ToString() && lockDown.SourceId == regionId)
                        {
                            lockDown.GroupName = _context.HousingUnitListBedGroup.Find(lockDown.SourceId).BedGroupName;
                            lockDownDetails.Add(lockDown);
                        }
                    });
                    break;
                case LockDownType.Cell:
                    HousingUnit cellDetails = housingUnit.First(w => w.HousingUnitListId == regionId && w.HousingUnitBedNumber == housingInfo);
                    housingLockdown.ForEach(lockDown =>
                    {
                        if (lockDown.Region == LockDownType.Facility.ToString() && lockDown.SourceId == cellDetails.FacilityId)
                        {
                            lockDown.FacilityAbbr = _context.Facility.Find(lockDown.SourceId).FacilityAbbr;
                            lockDownDetails.Add(lockDown);
                        }
                        if (lockDown.Region == LockDownType.Building.ToString()
                        && housingUnit.First(f => f.HousingUnitListId == lockDown.SourceId).HousingUnitLocation == cellDetails.HousingUnitLocation)
                        {
                            lockDown.HousingUnitLocation = cellDetails.HousingUnitLocation;
                            lockDownDetails.Add(lockDown);
                        }
                        if (lockDown.Region == LockDownType.HousingGroup.ToString()
                        && lockDown.SourceId == housingGroupAssign.FirstOrDefault(f => f.HousingUnitListId == cellDetails.HousingUnitListId && f.DeleteFlag == 0)?.HousingGroupId)
                        {
                            lockDown.GroupName = _context.HousingGroup.Find(lockDown.SourceId).GroupName;
                            lockDownDetails.Add(lockDown);
                        }
                        if (lockDown.Region == LockDownType.Pod.ToString() && lockDown.SourceId == cellDetails.HousingUnitListId)
                        {
                            lockDown.HousingUnitLocation = cellDetails.HousingUnitLocation;
                            lockDown.HousingUnitNumber = cellDetails.HousingUnitNumber;
                            lockDownDetails.Add(lockDown);
                        }
                        if (lockDown.Region == LockDownType.CellGroup.ToString() && lockDown.SourceId == cellDetails.HousingUnitListBedGroupId)
                        {
                            lockDown.GroupName = _context.HousingUnitListBedGroup.Find(lockDown.SourceId).BedGroupName;
                            lockDownDetails.Add(lockDown);
                        }
                        if (lockDown.Region == LockDownType.Cell.ToString() && lockDown.SourceId == cellDetails.HousingUnitId)
                        {
                            lockDown.HousingUnitBedNumber = cellDetails.HousingUnitBedNumber;
                            lockDownDetails.Add(lockDown);
                        }
                    });
                    break;
            }
            return lockDownDetails;
        }

        public List<HousingDetail> GetHousingLockdownNotifications(HousingLockInputVm value)
        {
            List<HousingDetail> lockHousingDetails = new List<HousingDetail>();

            List<LockdownVm> housingLockdown = _context.HousingLockdown.Where(w =>
           w.StartLockdown <= DateTime.Now && w.EndLockdown >= DateTime.Now && !w.DeleteFlag)
            .Select(s => new LockdownVm
            {
                Region = s.LockdownSource,
                SourceId = s.SourceId
            }).ToList();

            List<HousingUnit> housingUnit = _context.HousingUnit
                .Where(w => w.FacilityId == value.FacilityId).ToList();
            List<HousingGroupAssign> housingGroupAssign = _context.HousingGroupAssign
                .Where(w => w.HousingGroup.FacilityId == value.FacilityId && w.HousingUnitLocation != null && w.DeleteFlag == 0).ToList();
            switch (value.HousingType)
            {
                case HousingType.Facility:
                    housingLockdown.ForEach(lockDown =>
                    {
                        if (lockDown.Region == LockDownType.Facility.ToString())
                        {
                            lockHousingDetails.Add(new HousingDetail
                            {
                                FacilityId = lockDown.SourceId
                            });
                        }
                    });
                    break;
                case HousingType.HousingLocation:
                    housingLockdown.ForEach(lockDown =>
                    {
                        if (lockDown.Region == LockDownType.Facility.ToString() && lockDown.SourceId == value.FacilityId)
                        {
                            lockHousingDetails.Add(new HousingDetail
                            {
                                FacilityId = lockDown.SourceId
                            });
                        }
                        if (lockDown.Region == LockDownType.Building.ToString() && housingUnit.Count(w =>
                        w.FacilityId == value.FacilityId && w.HousingUnitListId == lockDown.SourceId) > 0)
                        {
                            lockHousingDetails.Add(new HousingDetail
                            {
                                HousingUnitLocation = housingUnit.First(f => f.HousingUnitListId == lockDown.SourceId).HousingUnitLocation
                            });
                        }
                    });
                    break;
                case HousingType.Number:
                    housingLockdown.ForEach(lockDown =>
                    {
                        if (lockDown.Region == LockDownType.Facility.ToString() && lockDown.SourceId == value.FacilityId)
                        {
                            lockHousingDetails.Add(new HousingDetail
                            {
                                FacilityId = lockDown.SourceId
                            });
                        }
                        if (lockDown.Region == LockDownType.Building.ToString()
                            && housingUnit.Count(f => f.FacilityId == value.FacilityId && f.HousingUnitLocation == value.HousingLocation
                                                                                   && f.HousingUnitListId == lockDown.SourceId) > 0)
                        {
                            lockHousingDetails.Add(new HousingDetail
                            {
                                HousingUnitLocation = housingUnit.First(f => f.HousingUnitListId == lockDown.SourceId).HousingUnitLocation
                            });
                        }
                        if (lockDown.Region == LockDownType.Pod.ToString()
                            && housingUnit.Count(f => f.FacilityId == value.FacilityId && f.HousingUnitLocation == value.HousingLocation
                                                                                       && f.HousingUnitListId == lockDown.SourceId) > 0)
                        {
                            lockHousingDetails.Add(new HousingDetail
                            {
                                HousingUnitListId = lockDown.SourceId
                            });
                        }
                        if (lockDown.Region == LockDownType.HousingGroup.ToString() && housingGroupAssign.Count(w =>
                                w.HousingGroupId == lockDown.SourceId && w.HousingUnitLocation == value.HousingLocation) > 0)
                        {
                            lockHousingDetails.AddRange(housingGroupAssign.Where(w => w.HousingGroupId == lockDown.SourceId
                                                                                 && w.HousingUnitLocation == value.HousingLocation)
                                .Select(s => new HousingDetail
                                {
                                    HousingUnitListId = s.HousingUnitListId
                                }));
                        }
                    });
                    break;
                case HousingType.BedNumber:
                    housingLockdown.ForEach(lockDown =>
                    {
                        if (lockDown.Region == LockDownType.Facility.ToString() && lockDown.SourceId == value.FacilityId)
                        {
                            lockHousingDetails.Add(new HousingDetail
                            {
                                FacilityId = lockDown.SourceId
                            });
                        }
                        if (lockDown.Region == LockDownType.Building.ToString()
                            && housingUnit.Count(f => f.FacilityId == value.FacilityId
                            && f.HousingUnitLocation.Trim() == value.HousingLocation.Trim()
                            && f.HousingUnitListId == lockDown.SourceId) > 0)
                        {
                            lockHousingDetails.Add(new HousingDetail
                            {
                                HousingUnitLocation = housingUnit.First(f => f.HousingUnitListId == lockDown.SourceId).HousingUnitLocation
                            });
                        }
                        if (lockDown.Region == LockDownType.Pod.ToString()
                            && housingUnit.Count(f => f.FacilityId == value.FacilityId && f.HousingUnitLocation.Trim() == value.HousingLocation.Trim()
                            && f.HousingUnitListId == value.HousingUnitListId
                            && f.HousingUnitListId == lockDown.SourceId) > 0)
                        {
                            lockHousingDetails.Add(new HousingDetail
                            {
                                HousingUnitListId = lockDown.SourceId
                            });
                        }
                        if (lockDown.Region == LockDownType.HousingGroup.ToString() && housingGroupAssign.Count(w =>
                                w.HousingGroupId == lockDown.SourceId && w.HousingUnitLocation.Trim() == value.HousingLocation.Trim()) > 0)
                        {
                            lockHousingDetails.AddRange(housingGroupAssign.Where(w => w.HousingGroupId == lockDown.SourceId
                               && w.HousingUnitLocation.Trim() == value.HousingLocation.Trim())
                                .Select(s => new HousingDetail
                                {
                                    HousingUnitListId = s.HousingUnitListId
                                }));
                        }
                        if (lockDown.Region == LockDownType.Cell.ToString() && housingUnit.Count(w => w.FacilityId == value.FacilityId 
                             && w.HousingUnitListId == value.HousingUnitListId
                             && w.HousingUnitId == lockDown.SourceId) > 0)
                        {
                            lockHousingDetails.Add(new HousingDetail
                            {
                                HousingUnitBedNumber = housingUnit.First(f => f.HousingUnitId == lockDown.SourceId).HousingUnitBedNumber
                            });
                        }
                        if (lockDown.Region == LockDownType.CellGroup.ToString() && housingUnit.Count(w => w.FacilityId == value.FacilityId
                            && w.HousingUnitListId == value.HousingUnitListId
                            && w.HousingUnitListBedGroupId == lockDown.SourceId) > 0)
                        {
                            lockHousingDetails.AddRange(housingUnit.Where(w => w.FacilityId == value.FacilityId
                            && w.HousingUnitListId == value.HousingUnitListId
                            && w.HousingUnitListBedGroupId == lockDown.SourceId)
                                .Select(s => new HousingDetail
                                {
                                    HousingUnitBedNumber = s.HousingUnitBedNumber
                                }));
                        }
                    });
                    break;
            }
            return lockHousingDetails;
        }
    }
}
