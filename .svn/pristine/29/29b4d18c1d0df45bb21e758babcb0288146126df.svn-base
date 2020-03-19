using System;
using System.Collections.Generic;
using System.Linq;
using ServerAPI.ViewModels;
using GenerateTables.Models;
using ServerAPI.Utilities;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace ServerAPI.Services
{
    public class HousingCellService : IHousingCellService
    {
        private readonly AAtims _context;
        private readonly IHousingService _housingService;
        private readonly ILockdownService _lockdownService;
        private IQueryable<Inmate> _inmateList;
        private IQueryable<HousingUnit> _listHousingUnit;
        private readonly int _personnelId;
        private readonly IPhotosService _photos;

        public HousingCellService(AAtims context, IHousingService housingService,
            IHttpContextAccessor httpContextAccessor, IPhotosService photosService, ILockdownService lockdownService)
        {
            _context = context;
            _housingService = housingService;
            _lockdownService = lockdownService;
            _photos = photosService;
            _personnelId = Convert.ToInt32(httpContextAccessor.HttpContext.User
                .FindFirst(AuthDetailConstants.PERSONNELID)?.Value);
        }

        public CellViewerDetails GetCellViewerDetails(int facilityId, int housingUnitListId, int housingGroupId)
        {
            CellViewerDetails cellViewerDetails = new CellViewerDetails();
            _listHousingUnit = _context.HousingUnit.Where(w => w.FacilityId == facilityId);

            //To get inmate details
            _inmateList = _context.Inmate
                .Where(w => w.HousingUnitId.HasValue && w.FacilityId == facilityId && w.InmateActive == 1);
            List<int> housingUnitListIds = new List<int>();
            if (housingUnitListId > 0)
            {
                _listHousingUnit = _listHousingUnit.Where(w => w.HousingUnitListId == housingUnitListId);
                _inmateList = _inmateList.Where(w => w.HousingUnit.HousingUnitListId == housingUnitListId);
                housingUnitListIds.Add(housingUnitListId);
            }
            else
            {
                housingUnitListIds =
                    _context.HousingGroupAssign
                        .Where(w => w.HousingGroupId == housingGroupId && w.HousingUnitListId.HasValue && w.DeleteFlag != 1)
                        .Select(s => s.HousingUnitListId ?? 0).ToList();

                _listHousingUnit = _listHousingUnit.Where(w => housingUnitListIds.Contains(w.HousingUnitListId));
                _inmateList = _inmateList.Where(w => housingUnitListIds.Contains(w.HousingUnit.HousingUnitListId));
            }

            List<int> inmateLists = _inmateList.Select(s => s.HousingUnit.HousingUnitListId).ToList();

            //To get HousingUnt Location details
            cellViewerDetails.NumberCapacityList = _listHousingUnit
               .GroupBy(g => new { g.HousingUnitListId, g.HousingUnitNumber, g.HousingUnitLocation }, (key, group)
                  => new HousingCapacityVm
                  {
                      HousingNumber = key.HousingUnitNumber,
                      HousingUnitListId = key.HousingUnitListId,
                      HousingLocation = key.HousingUnitLocation,
                      CurrentCapacity = group.Sum(c => c.HousingUnitActualCapacity ?? 0),
                      OutofService = group.Sum(c => c.HousingUnitOutOfService ?? 0),
                      Assigned = inmateLists.Count(c => c == key.HousingUnitListId)
                  }).ToList();

            if (housingUnitListId > 0)
            {
                CellViewerDetails bedCellViewerDetails = GetBedNumber(facilityId, housingUnitListId);

                cellViewerDetails.BedNumberCapacityList = bedCellViewerDetails.BedNumberCapacityList.ToList();
                cellViewerDetails.HousingCellLockdownList = bedCellViewerDetails.HousingCellLockdownList.ToList();
            }

            //To get inmate list
            cellViewerDetails.InmateDetailsList = GetInmateList();
            //To get Housing stats count
            cellViewerDetails.HousingStatsDetails = _housingService.LoadHousingStatsCount(_inmateList);

            if (!(_listHousingUnit is null))
                cellViewerDetails.HousingHeaderDetails = GetHousingHeaderDetails();

            if (cellViewerDetails.HousingHeaderDetails?.Visitation != null)
            {
                cellViewerDetails.HousingVisitationDetails = GetHousingVisitationDetails(housingUnitListIds);
            }

            cellViewerDetails.HousingLockdownList = GetHousingLockdownNotifications(facilityId,housingUnitListIds).Distinct().ToList();

            return cellViewerDetails;
        }

        private CellViewerDetails GetBedNumber(int facilityId, int housingUnitListId)
        {
            CellViewerDetails cellViewerDetails = new CellViewerDetails();

            List<HousingUnitVm> lstHousingUnit = _listHousingUnit.Where(w => w.HousingUnitInactive == null || w.HousingUnitInactive == 0)
                .Select(s => new HousingUnitVm
                {
                    HousingUnitBedNumber = s.HousingUnitBedNumber,
                    HousingUnitNumber = s.HousingUnitNumber,
                    HousingUnitLocation = s.HousingUnitLocation,
                    HousingUnitActualCapacity = s.HousingUnitActualCapacity,
                    HousingUnitOutOfService = s.HousingUnitOutOfService
                }).ToList();

            List<HousingDetail> lstInmate = _inmateList.Select(s => new HousingDetail
            {
                HousingUnitListId = s.HousingUnit.HousingUnitListId,
                HousingUnitBedNumber = s.HousingUnit.HousingUnitBedNumber

            }).ToList();

            //To get HousingUnt Location details
            List<HousingCapacityVm> lstHousingBedNumber = lstHousingUnit
                .OrderBy(o => o.HousingUnitBedNumber)
                .GroupBy(g => new
                {
                    g.HousingUnitBedNumber,
                    g.HousingUnitNumber,
                    g.HousingUnitBedLocation
                }, (key, group)
                => new HousingCapacityVm
                {
                    HousingLocation = group.Select(s => s.HousingUnitLocation).FirstOrDefault(),
                    HousingBedNumber = key.HousingUnitBedNumber,
                    HousingNumber = key.HousingUnitNumber,
                    HousingBedLocation = key.HousingUnitBedLocation
                }).Distinct().ToList();

            List<HousingCapacityVm> housingCapacityList = lstHousingBedNumber
                .Where(w => w.HousingBedNumber != null)
                .Select(s => new HousingCapacityVm
                {
                    FacilityId= facilityId,
                    HousingNumber = s.HousingNumber,
                    HousingBedNumber = s.HousingBedNumber,
                    HousingUnitListId = housingUnitListId,
                    HousingLocation = s.HousingLocation,
                    CurrentCapacity = lstHousingUnit
                        .Where(x => x.HousingUnitBedNumber == s.HousingBedNumber)
                        .Sum(c => c.HousingUnitActualCapacity ?? 0),
                    OutofService = lstHousingUnit
                        .Where(x => x.HousingUnitBedNumber == s.HousingBedNumber)
                        .Sum(c => c.HousingUnitOutOfService ?? 0),
                    Assigned = lstInmate.Count(c => c.HousingUnitListId == housingUnitListId
                                                    && c.HousingUnitBedNumber == s.HousingBedNumber)
                }).ToList();
            cellViewerDetails.BedNumberCapacityList = housingCapacityList;
            cellViewerDetails.HousingCellLockdownList = new List<HousingDetail>();
            if (housingCapacityList.Count > 0)
            {
                HousingLockInputVm podLockInputVm = new HousingLockInputVm()
                {
                    HousingType = HousingType.BedNumber,
                    FacilityId = facilityId,
                    HousingLocation = _context.HousingUnitList.FirstOrDefault(w => w.HousingUnitListId == housingUnitListId)?.HousingUnitLocation,
                    HousingUnitListId = housingUnitListId
                };
                cellViewerDetails.HousingCellLockdownList = _lockdownService.GetHousingLockdownNotifications(podLockInputVm).ToList();
            }

            return cellViewerDetails;
        }

        public CellViewerDetails GetBedNumberDetails(int facilityId, int housingUnitListId, string housingBedNumber)
        {
            CellViewerDetails cellViewerDetails = new CellViewerDetails();
            _listHousingUnit = _context.HousingUnit.Where(w =>
                w.FacilityId == facilityId && w.HousingUnitListId == housingUnitListId);

            //To get inmate details
            _inmateList = _context.Inmate
                .Where(w => w.HousingUnitId.HasValue && w.FacilityId == facilityId
                            && w.HousingUnit.HousingUnitListId == housingUnitListId && w.InmateActive == 1);

            if (housingBedNumber != null)
            {
                _listHousingUnit = _listHousingUnit.Where(w => w.HousingUnitBedNumber == housingBedNumber);
                _inmateList = _inmateList.Where(w => w.HousingUnit.HousingUnitBedNumber == housingBedNumber);
            }
            else
            {
                CellViewerDetails bedCellViewerDetails = GetBedNumber(facilityId, housingUnitListId);
                cellViewerDetails.BedNumberCapacityList = bedCellViewerDetails.BedNumberCapacityList;
                cellViewerDetails.HousingCellLockdownList = bedCellViewerDetails.HousingCellLockdownList.ToList();
            }
            cellViewerDetails.InmateDetailsList = GetInmateList();
            //To get Housing stats count
            cellViewerDetails.HousingStatsDetails = _housingService.LoadHousingStatsCount(_inmateList);

            if (!(_listHousingUnit is null))
                cellViewerDetails.HousingHeaderDetails = GetHousingHeaderDetails();

            if (cellViewerDetails.HousingHeaderDetails?.Visitation != null)
            {
                List<int> housingUnitListIds = new List<int> { housingUnitListId };
                cellViewerDetails.HousingVisitationDetails = GetHousingVisitationDetails(housingUnitListIds);
            }

            return cellViewerDetails;
        }

        //To get inmate list
        private List<InmateSearchVm> GetInmateList()
        {
            List<InmateSearchVm> inmateList = _inmateList
                .Where(w => w.HousingUnitId.HasValue)
                .Select(s => new InmateSearchVm
                {
                    InmateId = s.InmateId,
                    InmateNumber = s.InmateNumber,
                    LocationId = s.InmateCurrentTrackId,
                    Location = s.InmateCurrentTrack,
                    HousingDetail = new HousingDetail
                    {
                        HousingUnitId = s.HousingUnit.HousingUnitId,
                        HousingUnitListId = s.HousingUnit.HousingUnitListId,
                        HousingUnitLocation = s.HousingUnit.HousingUnitLocation,
                        HousingUnitNumber = s.HousingUnit.HousingUnitNumber,
                        HousingUnitBedNumber = s.HousingUnit.HousingUnitBedNumber,
                        HousingUnitBedLocation = s.HousingUnit.HousingUnitBedLocation
                    },
                    PersonDetail = new PersonInfoVm
                    {
                        PersonLastName = s.Person.PersonLastName,
                        PersonMiddleName = s.Person.PersonMiddleName,
                        PersonFirstName = s.Person.PersonFirstName,
                        PersonId = s.PersonId
                    },
                    PhotoFilePath =_photos.GetPhotoByCollectionIdentifier(s.Person.Identifiers)
                }).ToList();
            return inmateList;
        }

        public List<HousingUnitVm> GetOutOfServiceDetails(int housingUnitListId, int facilityId)
        {
            //To get inmate details
            _inmateList = _context.Inmate
                .Where(w => w.HousingUnitId.HasValue && w.FacilityId == facilityId && w.InmateActive == 1
                            && w.HousingUnit.HousingUnitListId == housingUnitListId);
            //get housing unit details
            List<HousingUnitVm> lstHousingCapacity = _context.HousingUnit
                .Where(w => w.HousingUnitListId == housingUnitListId && w.FacilityId == facilityId)
                .Select(s => new HousingUnitVm
                {
                    HousingUnitId = s.HousingUnitId,
                    HousingUnitBedNumber = s.HousingUnitBedNumber,
                    HousingUnitNumber = s.HousingUnitNumber,
                    HousingUnitBedLocation = s.HousingUnitBedLocation,
                    HousingUnitActualCapacity = s.HousingUnitActualCapacity,
                    HousingUnitOutOfService = s.HousingUnitOutOfService,
                    HousingUnitOutOfserviceReason = s.HousingUnitOutOfServiceReason,
                    HousingUnitOutOfServiceNote = s.HousingUnitOutOfServiceNote,
                    HousingUnitOutOfServiceBy = s.HousingUnitOutOfServiceBy,
                    HousingUnitOutOfServiceDate = s.HousingUnitOutOfServiceDate,
                    AssignedInmate = _context.Inmate.Count(c => c.InmateActive == 1 && c.HousingUnitId == s.HousingUnitId),
                    Assigned = s.HousingUnitBedNumber == null
                        ? _inmateList.Count()
                        : _inmateList.Count(w =>
                            w.HousingUnit.HousingUnitBedNumber == s.HousingUnitBedNumber
                            && w.HousingUnit.HousingUnitBedLocation == s.HousingUnitBedLocation &&
                            (w.HousingUnit.HousingUnitInactive == 0 || w.HousingUnit.HousingUnitInactive == null)),
                    PersonnelDetail = new PersonnelVm
                    {
                        PersonLastName = s.HousingUnitOutOfServiceByNavigation.PersonNavigation.PersonLastName,
                        OfficerBadgeNumber = s.HousingUnitOutOfServiceByNavigation.OfficerBadgeNum,
                    }
                }).ToList();

            return lstHousingCapacity;
        }

        public OutOfServiceHistory GetOutOfServiceHistory(HousingInputVm housingInput)
        {
            IQueryable<HousingUnitOosHistory> lstHousingOosHistory = _context.HousingUnitOosHistory
                .Where(w => w.HousingUnitId == w.HousingUnit.HousingUnitId);

            if (housingInput.HousingUnitListId > 0)
            {
                lstHousingOosHistory = lstHousingOosHistory.Where(w => w.HousingUnit.HousingUnitListId == housingInput.HousingUnitListId);
            }

            if (housingInput.HousingBedNumber != null)
            {
                lstHousingOosHistory = lstHousingOosHistory
                    .Where(w => w.HousingUnit.HousingUnitBedNumber == housingInput.HousingBedNumber);
            }
            if (housingInput.OfficerId > 0)
            {
                lstHousingOosHistory = lstHousingOosHistory
                    .Where(w => w.OutOfServiceOfficer == housingInput.OfficerId);
            }
            if (housingInput.HousingLocation != null)
            {
                lstHousingOosHistory = lstHousingOosHistory
                    .Where(w => w.HousingUnit.HousingUnitNumber == housingInput.HousingLocation);
            }
            if (housingInput.FromDate.HasValue && housingInput.ThruDate.HasValue)
            {
                lstHousingOosHistory = lstHousingOosHistory
                    .Where(w => w.OutOfServiceDate.HasValue && w.OutOfServiceDate.Value.Date >= housingInput.FromDate &&
                                w.OutOfServiceDate.Value.Date <= housingInput.ThruDate);
            }

            OutOfServiceHistory outOfServiceHistory = new OutOfServiceHistory
            {
                HousintUnitList = lstHousingOosHistory
                    .Select(s => new HousingUnitVm
                    {
                        HousingUnitBedNumber = s.HousingUnit.HousingUnitBedNumber,
                        HousingUnitNumber = s.HousingUnit.HousingUnitNumber,
                        HousingUnitBedLocation = s.HousingUnit.HousingUnitBedLocation,
                        HousingUnitOutOfService = s.OutOfServiceCount,
                        HousingUnitOutOfserviceReason = s.OutOfServiceReason,
                        HousingUnitOutOfServiceNote = s.OutOfServiceNote,
                        HousingUnitOutOfServiceBy = s.OutOfServiceOfficer,
                        HousingUnitOutOfServiceDate = s.OutOfServiceDate
                    }).Take(!housingInput.FromDate.HasValue && !housingInput.ThruDate.HasValue
                        ? 100
                        : lstHousingOosHistory.Count()).OrderByDescending(o => o.HousingUnitOutOfServiceDate).ToList()
            };

            int[] arrPersonnelId = outOfServiceHistory.HousintUnitList.Select(s => s.HousingUnitOutOfServiceBy ?? 0).ToArray();
            List<PersonnelVm> lstPersonDetails = _context.Personnel.Where(w => arrPersonnelId.Contains(w.PersonnelId))
                .Select(s => new PersonnelVm
                {
                    PersonLastName = s.PersonNavigation.PersonLastName,
                    OfficerBadgeNumber = s.OfficerBadgeNum,
                    PersonnelId = s.PersonnelId
                }).ToList();

            outOfServiceHistory.HousintUnitList.ForEach(item =>
            {
                item.PersonnelDetail = new PersonnelVm
                {
                    PersonLastName = lstPersonDetails.SingleOrDefault(s => s.PersonnelId == item.HousingUnitOutOfServiceBy)?.PersonLastName,
                    OfficerBadgeNumber = lstPersonDetails.SingleOrDefault(s => s.PersonnelId == item.HousingUnitOutOfServiceBy)?.OfficerBadgeNumber
                };
            });

            //get all bed number list from housing unit
            outOfServiceHistory.BedNumberList = _context.HousingUnit.Where(w => w.HousingUnitBedNumber != null && w.HousingUnitListId == housingInput.HousingUnitListId)
                .OrderBy(o => o.HousingUnitBedNumber).Select(s => s.HousingUnitBedNumber).Distinct().ToArray();
            return outOfServiceHistory;
        }

        public async Task<int> UpdateOutOfService(HousingUnitVm housingDet)
        {
            //update outofservice details in housing unit table
            HousingUnit dbHousingUnit =
                _context.HousingUnit.SingleOrDefault(w => w.HousingUnitId == housingDet.HousingUnitId);
            if (dbHousingUnit is null) return 0;
            dbHousingUnit.HousingUnitOutOfService = housingDet.HousingUnitOutOfService;
            dbHousingUnit.HousingUnitOutOfServiceNote = housingDet.HousingUnitOutOfService > 0 ? housingDet.HousingUnitOutOfServiceNote : null;
            dbHousingUnit.HousingUnitOutOfServiceReason = housingDet.HousingUnitOutOfserviceReason;
            dbHousingUnit.HousingUnitOutOfServiceBy = housingDet.HousingUnitOutOfService > 0 ? _personnelId : default(int?);
            dbHousingUnit.HousingUnitOutOfServiceDate = housingDet.HousingUnitOutOfService > 0 ? DateTime.Now : new DateTime?();

            //insert into outofservice history table
            HousingUnitOosHistory dbOosHistory = new HousingUnitOosHistory
            {
                HousingUnitId = housingDet.HousingUnitId,
                OutOfServiceCount = housingDet.HousingUnitOutOfService,
                OutOfServiceReason = housingDet.HousingUnitOutOfserviceReason,
                OutOfServiceOfficer = _personnelId,
                OutOfServiceDate = DateTime.Now,
                OutOfServiceNote = housingDet.HousingUnitOutOfServiceNote
            };
            _context.HousingUnitOosHistory.Add(dbOosHistory);

            return await _context.SaveChangesAsync();

        }

        private List<IssuedPropertyVm> GetIssuedProperty(List<int> housingUnitListIds, string housingBedNumber)
        {
            IQueryable<IssuedProperty> qryIssuedProperty = _context.IssuedProperty
                .Where(w => w.DeleteFlag == 0 && w.ActiveFlag.HasValue &&
                            housingUnitListIds.Contains(w.Inmate.HousingUnit.HousingUnitListId));
            if (housingBedNumber != null)
            {
                qryIssuedProperty =
                    qryIssuedProperty.Where(w => w.Inmate.HousingUnit.HousingUnitBedNumber == housingBedNumber);
            }

            List<IssuedPropertyVm> issuedPropertyList = qryIssuedProperty
                .Select(s => new IssuedPropertyVm
                {
                    PropertyName = s.IssuedPropertyLookup.PropertyName,
                    PropertyLookupId = s.IssuedPropertyLookupId,
                    HousingDetails = new HousingUnitVm
                    {
                        HousingUnitLocation = s.Inmate.HousingUnit.HousingUnitLocation,
                        HousingUnitNumber = s.Inmate.HousingUnit.HousingUnitNumber,
                        HousingUnitBedNumber = s.Inmate.HousingUnit.HousingUnitBedNumber,
                        HousingUnitBedLocation = s.Inmate.HousingUnit.HousingUnitBedLocation
                    },
                    Person = new PersonInfoVm
                    {
                        PersonLastName = s.Inmate.Person.PersonLastName,
                        PersonFirstName = s.Inmate.Person.PersonFirstName,
                        PersonMiddleName = s.Inmate.Person.PersonMiddleName,
                        PersonSuffix = s.Inmate.Person.PersonSuffix
                    },
                    Count = s.IssuedCount,
                    Note = s.IssueNote,
                    IssuedNumber = s.IssueNumber
                }).ToList();

            List<IssuedPropertyVm> issuedPropertyCountList = issuedPropertyList.GroupBy(g => new
            {
                g.PropertyLookupId,
                g.PropertyName,
                g.HousingDetails.HousingUnitLocation,
                g.HousingDetails.HousingUnitNumber
            }).Select(s => new IssuedPropertyVm
            {
                PropertyLookupId = s.Key.PropertyLookupId,
                PropertyName = s.Key.PropertyName,
                HousingDetails = new HousingUnitVm
                {
                    HousingUnitLocation = s.Key.HousingUnitLocation,
                    HousingUnitNumber = s.Key.HousingUnitNumber
                },
                Count = s.Count(),
                IssuedPropertyList = s.ToList()
            }).ToList();

            return issuedPropertyCountList;
        }

        private List<HousingLibraryVm> GetHousingLibrary(List<int> housingUnitListIds)
        {
            List<HousingLibraryVm> lstLibrary = _context.LibraryBook
                .Where(w => w.LibraryRoom.FacilityId == 1
                            && w.DeleteFlag == 0 &&
                            w.LibraryRoom.DeleteFlag == 0
                            && w.CurrentCheckoutInmateId.HasValue
                            && housingUnitListIds.Contains(w.CurrentCheckoutInmate.HousingUnit.HousingUnitListId))
                .Select(s => new HousingLibraryVm
                {
                    Person = new PersonInfoVm
                    {
                        InmateId = s.CurrentCheckoutInmateId,
                        InmateNumber = s.CurrentCheckoutInmate.InmateNumber,
                        PersonLastName = s.CurrentCheckoutInmate.Person.PersonLastName,
                        PersonFirstName = s.CurrentCheckoutInmate.Person.PersonFirstName,
                        PersonMiddleName = s.CurrentCheckoutInmate.Person.PersonMiddleName
                    },
                    HousingDetail = new HousingDetail
                    {
                        HousingUnitLocation = s.CurrentCheckoutInmate.HousingUnit.HousingUnitLocation,
                        HousingUnitNumber = s.CurrentCheckoutInmate.HousingUnit.HousingUnitNumber,
                        HousingUnitBedNumber = s.CurrentCheckoutInmate.HousingUnit.HousingUnitBedNumber,
                        HousingUnitBedLocation = s.CurrentCheckoutInmate.HousingUnit.HousingUnitBedLocation
                    },
                    Late = !s.CurrentSchdReturn.HasValue && s.CurrentSchdReturn < DateTime.Now ? 1 : 0
                }).ToList();

            List<HousingLibraryVm> housingLibrary = lstLibrary.GroupBy(g => g.Person.InmateId).Select(s =>
                new HousingLibraryVm
                {
                    Person = s.Select(se => se.Person).FirstOrDefault(),
                    HousingDetail = s.Select(se => se.HousingDetail).FirstOrDefault(),
                    Check = s.Count(),
                    Late = s.Sum(se => se.Late)
                }).ToList();

            return housingLibrary;
        }

        public CellPropertyLibraryDetails GetPropertyLibraryDetails(int housingUnitListId, int housingGroupId, string housingBedNumber)
        {
            CellPropertyLibraryDetails cellPropertyLibraryDetails = new CellPropertyLibraryDetails();
            List<int> housingUnitListIds = new List<int>();
            if (housingGroupId > 0)
            {
                housingUnitListIds =
                    _context.HousingGroupAssign
                        .Where(w => w.HousingGroupId == housingGroupId && w.HousingUnitListId.HasValue)
                        .Select(s => s.HousingUnitListId ?? 0).ToList();
            }
            else
            {
                housingUnitListIds.Add(housingUnitListId);
            }
            cellPropertyLibraryDetails.IssuedPropertyList = GetIssuedProperty(housingUnitListIds, housingBedNumber);
            cellPropertyLibraryDetails.LibraryList = GetHousingLibrary(housingUnitListIds);
            return cellPropertyLibraryDetails;
        }

        //To get housing header details
        private HousingHeaderVm GetHousingHeaderDetails() => _listHousingUnit
            .Select(s => new HousingHeaderVm
            {
                Status = s.Facility.DeleteFlag,
                Gender = _context.Lookup
                    .Where(w => w.LookupIndex == s.HousingUnitSex
                                && w.LookupType == LookupConstants.SEX)
                    .Select(se => se.LookupDescription).FirstOrDefault(),
                Floor = s.HousingUnitFloor,
                Offsite = s.HousingUnitOffsite,
                Medical = s.HousingUnitMedical,
                Mental = s.HousingUnitMental,
                Visitation = s.HousingUnitVisitAllow,
                Commission = s.HousingUnitCommAllow
            }).FirstOrDefault();

        //To get housing visitation details
        private List<HousingVisitationVm> GetHousingVisitationDetails(List<int> housingUnitListIds) =>
            _context.HousingUnitVisitation
                .Where(w => !w.DeleteFlag.HasValue && housingUnitListIds.Contains(w.HousingUnitListId ?? 0))
                .Select(s => new HousingVisitationVm
                {
                    VisitDay = s.VisitationDay,
                    VisitFrom = s.VisitationFrom,
                    VisitTo = s.VisitationTo
                }).ToList();

        private List<HousingDetail> GetHousingLockdownNotifications(int facilityId, List<int> housingUnitListIds)
        {
            List<HousingDetail> lockHousingDetails = new List<HousingDetail>();

            List<LockdownVm> housingLockdown = _context.HousingLockdown.Where(w =>
           w.StartLockdown <= DateTime.Now && w.EndLockdown >= DateTime.Now && !w.DeleteFlag)
            .Select(s => new LockdownVm
            {
                Region = s.LockdownSource,
                SourceId = s.SourceId
            }).ToList();

            if (housingLockdown.Count(w =>
                    w.Region == LockDownType.Facility.ToString() && w.SourceId == facilityId) > 0)
            {
                lockHousingDetails.Add(new HousingDetail()
                {
                    FacilityId = facilityId
                });
            }
            else
            {
                List<HousingUnitList> housingUnit = _context.HousingUnitList
                    .Where(w => w.FacilityId == facilityId).ToList();
                List<HousingGroupAssign> housingGroupAssign = _context.HousingGroupAssign
                    .Where(w => w.HousingGroup.FacilityId == facilityId && w.HousingUnitLocation != null && w.DeleteFlag==0).ToList();
                housingLockdown.ForEach(lockDown =>
                {
                    if (lockDown.Region == LockDownType.Building.ToString() && housingUnit.Count(f => f.FacilityId == facilityId
                                                                                                      && f.HousingUnitListId == lockDown.SourceId) > 0)
                    {
                        string housingUnitLocation = housingUnit
                            .First(f => f.HousingUnitListId == lockDown.SourceId).HousingUnitLocation;

                        lockHousingDetails.AddRange(housingUnit.Where(w => w.HousingUnitLocation == housingUnitLocation)
                            .Select(s => new HousingDetail
                            {
                                HousingUnitListId = s.HousingUnitListId
                            }));
                    }
                    if (lockDown.Region == LockDownType.Pod.ToString() && housingUnit.Count(f => f.FacilityId == facilityId
                                                                                                 && f.HousingUnitListId == lockDown.SourceId) > 0)
                    {
                        lockHousingDetails.Add(new HousingDetail()
                        {
                            HousingUnitListId = lockDown.SourceId
                        });
                    }
                    if (lockDown.Region == LockDownType.HousingGroup.ToString() && housingGroupAssign.Count(w =>
                            w.HousingGroupId == lockDown.SourceId) > 0)
                    {
                        lockHousingDetails.AddRange(housingGroupAssign.Where(w => w.HousingGroupId == lockDown.SourceId)
                            .Select(s => new HousingDetail
                            {
                                HousingUnitListId = s.HousingUnitListId
                            }));
                    }
                });
            }
            return lockHousingDetails;
        }
    }
}
