using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using ServerAPI.Services;
using ServerAPI.Tests;
using ServerAPI.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GenerateTables.Models;
using Xunit;
using ServerAPI.JMS.Tests.Helper;
using Microsoft.AspNetCore.Identity;
using Moq;
using JwtDb.Models;
using Microsoft.Extensions.Caching.Memory;


// ReSharper disable once CheckNamespace
namespace ServerAPI.JMS.Tests
{

    [Collection("Database collection")]
    public class HousingSupplyServiceTest
    {

        private readonly HousingSupplyService _housingSupplyService;
        private readonly IConfigurationRoot _configuration = MockHelpers.GetIConfigurationRoot();
        private readonly DbInitialize _fixture;
        private readonly Mock<IMemoryCache> _memory = new Mock<IMemoryCache>();

        public HousingSupplyServiceTest(DbInitialize fixture)
        {
            _fixture = fixture;
            PersonService personService = new PersonService(_fixture.Db);
            Mock<UserManager<AppUser>> userMgr = MockHelpers.MockUserManager<AppUser>();
            AppletsSavedService appletsSavedService = new AppletsSavedService(_fixture.Db, _configuration);
            PhotosService photosService = new PhotosService(_fixture.Db, _configuration);
            HttpContextAccessor httpContext = new HttpContextAccessor { HttpContext = fixture.Context.HttpContext };

            InterfaceEngineService interfaceEngineService = new InterfaceEngineService(_fixture.Db,
                _configuration, _memory.Object);
            CommonService commonService = new CommonService(_fixture.Db, _configuration,
                httpContext, personService, appletsSavedService, interfaceEngineService);
            FacilityPrivilegeService facilityPrivilegeService = new FacilityPrivilegeService(fixture.Db, httpContext,
                personService);
            InmateService inmateService = new InmateService(_fixture.Db, commonService, personService,
                httpContext, facilityPrivilegeService, photosService);

            _housingSupplyService = new HousingSupplyService(_fixture.Db, httpContext, commonService, personService,
                inmateService);
        }

        [Fact]
        public void GetClosetDetails()
        {
            //Act
            List<ClosetVm> lstCloset = _housingSupplyService.GetClosetDetails(2);

            //Assert
            ClosetVm closet = lstCloset.Single(c => c.HousingSupplyModuleId == 12);
            Assert.Equal("CLOSET C", closet.ClosetName);
        }


        [Fact]
        public void GetSupplyCheckList()
        {
            //Arrange
            HousingSupplyInput housingSupplyInput = new HousingSupplyInput
            {
                FacilityId = 1,
                HousingSupplyModuleId = new List<int>
                {10,11,12},
                PersonnelId = 11
            };
            //Act
            List<CheckListVm> lstCheckList = _housingSupplyService.GetSupplyCheckList(housingSupplyInput);
            //Assert
            Assert.True(lstCheckList.Count > 1);
        }

        //temporary hold this method
        //[Fact]

        //public void GetAvailableSupplyItems()
        //{
        //    int[] values = new int[]
        //        { 10,12,13};

        //    HousingSupplyVm housingSupply = _housingSupplyService.GetAvailableSupplyItems(1, values);
        //}

        [Fact]
        public void GetAvailableHistoryLst()
        {
            //Arrange
            int[] values = { 10, 11 };

            SupplyItemsVm supplyItems = new SupplyItemsVm
            {
                ItemName = "VOLLEYBALL",
                SupplyNumber = "SP_1400",
                ListModuleId = values
            };

            //Act
            List<SupplyItemsVm> lstSupplyItems = _housingSupplyService.GetAvailableHistoryLst(supplyItems);

            //Assert
            Assert.True(lstSupplyItems.Count > 4);
        }

        [Fact]

        public async Task InsertSupplyDetails()
        {
            //Arrange
            List<SupplyVm> lstSupply = new List<SupplyVm>
            {
                new SupplyVm
                {
                    HousingSupplyItemId = 10,
                    CheckoutInmateId = 104,
                    CheckInNote = "REPLACED DAMAGE ITEM",
                    CheckoutNote = "ALL THINGS ARE AVAILABLE",
                    CheckoutLocationId = 5,
                    CheckoutLocation = "CHENNAI",
                    CheckoutModuleId = 11
                },
                new SupplyVm
                {
                    HousingSupplyItemId = 11,

                    CheckoutInmateId = 105,
                    CheckoutLocationId = 6,
                    CheckoutLocation = "MADURAI"
                },
            };

            //Before Insert
            HousingSupplyCheckout housingSupplyCheckout =
                _fixture.Db.HousingSupplyCheckout.SingleOrDefault(h => h.CheckoutInmateId == "104");

            Assert.Null(housingSupplyCheckout);

            housingSupplyCheckout =
                _fixture.Db.HousingSupplyCheckout.SingleOrDefault(h => h.CheckoutInmateId == "105");
            Assert.Null(housingSupplyCheckout);

            //Before Update
            HousingSupplyItem housingSupplyItem =
                _fixture.Db.HousingSupplyItem.Single(h => h.HousingSupplyItemId == 10);
            Assert.Null(housingSupplyItem.CurrentCheckoutInmateId);
            Assert.Null(housingSupplyItem.CurrentCheckoutLocation);

            //Act
            await _housingSupplyService.InsertSupplyDetails(lstSupply);

            //After Insert
            housingSupplyCheckout =
                _fixture.Db.HousingSupplyCheckout.Single(h => h.CheckoutInmateId == "104");
            Assert.NotNull(housingSupplyCheckout);
            housingSupplyCheckout =
                _fixture.Db.HousingSupplyCheckout.Single(h => h.CheckoutInmateId == "105");
            Assert.NotNull(housingSupplyCheckout);

            //After Update
            housingSupplyItem =
                _fixture.Db.HousingSupplyItem.Single(h => h.HousingSupplyItemId == 10);
            Assert.Equal(104, housingSupplyItem.CurrentCheckoutInmateId);
            Assert.Equal("CHENNAI", housingSupplyItem.CurrentCheckoutLocation);

        }


        [Fact]
        public async Task UpdateDamage()
        {
            //Arrange
            List<SupplyVm> lstSupply = new List<SupplyVm>
            {
             new SupplyVm
             {
                 HousingSupplyItemId = 12,
                 DamageFlag = 1,
                 DamageFlagBy = 12,
                 DamageFlagDate = DateTime.Now,
                 DamageNote = "BROKEN ITEM RETURNED"
             }
            };
            //Before Update
            HousingSupplyItem housingSupply = _fixture.Db.HousingSupplyItem.Single(h => h.HousingSupplyItemId == 12);

            Assert.Null(housingSupply.DamageFlagNote);
            Assert.Null(housingSupply.DamageFlagBy);

            //Act
            await _housingSupplyService.UpdateDamage(lstSupply);

            //After Update
            //Assert
            housingSupply = _fixture.Db.HousingSupplyItem.Single(h => h.HousingSupplyItemId == 12);
            Assert.Equal("BROKEN ITEM RETURNED", housingSupply.DamageFlagNote);
            Assert.Equal(12, housingSupply.DamageFlagBy);
        }

        [Fact]
        public void GetManageSupplyItem()
        {
            //Arrange
            List<int> values = new List<int>
                {10,11,12};

            //Act
            List<SupplyItemsVm> lstSupplyItems = _housingSupplyService.GetManageSupplyItem(1, values);
            //Assert
            Assert.True(lstSupplyItems.Count > 0);

        }

        [Fact]
        public void GetSupplyItemLookup()
        {
            //Act
            List<SupplyItemsVm> lstSupplyItems = _housingSupplyService.GetSupplyItemLookup();
            //Assert
            Assert.True(lstSupplyItems.Count > 2);
        }

        [Fact]
        public void CheckSupplyItemExist()
        {
            //Arrange
            SupplyItemsVm supplyItems = new SupplyItemsVm
            {
                HousingSupplyModuleId = 12,
                HousingSupplyItemLookupId = 11,
                SupplyNumber = "SP_1400"
            };
            //Act
            int result = _housingSupplyService.CheckSupplyItemExist(supplyItems);
            //Assert
            Assert.Equal(1, result);
        }

        [Fact]
        public async Task UpdateSupplyItem()
        {
            //Arrange
            SupplyItemsVm supplyItems = new SupplyItemsVm
            {
                DeleteFlag = false,
                HousingSupplyModuleId = 12,
                HousingSupplyItemLookupId = 11,
                SupplyNumber = "SP_1401",
                SupplyDescription = "CLOTH ITEMS",
                FlagDurationMin = 1150,
                AllowCheckoutToHousing = true,
                AllowCheckoutToInmate = true,
                AllowCheckoutToLocation = false
            };
            //Before Update
            HousingSupplyItem housingSupplyItem =
                _fixture.Db.HousingSupplyItem.Single(h => h.HousingSupplyItemId == 13);

            Assert.Null(housingSupplyItem.SupplyDescription);
            Assert.Null(housingSupplyItem.AllowCheckoutToInmate);
            Assert.Equal(0, housingSupplyItem.AllowCheckoutToHousing);

            //Act
            await _housingSupplyService.UpdateSupplyItem(supplyItems);

            //Assert
            //After Update
            housingSupplyItem =
                _fixture.Db.HousingSupplyItem.Single(h => h.HousingSupplyItemId == 13);
            Assert.Equal("CLOTH ITEMS", housingSupplyItem.SupplyDescription);
            Assert.Equal(1, housingSupplyItem.AllowCheckoutToInmate);
            Assert.Equal(1, housingSupplyItem.AllowCheckoutToHousing);
        }


        [Fact]
        public async Task DeleteSupplyItem()
        {
            //Arrange
            SupplyItemsVm supplyItems = new SupplyItemsVm
            {
                DeleteFlag = false,
                HousingSupplyModuleId = 11,
                HousingSupplyItemLookupId = 10,
                SupplyNumber = "SP_1401",
                SupplyDescription = "CLOTH ITEMS",
                FlagDurationMin = 1100,
                Quantity = 1,
                ConsumedFlag = true
            };
            //Before Insert History table
            HousingSupplyItemHistory housingSupplyItemHistory =
                _fixture.Db.HousingSupplyItemHistory.SingleOrDefault(h => h.HousingSupplyItemId == 15);
            Assert.Null(housingSupplyItemHistory);

            //Before Delete
            HousingSupplyItem housingSupplyItem =
                _fixture.Db.HousingSupplyItem.Single(h => h.HousingSupplyItemId == 15);
            Assert.Null(housingSupplyItem.DeleteFlag);
            Assert.Null(housingSupplyItem.DeletedBy);

            //Act
            await _housingSupplyService.DeleteSupplyItem(supplyItems);

            //Assert
            //After Insert History table
            housingSupplyItemHistory =
                _fixture.Db.HousingSupplyItemHistory.Single(h => h.HousingSupplyItemId == 15);
            Assert.NotNull(housingSupplyItemHistory);

            //After Delete
            housingSupplyItem =
                _fixture.Db.HousingSupplyItem.Single(h => h.HousingSupplyItemId == 15);
            Assert.Equal(1, housingSupplyItem.DeleteFlag);
            Assert.Equal(11, housingSupplyItem.DeletedBy);
        }

        [Fact]
        public void GetWareHouseLookup()
        {
            //Act
            WareHouseLookup wareHouse = _housingSupplyService.GetWareHouseLookup(1);

            //Assert
            Assert.True(wareHouse.ListHousingDetail.Count > 5);
            Assert.True(wareHouse.ListLookup.Count > 2);
            Assert.True(wareHouse.ListWareHouseItem.Count > 1);
        }

        [Fact]
        public void GetHousingActiveRequestDetail()
        {
            //Arrange
            WareHouseItemVm wareHouse = new WareHouseItemVm
            {
                DeleteFlag = null,
                FacilityId = 1,
                HousingBuilding = "FLOOR2",
                HousingNumber = "UP-B"
            };

            //Act
            List<WareHouseItemVm> lstWareHouseItem = _housingSupplyService.GetHousingActiveRequestDetail(wareHouse);

            //Assert
            WareHouseItemVm wareHouseItem = lstWareHouseItem.Single(w => w.WareHouseRequestId == 10);
            Assert.Equal("MISC", wareHouseItem.ItemCategory);
            Assert.Equal("RING", wareHouseItem.ItemName);

        }

        [Fact]
        public async Task InsertCheckOutItem()
        {
            //Arrange
            List<SupplyItemsVm> lstSupplyItems = new List<SupplyItemsVm>
            {
               new SupplyItemsVm
               {
                  Quantity = 5,
                  InOutCount = 5,
                  CheckListNote = "CHECK THE ALL QUANTITY ITEM",
                  LstSupplyItemId = new List<int>
                      {11}
                  }
            };
            List<SupplyItemsVm> lstsuppList = new List<SupplyItemsVm>
            {
                new SupplyItemsVm
                {
                    Quantity = 10,
                    InOutCount = 10,
                    CheckListNote = "SEARCH DAMAGE PIECE",
                    LstSupplyItemId = new List<int>
                        { 12}
                }
            };
            HousingSupplyVm housingSupply = new HousingSupplyVm
            {
                LstCheckoutItems = lstSupplyItems,
                LstAvailableItems = lstsuppList
            };
            //Before Insert HousingSupplyCheckList table
            HousingSupplyCheckList housingSupplyCheckList = _fixture.Db.HousingSupplyCheckList.SingleOrDefault(h =>
                h.CheckListNote == "CHECKED DAMAGED ITEM");
            Assert.Null(housingSupplyCheckList);
            //Before Insert HousingSupplyCheckListXref table
            HousingSupplyCheckListXref supplyCheckListXref = _fixture.Db.HousingSupplyCheckListXref.SingleOrDefault(
                h => h.CheckListNote == "SEARCH DAMAGE PIECE");
            Assert.Null(supplyCheckListXref);

            //Act
            await _housingSupplyService.InsertCheckOutItem(housingSupply, "CHECKED DAMAGED ITEM");

            //Assert
            //After Insert HousingSupplyCheckList table
            housingSupplyCheckList = _fixture.Db.HousingSupplyCheckList.Single(h =>
                h.CheckListNote == "CHECKED DAMAGED ITEM");
            Assert.NotNull(housingSupplyCheckList);

            //After Insert HousingSupplyCheckListXref table
            supplyCheckListXref = _fixture.Db.HousingSupplyCheckListXref.Single(
                h => h.CheckListNote == "SEARCH DAMAGE PIECE");
            Assert.NotNull(supplyCheckListXref);
        }

        //temporary hold this method
        //[Fact]
        //public void GetSupplyItems()
        //{
        //    _housingSupplyService.GetSupplyItems(2, 10, 10);

        //}

        [Fact]
        public async Task DeleteUndoCheckList()
        {
            //Arrange
            CheckListVm checkList = new CheckListVm
            {
                CheckListId = 12
            };

            //Before Delete
            HousingSupplyCheckList housingSupplyCheck =
                _fixture.Db.HousingSupplyCheckList.Single(h => h.HousingSupplyCheckListId == 12);
            Assert.Null(housingSupplyCheck.DeleteFlag);
            Assert.Null(housingSupplyCheck.DeletedBy);

            //Act
            await _housingSupplyService.DeleteUndoCheckList(checkList);

            //Assert
            //After Delete
            housingSupplyCheck =
                _fixture.Db.HousingSupplyCheckList.Single(h => h.HousingSupplyCheckListId == 12);
            Assert.Equal(1, housingSupplyCheck.DeleteFlag);
            Assert.Equal(11, housingSupplyCheck.DeletedBy);
        }


        [Fact]
        public async Task WareHouseInsert()
        {
            //Arrange
            List<WareHouseItemVm> lstHouseItem = new List<WareHouseItemVm>
            {
                new WareHouseItemVm
                {
                    DeleteFlag = null,
                    FacilityId = 1,
                    RequestedBy= 11,
                    DeliveryDispo = "MISC",
                    RequestedQty = 3
                }
            };
            //Before Insert
            WarehouseRequest warehouse = _fixture.Db.WarehouseRequest.SingleOrDefault(w => w.DeliveryDispo == "MISC");
            Assert.Null(warehouse);

            //Act
            await _housingSupplyService.WareHouseInsert(lstHouseItem);

            //Assert
            //After Insert
            warehouse = _fixture.Db.WarehouseRequest.SingleOrDefault(w => w.DeliveryDispo == "MISC");
            Assert.NotNull(warehouse);

        }

        [Fact]
        public void GetHousingSupplyHistory()
        {
            //Arrange
            SupplyItemsVm supplyItems = new SupplyItemsVm
            {
                HousingSupplyItemLookupId = 11,
                HousingSupplyModuleId = 11,
                SupplyNumber = "SP_1400",
                HousingSupplyItemHistoryList = @"{ 'DeleteFlag':'1','DeleteBy':'11','Deletedate':'02-26-2019 14:00'}"
            };
            //Act
            List<HousingSupplyItemHistoryVm> lstHousingSupplyItemHistory = _housingSupplyService.GetHousingSupplyHistory(supplyItems);

            //Assert
            Assert.True(lstHousingSupplyItemHistory.Count > 2);
        }
    }
}
