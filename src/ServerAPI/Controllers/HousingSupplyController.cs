﻿using Microsoft.AspNetCore.Mvc;
using ServerAPI.Authorization;
using ServerAPI.Services;
using ServerAPI.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ServerAPI.Controllers
{
    [Route("[controller]")]
    public class HousingSupplyController : ControllerBase
    {
        private readonly IHousingSupplyService _housingSupplyService;

        public HousingSupplyController(IHousingSupplyService housingSupplyService)
        {
            _housingSupplyService = housingSupplyService;
        }

        [HttpGet("GetClosetDetails")]
        public IActionResult GetClosetDetails(int facilityId)
        {
            return Ok(_housingSupplyService.GetClosetDetails(facilityId));
        }

        #region SupplyItem       
        [FuncPermission(1110, FuncPermissionType.Access)]
        [HttpGet("GetAvailableSupplyItems")]
        public IActionResult GetAvailableSupplyItems(int facilityId, int[] moduleId)
        {
            return Ok(_housingSupplyService.GetAvailableSupplyItems(facilityId, moduleId));
        }
                
        [HttpGet("GetHistoryDetails")]
        public IActionResult GetAvailableHistoryLst(SupplyItemsVm supplyItemsVm)
        {
            return Ok(_housingSupplyService.GetAvailableHistoryLst(supplyItemsVm));
        }

        [FuncPermission(1116, FuncPermissionType.Add)]
        [HttpPost("InsertSupplyDetailsByHousing")]
        public async Task<IActionResult> InsertSupplyDetailsByHousing([FromBody] List<SupplyVm> supplyItems)
        {
            return Ok(await _housingSupplyService.InsertSupplyDetails(supplyItems));
        }

        [FuncPermission(1115, FuncPermissionType.Add)]
        [HttpPost("InsertSupplyDetailsByLocation")]
        public async Task<IActionResult> InsertSupplyDetailsByLocation([FromBody] List<SupplyVm> supplyItems)
        {
            return Ok(await _housingSupplyService.InsertSupplyDetails(supplyItems));
        }

        [FuncPermission(1114, FuncPermissionType.Add)]
        [HttpPost("InsertSupplyDetailsByInmate")]
        public async Task<IActionResult> InsertSupplyDetailsByInmate([FromBody] List<SupplyVm> supplyItems)
        {
            return Ok(await _housingSupplyService.InsertSupplyDetails(supplyItems));
        }

        [FuncPermission(1117, FuncPermissionType.Edit)]
        [HttpPut("UpdateCheckInTo")]
        public async Task<IActionResult> UpdateCheckInTo([FromBody] List<SupplyVm> supplyItems)
        {
            return Ok(await _housingSupplyService.UpdateCheckInTo(supplyItems));
        }

        [HttpPut("UpdateDamage")]
        public async Task<IActionResult> UpdateDamage([FromBody] List<SupplyVm> supplyItems)
        {
            return Ok(await _housingSupplyService.UpdateDamage(supplyItems));
        }

        #endregion

        [FuncPermission(1113, FuncPermissionType.Access)]
        [HttpGet("GetManageSupplyItem")]
        public IActionResult GetManageSupplyItem(int facilityId, List<int> housingSupplyModuleId)
        {
            return Ok(_housingSupplyService.GetManageSupplyItem(facilityId, housingSupplyModuleId));
        }

        [HttpGet("GetSupplyItemLookup")]
        public IActionResult GetSupplyItemLookup()
        {
            return Ok(_housingSupplyService.GetSupplyItemLookup());
        }

        [FuncPermission(1113, FuncPermissionType.Add)]
        [HttpPost("InsertSupplyItem")]
        public async Task<IActionResult> InsertSupplyItem([FromBody] SupplyItemsVm supplyItems)
        {
            return Ok(await _housingSupplyService.InsertSupplyItem(supplyItems));
        }

        [FuncPermission(1113, FuncPermissionType.Edit)]
        [HttpPut("UpdateSupplyItem")]
        public async Task<IActionResult> UpdateSupplyItem([FromBody] SupplyItemsVm supplyItems)
        {
            return Ok(await _housingSupplyService.UpdateSupplyItem(supplyItems));
        }

        [FuncPermission(1113, FuncPermissionType.Delete)]
        [HttpDelete("DeleteSupplyItem")]
        public async Task<IActionResult> DeleteSupplyItem([FromQuery] SupplyItemsVm supplyItems)
        {
            return Ok(await _housingSupplyService.DeleteSupplyItem(supplyItems));
        }

        [HttpGet("GetWareHouseLookup")]
        public IActionResult GetWareHouseLookup(int facilityId)
        {
            return Ok(_housingSupplyService.GetWareHouseLookup(facilityId));
        }

        [HttpGet("GetHousingActiveRequestDetail")]
        public IActionResult GetHousingActiveRequestDetail(WareHouseItemVm wareHouseItem)
        {
            return Ok(_housingSupplyService.GetHousingActiveRequestDetail(wareHouseItem));
        }

        
        [HttpGet("GetSupplyCheckList")]
        public IActionResult GetSupplyCheckList(HousingSupplyInput input)
        {
            return Ok(_housingSupplyService.GetSupplyCheckList(input));
        }

        [FuncPermission(1111, FuncPermissionType.Add)]
        [HttpPost("InsertCheckOutItem")]
        public async Task<IActionResult> InsertCheckOutItem([FromBody]HousingSupplyVm housingSupply, string note)
        {
            return Ok(await _housingSupplyService.InsertCheckOutItem(housingSupply, note));
        }

        [HttpGet("GetSupplyItems")]
        public IActionResult GetSupplyItems(int facilityId, int housingSupplyModuleId, int checkListId)
        {
            return Ok(_housingSupplyService.GetSupplyItems(facilityId, housingSupplyModuleId, checkListId));
        }

        [FuncPermission(1111, FuncPermissionType.Delete)]
        [HttpDelete("DeleteUndoCheckList")]
        public async Task<IActionResult> DeleteUndoCheckList([FromQuery] CheckListVm checkListItem)
        {
            return Ok(await _housingSupplyService.DeleteUndoCheckList(checkListItem));
        }

        [HttpPost("WareHouseInsert")]
        public async Task<IActionResult> WareHouseInsert([FromBody] List<WareHouseItemVm> wareHouseItem)
        {
            return Ok(await _housingSupplyService.WareHouseInsert(wareHouseItem));
        }

        [FuncPermission(7071, FuncPermissionType.Add)]
        [HttpPost("WareHouseInsertFromItem")]
        public async Task<IActionResult> WareHouseInsertFromItem([FromBody] List<WareHouseItemVm> wareHouseItem)
        {
            return Ok(await _housingSupplyService.WareHouseInsert(wareHouseItem));
        }

        [FuncPermission(1112, FuncPermissionType.Access)]
        [HttpGet("GetHousingSupplyHistory")]
        public IActionResult GetHousingSupplyHistory(SupplyItemsVm supplyItems)
        {
            return Ok(_housingSupplyService.GetHousingSupplyHistory(supplyItems));
        }

        [HttpGet("GetSupplyHistoryLocation")]
        public IActionResult GetSupplyHistoryLocation(int facilityId, int[] moduleId)
        {
            return Ok(_housingSupplyService.GetSupplyHistoryLocation(facilityId, moduleId));
        }

    }
}