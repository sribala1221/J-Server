using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GenerateTables.Models;
using ServerAPI.ViewModels;
using Microsoft.AspNetCore.Http;

// ReSharper disable once CheckNamespace
namespace ServerAPI.Services
{
    public class InventoryLostFoundService : IInventoryLostFoundService
    {
        private readonly AAtims _context;
        private readonly IInventoryInmateService _inventoryInmateService;
        private readonly int _personnelId;
        private List<HousingUnit> _housingUnitLst;
        private List<Privileges> _privilegesLst;
        private readonly IPhotosService _photos;

        #region Inventory Lost Found Details(Get)

        public InventoryLostFoundService(AAtims context, IHttpContextAccessor httpContextAccessor,
            IInventoryInmateService inventoryInmateService, IPhotosService photosService)
        {
            _context = context;
            _inventoryInmateService = inventoryInmateService;
            _personnelId = Convert.ToInt32(httpContextAccessor.HttpContext.User.FindFirst("personnelId")?.Value);
            _photos = photosService;

        }
        // Main grid details get method    
        public LostFoundInventory GetLostFoundInventory(SearchOptionsView lostFoundFilters)
        {
            List<InventoryDetails> details = LostFoundInventoryDetail(lostFoundFilters);
            LostFoundInventory lostFoundInventory = new LostFoundInventory
            {
                InventoryGrid = details,
                PropertyType = PropertyTypeList(details),
                LostFoundBins = InventoryLostCount(details, lostFoundFilters.DeleteFlag)
            };

            //Filters DD's
            // Housing Details

            IQueryable<HousingUnit> housingUnit =
                _context.HousingUnit.Where(hu => !hu.HousingUnitInactive.HasValue || hu.HousingUnitInactive == 0);
            _housingUnitLst = lostFoundFilters.GlobalFacilityId > 0
                ? housingUnit.Where(hu => hu.FacilityId == lostFoundFilters.GlobalFacilityId).ToList()
                : housingUnit.ToList();

            //location
            lostFoundInventory.HousingUnitLoc = _housingUnitLst.Select(s => s.HousingUnitLocation.Trim()).OrderBy(o => o).Distinct().ToList();

            //number
            if (lostFoundFilters.Building != null)
            {
                _housingUnitLst = _housingUnitLst.Where(hu =>
                    !string.IsNullOrWhiteSpace(hu.HousingUnitLocation) &&
                    hu.HousingUnitLocation.Trim() == lostFoundFilters.Building).ToList();

                lostFoundInventory.HousingUnitNum = _housingUnitLst
                .GroupBy(g => new { g.HousingUnitListId, g.HousingUnitLocation, g.HousingUnitNumber })
                .Select(s => new HousingDropDown
                {
                    HousingUnitListId = s.Key.HousingUnitListId,
                    HousingUnitLocation = s.Key.HousingUnitLocation,
                    HousingUnitNumber = s.Key.HousingUnitNumber
                }).OrderBy(o => o.HousingUnitLocation).ThenBy(t => t.HousingUnitNumber).Distinct().ToList();
            }
            InventoryLookupVm inventoryLookup = _inventoryInmateService.GetLookupDetails();
            // To get HousingUnitLocation by facilityId           
            lostFoundInventory.InventoryLookupVm = inventoryLookup;
            lostFoundInventory.Location = Location(lostFoundFilters.GlobalFacilityId);
            //Location Details
            IQueryable<Privileges> privileges =
                _context.Privileges.Where(p => p.InactiveFlag == 0 && p.RemoveFromTrackingFlag == 0);
            _privilegesLst = lostFoundFilters.GlobalFacilityId > 0 ? privileges.Where(p =>
                    p.FacilityId == lostFoundFilters.GlobalFacilityId || !p.FacilityId.HasValue || p.FacilityId == 0).ToList()
                : privileges.Where(p => !p.FacilityId.HasValue || p.FacilityId == 0 || true).ToList();
            lostFoundInventory.LocationDropdown = _privilegesLst
                .Select(pl => new Privilege { Id = pl.PrivilegeId, Description = pl.PrivilegeDescription })
                .ToList();
            return lostFoundInventory;
        }


        //To get HousingUnitNumber by facilityId & housing Unit location for dropdown
        public List<HousingDetail> GetHousingUnitNumberDetails(int facilityId, string housingUnitLocation) =>
            _context.HousingUnit
                .Where(hu => hu.FacilityId == facilityId
                && hu.HousingUnitLocation == housingUnitLocation)
                .GroupBy(g => new { g.HousingUnitLocation, g.HousingUnitNumber, g.HousingUnitListId })
                .Select(hu => new HousingDetail
                {
                    HousingUnitNumber = hu.Key.HousingUnitNumber,
                    HousingUnitLocation = hu.Key.HousingUnitLocation,
                    HousingUnitListId = hu.Key.HousingUnitListId
                }).ToList();

        //Identity pop-up get method
        public InventoryLostFoundVm ItemList(int groupId) =>
            _context.PersonalInventoryGroup.Where(i => i.PersonalInventoryGroupId == groupId)
                 .Select(s => new InventoryLostFoundVm
                 {
                     LostFoundByPersonnelId = s.LostFoundByPersonnelId,
                     LostFoundByInmateId = s.LostFoundByInmateId,
                     LostFoundByOther = s.LostFoundByOther,
                     LostFountDate = s.LostFoundDate,
                     LostFoundLocFacilityId = s.LostFoundLocFacilityId,
                     HousingUnitLocation = s.LostFoundLocHousingUnitLocation,
                     HousingUnitNumber = s.LostFoundLocHousingUnitNumber,
                     LostFoundLocPrivilegeId = s.LostFoundLocPrivilegeId ?? 0,
                     LostFoundLocOther = s.LostFoundLocOther,
                     LostFoundCircumstance = s.LostFoundCircumstance
                 }).Single();

        //Identity pop-up save method 
        public async Task<int> InsertInventory(IdentitySave value)
        {
            PersonalInventoryGroup details = _context.PersonalInventoryGroup
                .Single(w => w.PersonalInventoryGroupId == value.PersonalInventoryGroupId);
            _context.PersonalInventoryGroupHistory.Add(new PersonalInventoryGroupHistory
            {
                LostFoundLocPrivilege = details.LostFoundLocPrivilege,
                PersonalInventoryGroupId = details.PersonalInventoryGroupId,
                CreateDate = details.CreateDate,
                CreateBy = details.CreateBy,
                GroupNote = details.GroupNote,
                DeleteFlag = details.DeleteFlag,
                LostFoundByPersonnelId = details.LostFoundByPersonnelId > 0 ? details.LostFoundByPersonnelId : null,
                LostFoundByInmateId = details.LostFoundByInmateId > 0 ? details.LostFoundByInmateId : null,
                LostFoundByOther = details.LostFoundByOther,
                LostFoundDate = details.LostFoundDate,
                LostFoundLocFacilityId = details.LostFoundLocFacilityId,
                LostFoundLocHousingUnitLocation = details.LostFoundLocHousingUnitLocation,
                LostFoundLocHousingUnitNumber = details.LostFoundLocHousingUnitNumber,
                LostFoundLocPrivilegeId = details.LostFoundLocPrivilegeId,
                LostFoundLocOther = details.LostFoundLocOther,
                LostFoundCircumstance = details.LostFoundCircumstance,
                LostFoundIdentifiedBy = details.LostFoundIdentifiedBy,
                LostFoundIdentifiedDate = details.LostFoundIdentifiedDate,
                InmateId = value.AppliedInmateId
            });
            details.LostFoundIdentified = true;
            details.InmateId = value.AppliedInmateId;
            details.LostFoundIdentifiedDate = DateTime.Now;
            details.LostFoundIdentifiedBy = _personnelId;
            IQueryable<PersonalInventory> personalInventories = _context.PersonalInventory
                .Where(a => value.PersonalInventoryId.Contains(a.PersonalInventoryId));
            _context.PersonalInventoryHistory.AddRange(personalInventories.
                Select(s => new PersonalInventoryHistory
                {
                    PersonalInventoryId = s.PersonalInventoryId,
                    InventoryDate = s.InventoryDate,
                    InventoryArticles = s.InventoryArticles,
                    InventoryQuantity = s.InventoryQuantity,
                    InventoryUom = s.InventoryUom,
                    InventoryDescription = s.InventoryDescription,
                    InventoryDispositionCode = s.InventoryDispositionCode,
                    InventoryValue = s.InventoryValue,
                    InventoryDestroyed = s.InventoryDestroyed,
                    InventoryMailed = s.InventoryMailed,
                    InventoryMailPersonId = s.InventoryMailPersonId,
                    InventoryMailAddressId = s.InventoryMailAddressId,
                    PersonName = s.PersonName,
                    PersonIdType = s.PersonIdType,
                    UpdatedBy = s.UpdatedBy,
                    DeleteFlag = s.DeleteFlag,
                    DeleteDate = s.DeleteDate,
                    DeletedBy = s.DeletedBy,
                    PersonAddress = s.PersonAddress,
                    DispoNotes = s.DispoNotes,
                    InventoryColor = s.InventoryColor,
                    InventoryBinNumber = s.InventoryBinNumber,
                    CityStateZip = s.CityStateZip,
                    DeleteReason = s.DeleteReason,
                    DeleteReasonNote = s.DeleteReasonNote,
                    InventoryEvidencePersonnelId = s.InventoryEvidencePersonnelId,
                    InventoryEvidenceAgencyId = s.InventoryEvidenceAgencyId,
                    InventoryEvidenceCaseNumber = s.InventoryEvidenceCaseNumber,
                    InventoryReturnDate = s.InventoryReturnDate,
                    CreateDate = s.CreateDate,
                    CreatedBy = s.CreatedBy,
                    UpdateDate = s.UpdateDate,
                    PersonalInventoryBinId = s.PersonalInventoryBinId ?? 0,
                    PersonalInventoryGroupId = s.PersonalInventoryGroupId,
                    InventoryOfficerId = s.InventoryOfficerId,
                    InmateId = value.AppliedInmateId
                }));
            personalInventories.ToList().ForEach(s => s.InmateId = value.AppliedInmateId);
            return await _context.SaveChangesAsync();

        }

        //Delete inventory lost found 
        public async Task<int> DeleteOrUndoLostFound(InventoryDetails inventoryDetails)
        {
            PersonalInventory deleteInventory = _context.PersonalInventory.Single(s =>
            s.PersonalInventoryId == inventoryDetails.PersonalInventoryId);
            deleteInventory.DeleteFlag = inventoryDetails.DeleteFlag;
            if (inventoryDetails.DeleteFlag > 0)
            {
                deleteInventory.DeleteReason = inventoryDetails.DeleteReason;
                deleteInventory.DeleteReasonNote = inventoryDetails.DeleteReasonNote;
                deleteInventory.DeleteDate = DateTime.Now;
                deleteInventory.DeletedBy = _personnelId;
                _context.PersonalInventoryHistory.Add(new PersonalInventoryHistory
                {
                    PersonalInventoryId = deleteInventory.PersonalInventoryId,
                    InmateId = deleteInventory.InmateId,
                    InventoryDate = deleteInventory.InventoryDate,
                    InventoryArticles = deleteInventory.InventoryArticles,
                    InventoryQuantity = deleteInventory.InventoryQuantity,
                    InventoryUom = deleteInventory.InventoryUom,
                    InventoryDescription = deleteInventory.InventoryDescription,
                    InventoryDispositionCode = deleteInventory.InventoryDispositionCode,
                    InventoryValue = deleteInventory.InventoryValue,
                    InventoryDestroyed = deleteInventory.InventoryDestroyed,
                    InventoryMailed = deleteInventory.InventoryMailed,
                    InventoryMailPersonId = deleteInventory.InventoryMailPersonId,
                    InventoryMailAddressId = deleteInventory.InventoryMailAddressId,
                    InventoryOfficerId = deleteInventory.InventoryOfficerId,
                    InventoryColor = deleteInventory.InventoryColor,
                    InventoryBinNumber = deleteInventory.InventoryBinNumber,
                    InventoryReturnDate = deleteInventory.InventoryReturnDate,
                    CreateDate = deleteInventory.CreateDate,
                    CreatedBy = deleteInventory.CreatedBy,
                    UpdateDate = deleteInventory.UpdateDate,
                    PersonalInventoryBinId = deleteInventory.PersonalInventoryBinId ?? 0,
                    PersonName = deleteInventory.PersonName,
                    PersonIdType = deleteInventory.PersonIdType,
                    PersonAddress = deleteInventory.PersonAddress,
                    DispoNotes = deleteInventory.DispoNotes,
                    UpdatedBy = deleteInventory.UpdatedBy,
                    DeleteFlag = deleteInventory.DeleteFlag,
                    DeleteDate = deleteInventory.DeleteDate,
                    DeletedBy = deleteInventory.DeletedBy,
                    CityStateZip = deleteInventory.CityStateZip,
                    PersonalInventoryGroupId = deleteInventory.PersonalInventoryGroupId,
                    DeleteReason = deleteInventory.DeleteReason,
                    DeleteReasonNote = deleteInventory.DeleteReasonNote,
                    InventoryEvidencePersonnelId = deleteInventory.InventoryEvidencePersonnelId,
                    InventoryEvidenceAgencyId = deleteInventory.InventoryEvidenceAgencyId,
                    InventoryEvidenceCaseNumber = deleteInventory.InventoryEvidenceCaseNumber
                });
            }
            else
            {
                //deleteInventory.DeleteFlag = 0;
                deleteInventory.DeleteReason = null;
                deleteInventory.DeleteReasonNote = null;
                deleteInventory.DeleteDate = null;
                deleteInventory.DeletedBy = null;
            }

            return await _context.SaveChangesAsync();
        }

        //Location dropdown 
        private List<KeyValuePair<int, string>> Location(int facilityId) => _context.Privileges.Where(w =>
       w.FacilityId == facilityId && w.InactiveFlag == 0).Select(s =>
       new KeyValuePair<int, string>(s.PrivilegeId, s.PrivilegeDescription)).ToList();

        private List<InventoryDetails> LostFoundInventoryDetail(SearchOptionsView search)
        {
            IQueryable<PersonalInventory> lostFoundInventoryDetails = _context.PersonalInventory
                .Where(p => (p.PersonalInventoryBin.FacilityId == search.GlobalFacilityId || !p.PersonalInventoryBin.FacilityId.HasValue)
                && p.PersonalInventoryBinId.HasValue && (search.DeleteFlag || p.DeleteFlag == 0)
            && !p.InventoryReturnDate.HasValue && (search.GroupId == 0 || p.PersonalInventoryGroupId == search.GroupId)
              && (search.FoundPersonalId == 0
            || p.PersonalInventoryGroup.LostFoundByPersonnelId == search.FoundPersonalId)
            && ((search.DispositionCode == 0 || search.DispositionCode == null) || p.InventoryDispositionCode == search.DispositionCode)
            && (search.FacilityId == 0 || p.PersonalInventoryGroup.LostFoundLocFacilityId == search.FacilityId)
            && (search.BinId == 0 || p.PersonalInventoryBinId == search.BinId)
            && (string.IsNullOrEmpty(search.Number)
            || p.PersonalInventoryGroup.LostFoundLocHousingUnitNumber.Contains(search.Number))
            && (string.IsNullOrEmpty(search.Building)
            || p.PersonalInventoryGroup.LostFoundLocHousingUnitLocation.Contains(search.Building))
            && (string.IsNullOrEmpty(search.Color)
            || p.InventoryColor.Contains(search.Color))
            && (string.IsNullOrEmpty(search.DiscriptionKey1)
            || p.InventoryDescription.Contains(search.DiscriptionKey1))
            && (string.IsNullOrEmpty(search.DiscriptionKey2)
            || p.InventoryDescription.Contains(search.DiscriptionKey2))
            && (string.IsNullOrEmpty(search.DiscriptionKey3)
            || p.InventoryDescription.Contains(search.DiscriptionKey3))
            && (search.Artical == 0 || p.InventoryArticles == search.Artical)
            && (string.IsNullOrEmpty(search.InventoryBin)
            || p.InventoryBinNumber.Contains(search.InventoryBin))
            && ((!search.FoundFromDate.HasValue && !search.FoundToDate.HasValue)
            || (p.PersonalInventoryGroup.LostFoundDate.HasValue
            && p.PersonalInventoryGroup.LostFoundDate.Value.Date >= search.FoundFromDate.Value.Date
            && p.PersonalInventoryGroup.LostFoundDate.Value.Date <= search.FoundToDate.Value.Date))
            && (search.PropertyType == 0 || p.InventoryArticles == search.PropertyType)
            && (string.IsNullOrEmpty(search.PropertyGroup)
            || p.PersonalInventoryGroup.GroupNumber.Contains(search.PropertyGroup)));
            if (search.IsAdvanceOption)
            {
                if (!string.IsNullOrEmpty(search.Circumstance1))
                {
                    lostFoundInventoryDetails = lostFoundInventoryDetails.Where(p =>
                   (!p.InmateId.HasValue || p.InmateId == 0)
                   && p.PersonalInventoryGroup.LostFoundCircumstance.Contains(search.Circumstance1));
                }
                if (!string.IsNullOrEmpty(search.Circumstance2))
                {
                    lostFoundInventoryDetails = lostFoundInventoryDetails.Where(p =>
                    (!p.InmateId.HasValue || p.InmateId == 0)
                    && p.PersonalInventoryGroup.LostFoundCircumstance.Contains(search.Circumstance2));
                }
                if (search.FoundInmateId > 0)
                {
                    lostFoundInventoryDetails = lostFoundInventoryDetails.Where(p =>
                    (!p.InmateId.HasValue || p.InmateId == 0)
                    && p.PersonalInventoryGroup.LostFoundByInmateId == search.FoundInmateId);
                }
                if (search.PrivilegeId > 0)
                {
                    lostFoundInventoryDetails = lostFoundInventoryDetails.Where(p =>
                    (!p.InmateId.HasValue || p.InmateId == 0)
                    && p.PersonalInventoryGroup.LostFoundLocPrivilegeId == search.PrivilegeId);
                }
                if (!string.IsNullOrEmpty(search.OtherLocation))
                {
                    lostFoundInventoryDetails = lostFoundInventoryDetails.Where(p =>
                    (!p.InmateId.HasValue || p.InmateId == 0)
                    && p.PersonalInventoryGroup.LostFoundLocOther.Contains(search.OtherLocation));
                }
                if (search.AppliedInmateId > 0)
                {
                    lostFoundInventoryDetails = lostFoundInventoryDetails.Where(w =>
                    w.InmateId == search.AppliedInmateId && w.PersonalInventoryGroup.InmateId == search.AppliedInmateId);
                }
                if (search.IsIdentify)
                {
                    lostFoundInventoryDetails = lostFoundInventoryDetails.Where(p => p.InmateId > 0);
                }
                if ((search.IsIdentify && search.IdentifiedFromDate.HasValue && search.IdentifiedToDate.HasValue))
                {
                    lostFoundInventoryDetails = lostFoundInventoryDetails.Where(p => p.InmateId > 0 &&
                    p.PersonalInventoryGroup.LostFoundIdentifiedDate >= search.IdentifiedFromDate
                    && p.PersonalInventoryGroup.LostFoundIdentifiedDate <= search.IdentifiedToDate);
                }
                else if (!search.IsIdentify && (search.IdentifiedFromDate.HasValue && search.IdentifiedToDate.HasValue))
                {
                    lostFoundInventoryDetails = lostFoundInventoryDetails.Where(p =>
                    p.InventoryReturnDate.Value.Date >= search.IdentifiedFromDate
                    && p.InventoryReturnDate.Value.Date <= search.IdentifiedToDate);
                }
            }
            else
            {
                lostFoundInventoryDetails = lostFoundInventoryDetails.Where(w => !w.InmateId.HasValue || w.InmateId == 0);
            }

            List<InventoryDetails> lostFoundInventoryDetail = lostFoundInventoryDetails
                .GroupBy(a => new
                {
                    a.PersonalInventoryGroup.LostFoundDate,
                    a.PersonalInventoryBin.BinName,
                    a.PersonalInventoryGroup.GroupNumber,
                    a.PersonalInventoryGroup.GroupNote,
                    a.PersonalInventoryGroup.LostFoundCircumstance,
                    a.PersonalInventoryGroup.LostFoundByInmateId,
                    a.PersonalInventoryGroup.LostFoundByPersonnelId,
                    a.PersonalInventoryGroup.LostFoundLocFacilityId,
                    a.PersonalInventoryGroup.LostFoundByOther,
                    personLast = a.PersonalInventoryGroup.LostFoundByPersonnel.PersonNavigation.PersonLastName,
                    personFirst = a.PersonalInventoryGroup.LostFoundByPersonnel.PersonNavigation.PersonFirstName,
                    inmateLast = a.PersonalInventoryGroup.LostFoundByInmate.Person.PersonLastName,
                    inmateFirst = a.PersonalInventoryGroup.LostFoundByInmate.Person.PersonFirstName,
                    a.PersonalInventoryGroup.LostFoundByInmate.InmateNumber,
                    a.PersonalInventoryGroup.LostFoundByPersonnel.OfficerBadgeNum,
                    a.PersonalInventoryGroup.LostFoundLocFacility.FacilityAbbr,
                    a.PersonalInventoryGroup.LostFoundLocHousingUnitLocation,
                    a.PersonalInventoryGroup.LostFoundLocHousingUnitNumber,
                    a.PersonalInventoryGroup.LostFoundLocPrivilegeId,
                    a.PersonalInventoryGroup.LostFoundLocOther,
                    a.PersonalInventoryGroup.LostFoundLocPrivilege.PrivilegeDescription,
                    a.PersonalInventoryBinId,
                    a.PersonalInventoryGroupId,
                    a.PersonalInventoryGroup

                }).Select(s => new InventoryDetails
                {
                    LostFountDate = s.Key.LostFoundDate,
                    PersonalBinName = s.Key.BinName,
                    PersonalGroupName = s.Key.GroupNumber,
                    PropertyGroupNotes = s.Key.GroupNote,
                    LostFoundCircumstance = s.Key.LostFoundCircumstance,
                    Count = s.Count(),
                    FoundBy = new FoundBy
                    {
                        InmateNumber = s.Key.InmateNumber,
                        InmateVm = new PersonnelVm
                        {
                            PersonLastName = s.Key.inmateLast,
                            PersonFirstName = s.Key.inmateFirst
                        },
                        PersonVm = new PersonnelVm
                        {
                            PersonLastName = s.Key.personLast,
                            PersonFirstName = s.Key.personFirst,
                            OfficerBadgeNumber = s.Key.OfficerBadgeNum
                        },
                        LostFoundByOther = s.Key.LostFoundByOther
                    },
                    FoundIn = new FoundIn
                    {
                        FacilityAbbr = s.Key.FacilityAbbr,
                        HousingUnitLocation = s.Key.LostFoundLocHousingUnitLocation,
                        HousingUnitNumber = s.Key.LostFoundLocHousingUnitNumber,
                        LostFoundLocOther = s.Key.LostFoundLocOther,
                        PrivilegeDescription = s.Key.PrivilegeDescription
                    },
                    LostFoundLocOther = s.Key.LostFoundLocOther,
                    LostFoundByPersonnelId = s.Key.LostFoundByPersonnelId,
                    LostFoundByInmateId = s.Key.LostFoundByInmateId,
                    LostFoundLocFacilityId = s.Key.LostFoundLocFacilityId,
                    LostFoundByOther = s.Key.LostFoundByOther,
                    LostFoundLocPrivilegeId = s.Key.LostFoundLocPrivilegeId,
                    PersonalInventoryBinId = s.Key.PersonalInventoryBinId,
                    PersonalInventoryGroupId = s.Key.PersonalInventoryGroupId,
                    LostFoundLocHousingUnitLocation = s.Key.LostFoundLocHousingUnitLocation,
                    LostFoundLocHousingUnitNumber = s.Key.LostFoundLocHousingUnitNumber,
                    InventoryItemDetails = s.Select(h => new InventoryItemDetails
                    {
                        InventoryArticles = h.InventoryArticles,
                        InventoryColor = h.InventoryColor,
                        InventoryQuantity = h.InventoryQuantity,
                        InventoryDescription = h.InventoryDescription,
                        PersonalInventoryId = h.PersonalInventoryId,
                        DeleteFlag = h.DeleteFlag,
                        InventoryDamageFlag = h.InventoryDamageFlag,
                        InventoryDamageDescription = h.InventoryDamageDescription
                    }).ToList()
                }).OrderBy(o => o.PropertyGroupNotes).ToList();

            List<int> personnelIds = lostFoundInventoryDetail.Select(s => s.PersonalInventoryBinId ?? 0).Distinct().ToList();

            IQueryable<InventoryDetails> countDetails = _context.PersonalInventory.Where(w =>
             personnelIds.Contains(w.PersonalInventoryBinId ?? 0)
             && (search.DeleteFlag || w.DeleteFlag == 0) && !w.InventoryReturnDate.HasValue
             && w.InventoryDispositionCode == search.DispositionCode
             && (w.InmateId > 0)).
                Select(s => new InventoryDetails { PersonalInventoryBinId = s.PersonalInventoryBinId ?? 0 });
            lostFoundInventoryDetail.ForEach(value =>
              {

                  value.Itemscount = countDetails.Count(c => c.PersonalInventoryBinId == value.PersonalInventoryBinId);

              });
            return lostFoundInventoryDetail;
        }
        private List<LostFoundBin> InventoryLostCount(List<InventoryDetails> details, bool deleteFlag) =>
            details.Where(a => deleteFlag || a.DeleteFlag == 0).GroupBy(i => new
            {
                i.PersonalBinName,
                i.Itemscount
            }).Select(a => new LostFoundBin
            {
                BinName = a.Key.PersonalBinName,
                LostFound = a.Sum(s => s.Count),
                ItemsCount = a.Key.Itemscount
            }).OrderBy(o => o.BinName).ToList();

        private List<KeyValuePair<int, int>> PropertyTypeList(List<InventoryDetails> details) => details.
            SelectMany(a => a.InventoryItemDetails).GroupBy(a => a.InventoryArticles)
                 .Select(a => new KeyValuePair<int, int>(a.Key, a.Count())).OrderBy(a => a.Key).ToList();

        //Inventory Pdf get method
        public InventoryLostFoundVm InventoryPdfGet(int groupId) =>
          _context.PersonalInventory.Where(w => w.PersonalInventoryGroupId == groupId)
                    .Select(s => new InventoryLostFoundVm
                    {
                        LostFountDate = s.PersonalInventoryGroup.LostFoundDate,
                        LostFoundCircumstance = s.PersonalInventoryGroup.LostFoundCircumstance,
                        FoundBy = new FoundBy
                        {
                            InmateNumber = s.PersonalInventoryGroup.LostFoundByInmate.InmateNumber,
                            InmateVm = new PersonnelVm
                            {
                                PersonLastName = s.PersonalInventoryGroup.LostFoundByPersonnel.PersonNavigation.PersonLastName,
                                PersonFirstName = s.PersonalInventoryGroup.LostFoundByPersonnel.PersonNavigation.PersonFirstName
                            },
                            PersonVm = new PersonnelVm
                            {
                                PersonFirstName = s.PersonalInventoryGroup.LostFoundByInmate.Person.PersonFirstName,
                                PersonLastName = s.PersonalInventoryGroup.LostFoundByInmate.Person.PersonLastName,
                                OfficerBadgeNumber = s.PersonalInventoryGroup.LostFoundByPersonnel.OfficerBadgeNum
                            },
                            LostFoundByOther = s.PersonalInventoryGroup.LostFoundByOther
                        },
                        FoundIn = new FoundIn
                        {
                            FacilityAbbr = s.PersonalInventoryGroup.LostFoundLocFacility.FacilityAbbr,
                            HousingUnitLocation = s.PersonalInventoryGroup.LostFoundLocHousingUnitLocation,
                            HousingUnitNumber = s.PersonalInventoryGroup.LostFoundLocHousingUnitNumber,
                            LostFoundLocOther = s.PersonalInventoryGroup.LostFoundLocOther,
                            PrivilegeDescription = s.PersonalInventoryGroup.LostFoundLocPrivilege.PrivilegeDescription
                        },
                        PhotoFilePath = _photos.GetPhotoByPerson(s.PersonalInventoryGroup.LostFoundByInmate.Person),

                        InventoryPdfLostFound = _context.Personnel.Where(w => w.PersonnelId == _personnelId)
                        .Select(p => new InventoryPdfLostFoundVm
                        {
                            AgencyName = p.Agency.AgencyName,
                            StampDate = DateTime.Now,
                            Officer = new PersonnelVm
                            {
                                PersonLastName = p.PersonNavigation.PersonLastName,
                                OfficerBadgeNumber = p.OfficerBadgeNum
                            }
                        }).ToList(),
                    }).FirstOrDefault();


        public List<KeyValuePair<int, string>> GetBinType(int facilityId) =>
        _context.PersonalInventoryBin.Where(w => w.FacilityId == facilityId || !w.FacilityId.HasValue).
        Select(s => new KeyValuePair<int, string>(s.PersonalInventoryBinId, s.BinName)).ToList();

        public List<InventoryLostFoundHistory> HistoryDetails(HistorySearch value)
        {
            IQueryable<PersonalInventory> personInventory = _context.PersonalInventory
            .Where(w => w.Inmate.FacilityId == value.FacilityId || !w.InmateId.HasValue);
            if (value.DispositionCode > 0)
            {
                personInventory = personInventory.Where(w => w.InventoryDispositionCode == value.DispositionCode);
            }
            if (!string.IsNullOrEmpty(value.BinName))
            {
                personInventory = personInventory.Where(w =>
                value.BinName.Equals(w.InventoryBinNumber, StringComparison.CurrentCultureIgnoreCase)
                );
            }
            if (!value.Deleted)
            {
                personInventory = personInventory.Where(w => w.DeleteFlag == (value.Deleted ? 1 : 0));

            }
            if (!string.IsNullOrEmpty(value.Color))
            {
                personInventory = personInventory.Where(w =>
                value.Color.Equals(w.InventoryColor, StringComparison.CurrentCultureIgnoreCase)
                );
            }
            if (value.Damaged)
            {
                personInventory = personInventory.Where(w => w.InventoryDamageFlag);
            }
            if(value.Misplaced)
            {
                 personInventory = personInventory.Where(w => w.InventoryMisplacedFlag);
            }
            if (value.FoundBy)
            {
                personInventory = personInventory.Where(w => w.PersonalInventoryGroup.LostFoundDate.HasValue);
            }
            if (value.Type > 0)
            {
                personInventory = personInventory.Where(w => w.InventoryArticles == value.Type);
            }
            if (!string.IsNullOrEmpty(value.Keyword1))
            {
                personInventory = personInventory.Where(w => w.InventoryDescription.Contains(value.Keyword1));
            }
            if (!string.IsNullOrEmpty(value.Keyword2))
            {
                personInventory = personInventory.Where(w => w.InventoryDescription.Contains(value.Keyword2));
            }
            if (!string.IsNullOrEmpty(value.Keyword3))
            {
                personInventory = personInventory.Where(w => w.InventoryDescription.Contains(value.Keyword3));
            }
            if (!string.IsNullOrEmpty(value.PropertyGroup))
            {
                personInventory = personInventory.Where(w => w.PersonalInventoryGroup.GroupNumber.Contains(value.PropertyGroup));
            }
            if (!string.IsNullOrEmpty(value.EvidCase))
            {
                personInventory = personInventory.Where(w => w.InventoryEvidenceCaseNumber.Contains(value.EvidCase));
            }
            if (value.BelongsInmate > 0)
            {
                personInventory = personInventory.Where(w => w.PersonalInventoryGroup.InmateId == value.BelongsInmate);
            }

            if (value.CreatedPersonnalId > 0)
            {
                personInventory = personInventory.Where(w => w.CreatedByNavigation.PersonnelId == value.CreatedPersonnalId);
            }
            if (value.DisposePersonnalId > 0)
            {
                personInventory = personInventory.Where(w => w.InventoryOfficerId == value.DisposePersonnalId);
            }

            if (value.CreatedFlag)
            {
                personInventory = personInventory.Where(w => w.CreateDate.Value.Date >= value.CreatedFromDate.Date && w.CreateDate.Value.Date <= value.CreatedToDate.Date);
            }
            if (value.DisposeFlag)
            {
                personInventory = personInventory.Where(w => w.DeleteDate >= value.DisposeFromDate && w.DeleteDate <= value.DisposeToDate);
            }
            if (!string.IsNullOrEmpty(value.ReleaseToPerson))
            {
                personInventory = personInventory.Where(w => w.PersonName.Contains(value.ReleaseToPerson));
            }
            return personInventory.OrderByDescending(od => od.PersonalInventoryId).Take(value.TopRecords)
                .Select(s => new InventoryLostFoundHistory
                {
                    DispositionCode = s.InventoryDispositionCode ?? 0,
                    Qty = s.InventoryQuantity,
                    InmateId = s.InmateId,
                    Description = s.InventoryDescription,
                    Bin = s.InventoryBinNumber,
                    Type = s.InventoryArticles,
                    PersonalInventoryBinId = s.PersonalInventoryBinId,
                    GroupNumber = s.PersonalInventoryGroup.GroupNumber,
                    EvidCase = s.InventoryEvidenceCaseNumber,
                    DeletedFlag = s.DeleteFlag > 0,
                    InmateDetails = new PersonVm
                    {
                        PersonLastName = s.Inmate.Person.PersonLastName,
                        PersonFirstName = s.Inmate.Person.PersonFirstName,
                        PersonMiddleName = s.Inmate.Person.PersonMiddleName,
                        InmateNumber = s.Inmate.InmateNumber
                    },
                    CreatedDate = s.CreateDate,
                    CreatedBy = new PersonnelVm
                    {
                        PersonLastName = s.CreatedByNavigation.PersonNavigation.PersonLastName,
                        PersonFirstName = s.CreatedByNavigation.PersonNavigation.PersonFirstName,
                        OfficerBadgeNumber = s.CreatedByNavigation.OfficerBadgeNum
                    },
                    DispodBy = new PersonnelVm
                    {
                        PersonLastName = s.UpdatedByNavigation.PersonNavigation.PersonLastName,
                        PersonFirstName = s.UpdatedByNavigation.PersonNavigation.PersonFirstName,
                        OfficerBadgeNumber = s.UpdatedByNavigation.OfficerBadgeNum
                    },
                    DispoDate = s.UpdateDate,
                    PersonalInventoryId = s.PersonalInventoryId,
                    InventoryDamagedDescription = s.InventoryDamageDescription,
                    ReleaseToPerson = s.PersonName,
                    DispositionDetail=s.DispoNotes
                }).ToList();
        }

        //insert Damage and Disposition
        public async Task<int> Disposition(InventoryDetails inventoryDetails)
        {
            PersonalInventory personalInventoryDispose = _context.PersonalInventory.Single(s =>
            s.PersonalInventoryId == inventoryDetails.PersonalInventoryId);
            personalInventoryDispose.InventoryDamageFlag = inventoryDetails.InventoryDamageFlag;
            personalInventoryDispose.InventoryDamageDescription = inventoryDetails.InventoryDamageDescription;

            _context.PersonalInventoryHistory.Add(new PersonalInventoryHistory
            {
                InventoryDamageFlag = personalInventoryDispose.InventoryDamageFlag,
                InventoryDamageDescription = personalInventoryDispose.InventoryDamageDescription,
                PersonalInventoryId = personalInventoryDispose.PersonalInventoryId,
                InmateId = personalInventoryDispose.InmateId,
                InventoryDate = personalInventoryDispose.InventoryDate,
                InventoryArticles = personalInventoryDispose.InventoryArticles,
                InventoryQuantity = personalInventoryDispose.InventoryQuantity,
                InventoryUom = personalInventoryDispose.InventoryUom,
                InventoryDescription = personalInventoryDispose.InventoryDescription,
                InventoryDispositionCode = personalInventoryDispose.InventoryDispositionCode,
                InventoryValue = personalInventoryDispose.InventoryValue,
                InventoryDestroyed = personalInventoryDispose.InventoryDestroyed,
                InventoryMailed = personalInventoryDispose.InventoryMailed,
                InventoryMailPersonId = personalInventoryDispose.InventoryMailPersonId,
                InventoryMailAddressId = personalInventoryDispose.InventoryMailAddressId,
                InventoryOfficerId = personalInventoryDispose.InventoryOfficerId,
                InventoryColor = personalInventoryDispose.InventoryColor,
                InventoryBinNumber = personalInventoryDispose.InventoryBinNumber,
                InventoryReturnDate = personalInventoryDispose.InventoryReturnDate,
                CreateDate = personalInventoryDispose.CreateDate,
                CreatedBy = personalInventoryDispose.CreatedBy,
                UpdateDate = personalInventoryDispose.UpdateDate,
                PersonalInventoryBinId = personalInventoryDispose.PersonalInventoryBinId ?? 0,
                PersonName = personalInventoryDispose.PersonName,
                PersonIdType = personalInventoryDispose.PersonIdType,
                PersonAddress = personalInventoryDispose.PersonAddress,
                DispoNotes = personalInventoryDispose.DispoNotes,
                UpdatedBy = personalInventoryDispose.UpdatedBy,
                DeleteFlag = personalInventoryDispose.DeleteFlag,
                DeleteDate = personalInventoryDispose.DeleteDate,
                DeletedBy = personalInventoryDispose.DeletedBy,
                CityStateZip = personalInventoryDispose.CityStateZip,
                PersonalInventoryGroupId = personalInventoryDispose.PersonalInventoryGroupId,
                DeleteReason = personalInventoryDispose.DeleteReason,
                DeleteReasonNote = personalInventoryDispose.DeleteReasonNote,
                InventoryEvidencePersonnelId = personalInventoryDispose.InventoryEvidencePersonnelId,
                InventoryEvidenceAgencyId = personalInventoryDispose.InventoryEvidenceAgencyId,
                InventoryEvidenceCaseNumber = personalInventoryDispose.InventoryEvidenceCaseNumber
            });

            return await _context.SaveChangesAsync();
        }

        #endregion
    }
}