using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ServerAPI.Services;
using ServerAPI.ViewModels;
using Newtonsoft.Json;
using jsreport.Client;
using jsreport.Types;
using Microsoft.Extensions.Configuration;
using ServerAPI.Utilities;
using System;
using ServerAPI.Authorization;
using Newtonsoft.Json.Linq;

namespace ServerAPI.Controllers
{
    [Route("[controller]")]
    public class InventoryInmateController : ControllerBase
    {
        private readonly IInventoryInmateService _inventoryservice;
        private readonly IConfiguration _configuration;
        private readonly ICommonService _commonService;

        public InventoryInmateController(IInventoryInmateService inv, IConfiguration configuration,
            ICommonService commonService)
        {
            _inventoryservice = inv;
            _configuration = configuration;
            _commonService = commonService;
        }

        //getting inventory grid loaded
        //As per updated functionality Permission , access permission not exist
        // [FuncPermission(7000, FuncPermissionType.Access)]
        [HttpGet("GetInventory")]
        public IActionResult GetInventory(int inmateId, Disposition disposition, int showDeleteFlag)
        {
            return Ok(_inventoryservice.GetInventoryGrid(inmateId, disposition, showDeleteFlag));
        }

        [FuncPermission(7002, FuncPermissionType.Add)]
        [HttpGet("GetBinGroup")]
        public IActionResult GetBinGroup()
        {
            return NoContent();
        }
        [FuncPermission(7002, FuncPermissionType.Edit)]
        [HttpGet("EditBinGroup")]
        public IActionResult EditBinGroup()
        {
            return NoContent();
        }
        [FuncPermission(7002,FuncPermissionType.Delete)]
        [HttpDelete("DeleteReceiveBin")]
        public IActionResult DeleteReceiveBin()
        {
            return Ok();
        }
        [FuncPermission(7000,FuncPermissionType.Delete)]
        [HttpDelete("DeleteOtherBin")]
        public IActionResult DeleteOtherBin()
        {
            return Ok();
        }
        [FuncPermission(7003, FuncPermissionType.Edit)]
        [HttpPut("GetPermissionInventoryMove")]
        public IActionResult GetPermissionInventoryMove()
        {
            return NoContent();
        }

        [FuncPermission(7004, FuncPermissionType.Edit)]
        [HttpPut("GetPermissionInventoryRelease")]
        public IActionResult GetPermissionInventoryRelease()
        {
            return NoContent();
        }

        [FuncPermission(7005, FuncPermissionType.Edit)]
        [HttpPut("GetPermissionInventoryMail")]
        public IActionResult GetPermissionInventoryMail()
        {
            return NoContent();
        }

        [FuncPermission(7006, FuncPermissionType.Edit)]
        [HttpPut("GetPermissionInventoryDonate")]
        public IActionResult GetPermissionInventoryDonate()
        {
            return NoContent();
        }

        [FuncPermission(7007, FuncPermissionType.Edit)]
        [HttpPut("GetPermissionInventoryKeep")]
        public IActionResult GetPermissionInventoryKeep()
        {
            return NoContent();
        }

        [FuncPermission(7008, FuncPermissionType.Edit)]
        [HttpPut("GetPermissionInventoryEvid")]
        public IActionResult GetPermissionInventoryEvid()
        {
            return NoContent();
        }

        [FuncPermission(7000, FuncPermissionType.Print)]
        [HttpGet("GetInventoryReceipt")]
        public IActionResult GetInventoryReceipt()
        {
            return NoContent();
        }

        //getting inventory history in click event
        [HttpGet("GetInventoryHistory")]
        public IActionResult GetInventoryHistory(int inventoryId, int inmateId, Disposition disposition, bool inventoryFlag)
        {
            return Ok(_inventoryservice.GetInventoryHistory(inventoryId, inmateId, disposition, inventoryFlag));
        }

        // insert & update the inventory bin and group
        [HttpPost("UpdateInventoryInmate")]
        public async Task<IActionResult> UpdateInventoryInmate([FromBody] InventoryChangeGroupVm value)
        {
            return Ok(await _inventoryservice.UpdateInventoryInmate(value));
        }

        //getting  Property Group inventory history in click event 
        [HttpGet("PropertyGroupHistory")]
        public IActionResult PropertyGroupHistory(int propGroupId)
        {
            return Ok(_inventoryservice.PropertyGroupHistory(propGroupId));
        }

        // get the change inventory inmate group grid details and load drop down
        [HttpGet("LoadInventoryDetails")]
        public IActionResult LoadInventoryDetails(int inmateId, int inventoryBinId, int inventoryGroupId,
            Disposition disposition)
        {
            return
                Ok(_inventoryservice.LoadChangeInventoryDetails(inmateId, inventoryBinId, inventoryGroupId, disposition));
        }

        // get the bin inmate details
        [HttpGet("BinInmateDetails")]
        public IActionResult BinInmateDetails(int personalInventoryBinId)
        {
            return Ok(_inventoryservice.BinInmateDetails(personalInventoryBinId));
        }

        // get the available bin items for inmate 
        [HttpGet("AvailableBinItems")]
        public IActionResult AvailableBinItems(int facilityId)
        {
            return Ok(_inventoryservice.AvailableBinItems(facilityId));
        }

        // get the grid and dropdown list in the delete entry
        [HttpGet("DeleteInventoryDetails")]
        public IActionResult DeleteInventoryDetails()
        {
            return Ok(_inventoryservice.DeleteInventoryDetails());
        }

        // delete the bin items in inventory grid
        [FuncPermission(7012, FuncPermissionType.Delete)]
        [HttpPost("UpdateDeleteInventory")]
        public async Task<IActionResult> UpdateDeleteInventory([FromBody] InventoryDetails obj)
        {
            return Ok(await _inventoryservice.DeleteandUndoInventory(obj));
        }

        [HttpPost("UndoDeleteInventory")]
        public async Task<IActionResult> UndoDeleteInventory([FromBody] InventoryDetails obj)
        {
            return Ok(await _inventoryservice.DeleteandUndoInventory(obj));
        }

        // load the inventory from prebook 
        [HttpGet("GetPreBookInventoryItem")]
        public IActionResult GetPreBookInventoryItem(int inmateId, int personalInventoryId)
        {
            return Ok(_inventoryservice.GetPreBookInventoryItem(inmateId, personalInventoryId));
        }

        // Save the inventory add items 
        [FuncPermission(7000, FuncPermissionType.Add)]
        [HttpPost("InsertInventoryAddItems")]
        public async Task<IActionResult> InsertInventoryAddItems([FromBody] PersonalInventoryPreBookVm obj)
        {
            return Ok(await _inventoryservice.InsertInventoryAddItems(obj));
        }

        [FuncPermission(7011, FuncPermissionType.Add)]
        [HttpPost("InsertInventoryAddItemsLost")]
        public async Task<IActionResult> InsertInventoryAddItemsLost([FromBody] PersonalInventoryPreBookVm obj)
        {
            return Ok(await _inventoryservice.InsertInventoryAddItems(obj));
        }

        // update PropertyGroupNotes Entry Details grid in click events
        [FuncPermission(7000, FuncPermissionType.Edit)]
        [HttpPost("UpdatePropertyGroupNotesEntry")]
        public async Task<IActionResult> UpdatePropertyGroupNotesEntry([FromBody] InventoryItemDetails value)
        {
            return Ok(await _inventoryservice.UpdatePropertyGroupNotesEntry(value));
        }

        // get the other inventory bin details
        [HttpGet("MoveBinInmateDetails")]
        public IActionResult MoveBinInmateDetails(int inmateId, int inventoryBinId)
        {
            return Ok(_inventoryservice.MoveBinInmateDetails(inmateId, inventoryBinId));
        }

        // insert the release items in inventory
        [HttpPost("InsertInventoryMove")]
        public async Task<IActionResult> InsertInventoryMove([FromBody] MoveBinVm value)
        {
            return Ok(await _inventoryservice.InsertInventoryMove(value));
        }

        // get the release inmate details
        [HttpGet("GetReleaseItems")]
        public IActionResult GetReleaseItems(int inmateId, Disposition disposition, int showDeleteFlag)
        {
            return Ok(_inventoryservice.GetReleaseItems(inmateId, disposition, showDeleteFlag));
        }

        //get inventory lookup details list
        [HttpGet("GetLookupDetails")]
        public IActionResult GetLookupDetails()
        {
            return Ok(_inventoryservice.GetLookupDetails());
        }

        [HttpGet("ReleaseItemAddressDetails")]
        public IActionResult ReleaseItemAddressDetails(int personId)
        {
            return Ok(_inventoryservice.ReleaseItemAddressDetails(personId));
        }

        // Get Inventory in storage details
        [HttpPost("GetInventoryInStorage")]
        public IActionResult GetInventoryInStorage([FromBody] InventoryVm inventoryDetails, [FromQuery] bool isRelease)
        {
            return Ok(_inventoryservice.GetInventoryInStorage(inventoryDetails, isRelease));
        }

        // Create Inventory in storage report
        [HttpPost("CreateInventoryReport")]
        public async Task<IActionResult> CreateInventoryReport([FromBody] InventoryInStorage inventoryReceiptDetails)
        {
            ReportingService rs = new ReportingService(_configuration.GetSection("SiteVariables")["ReportUrl"]);
            //try catch used for to handle 2 exceptions (10061 & 404) from jsreport.
            try
            {
                _commonService.atimsReportsContentLog(InventoryQueueConstants.INVENTORYINSTORAGENAME, JsonConvert.SerializeObject(inventoryReceiptDetails));
                Report report =
                    await rs.RenderByNameAsync(InventoryQueueConstants.INVENTORYINSTORAGENAME, JsonConvert.SerializeObject(inventoryReceiptDetails));
                FileContentResult result =
                    new FileContentResult(_commonService.ConvertStreamToByte(report.Content), "application/pdf");
                return result;
            }
            catch (Exception ex)
            {
                //10061 is status code for no connection could be made because the target machine actively refused it
                //404 for file not found in jsreport
                return ex.InnerException != null ? Ok(10061) : Ok(404);
            }
        }

        // Get Inventory in storage details
        [HttpPost("InsertPropertyPhoto")]
        public async Task<IActionResult> InsertPropertyPhoto([FromBody] PersonPhoto personPhoto)
        {
            return Ok(await _inventoryservice.InsertPropertyPhoto(personPhoto));
        }

        //Delete property photo 
        [HttpPut("DeletePropertyPhoto")]
        public async Task<IActionResult> DeletePropertyPhoto([FromBody] int identifiersId)
        {
            return Ok(await _inventoryservice.DeletePropertyPhoto(identifiersId));
        }

        //property receipt
        [HttpPost("GetPropertyGroupReceipt")]
        public async Task<IActionResult> GetPropertyGroupReceipt([FromBody] InventoryDetails inventoryDetails)
        {
            InventoryInStorage proertyGroupDetails = _inventoryservice.GetPropertyGroupDetails(inventoryDetails);
            JObject customLabel = _commonService.GetCustomMapping();
            proertyGroupDetails.CustomLabel = customLabel;

            string json = JsonConvert.SerializeObject(proertyGroupDetails);
            ReportingService rs = new ReportingService(_configuration.GetSection("SiteVariables")["ReportUrl"]);
            //try catch used for to handle 2 exceptions (10061 & 404) from jsreport.
            try
            {
                _commonService.atimsReportsContentLog(InventoryQueueConstants.PROPERTYGROUP, json);
                Report report = await rs.RenderByNameAsync(InventoryQueueConstants.PROPERTYGROUP, json);
                FileContentResult result =
                    new FileContentResult(_commonService.ConvertStreamToByte(report.Content), "application/pdf");
                return result;
            }
            catch (Exception ex)
            {
                //10061 is status code for no connection could be made because the target machine actively refused it
                //404 for file not found in jsreport
                return ex.InnerException != null ? Ok(10061) : Ok(404);
            }
        }

         // insert the misplaced items in inventory
        [FuncPermission(8431, FuncPermissionType.Edit)]
        [HttpPost("InsertInventoryMisplacedItems")]
        public async Task<IActionResult> InsertInventoryMisplacedValues([FromBody] InventoryDetails inventoryDetails)
        {
            return Ok(await _inventoryservice.InsertInventoryMisplacedValues(inventoryDetails));
        }

    }
}