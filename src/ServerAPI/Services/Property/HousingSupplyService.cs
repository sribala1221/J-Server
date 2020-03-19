using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ServerAPI.ViewModels;
using GenerateTables.Models;
using ServerAPI.Utilities;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace ServerAPI.Services
{
    public class HousingSupplyService : IHousingSupplyService
    {
        private readonly AAtims _context;
        private readonly int _personnelId;
        private readonly ICommonService _commonService;
        private readonly IPersonService _personService;
        private readonly IInmateService _inmateService;

        public HousingSupplyService(AAtims context, IHttpContextAccessor httpContextAccessor,
            ICommonService commonService, IPersonService personService, IInmateService inmateService)
        {
            _context = context;
            _personnelId = Convert.ToInt32(httpContextAccessor.HttpContext.User
                .FindFirst(AuthDetailConstants.PERSONNELID)?.Value);
            _commonService = commonService;
            _personService = personService;
            _inmateService = inmateService;
        }

        public List<ClosetVm> GetClosetDetails(int facilityId)
        {
            List<ClosetVm> checkList = _context.HousingSupplyModule.Where(a =>
                    (facilityId <= 0 || a.FacilityId == facilityId) && !a.DeleteFlag.HasValue)
                .Select(a => new ClosetVm
                {
                    HousingSupplyModuleId = a.HousingSupplyModuleId,
                    ClosetName = a.ClosetName,
                    ClosetBelongsTo = a.ClosetBelongsTo
                }).ToList();

            return checkList;
        }

        public List<CheckListVm> GetSupplyCheckList(HousingSupplyInput input)
        {
            List<CheckListVm> checkList = _context.HousingSupplyCheckListXref
                .Where(w => w.HousingSupplyItem.HousingSupplyModule.FacilityId == input.FacilityId
                            && (input.PersonnelId > 0 ?
                                w.HousingSupplyCheckList.CheckListBy == input.PersonnelId : true)
                            && (input.HousingSupplyModuleId.Count <= 0
                                || input.HousingSupplyModuleId.Contains(w.HousingSupplyItem.HousingSupplyModule
                                .HousingSupplyModuleId))
                            && (!input.StartDate.HasValue ||
                                (w.HousingSupplyCheckList.CheckListDate.Value.Date >= input.StartDate.Value.Date &&
                                w.HousingSupplyCheckList.CheckListDate.Value.Date <= input.EndDate.Value.Date)) && w.HousingSupplyItem.DeleteFlag != 1
              && w.HousingSupplyItem.HousingSupplyModule.DeleteFlag != 1)
                .Select(s => new
                {
                    s.HousingSupplyCheckListId,
                    s.HousingSupplyItem.HousingSupplyModule.ClosetName,
                    s.HousingSupplyItem.HousingSupplyModule.HousingSupplyModuleId,
                    s.HousingSupplyCheckListXrefId,
                    s.CheckListFlag,
                    s.HousingSupplyCheckList.CheckListDate,
                    s.HousingSupplyCheckList.CheckListNote,
                    s.HousingSupplyCheckList.CheckListBy,
                    s.HousingSupplyCheckList.DeleteFlag
                }).GroupBy(g => new { g.HousingSupplyCheckListId, g.ClosetName })
                .Select(s => new CheckListVm
                {
                    CheckListId = s.Key.HousingSupplyCheckListId,
                    SupplyModuleId = s.Select(se => se.HousingSupplyModuleId).FirstOrDefault(),
                    ClosetName = s.Key.ClosetName,
                    ActualCount = s.Select(se => se.HousingSupplyCheckListXrefId).Count(),
                    CheckCount = s.Sum(se => se.CheckListFlag),
                    ChecklistDate = s.Select(se => se.CheckListDate).FirstOrDefault(),
                    CheckListNote = s.Select(se => se.CheckListNote).FirstOrDefault(),
                    CheckListBy = s.Select(se => se.CheckListBy).FirstOrDefault(),
                    DeleteFlag = s.Select(se => se.DeleteFlag).FirstOrDefault()
                }).OrderByDescending(d => d.ChecklistDate).ToList();

            if (input.Last > 0)
            {
                checkList = checkList.Take(input.Last ?? 0).ToList();
            }

            List<int> personnelIdLst = checkList.Select(c => c.CheckListBy ?? 0).ToList();
            List<PersonnelVm> lstPersonDetails = _personService.GetPersonNameList(personnelIdLst).ToList();

            checkList.ForEach(item =>
            {
                item.LstPersonnel = lstPersonDetails.Single(p => p.PersonnelId == item.CheckListBy);
            });

            return checkList;
        }

        #region SupplyItem

        public HousingSupplyVm GetAvailableSupplyItems(int facilityId, int[] moduleId)
        {
            HousingSupplyVm housingSupplyItems = new HousingSupplyVm
            {
                LstAvailableItems = _context.HousingSupplyItem
                    .Where(w => w.CurrentCheckoutLocation == null
                                && w.CurrentCheckoutHousing == null
                                && (w.CurrentCheckoutInmateId == null || w.CurrentCheckoutInmateId == 0)
                                && w.DeleteFlag != 1
                                && w.HousingSupplyItemLookup.DeleteFlag != 1
                                && w.HousingSupplyModule.FacilityId == facilityId
                                && (moduleId.Length <= 0
                                    || moduleId.Contains(w.HousingSupplyModule.HousingSupplyModuleId))
                                && w.HousingSupplyModule.DeleteFlag != 1)
                    .Select(h => new
                    {
                        h.AllowCheckoutToHousing,
                        h.AllowCheckoutToInmate,
                        h.AllowCheckoutToLocation,
                        h.HousingSupplyItemId,
                        h.HousingSupplyItemLookupId,
                        h.HousingSupplyModuleId,
                        h.SupplyNumber,
                        h.SupplyDescription,
                        h.CurrentCheckoutLocation,
                        h.CurrentCheckoutHousing,
                        h.CurrentCheckoutInmateId,
                        h.CurrentCheckoutDateTime,
                        h.HousingSupplyItemLookup.ItemName,
                        h.HousingSupplyItemLookup.OneItemOnlyFlag,
                        h.HousingSupplyModule.HousingUnitLocation,
                        h.HousingSupplyModule.HousingUnitNumber,
                        h.HousingSupplyModule.Facility.FacilityAbbr,
                        h.DamageFlag,
                        h.ConsumedFlag

                    }).GroupBy(g => new
                    {
                        g.HousingSupplyItemLookupId,
                        g.HousingSupplyModuleId,
                        g.ItemName,
                        g.SupplyNumber,
                        g.HousingUnitLocation,
                        g.HousingUnitNumber,
                        g.CurrentCheckoutLocation,
                        g.CurrentCheckoutHousing,
                        g.FacilityAbbr,
                        g.ConsumedFlag,
                        g.DamageFlag
                    }).Select(s => new SupplyItemsVm
                    {
                        Quantity = s.Select(se => se.HousingSupplyItemId).Count(),
                        HousingSupplyItemLookupId = s.Key.HousingSupplyItemLookupId,
                        HousingSupplyModuleId = s.Key.HousingSupplyModuleId,
                        SupplyNumber = s.Key.SupplyNumber,
                        SupplyDescription = s.Select(se => se.SupplyDescription).FirstOrDefault(),
                        CurrentCheckoutLocation = s.Key.CurrentCheckoutLocation,
                        CurrentCheckoutHousing = s.Key.CurrentCheckoutHousing,
                        CurrentCheckoutInmateId = s.Select(se => se.CurrentCheckoutInmateId).FirstOrDefault(),
                        CurrentCheckoutDateTime = s.Select(se => se.CurrentCheckoutDateTime).FirstOrDefault(),
                        ItemName = s.Key.ItemName,
                        HousingUnitLocation = s.Key.HousingUnitLocation,
                        HousingUnitNumber = s.Key.HousingUnitNumber,
                        FacilityAbbr = s.Key.FacilityAbbr,
                        AllowCheckoutToLocation = s.Select(se => se.AllowCheckoutToLocation).FirstOrDefault() > 0,
                        AllowCheckoutToHousing = s.Select(se => se.AllowCheckoutToHousing).FirstOrDefault() > 0,
                        AllowCheckoutToInmate = s.Select(se => se.AllowCheckoutToInmate).FirstOrDefault() > 0,
                        DamageFlag = s.Select(se => se.DamageFlag).FirstOrDefault() > 0,
                        ConsumedFlag = s.Select(se => se.ConsumedFlag).FirstOrDefault() > 0,
                        OneItemOnlyFlag = s.Select(se => se.OneItemOnlyFlag).FirstOrDefault() > 0,
                        LstSupplyItemId = s.Select(se => se.HousingSupplyItemId).ToList()
                    }).OrderBy(o => o.ItemName).ToList()
            };

            List<int> lstCheckoutInmateId = housingSupplyItems.LstAvailableItems
                .Select(s => s.CurrentCheckoutInmateId ?? 0).ToList();

            //Get Person details by list of inmate id
            List<PersonInfoVm> lstPersonDetails = _context.Inmate
                .Where(s => lstCheckoutInmateId.Contains(s.InmateId))
                .Select(s => new PersonInfoVm
                {
                    InmateId = s.InmateId,
                    PersonId = s.PersonId,
                    InmateNumber = s.InmateNumber,
                    PersonFirstName = s.Person.PersonFirstName,
                    PersonLastName = s.Person.PersonLastName,
                    PersonMiddleName = s.Person.PersonMiddleName
                }).ToList();

            //Get checkout list location
            List<int> lstFacilityId = _context.HousingSupplyModule
                .Where(w => w.FacilityId == facilityId && moduleId.Contains(w.HousingSupplyModuleId))
                .Select(w => w.FacilityId).ToList();
            housingSupplyItems.LocationList = _context.Privileges.OrderBy(p => p.PrivilegeDescription)
                .Where(p => p.InactiveFlag == 0 && p.SupplyLocationFlag == 1 &&
                            lstFacilityId.Contains(p.FacilityId ?? 0))
                .Select(p => new KeyValuePair<int, string>(p.PrivilegeId, p.PrivilegeDescription)).ToList();

            //Get checkout list Housing
            housingSupplyItems.HousingList = _context.HousingSupplyModule
                .Where(p => p.FacilityId == facilityId && !p.DeleteFlag.HasValue && !moduleId.Contains(p.HousingSupplyModuleId))
                .Select(s => new KeyValuePair<int, string>(s.HousingSupplyModuleId, s.ClosetBelongsTo)).ToList();

            housingSupplyItems.LstAvailableItems.ForEach(item =>
            {
                item.PersonDetails =
                    lstPersonDetails.SingleOrDefault(p => p.InmateId == item.CurrentCheckoutInmateId);
            });

            housingSupplyItems.LstCheckoutItems = GetCheckoutItemsLst(facilityId, moduleId);

            return housingSupplyItems;
        }

        private List<SupplyItemsVm> GetCheckoutItemsLst(int facilityId, int[] moduleId)
        {
            List<SupplyItemsVm> lstCheckoutItemsLst = _context.HousingSupplyItem
                .Where(h => (h.CurrentCheckoutLocation != null
                             || h.CurrentCheckoutHousing != null
                             || h.CurrentCheckoutInmateId > 0)
                            && h.DeleteFlag != 1
                            && h.HousingSupplyItemLookup.DeleteFlag != 1
                            && h.HousingSupplyModule.FacilityId == facilityId
                            && h.HousingSupplyModule.DeleteFlag != 1
                            && (moduleId.Length <= 0
                                || moduleId.Contains(h.HousingSupplyModule.HousingSupplyModuleId)))
                .Select(h => new
                {
                    h.HousingSupplyItemId,
                    h.HousingSupplyItemLookupId,
                    h.HousingSupplyModuleId,
                    h.SupplyNumber,
                    h.SupplyDescription,
                    h.CurrentCheckoutLocation,
                    h.CurrentCheckoutHousing,
                    h.CurrentCheckoutInmateId,
                    h.CurrentCheckoutDateTime,
                    h.FlagDurationMin,
                    h.HousingSupplyItemLookup.ItemName,
                    h.HousingSupplyModule.HousingUnitLocation,
                    h.HousingSupplyModule.HousingUnitNumber,
                    h.HousingSupplyModule.Facility.FacilityAbbr,
                    h.DamageFlag,
                    h.ConsumedFlag
                }).GroupBy(g => new
                {
                    g.HousingSupplyItemLookupId,
                    g.HousingSupplyModuleId,
                    g.ItemName,
                    g.SupplyNumber,
                    g.CurrentCheckoutLocation,
                    g.CurrentCheckoutHousing,
                    g.CurrentCheckoutInmateId,
                    g.HousingUnitLocation,
                    g.HousingUnitNumber,
                    g.FacilityAbbr,
                    g.ConsumedFlag,
                    g.DamageFlag
                })
                .Select(s => new SupplyItemsVm
                {
                    Quantity = s.Select(se => se.HousingSupplyItemId).Count(),
                    LstSupplyItemId = s.Select(se => se.HousingSupplyItemId).ToList(),
                    HousingSupplyItemLookupId = s.Key.HousingSupplyItemLookupId,
                    HousingSupplyModuleId = s.Key.HousingSupplyModuleId,
                    SupplyNumber = s.Key.SupplyNumber,
                    SupplyDescription = s.Select(se => se.SupplyDescription).FirstOrDefault(),
                    CurrentCheckoutLocation = s.Key.CurrentCheckoutLocation,
                    CurrentCheckoutHousing = s.Key.CurrentCheckoutHousing,
                    CurrentCheckoutInmateId = s.Key.CurrentCheckoutInmateId,
                    CurrentCheckoutDateTime = s.Select(se => se.CurrentCheckoutDateTime).FirstOrDefault(),
                    FlagDurationMin = s.Select(se => se.FlagDurationMin).FirstOrDefault(),
                    ItemName = s.Key.ItemName,
                    HousingUnitLocation = s.Key.HousingUnitLocation,
                    HousingUnitNumber = s.Key.HousingUnitNumber,
                    FacilityAbbr = s.Key.FacilityAbbr,
                    //  HousingSupplyItemId = s.Key.HousingSupplyItemId,
                    DamageFlag = s.Select(se => se.DamageFlag).FirstOrDefault() > 0,
                    ConsumedFlag = s.Select(se => se.ConsumedFlag).FirstOrDefault() > 0

                }).ToList();

            //Get checkoutinmateid List
            List<int> lstCheckoutInmateId = lstCheckoutItemsLst
                .Select(s => s.CurrentCheckoutInmateId ?? 0).ToList();

            //Get Person details by list of inmate id
            List<PersonInfoVm> lstPersonDetails = _context.Inmate
                .Where(s => lstCheckoutInmateId.Contains(s.InmateId))
                .Select(s => new PersonInfoVm
                {
                    InmateId = s.InmateId,
                    PersonId = s.PersonId,
                    InmateNumber = s.InmateNumber,
                    PersonFirstName = s.Person.PersonFirstName,
                    PersonLastName = s.Person.PersonLastName,
                    PersonMiddleName = s.Person.PersonMiddleName
                }).ToList();

            //Get HousingSupplyItemId list
            List<int> lstHousingSupplyItemId = lstCheckoutItemsLst.SelectMany(d => d.LstSupplyItemId).ToList();

            //Get HouisngSupplyCheckoutId by HousingSupplyItem
            List<SupplyVm> lstSupplyCheckoutId = _context.HousingSupplyCheckout
                .Where(w => lstHousingSupplyItemId.Contains(w.HousingSupplyItemId)
                            && w.CheckinDate == null)
                .Select(s => new SupplyVm
                {
                    HousingSupplyCheckoutId = s.HousingSupplyCheckoutId,
                    HousingSupplyItemId = s.HousingSupplyItemId
                }).ToList();

            lstCheckoutItemsLst.ForEach(item =>
            {
                item.PersonDetails =
                    lstPersonDetails.SingleOrDefault(p => p.InmateId == item.CurrentCheckoutInmateId);
                item.LstCheckoutItemIdPair = lstSupplyCheckoutId
                    .Where(w => item.LstSupplyItemId.Contains(w.HousingSupplyItemId))
                    .Select(p => new KeyValuePair<int, int>(p.HousingSupplyItemId, p.HousingSupplyCheckoutId)).ToList();
            });

            return lstCheckoutItemsLst;
        }

        public List<SupplyItemsVm> GetAvailableHistoryLst(SupplyItemsVm supplyItemsVm)
        {
            // CHECKOUT & CHECK IN details
            List<SupplyItemsVm> lstCheckoutCheckIn = _context.HousingSupplyCheckout.Where(w =>
                    (!supplyItemsVm.HousingItemFlag
                        || (w.HousingSupplyItem.HousingSupplyItemLookup.ItemName == supplyItemsVm.ItemName
                            && w.HousingSupplyItem.SupplyNumber == supplyItemsVm.SupplyNumber
                            && supplyItemsVm.ListModuleId.Contains(w.HousingSupplyItem.HousingSupplyModuleId)
                            && (w.CheckinDate.HasValue || w.CheckoutDate.HasValue)))
                            && (supplyItemsVm.HousingItemFlag || (
                            (supplyItemsVm.ListModuleId == null || supplyItemsVm.ListModuleId.Length <= 0 ||
                            supplyItemsVm.ListModuleId.Contains(w.HousingSupplyItem.HousingSupplyModuleId))
                            && (supplyItemsVm.DeletedHistory ||
                            !w.HousingSupplyItem.DeleteFlag.HasValue || w.HousingSupplyItem.DeleteFlag == 0)
                            && (supplyItemsVm.FacilityId <= 0 ||
                            w.HousingSupplyItem.HousingSupplyModule.FacilityId == supplyItemsVm.FacilityId)
                            && (supplyItemsVm.SearchTextValue != 1 || w.HousingSupplyItem.ConsumedFlag == 1)
                            && (supplyItemsVm.SearchTextValue != 2 || w.HousingSupplyItem.DamageFlag == 1)
                            && (supplyItemsVm.SearchTextValue != 3 || w.HousingSupplyItem.DeleteFlag == 1))))
                .Select(s => new SupplyItemsVm
                {
                    ClosetName = s.HousingSupplyItem.HousingSupplyModule.ClosetName,
                    ItemName = s.HousingSupplyItem.HousingSupplyItemLookup.ItemName,
                    SupplyNumber = s.HousingSupplyItem.SupplyNumber,
                    CheckoutDate = s.CheckoutDate,
                    CheckInDate = s.CheckinDate,
                    CheckInNote = s.CheckinNote,
                    PersonnelCheckoutDetails = new PersonnelVm
                    {
                        PersonLastName = s.CheckoutByNavigation.PersonNavigation.PersonLastName,
                        OfficerBadgeNumber = s.CheckoutByNavigation.OfficerBadgeNum
                    },
                    PersonnelCheckInDetails = new PersonnelVm
                    {
                        PersonLastName = s.CheckinByNavigation.PersonNavigation.PersonLastName,
                        OfficerBadgeNumber = s.CheckinByNavigation.OfficerBadgeNum
                    },
                    CheckoutLocation = s.CheckoutLocation,
                    CheckoutNote = s.CheckoutNote,
                    CheckoutInmateId = s.CheckoutInmateId,
                    CurrentCheckoutHousing = s.CheckoutHousing,
                    CheckedoutDate = s.CheckoutDate.Value.Date,
                    Hour = s.CheckoutDate.Value.Hour,
                    Minute = s.CheckoutDate.Value.Minute,
                    Second = s.CheckoutDate.Value.Second,
                    ConsumedFlag = s.HousingSupplyItem.ConsumedFlag == 1,
                    DamageFlag = s.HousingSupplyItem.DamageFlag == 1,
                    DeleteFlag = s.HousingSupplyItem.DeleteFlag == 1,
                    ConsumedDate = s.HousingSupplyItem.ConsumedDate,
                    DamageDate = s.HousingSupplyItem.DamageFlagDate,
                    DeleteDate = s.HousingSupplyItem.DeleteDate,
                    Checked = true,
                    CheckoutBy = s.CheckoutBy,
                    CheckInBy = s.CheckinBy,
                    CheckoutLocationId = s.CheckoutLocationId

                }).ToList();

            if (supplyItemsVm.HousingSupplyCountFlag)
            {
                if (supplyItemsVm.HousingSupplyCountFlag && !string.IsNullOrEmpty(supplyItemsVm.ItemName) &&
                !string.IsNullOrEmpty(supplyItemsVm.SupplyNumber))
                {
                    List<string> lstItemName = supplyItemsVm.ItemName.Split(",").ToList();
                    List<string> lstSupplyNumber = supplyItemsVm.SupplyNumber.Split(",").ToList();
                    supplyItemsVm.SupplyList = new List<SupplyItemsVm>();
                    for (int index = 0; lstItemName.Count > index;)
                    {
                        supplyItemsVm.SupplyList.Add(new SupplyItemsVm());
                        supplyItemsVm.SupplyList[index].ItemName = lstItemName[index];
                        supplyItemsVm.SupplyList[index].SupplyNumber = lstSupplyNumber[index];
                        index++;
                    }
                    lstCheckoutCheckIn = lstCheckoutCheckIn.Where(w =>
                    supplyItemsVm.SupplyList.Any(a => a.ItemName == w.ItemName && a.SupplyNumber == w.SupplyNumber)
                    ).ToList();
                }
                else
                {
                    lstCheckoutCheckIn = lstCheckoutCheckIn.Where(w => w.ItemName == supplyItemsVm.ItemName &&
                    w.SupplyNumber == supplyItemsVm.SupplyNumber).ToList();
                }
            }
            // List Checkout Details
            List<SupplyItemsVm> lstCheckout = lstCheckoutCheckIn.Where(w =>
                w.CheckoutDate.HasValue && (!supplyItemsVm.Last12hours ||
                DateTime.Now.Subtract(w.CheckoutDate.Value).TotalHours < supplyItemsVm.Hours) &&
                (!supplyItemsVm.DateRange || w.CheckoutDate.Value.Date >= supplyItemsVm.FromDate.Value.Date &&
                w.CheckoutDate.Value.Date <= supplyItemsVm.ToDate.Value.Date) 
                && (supplyItemsVm.OfficerId <= 0 || w.CheckoutBy == supplyItemsVm.OfficerId)
                && (string.IsNullOrEmpty(supplyItemsVm.InmateId) || w.CheckoutInmateId == supplyItemsVm.InmateId)
                && (supplyItemsVm.Location <= 0 || w.CheckoutLocationId == supplyItemsVm.Location))
                .GroupBy(g => new
                {
                    g.ClosetName,
                    g.ItemName,
                    g.SupplyNumber,
                    g.CheckedoutDate,
                    g.Hour,
                    g.Minute,
                    g.Second,
                    g.CheckoutBy,
                    g.CheckoutLocation,
                    g.CheckoutNote,
                    g.CheckoutInmateId,
                    g.CurrentCheckoutHousing,
                    g.ConsumedFlag,
                    g.DamageFlag,
                    g.DeleteFlag,
                    g.ConsumedDate,
                    g.DamageDate,
                    g.DeleteDate,
                    g.Checked ,
                    g.CheckoutLocationId                

                }).Select(s => new SupplyItemsVm
                {
                    Quantity = s.Count(),
                    ClosetName = s.Key.ClosetName,
                    ItemName = s.Key.ItemName,
                    ActionType = ActionType.CHECKOUT,
                    SupplyNumber = s.Key.SupplyNumber,
                    CheckoutDate = s.Select(w => w.CheckoutDate).FirstOrDefault(),
                    CheckoutBy = s.Key.CheckoutBy,
                    CheckoutLocation = s.Key.CheckoutLocation,
                    CheckoutNote = s.Key.CheckoutNote,
                    CheckoutInmateId = s.Key.CheckoutInmateId,
                    PersonnelCheckoutDetails = new PersonnelVm
                    {
                        PersonLastName = s.Select(w => w.PersonnelCheckoutDetails.PersonLastName).FirstOrDefault(),
                        OfficerBadgeNumber =
                            s.Select(w => w.PersonnelCheckoutDetails.OfficerBadgeNumber).FirstOrDefault()
                    },
                    CurrentCheckoutHousing = s.Key.CurrentCheckoutHousing,
                    ConsumedFlag = s.Key.ConsumedFlag,
                    DamageFlag = s.Key.DamageFlag,
                    DeleteFlag = s.Key.DeleteFlag,
                    ConsumedDate = s.Key.ConsumedDate,
                    DamageDate = s.Key.DamageDate,
                    DeleteDate = s.Key.DeleteDate,
                    Checked = s.Key.Checked,
                    CheckoutLocationId = s.Key.CheckoutLocationId                   
                    
                }).ToList();
            // List CheckIn Details
            List<SupplyItemsVm> lstCheckIn = lstCheckoutCheckIn.Where(w =>
                w.CheckInDate.HasValue && (!supplyItemsVm.Last12hours ||
                DateTime.Now.Subtract(w.CheckInDate.Value).TotalHours < supplyItemsVm.Hours) &&
                (!supplyItemsVm.DateRange || w.CheckInDate.Value.Date >= supplyItemsVm.FromDate.Value.Date &&
                w.CheckInDate.Value.Date <= supplyItemsVm.ToDate.Value.Date)
                && (supplyItemsVm.OfficerId <= 0 || w.CheckInBy == supplyItemsVm.OfficerId))
                .GroupBy(g => new
                {
                    g.ClosetName,
                    g.ItemName,
                    g.SupplyNumber,
                    g.CheckInDate,
                    g.CheckInBy,
                    g.CheckInNote,
                    g.CheckoutInmateId,
                    g.CheckoutLocation,
                    g.CheckoutDate,
                    g.CurrentCheckoutHousing,
                    g.ConsumedFlag,
                    g.DamageFlag,
                    g.DeleteFlag,
                    g.ConsumedDate,
                    g.DamageDate,
                    g.DeleteDate,
                    g.Checked

                }).Select(s => new SupplyItemsVm
                {
                    Quantity = s.Count(),
                    ClosetName = s.Key.ClosetName,
                    ItemName = s.Key.ItemName,
                    ActionType = ActionType.CHECKIN,
                    SupplyNumber = s.Key.SupplyNumber,
                    CheckoutDate = s.Key.CheckoutDate,
                    CheckoutBy = s.Key.CheckInBy,
                    CheckoutLocation = s.Key.CheckoutLocation,
                    CheckInNote = s.Key.CheckInNote,
                    CheckoutInmateId = s.Key.CheckoutInmateId,
                    CheckInDate = s.Key.CheckInDate,
                    PersonnelCheckInDetails = new PersonnelVm
                    {
                        PersonLastName = s.Select(w => w.PersonnelCheckInDetails.PersonLastName).FirstOrDefault(),
                        OfficerBadgeNumber =
                            s.Select(w => w.PersonnelCheckInDetails.OfficerBadgeNumber).FirstOrDefault()
                    },
                    CurrentCheckoutHousing = s.Key.CurrentCheckoutHousing,
                    ConsumedFlag = s.Key.ConsumedFlag,
                    DamageFlag = s.Key.DamageFlag,
                    DeleteFlag = s.Key.DeleteFlag,
                    ConsumedDate = s.Key.ConsumedDate,
                    DamageDate = s.Key.DamageDate,
                    DeleteDate = s.Key.DeleteDate,
                    Checked = s.Key.Checked

                }).ToList();

            lstCheckout.AddRange(lstCheckIn);
            lstCheckout.AddRange(GetMoveDetails(supplyItemsVm));
            lstCheckout.AddRange(GetCheckListDetails(supplyItemsVm));

            List<int> lstInmateId = lstCheckoutCheckIn.Select(c => Convert.ToInt32(c.CheckoutInmateId)).ToList();

            //Get Person details by list of inmate id
            List<PersonVm> lstPersonDetails = _inmateService.GetInmateDetails(lstInmateId);

            lstCheckout.Where(w => !string.IsNullOrEmpty(w.CheckoutInmateId)).ToList().ForEach(item =>
              {
                  item.InmateDetails =
                    lstPersonDetails.Single(c => c.InmateId == Convert.ToInt32(item.CheckoutInmateId));
              });

            return lstCheckout;
        }

        private List<SupplyItemsVm> GetMoveDetails(SupplyItemsVm supplyItemsVm)
        {
            // Move details
            List<SupplyItemsVm> lstMove = _context.HousingSupplyItem.SelectMany(hsi =>
                    _context.HousingSupplyMove.Where(hsm =>
                        (!supplyItemsVm.HousingItemFlag ||
                        hsm.HousingSupplyItemId == hsi.HousingSupplyItemId
                        && hsi.SupplyNumber == supplyItemsVm.SupplyNumber
                        && hsi.HousingSupplyItemLookup.ItemName == supplyItemsVm.ItemName
                        && hsm.MoveDate.HasValue
                        && supplyItemsVm.ListModuleId.Contains(hsi.HousingSupplyModule.HousingSupplyModuleId))
                        && (supplyItemsVm.HousingItemFlag || (hsm.HousingSupplyItemId == hsi.HousingSupplyItemId)
                        && (supplyItemsVm.ListModuleId == null || supplyItemsVm.ListModuleId.Length <= 0 ||
                        supplyItemsVm.ListModuleId.Contains(hsi.HousingSupplyModule.HousingSupplyModuleId))
                        && (supplyItemsVm.DeletedHistory ||
                            !hsi.DeleteFlag.HasValue || hsi.DeleteFlag == 0)
                        && hsm.MoveDate.HasValue
                        && (supplyItemsVm.FacilityId == 0 || hsi.HousingSupplyModule.FacilityId == supplyItemsVm.FacilityId)
                        && (supplyItemsVm.OfficerId == 0 || hsm.MoveBy == supplyItemsVm.OfficerId)
                        && (!supplyItemsVm.Last12hours || hsm.MoveDate.HasValue &&
                        DateTime.Now.Subtract(hsm.MoveDate.Value).TotalHours < supplyItemsVm.Hours)
                        && (supplyItemsVm.DateRange == false ||
                        (hsm.MoveDate.Value.Date >= supplyItemsVm.FromDate.Value.Date
                        && hsm.MoveDate.Value.Date <= supplyItemsVm.ToDate.Value.Date))
                        // 1 = Consumed Item,2 = Damaged Item,3 = Deleted Item
                        && (supplyItemsVm.SearchTextValue != 1 || hsi.ConsumedFlag == 1)
                        && (supplyItemsVm.SearchTextValue != 2 || hsi.DamageFlag == 1)
                        && (supplyItemsVm.SearchTextValue != 3 || hsi.DeleteFlag == 1))),
                (s, m) => new SupplyItemsVm
                {
                    ClosetName = s.HousingSupplyModule.ClosetName,
                    ItemName = s.HousingSupplyItemLookup.ItemName,
                    SupplyNumber = s.SupplyNumber,
                    MoveDate = m.MoveDate,
                    MoveBy = m.MoveBy,
                    MoveNote = m.MoveNote,
                    FromModuleId = m.FromHousingSupplyModuleId,
                    ToModuleId = m.ToHousingSupplyModuleId,
                    PersonnelMoveDetails = new PersonnelVm
                    {
                        PersonLastName = m.MoveByNavigation.PersonNavigation.PersonLastName,
                        OfficerBadgeNumber = m.MoveByNavigation.OfficerBadgeNum
                    },
                    ConsumedFlag = s.ConsumedFlag == NumericConstants.ONE,
                    DamageFlag = s.DamageFlag == NumericConstants.ONE,
                    DeleteFlag = s.DeleteFlag == NumericConstants.ONE,
                    ConsumedDate = s.ConsumedDate,
                    DamageDate = s.DamageFlagDate,
                    DeleteDate = s.DeleteDate,
                    Checked = true
                }).ToList();
            if (supplyItemsVm.HousingSupplyCountFlag)
            {
                if (!string.IsNullOrEmpty(supplyItemsVm.ItemName) &&
                    !string.IsNullOrEmpty(supplyItemsVm.SupplyNumber))
                {
                    List<string> lstItemName = supplyItemsVm.ItemName.Split(",").ToList();
                    List<string> lstSupplyNumber = supplyItemsVm.SupplyNumber.Split(",").ToList();
                    supplyItemsVm.SupplyList = new List<SupplyItemsVm>();
                    for (int index = 0; lstItemName.Count > index;)
                    {
                        supplyItemsVm.SupplyList.Add(new SupplyItemsVm());
                        supplyItemsVm.SupplyList[index].ItemName = lstItemName[index];
                        supplyItemsVm.SupplyList[index].SupplyNumber = lstSupplyNumber[index];
                        index++;
                    }
                    lstMove = lstMove.Where(w =>
                    supplyItemsVm.SupplyList.Any(a => a.ItemName == w.ItemName &&
                        a.SupplyNumber == w.SupplyNumber)).ToList();
                }
                else
                {
                    lstMove = lstMove.Where(w => w.ItemName == supplyItemsVm.ItemName &&
                    w.SupplyNumber == supplyItemsVm.SupplyNumber).ToList();
                }
            }
            int[] fromHousingSupplyModuleIds = lstMove.Select(s => s.FromModuleId ?? 0).ToArray();
            int[] toHousingSupplyModuleIds = lstMove.Select(s => s.ToModuleId ?? 0).ToArray();
            List<HousingSupplyModule> lstFromHousing = _context.HousingSupplyModule
                .Where(w => fromHousingSupplyModuleIds.Contains(w.HousingSupplyModuleId))
                .Select(s => new HousingSupplyModule
                {
                    HousingUnitLocation = s.HousingUnitLocation,
                    HousingUnitNumber = s.HousingUnitNumber
                }).ToList();
            List<HousingSupplyModule> lstToHousing = _context.HousingSupplyModule
                .Where(w => toHousingSupplyModuleIds.Contains(w.HousingSupplyModuleId))
                .Select(s => new HousingSupplyModule
                {
                    HousingUnitLocation = s.HousingUnitLocation,
                    HousingUnitNumber = s.HousingUnitNumber
                }).ToList();
            lstMove.ForEach(item =>
            {
                HousingSupplyModule fromHousing =
                    lstFromHousing.Single(w => w.HousingSupplyModuleId == item.FromModuleId);
                HousingSupplyModule toHousing = lstToHousing.Single(w => w.HousingSupplyModuleId == item.ToModuleId);

                item.FromLocation = fromHousing.HousingUnitLocation;
                item.FromNumber = fromHousing.HousingUnitNumber;
                item.ToLocation = toHousing.HousingUnitLocation;
                item.ToNumber = toHousing.HousingUnitNumber;
            });

            return lstMove.GroupBy(g => new
            {
                g.ClosetName,
                g.ItemName,
                g.SupplyNumber,
                g.MoveDate,
                g.MoveBy,
                g.MoveNote,
                g.FromLocation,
                g.FromNumber,
                g.ToLocation,
                g.ToNumber,
                g.ConsumedFlag,
                g.DamageFlag,
                g.DeleteFlag,
                g.ConsumedDate,
                g.DamageDate,
                g.DeleteDate,
                g.Checked

            }).Select(s => new SupplyItemsVm
            {
                Quantity = s.Count(),
                ClosetName = s.Key.ClosetName,
                ItemName = s.Key.ItemName,
                SupplyNumber = s.Key.SupplyNumber,
                MoveDate = s.Key.MoveDate,
                MoveBy = s.Key.MoveBy,
                ActionType = ActionType.MOVE,
                MoveNote = s.Key.MoveNote,
                FromLocation = s.Key.FromLocation,
                FromNumber = s.Key.FromNumber,
                ToLocation = s.Key.ToLocation,
                ToNumber = s.Key.ToNumber,
                PersonnelMoveDetails = new PersonnelVm
                {
                    PersonLastName = s.Select(w => w.PersonnelMoveDetails.PersonLastName).FirstOrDefault(),
                    OfficerBadgeNumber = s.Select(w => w.PersonnelMoveDetails.OfficerBadgeNumber).FirstOrDefault(),
                },
                ConsumedFlag = s.Key.ConsumedFlag,
                DamageFlag = s.Key.DamageFlag,
                DeleteFlag = s.Key.DeleteFlag,
                ConsumedDate = s.Key.ConsumedDate,
                DamageDate = s.Key.DamageDate,
                DeleteDate = s.Key.DeleteDate,
                Checked = s.Key.Checked

            }).ToList();
        }

        private List<SupplyItemsVm> GetCheckListDetails(SupplyItemsVm supplyItemsVm)
        {

            // CHECKLIST details
            List<SupplyItemsVm> lstChecklist = _context.HousingSupplyItem.SelectMany(si =>
                    _context.HousingSupplyCheckListXref.Where(cl =>
                        (!supplyItemsVm.HousingItemFlag ||
                        cl.HousingSupplyItemId == si.HousingSupplyItemId
                        && si.SupplyNumber == supplyItemsVm.SupplyNumber
                        && si.HousingSupplyItemLookup.ItemName == supplyItemsVm.ItemName
                        && cl.HousingSupplyCheckList.CheckListDate.HasValue
                        && supplyItemsVm.ListModuleId.Contains(si.HousingSupplyModule.HousingSupplyModuleId))
                        && (supplyItemsVm.HousingItemFlag || (cl.HousingSupplyItemId == si.HousingSupplyItemId)
                        && (supplyItemsVm.ListModuleId == null || supplyItemsVm.ListModuleId.Length <= 0 ||
                        supplyItemsVm.ListModuleId.Contains(si.HousingSupplyModule.HousingSupplyModuleId))
                        && (supplyItemsVm.DeletedHistory ||
                            !si.DeleteFlag.HasValue || si.DeleteFlag == 0)
                        && cl.HousingSupplyCheckList.CheckListDate.HasValue
                        && (supplyItemsVm.FacilityId == 0 || si.HousingSupplyModule.FacilityId == supplyItemsVm.FacilityId)
                        && (supplyItemsVm.OfficerId == 0 || cl.HousingSupplyCheckList.CheckListBy == supplyItemsVm.OfficerId)
                        && (!supplyItemsVm.Last12hours || cl.HousingSupplyCheckList.CheckListDate.HasValue &&
                        DateTime.Now.Subtract(cl.HousingSupplyCheckList.CheckListDate.Value).TotalHours < supplyItemsVm.Hours)
                        && (!supplyItemsVm.DateRange ||
                        (cl.HousingSupplyCheckList.CheckListDate.Value.Date >= supplyItemsVm.FromDate.Value.Date
                        && cl.HousingSupplyCheckList.CheckListDate.Value.Date <= supplyItemsVm.ToDate.Value.Date))
                        // 1 = Consumed Item,2 = Damaged Item,3 = Deleted Item
                        && (supplyItemsVm.SearchTextValue != 1 || si.ConsumedFlag == 1)
                        && (supplyItemsVm.SearchTextValue != 2 || si.DamageFlag == 1)
                        && (supplyItemsVm.SearchTextValue != 3 || si.DeleteFlag == 1))),
                (ssi, scl) => new SupplyItemsVm
                {
                    ClosetName = ssi.HousingSupplyModule.ClosetName,
                    ItemName = ssi.HousingSupplyItemLookup.ItemName,
                    SupplyNumber = ssi.SupplyNumber,
                    CheckListDate = scl.HousingSupplyCheckList.CheckListDate,
                    CheckListBy = scl.HousingSupplyCheckList.CheckListBy,
                    CheckListNote = scl.HousingSupplyCheckList.CheckListNote,
                    CheckListFlag = scl.CheckListFlag,
                    PersonnelCheckListDetails = new PersonnelVm
                    {
                        PersonLastName =
                            scl.HousingSupplyCheckList.CheckListByNavigation.PersonNavigation.PersonLastName,
                        OfficerBadgeNumber = scl.HousingSupplyCheckList.CheckListByNavigation.OfficerBadgeNum
                    },
                    ConsumedFlag = ssi.ConsumedFlag == NumericConstants.ONE,
                    DamageFlag = ssi.DamageFlag == NumericConstants.ONE,
                    DeleteFlag = ssi.DeleteFlag == NumericConstants.ONE,
                    ConsumedDate = ssi.ConsumedDate,
                    DamageDate = ssi.DamageFlagDate,
                    DeleteDate = ssi.DeleteDate,
                    Checked = true
                }).GroupBy(g => new
                {
                    g.ClosetName,
                    g.ItemName,
                    g.SupplyNumber,
                    g.CheckListDate,
                    g.CheckListBy,
                    g.CheckListNote,
                    g.CheckListFlag,
                    g.ConsumedFlag,
                    g.DamageFlag,
                    g.DeleteFlag,
                    g.ConsumedDate,
                    g.DamageDate,
                    g.DeleteDate,
                    g.Checked

                }).Select(s => new SupplyItemsVm
                {
                    Quantity = s.Count(),
                    ClosetName = s.Key.ClosetName,
                    ItemName = s.Key.ItemName,
                    SupplyNumber = s.Key.SupplyNumber,
                    ActionType = ActionType.CHECKLIST,
                    CheckListDate = s.Key.CheckListDate,
                    CheckListBy = s.Key.CheckListBy,
                    CheckListNote = s.Key.CheckListNote,
                    CheckListFlag = s.Key.CheckListFlag,
                    PersonnelCheckListDetails = new PersonnelVm
                    {
                        PersonLastName = s.Select(w => w.PersonnelCheckListDetails.PersonLastName).FirstOrDefault(),
                        OfficerBadgeNumber = s.Select(w => w.PersonnelCheckListDetails.OfficerBadgeNumber).FirstOrDefault()
                    },
                    ConsumedFlag = s.Key.ConsumedFlag,
                    DamageFlag = s.Key.DamageFlag,
                    DeleteFlag = s.Key.DeleteFlag,
                    ConsumedDate = s.Key.ConsumedDate,
                    DamageDate = s.Key.DamageDate,
                    DeleteDate = s.Key.DeleteDate,
                    Checked = s.Key.Checked
                }).ToList();
            if (supplyItemsVm.HousingSupplyCountFlag)
            {
                if (!string.IsNullOrEmpty(supplyItemsVm.ItemName) &&
                     !string.IsNullOrEmpty(supplyItemsVm.SupplyNumber))
                {
                    List<string> lstItemName = supplyItemsVm.ItemName.Split(",").ToList();
                    List<string> lstSupplyNumber = supplyItemsVm.SupplyNumber.Split(",").ToList();
                    supplyItemsVm.SupplyList = new List<SupplyItemsVm>();
                    for (int index = 0; lstItemName.Count > index;)
                    {
                        supplyItemsVm.SupplyList.Add(new SupplyItemsVm());
                        supplyItemsVm.SupplyList[index].ItemName = lstItemName[index];
                        supplyItemsVm.SupplyList[index].SupplyNumber = lstSupplyNumber[index];
                        index++;
                    }
                    lstChecklist = lstChecklist.Where(w =>
                    supplyItemsVm.SupplyList.Any(a => a.ItemName == w.ItemName && a.SupplyNumber == w.SupplyNumber)
                    ).ToList();
                }
                else
                {
                    lstChecklist = lstChecklist.Where(w => w.ItemName == supplyItemsVm.ItemName &&
                    w.SupplyNumber == supplyItemsVm.SupplyNumber).ToList();
                }
            }

            return lstChecklist;
        }

        public async Task<int> InsertSupplyDetails(List<SupplyVm> supplyItems)
        {
            supplyItems.ForEach(item =>
            {
                HousingSupplyCheckout supplyCheckout = new HousingSupplyCheckout
                {
                    HousingSupplyItemId = item.HousingSupplyItemId,
                    CheckoutInmateId = item.CheckoutInmateId?.ToString(),
                    CheckoutNote = item.CheckoutNote,
                    CheckoutHousing = item.CheckoutHousing,
                    CheckoutLocation = item.CheckoutLocation,
                    CheckoutLocationId = item.CheckoutLocationId,
                    CheckoutDate = DateTime.Now,
                    CheckoutBy = _personnelId
                };
                _context.HousingSupplyCheckout.Add(supplyCheckout);
                UpdateSupplyItems(item.HousingSupplyItemId, item);
            });
            return await _context.SaveChangesAsync();
        }

        private void UpdateSupplyItems(int housingSupplyItemId, SupplyVm supplyItems)
        {
            IQueryable<HousingSupplyItem> listHousingSupplyItem = _context.HousingSupplyItem
                .Where(w => w.HousingSupplyItemId == housingSupplyItemId);

            int consumable = listHousingSupplyItem.Select(w => w.HousingSupplyItemLookup.ConsumableFlag).Single();

            HousingSupplyItem housingSupplyItem = listHousingSupplyItem.Single();

            if (consumable == 1)
            {
                housingSupplyItem.ConsumedFlag = 1;
                housingSupplyItem.ConsumedBy = _personnelId;
                housingSupplyItem.ConsumedDate = DateTime.Now;
            }

            housingSupplyItem.CurrentCheckoutInmateId = supplyItems.CheckoutInmateId;
            housingSupplyItem.CurrentCheckoutHousing = supplyItems.CheckoutHousing;
            housingSupplyItem.CurrentCheckoutLocation = supplyItems.CheckoutLocation;
            housingSupplyItem.CurrentCheckoutModuleId = supplyItems.CheckoutModuleId;
            housingSupplyItem.CurrentCheckoutDateTime = supplyItems.CheckoutDateTime;
        }


        public async Task<int> UpdateDamage(List<SupplyVm> supplyItems)
        {
            supplyItems.ForEach(item =>
            {
                HousingSupplyItem housingSupplyItem = _context.HousingSupplyItem
                    .Single(w => w.HousingSupplyItemId == item.HousingSupplyItemId);

                housingSupplyItem.DamageFlag = item.DamageFlag;
                housingSupplyItem.DamageFlagNote = item.DamageNote;
                housingSupplyItem.DamageFlagBy = item.DamageFlagBy;
                housingSupplyItem.DamageFlagDate = item.DamageFlagDate;
            });

            return await _context.SaveChangesAsync();
        }

        public async Task<int> UpdateCheckInTo(List<SupplyVm> supplyItems)
        {
            supplyItems.ForEach(item =>
            {
                UpdateSupplyItems(item.HousingSupplyItemId, item);
                HousingSupplyCheckout supplyCheckout = _context.HousingSupplyCheckout
                    .Single(w => w.HousingSupplyCheckoutId == item.HousingSupplyCheckoutId);
                supplyCheckout.CheckinDate = DateTime.Now;
                supplyCheckout.CheckinBy = _personnelId;
                supplyCheckout.CheckinNote = item.CheckInNote;
            });
            return await _context.SaveChangesAsync();
        }

        #endregion
        //In-progress
        public List<SupplyItemsVm> GetManageSupplyItem(int facilityId, List<int> housingSupplyModuleId) //Tuning - In Progress
        {
            List<SupplyItemsVm> listHousingSupplyItem = _context.HousingSupplyItem
               .Where(w => (facilityId <= 0 || w.HousingSupplyModule.FacilityId == facilityId)
                           // && !w.HousingSupplyModule.DeleteFlag.HasValue
                           && !Convert.ToBoolean(w.HousingSupplyItemLookup.DeleteFlag) 
                           && (housingSupplyModuleId.Count <= 0 ||
                               housingSupplyModuleId.Contains(w.HousingSupplyModule
                                   .HousingSupplyModuleId)))
               .Select(s => new SupplyItemsVm
               {
                   ItemName = s.HousingSupplyItemLookup.ItemName,
                   ClosetName = s.HousingSupplyModule.ClosetName,
                   SupplyNumber = s.SupplyNumber,
                   SupplyDescription = s.SupplyDescription,
                   HousingSupplyItemLookupId = s.HousingSupplyItemLookup.HousingSupplyItemLookupId,
                   HousingSupplyModuleId = s.HousingSupplyModuleId,
                   DeleteFlag = s.DeleteFlag == NumericConstants.ONE,
                   AllowCheckoutToLocation = s.AllowCheckoutToLocation == NumericConstants.ONE,
                   AllowCheckoutToInmate = s.AllowCheckoutToInmate == NumericConstants.ONE,
                   AllowCheckoutToHousing = s.AllowCheckoutToHousing == NumericConstants.ONE,
                   FlagDurationMin = s.FlagDurationMin ?? 0,
                   ConsumedFlag =  s.ConsumedFlag == NumericConstants.ONE,
                   DamageFlag = s.DamageFlag == NumericConstants.ONE,
                   HousingSupplyItemId = s.HousingSupplyItemId
               }).ToList();

            int[] listItemId = listHousingSupplyItem.Select(s => s.HousingSupplyItemId).ToArray();
            List<HousingSupplyCheckout> listCheckedCheckouts = _context.HousingSupplyCheckout
                .Where(w => listItemId.Contains(w.HousingSupplyItemId)).Select(s => new HousingSupplyCheckout
                {
                    HousingSupplyItemId = s.HousingSupplyItemId,
                    CheckoutDate = s.CheckoutDate,
                    CheckinDate = s.CheckinDate
                }).ToList();

            listHousingSupplyItem.ForEach(item =>
            {
                HousingSupplyCheckout checkoutDetail =
                    listCheckedCheckouts.LastOrDefault(s => s.HousingSupplyItemId == item.HousingSupplyItemId);
                if (checkoutDetail != null)
                {
                    item.CheckoutDate = checkoutDetail.CheckoutDate;
                    item.CheckInDate = checkoutDetail.CheckinDate;
                }
            });


            //available item
            List<SupplyItemsVm> supplyManageItems = new List<SupplyItemsVm>();

            supplyManageItems.AddRange(listHousingSupplyItem
               .Where(w => (!w.CheckInDate.HasValue && !w.CheckoutDate.HasValue || w.CheckInDate.HasValue && w.CheckoutDate.HasValue) && (!w.ConsumedFlag || w.ConsumedFlag) && (!w.DamageFlag || w.DamageFlag) && !w.DeleteFlag)
               .GroupBy(g => new
               {
                   g.ClosetName,
                   g.ItemName,
                   g.SupplyNumber,
                   g.SupplyDescription,
                   g.HousingSupplyItemLookupId,
                   g.HousingSupplyModuleId,
                   g.AllowCheckoutToLocation,
                   g.AllowCheckoutToInmate,
                   g.AllowCheckoutToHousing,
                   g.FlagDurationMin
               }).Select(s => new SupplyItemsVm
               {
                   ItemName = s.Key.ItemName,
                   ClosetName = s.Key.ClosetName,
                   SupplyNumber = s.Key.SupplyNumber,
                   SupplyDescription = s.Key.SupplyDescription,
                   HousingSupplyItemLookupId = s.Key.HousingSupplyItemLookupId,
                   HousingSupplyModuleId = s.Key.HousingSupplyModuleId,
                   AllowCheckoutToLocation = s.Key.AllowCheckoutToLocation,
                   AllowCheckoutToInmate = s.Key.AllowCheckoutToInmate,
                   AllowCheckoutToHousing = s.Key.AllowCheckoutToHousing,
                   FlagDurationMin = s.Key.FlagDurationMin,
                   Quantity = s.Count(c => (!c.CheckInDate.HasValue && !c.CheckoutDate.HasValue || c.CheckInDate.HasValue && c.CheckoutDate.HasValue) && !c.ConsumedFlag && !c.DamageFlag && !c.DeleteFlag),
                   ConsumedFlagCount = s.Count(c => c.CheckInDate.HasValue && c.CheckoutDate.HasValue && c.ConsumedFlag && !c.DamageFlag && !c.DeleteFlag),
                   DamagedFlagCount = s.Count(c => ((c.CheckInDate.HasValue && c.CheckoutDate.HasValue && c.DamageFlag && !c.DeleteFlag) ||
                                                   (!c.CheckInDate.HasValue && !c.CheckoutDate.HasValue && c.DamageFlag && !c.DeleteFlag))),
                   DeleteFlag = s.Select(w => w.DeleteFlag).FirstOrDefault(),
                   CheckInDate = s.Select(w => w.CheckInDate).FirstOrDefault(),
                   CheckoutDate = s.Select(w => w.CheckoutDate).FirstOrDefault()
               }).ToList());

            //checkout
            supplyManageItems.AddRange(listHousingSupplyItem
              .Where(w => !w.CheckInDate.HasValue && w.CheckoutDate.HasValue)
              .GroupBy(g => new
              {
                  g.ClosetName,
                  g.ItemName,
                  g.SupplyNumber,
                  g.HousingSupplyItemLookupId,
                  g.HousingSupplyModuleId,

              }).Select(s => new SupplyItemsVm
              {
                  ItemName = s.Key.ItemName,
                  ClosetName = s.Key.ClosetName,
                  SupplyNumber = s.Key.SupplyNumber,
                  HousingSupplyItemLookupId = s.Key.HousingSupplyItemLookupId,
                  HousingSupplyModuleId = s.Key.HousingSupplyModuleId,
                  Quantity = s.Count(),
                  ConsumedFlagCount = s.Count(se => se.ConsumedFlag && !se.DamageFlag),
                  DamagedFlagCount = s.Count(se => se.DamageFlag),
                  DeleteFlag = s.Select(w => w.DeleteFlag).FirstOrDefault(),
                  CheckoutDate = s.Select(w => w.CheckoutDate).FirstOrDefault(),
                  CheckInDate = s.Select(w => w.CheckInDate).FirstOrDefault(),
                  SupplyDescription = s.Select(w => w.SupplyDescription).FirstOrDefault(),
                  AllowCheckoutToHousing = s.Select(w => w.AllowCheckoutToHousing).FirstOrDefault(),
                  AllowCheckoutToLocation = s.Select(w => w.AllowCheckoutToLocation).FirstOrDefault(),
                  AllowCheckoutToInmate = s.Select(w => w.AllowCheckoutToInmate).FirstOrDefault()
              }).ToList());

            //delete count
            supplyManageItems.AddRange(listHousingSupplyItem
                .Where(w => w.DeleteFlag)
                .GroupBy(g => new
                {
                    g.ClosetName,
                    g.ItemName,
                    g.SupplyNumber,
                    g.HousingSupplyItemLookupId,
                    g.HousingSupplyModuleId,
                }).Select(s => new SupplyItemsVm
                {
                    ItemName = s.Key.ItemName,
                    ClosetName = s.Key.ClosetName,
                    SupplyNumber = s.Key.SupplyNumber,
                    HousingSupplyItemLookupId = s.Key.HousingSupplyItemLookupId,
                    HousingSupplyModuleId = s.Key.HousingSupplyModuleId,
                    Quantity = s.Count(w => !w.ConsumedFlag && !w.DamageFlag),
                    ConsumedFlagCount = s.Count(se => se.ConsumedFlag && !se.DamageFlag),
                    DamagedFlagCount = s.Count(se => se.DamageFlag),
                    DeleteFlag = s.Select(w => w.DeleteFlag).FirstOrDefault(),
                    CheckoutDate = s.Select(w => w.CheckoutDate).FirstOrDefault(),
                    CheckInDate = s.Select(w => w.CheckInDate).FirstOrDefault(),
                    SupplyDescription = s.Select(w => w.SupplyDescription).FirstOrDefault(),
                    AllowCheckoutToHousing = s.Select(w => w.AllowCheckoutToHousing).FirstOrDefault(),
                    AllowCheckoutToLocation = s.Select(w => w.AllowCheckoutToLocation).FirstOrDefault(),
                    AllowCheckoutToInmate = s.Select(w => w.AllowCheckoutToInmate).FirstOrDefault()
                }).ToList());
            return supplyManageItems;
        }

        public List<SupplyItemsVm> GetSupplyItemLookup()
        {
            List<SupplyItemsVm> listHousingSupplyLookup = _context.HousingSupplyItemLookup
                .Where(w => !w.DeleteFlag.HasValue)
                .Select(s => new SupplyItemsVm
                {
                    HousingSupplyItemLookupId = s.HousingSupplyItemLookupId,
                    ItemName = s.ItemName
                }).ToList();

            return listHousingSupplyLookup;
        }

        public int CheckSupplyItemExist(SupplyItemsVm supplyItems)
        {
            List<HousingSupplyItem> housingSupplyItem = _context.HousingSupplyItem
                .Where(w => w.HousingSupplyItemLookupId == supplyItems.HousingSupplyItemLookupId
                            && w.HousingSupplyModuleId == supplyItems.HousingSupplyModuleId
                            && w.SupplyNumber == supplyItems.SupplyNumber &&
                            (!w.DeleteFlag.HasValue || w.DeleteFlag.Value != 1)
                            && (!w.HousingSupplyModule.DeleteFlag.HasValue ||
                                w.HousingSupplyModule.DeleteFlag.Value != 1)).Select(s => s).ToList();

            return housingSupplyItem.Count;
        }

        public async Task<int> InsertSupplyItem(SupplyItemsVm supplyItems)
        {
            if (CheckSupplyItemExist(supplyItems) == 0 || supplyItems.AddQuantity)
            {
                List<HousingSupplyItemHistory> listHistory = new List<HousingSupplyItemHistory>();
                DateTime currentDate = DateTime.Now;
                for (int i = 1; i <= supplyItems.Quantity; i++)
                {
                    HousingSupplyItem housingSupplyItem = new HousingSupplyItem
                    {
                        HousingSupplyModuleId = supplyItems.HousingSupplyModuleId,
                        HousingSupplyItemLookupId = supplyItems.HousingSupplyItemLookupId,
                        SupplyDescription = supplyItems.SupplyDescription,
                        SupplyNumber = supplyItems.SupplyNumber,
                        AllowMove = NumericConstants.ONE,
                        AllowCheckoutToLocation = supplyItems.AllowCheckoutToLocation
                            ? NumericConstants.ONE
                            : NumericConstants.ZERO,
                        AllowCheckoutToInmate = supplyItems.AllowCheckoutToInmate
                            ? NumericConstants.ONE
                            : NumericConstants.ZERO,
                        AllowCheckoutToHousing = supplyItems.AllowCheckoutToHousing
                            ? NumericConstants.ONE
                            : NumericConstants.ZERO,
                        FlagDurationMin = supplyItems.FlagDurationMin,
                        CreateDate = currentDate,
                        CreatedBy = _personnelId
                    };
                    _context.HousingSupplyItem.Add(housingSupplyItem);

                    listHistory.Add(new HousingSupplyItemHistory
                    {
                        CreateDate = currentDate,
                        HousingSupplyItemId = housingSupplyItem.HousingSupplyItemId,
                        PersonnelId = _personnelId,
                        HousingSupplyItemHistoryList = (supplyItems.ConsumedFlag || supplyItems.DamageFlag) ? supplyItems.HousingSupplyItemHistoryReplaceList : supplyItems.HousingSupplyItemHistoryList
                    });

                }

                _context.HousingSupplyItemHistory.AddRange(listHistory);
            }

            return await _context.SaveChangesAsync();
        }

        public async Task<int> UpdateSupplyItem(SupplyItemsVm supplyItems)
        {
            List<HousingSupplyItem> housingSupplyItem = _context.HousingSupplyItem
                .Where(w => w.HousingSupplyItemLookupId == supplyItems.HousingSupplyItemLookupId
                            && w.HousingSupplyModuleId == supplyItems.HousingSupplyModuleId &&
                            w.SupplyNumber == supplyItems.SupplyNumber && w.ConsumedFlag != NumericConstants.ONE && w.DamageFlag != NumericConstants.ONE && w.DeleteFlag != NumericConstants.ONE).ToList();

            DateTime currentDate = DateTime.Now;
            housingSupplyItem.ForEach(item =>
            {
                item.UpdatedBy = _personnelId;
                item.UpdateDate = currentDate;
                item.SupplyDescription = supplyItems.SupplyDescription;
                item.FlagDurationMin = supplyItems.FlagDurationMin;
                item.AllowCheckoutToHousing =
                    supplyItems.AllowCheckoutToHousing ? NumericConstants.ONE : NumericConstants.ZERO;
                item.AllowCheckoutToLocation =
                    supplyItems.AllowCheckoutToLocation ? NumericConstants.ONE : NumericConstants.ZERO;
                item.AllowCheckoutToInmate =
                    supplyItems.AllowCheckoutToInmate ? NumericConstants.ONE : NumericConstants.ZERO;
            });

            _context.HousingSupplyItem.UpdateRange(housingSupplyItem);

            return await _context.SaveChangesAsync();
        }

        public async Task<int> DeleteSupplyItem(SupplyItemsVm supplyItems) //In Progress
        {
            List<HousingSupplyItem> listHousingSupplyItem = _context.HousingSupplyItem
                .Where(w => (!w.DeleteFlag.HasValue || w.DeleteFlag == 0) &&
                            w.HousingSupplyItemLookupId == supplyItems.HousingSupplyItemLookupId
                            && w.HousingSupplyModuleId == supplyItems.HousingSupplyModuleId &&
                            w.SupplyNumber == supplyItems.SupplyNumber).ToList();

            int[] listItemId = listHousingSupplyItem.Select(s => s.HousingSupplyItemId).ToArray();
            List<HousingSupplyCheckout> listCheckedCheckouts = _context.HousingSupplyCheckout
                .Where(w => listItemId.Contains(w.HousingSupplyItemId)).Select(s => new HousingSupplyCheckout
                {
                    HousingSupplyItemId = s.HousingSupplyItemId,
                    CheckoutDate = s.CheckoutDate,
                    CheckinDate = s.CheckinDate
                }).ToList();

            if (supplyItems.ConsumedFlag && !supplyItems.DamageFlag)
            {
                listHousingSupplyItem = listHousingSupplyItem.Where(w => w.ConsumedFlag == NumericConstants.ONE && w.DamageFlag != NumericConstants.ONE && (!w.CurrentCheckoutDateTime.HasValue ||
                                                                              listCheckedCheckouts.Any(a => w.HousingSupplyItemId == a.HousingSupplyItemId && a.CheckinDate.HasValue))).ToList();
            }
            else if (!supplyItems.ConsumedFlag && supplyItems.DamageFlag)
            {
                listHousingSupplyItem = listHousingSupplyItem.Where(w => w.DamageFlag == NumericConstants.ONE && (!w.CurrentCheckoutDateTime.HasValue ||
                                                                              listCheckedCheckouts.Any(a => w.HousingSupplyItemId == a.HousingSupplyItemId && a.CheckinDate.HasValue))).ToList();
            }
            else if (!supplyItems.ConsumedFlag && !supplyItems.DamageFlag)
            {
                //listHousingSupplyItem = listHousingSupplyItem.Where(w => w.ConsumedFlag != NumericConstants.ONE && w.DamageFlag != NumericConstants.ONE && !w.CurrentCheckoutDateTime.HasValue).ToList();
                // Delete Flag Update
                listHousingSupplyItem = listHousingSupplyItem.Where(w => w.ConsumedFlag != NumericConstants.ONE && w.DamageFlag != NumericConstants.ONE).ToList();
            }

            DateTime currentDate = DateTime.Now;
            List<HousingSupplyItemHistory> listHistory = new List<HousingSupplyItemHistory>();
            for (int i = 0; i < supplyItems.Quantity; i++)
            {
                if (supplyItems.ConsumedFlag)
                {
                    listHousingSupplyItem[i].ConsumedFlagReplaced = NumericConstants.ONE;
                    listHousingSupplyItem[i].ConsumedFlagReplacedBy = _personnelId;
                    listHousingSupplyItem[i].ConsumedFlagReplacedDate = currentDate;
                }

                if (supplyItems.DamageFlag)
                {
                    listHousingSupplyItem[i].DamageFlagReplaced = NumericConstants.ONE;
                    listHousingSupplyItem[i].DamageFlagReplacedBy = _personnelId;
                    listHousingSupplyItem[i].DamageFlagReplacedDate = currentDate;
                }

                listHousingSupplyItem[i].DeleteFlag = NumericConstants.ONE;
                listHousingSupplyItem[i].DeletedBy = _personnelId;
                listHousingSupplyItem[i].DeleteDate = currentDate;

                listHistory.Add(new HousingSupplyItemHistory
                {
                    CreateDate = currentDate,
                    HousingSupplyItemId = listHousingSupplyItem[i].HousingSupplyItemId,
                    PersonnelId = _personnelId,
                    HousingSupplyItemHistoryList = supplyItems.HousingSupplyItemHistoryList
                });

            }

            _context.HousingSupplyItemHistory.AddRange(listHistory);
            if (supplyItems.ConsumedFlag || supplyItems.DamageFlag)
            {
                supplyItems.AddQuantity = true;
                await InsertSupplyItem(supplyItems);
            }

            return await _context.SaveChangesAsync();
        }

        public WareHouseLookup GetWareHouseLookup(int facilityId)
        {
            WareHouseLookup wareHouseLookup = new WareHouseLookup();
            List<HousingDetail> listHousingDetail = _context.HousingUnit.Where(w => w.FacilityId == facilityId)
                .Select(s => new HousingDetail
                {
                    HousingUnitLocation = s.HousingUnitLocation,
                    HousingUnitNumber = s.HousingUnitNumber,
                    HousingUnitListId = s.HousingUnitListId
                }).ToList();

            List<WareHouseItemVm> listWareHouseItem = _context.WarehouseItem
                .Where(w => w.FacilityId == facilityId && !w.DeleteFlag.HasValue)
                .Select(s => new WareHouseItemVm
                {
                    ItemName = s.ItemName,
                    WareHouseItemId = s.WarehouseItemId,
                    ItemCategory = s.ItemCategory
                }).ToList();

            wareHouseLookup.ListLookup =
                _commonService.GetLookups(new[] { LookupConstants.WAREHOUSECAT, LookupConstants.DELIVERDISPO });

            wareHouseLookup.ListHousingDetail = listHousingDetail;
            wareHouseLookup.ListWareHouseItem = listWareHouseItem;
            return wareHouseLookup;
        }

        public List<WareHouseItemVm> GetHousingActiveRequestDetail(WareHouseItemVm wareHouseItem)
        {
            List<WareHouseItemVm> listHousingActive = _context.WarehouseRequest
                .Where(w => w.FacilityId == wareHouseItem.FacilityId &&
                            w.HousingBuilding == wareHouseItem.HousingBuilding
                            && w.HousingNumber == wareHouseItem.HousingNumber && !w.DeleteFlag.HasValue)
                .Select(s => new WareHouseItemVm
                {
                    WareHouseRequestId = s.WarehouseRequestId,
                    HousingBuilding = s.HousingBuilding,
                    HousingNumber = s.HousingNumber,
                    RequestedDate = s.RequestedDate,
                    ItemCategory = s.WarehouseItem.ItemCategory,
                    ItemName = s.WarehouseItem.ItemName,
                    RequestedQty = s.RequestedQty,
                    RequestNote = s.RequestNote,
                    TotalDeliveredQty = s.TotalDeliveredQty,
                    DeliveryDispo = s.DeliveryDispo,
                    DeliveryNote = s.DeliveryNote,
                    CompleteFlag = s.CompleteFlag,
                    InProgressFlag = s.InProgressFlag,
                    DeleteFlag = s.DeleteFlag,
                    RequestedBy = s.RequestedBy,
                    CompleteDate = s.CompleteDate,
                    CompleteBy = s.CompleteBy
                }).ToList();

            if (listHousingActive.Count > 0)
            {
                List<int> personnelId = listHousingActive.Select(i => new[] { i.RequestedBy, i.CompleteBy })
                    .SelectMany(i => i)
                    .Where(i => i.HasValue)
                    .Select(i => i.Value)
                    .ToList();
                List<PersonnelVm> lstPersonDet = _personService.GetPersonNameList(personnelId);

                listHousingActive.ForEach(item =>
                {
                    if (item.RequestedBy.HasValue)
                    {
                        PersonnelVm personDetail = lstPersonDet.SingleOrDefault(s => s.PersonnelId == item.RequestedBy);
                        item.RequestedByPerson = new PersonnelVm
                        {
                            PersonLastName = personDetail?.PersonLastName,
                            PersonFirstName = personDetail?.PersonFirstName,
                            PersonMiddleName = personDetail?.PersonMiddleName,
                            PersonnelNumber = personDetail?.PersonnelNumber
                        };
                    }

                    if (item.CompleteBy.HasValue)
                    {
                        PersonnelVm personDetail = lstPersonDet.SingleOrDefault(s => s.PersonnelId == item.RequestedBy);
                        item.CompleteByPerson = new PersonnelVm
                        {
                            PersonLastName = personDetail?.PersonLastName,
                            PersonnelNumber = personDetail?.PersonnelNumber
                        };
                    }
                });
            }

            return listHousingActive;
        }

        public async Task<int> InsertCheckOutItem(HousingSupplyVm housingSupply, string note)
        {
            HousingSupplyCheckList checkList = new HousingSupplyCheckList
            {
                CheckListDate = DateTime.Now,
                CheckListBy = _personnelId,
                CheckListNote = note,
                CreateDate = DateTime.Now,
                CreatedBy = _personnelId
            };
            _context.HousingSupplyCheckList.Add(checkList);
            _context.SaveChanges();

            int checkListId = checkList.HousingSupplyCheckListId;

            InsertSupplyCheckListXref(checkListId, housingSupply.LstCheckoutItems);
            InsertSupplyCheckListXref(checkListId, housingSupply.LstAvailableItems);
            return await _context.SaveChangesAsync();
        }

        private void InsertSupplyCheckListXref(int checkListId, List<SupplyItemsVm> supplyList)
        {
            supplyList.ForEach(item =>
            {

                int flag = (item.InOutCount == item.Quantity) ? 1 : 0;

                item.LstSupplyItemId.ForEach(key =>
                {
                    HousingSupplyCheckListXref lstXref = new HousingSupplyCheckListXref
                    {
                        HousingSupplyCheckListId = checkListId,
                        HousingSupplyItemId = key,
                        CheckListFlag = flag,
                        CheckListNote = item.CheckListNote,
                        CreateDate = DateTime.Now,
                        CreatedBy = _personnelId
                    };
                    _context.HousingSupplyCheckListXref.Add(lstXref);

                });
            });

        }

        public HousingSupplyVm GetSupplyItems(int facilityId, int housingSupplyModuleId, int checkListId)
        {

            HousingSupplyVm supplyItems = GetAvailableSupplyItems(facilityId, new int[] { housingSupplyModuleId });

            IQueryable<HousingSupplyCheckListXref> checkListXref =
                _context.HousingSupplyCheckListXref.Where(s => s.HousingSupplyCheckListId == checkListId);



            supplyItems.LstAvailableItems = GetSupplyItemNotes(supplyItems.LstAvailableItems, checkListXref);
            supplyItems.LstCheckoutItems = GetSupplyItemNotes(supplyItems.LstCheckoutItems, checkListXref);
            return supplyItems;
        }

        private List<SupplyItemsVm> GetSupplyItemNotes(List<SupplyItemsVm> supplyItems,
            IQueryable<HousingSupplyCheckListXref> checkListXref)
        {
            List<SupplyItemsVm> supplyDetails = new List<SupplyItemsVm>();
            supplyItems.ForEach(key =>
            {
                SupplyItemsVm itemNote = checkListXref.Where(c => key.LstSupplyItemId.Contains(c.HousingSupplyItemId))
                    .GroupBy(g => new
                    {
                        g.CheckListFlag,
                        g.CheckListNote
                    }).Select(c => new SupplyItemsVm
                    {
                        CheckListNote = c.Key.CheckListNote,
                        ItemFlag = c.Key.CheckListFlag
                    }).SingleOrDefault();

                if (itemNote != null)
                {
                    key.CheckListNote = itemNote.CheckListNote;
                    key.ItemFlag = itemNote.ItemFlag;
                    supplyDetails.Add(key);
                }
            });

            return supplyDetails;
        }

        public async Task<int> DeleteUndoCheckList(CheckListVm checkList)
        {
            HousingSupplyCheckList supplyCheckList =
                _context.HousingSupplyCheckList.Single(s => s.HousingSupplyCheckListId == checkList.CheckListId);

            DateTime? deleteDate = DateTime.Now;
            supplyCheckList.DeleteFlag = (supplyCheckList.DeleteFlag == 1) ? 0 : 1;
            supplyCheckList.DeletedBy = (supplyCheckList.DeleteFlag == 1) ? _personnelId : new int();
            supplyCheckList.DeleteDate = (supplyCheckList.DeleteFlag == 1) ? deleteDate : null;

            return await _context.SaveChangesAsync();
        }

        public async Task<int> WareHouseInsert(List<WareHouseItemVm> wareHouseItem)
        {
            List<WarehouseRequest> listWareHouseItem = wareHouseItem.Select(s =>
                new WarehouseRequest
                {
                    FacilityId = s.FacilityId,
                    HousingBuilding = s.HousingBuilding,
                    HousingNumber = s.HousingNumber,
                    WarehouseItemId = s.WareHouseItemId,
                    RequestedQty = s.RequestedQty,
                    RequestedDate = s.RequestedDate,
                    RequestedBy = _personnelId,
                    DeliveryDispo = s.DeliveryDispo,
                    RequestNote = s.RequestNote,
                    CreateDate = DateTime.Now,
                    CreatedBy = _personnelId,
                    HousingUnitListId = s.HousingUnitListId
                }).ToList();

            _context.WarehouseRequest.AddRange(listWareHouseItem);
            return await _context.SaveChangesAsync();
        }

        //In-progress
        public List<HousingSupplyItemHistoryVm> GetHousingSupplyHistory(SupplyItemsVm supplyItems)
        {
            List<SupplyItemsVm> housingSupplyItem = _context.HousingSupplyItem
                .Where(w => w.HousingSupplyItemLookupId == supplyItems.HousingSupplyItemLookupId
                            && w.HousingSupplyModuleId == supplyItems.HousingSupplyModuleId &&
                            w.SupplyNumber == supplyItems.SupplyNumber)
                .Select(s => new SupplyItemsVm
                {
                    ItemName = s.HousingSupplyItemLookup.ItemName,
                    ClosetName = s.HousingSupplyModule.ClosetName,
                    SupplyNumber = s.SupplyNumber,
                    HousingSupplyItemId = s.HousingSupplyItemId,
                    SupplyDescription = s.SupplyDescription,
                    HousingSupplyItemLookupId = s.HousingSupplyItemLookup.HousingSupplyItemLookupId,
                    HousingSupplyModuleId = s.HousingSupplyModuleId,
                    DeleteFlag = s.DeleteFlag == NumericConstants.ONE
                }).ToList();

            int[] listItemId = housingSupplyItem.Select(s => s.HousingSupplyItemId).ToArray();

            List<HousingSupplyItemHistoryVm> lstItemHistory = _context.HousingSupplyItemHistory
                .Where(w => listItemId.Contains(w.HousingSupplyItemId))
                .Select(s => new HousingSupplyItemHistoryVm
                {
                    HistoryId = s.HousingSupplyItemHistoryId,
                    CreateDate = s.CreateDate,
                    HousingSupplyItemId = s.HousingSupplyItemId,
                    HousingSupplyItemHistoryList = s.HousingSupplyItemHistoryList,
                    PersonnelId = s.PersonnelId
                }).ToList();

            lstItemHistory.ForEach(item =>
            {
                SupplyItemsVm supplyItem =
                    housingSupplyItem.Single(w => w.HousingSupplyItemId == item.HousingSupplyItemId);
                item.ItemName = supplyItem.ItemName;
                item.ClosetName = supplyItem.ClosetName;
                item.SupplyNumber = supplyItem.SupplyNumber;
                item.SupplyDescription = supplyItem.SupplyDescription;
                item.HousingSupplyItemLookupId = supplyItem.HousingSupplyItemLookupId;
                item.HousingSupplyModuleId = supplyItem.HousingSupplyModuleId;
            });

            lstItemHistory = lstItemHistory.GroupBy(g => new
            {
                g.ItemName,
                g.ClosetName,
                g.SupplyNumber,
                g.SupplyDescription,
                g.HousingSupplyItemLookupId,
                g.HousingSupplyModuleId,
                g.PersonnelId,
                g.CreateDate,
                g.HousingSupplyItemHistoryList
            }).Select(s => new HousingSupplyItemHistoryVm
            {
                CreateDate = s.Key.CreateDate,
                PersonnelId = s.Key.PersonnelId,
                HousingSupplyItemHistoryList = s.Key.HousingSupplyItemHistoryList
            }).ToList();

            List<int> listPersonnelId = lstItemHistory.Select(s => s.PersonnelId).ToList();
            List<PersonnelVm> listPerson = _personService.GetPersonNameList(listPersonnelId);

            lstItemHistory.ForEach(item =>
            {

                item.PersonLastName =
                    listPerson.SingleOrDefault(s => s.PersonnelId == item.PersonnelId)?.PersonLastName;
                item.OfficerBadgeNumber = listPerson.SingleOrDefault(s => s.PersonnelId == item.PersonnelId)
                    ?.OfficerBadgeNumber;
                //To GetJson Result Into Dictionary
                Dictionary<string, string> historyList =
                    JsonConvert.DeserializeObject<Dictionary<string, string>>(item.HousingSupplyItemHistoryList);
                item.HistoryList = historyList.Select(ph => new PersonHeader { Header = ph.Key, Detail = ph.Value })
                    .ToList();
            });
            return lstItemHistory;
        }
        public List<HousingSupplyHistoryLocation> GetSupplyHistoryLocation(int facilityId, int[] moduleId)
        {
            List<HousingSupplyHistoryLocation> listHousingSupplyHistoryLocation = _context.Privileges
                .Where(w => w.InactiveFlag == 0 && w.SupplyLocationFlag == 1)
                .Select(s => new HousingSupplyHistoryLocation
                {
                    PrivilegeDescription = s.PrivilegeDescription,
                    PrivilegeId = s.PrivilegeId,
                    FacilityId = s.FacilityId ?? 0
                }).ToList();
            List<HousingSupplyHistoryLocation> housingSupplyModule = _context.HousingSupplyModule
                .Where(w => listHousingSupplyHistoryLocation.Any(a => a.FacilityId == w.FacilityId) &&
                w.FacilityId == facilityId && moduleId.Contains(w.HousingSupplyModuleId))
                .Select(s => new HousingSupplyHistoryLocation
                {
                    FacilityId = s.FacilityId
                }).ToList();
            listHousingSupplyHistoryLocation = listHousingSupplyHistoryLocation
                .Where(w => housingSupplyModule.Any(a => a.FacilityId == w.FacilityId))
                .OrderBy(o => o.PrivilegeDescription).ToList();

            return listHousingSupplyHistoryLocation;
        }
    }
}
