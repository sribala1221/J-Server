using GenerateTables.Models;
using ServerAPI.Services;
using ServerAPI.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Microsoft.AspNetCore.Http;
using ServerAPI.Tests;
using Microsoft.Extensions.Configuration;
using System;
using ServerAPI.JMS.Tests.Helper;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using Microsoft.AspNetCore.Identity;
using JwtDb.Models;

namespace ServerAPI.JMS.Tests
{
    [Collection("Database collection")]
    public class InventoryInmateServiceTest
    {
        private readonly InventoryInmateService _inventoryInmate;
        private readonly DbInitialize _fixture;
        private readonly IConfigurationRoot _configuration = MockHelpers.GetIConfigurationRoot();
        private readonly Mock<IMemoryCache> _memory = new Mock<IMemoryCache>();


        public InventoryInmateServiceTest(DbInitialize fixture)
        {
            _fixture = fixture;
            PersonService personService = new PersonService(_fixture.Db);
            Mock<UserManager<AppUser>> userMgr = MockHelpers.MockUserManager<AppUser>();
            AppletsSavedService appletsSavedService = new AppletsSavedService(_fixture.Db, _configuration);
            PhotosService photosService = new PhotosService(_fixture.Db, _configuration);
            HttpContextAccessor httpContext = new HttpContextAccessor { HttpContext = fixture.Context.HttpContext };
          
            InterfaceEngineService interfaceEngineService = new InterfaceEngineService(_fixture.Db, _configuration,
                _memory.Object);
            CommonService commonService = new CommonService(_fixture.Db, _configuration, httpContext, personService, appletsSavedService,interfaceEngineService);
            FacilityHousingService facilityHousingService = new FacilityHousingService(_fixture.Db, _configuration,
                httpContext);
            FormsService formsService = new FormsService(_fixture.Db, httpContext, commonService, personService,
                interfaceEngineService);

            _inventoryInmate = new InventoryInmateService(_fixture.Db, commonService, httpContext, formsService,
                _configuration, personService, photosService, facilityHousingService, interfaceEngineService);
        }

        [Fact]
        public void InventoryInmateService_GetInventoryGrid()
        {
            //For-> In Storage
            //Act
            InventoryVm inventory = _inventoryInmate.GetInventoryGrid(102, Disposition.Storage, 1);
            //Assert
            //Get Inventory Details
            InventoryDetails inventoryDetails = inventory.InventoryDetails.Single(i => i.PersonalBinName == "BIN001");
            Assert.Equal("PG100022", inventoryDetails.PersonalGroupName);
            Assert.Equal(4, inventoryDetails.InventoryDispositionCode);
            //Get Inventory Item Details
            InventoryItemDetails itemdetails =
                inventory.InventoryItemDetails.Single(i => i.InventoryArticles == 1);
            Assert.Equal("BLACK", itemdetails.InventoryColor);

            //For-> Released Items
            //Act
            inventory = _inventoryInmate.GetInventoryGrid(101, Disposition.ReleasedToPerson, 1);
            //Assert
            InventoryPropGroupDetails inventorypropgroup =
                inventory.InventoryPropGroup.Single(i => i.PropGroupId == 5);

            Assert.Equal("PG100020", inventorypropgroup.PropGroupName);
            Assert.Equal("SHARE BAG INTO SAME BIN", inventorypropgroup.PropGroupNotes);
        }

        [Fact]
        public void InventoryInmateService_GetInventoryHistory()
        {
            //Act
            List<InventoryHistoryVm> lstinventoryhistory = _inventoryInmate.GetInventoryHistory(6, 103,
                Disposition.Storage, true);
            //Assert
            InventoryHistoryVm inventoryhistoryvm = lstinventoryhistory.First(i => i.CreatedBy == 11);
            Assert.Equal("SANGEETHA", inventoryhistoryvm.CreatedPersonFirstName);
            Assert.Equal("VIJAYA", inventoryhistoryvm.CreatedPersonLastName);
            Assert.Equal("JUNIOR BAIL LEAF MANAGER", inventoryhistoryvm.AgencyName);
            Assert.Equal(1, inventoryhistoryvm.InventoryArticles);
            Assert.Equal("YELLOW", inventoryhistoryvm.InventoryColor);
        }

        [Fact]
        public void InventoryInmateService_PropertyGroupHistory()
        {
            //Act
            List<PropGroupHistoryDetails> lstpropgrouphistory = _inventoryInmate.PropertyGroupHistory(6);
            //Assert
            PropGroupHistoryDetails propGroupHistory = lstpropgrouphistory.Single(p => p.PersonalGroupHistoryId == 5);
            Assert.Equal("TWO INMATES PROPERTY", propGroupHistory.InventoryNotes);
            Assert.Equal("SURUTHI", propGroupHistory.PropGrpPersonMiddleName);
        }

        [Fact]
        public void InventoryInmateService_DeleteInventoryDetails()
        {
            //Act
            BinDeleteVm binDelete = _inventoryInmate.DeleteInventoryDetails();
            //Assert
            List<KeyValuePair<int, string>> lstinventorylookup = binDelete.ListInventoryLookUpDetails;
            Assert.Equal(10, lstinventorylookup[0].Key);
            Assert.Equal("SPELLING", lstinventorylookup[0].Value);
        }

        [Fact]
        public void InventoryInmateService_GetPreBookInventoryItem()
        {
            //Act
            List<PersonalInventoryPreBookVm> lstpersonalinventory = _inventoryInmate.GetPreBookInventoryItem(104, 1);
            //Assert
            PersonalInventoryPreBookVm result = lstpersonalinventory.Single(p => p.PersonalInventoryPreBookId == 7);
            Assert.Equal(1, result.InventoryArticles);

            //7884 =>Issues=> added Article name

            Assert.Equal("JACKET", result.InventoryArticlesName);

        }

        [Fact]
        public void InventoryInmateService_MoveBinInmateDetails()
        {
            //Act
            MoveBinVm movebin = _inventoryInmate.MoveBinInmateDetails(105, 5);
            //Assert
            Assert.Equal(InmateBinEvent.Move, movebin.InmateBinEvents);
            MoveBinDetails moveBinDetails = movebin.MoveBinDetails.Single(m => m.MoveBinInmateId == 100);
            Assert.Equal("BIN001", moveBinDetails.MoveBinName);
            Assert.Equal("VIJAYA", moveBinDetails.MoveBinLastName);
            moveBinDetails = movebin.MoveBinDetailsItems.Single(m => m.MoveBinInventoryId == 5);
            Assert.Equal("RED", moveBinDetails.MoveBinInventoryColor);
            Assert.Equal("PANTS", moveBinDetails.MoveBinInventoryDescription);
        }

        [Fact]
        public void InventoryInmateService_GetReleaseItems()
        {
            //Act
            InventoryVm invent = _inventoryInmate.GetReleaseItems(130, Disposition.ReleasedToPerson, 0);
            //Assert
            InventoryPropGroupDetails inventoryPropGroup = invent.InventoryPropGroup.Single(p => p.PropGroupId == 7);
            Assert.Equal("PG100102", inventoryPropGroup.PropGroupName);
            MoveBinDetails movebin =
                invent.ReleaseBinDetails.Single(r => r.PersonAddress == "THENAMPET");
            Assert.Equal("PERSON_TP1125", movebin.PersonIdType);
        }

        [Fact]
        public void InventoryInmateService_LoadChangeInventoryDetails()
        {
            //Act
            InventoryChangeGroupVm inventoryChangeGroup = _inventoryInmate.LoadChangeInventoryDetails(125, 5, 7,
                Disposition.Storage);
            //Assert
            InventoryItemDetails inventoryItemDetails =
                inventoryChangeGroup.InventoryChangeGroupItemDetails.Single(i => i.PersonalInventoryGroupId == 7);
            Assert.Equal("VENKAT", inventoryItemDetails.PersonName);
            Assert.Equal("PHANT", inventoryItemDetails.InventoryDescription);

            InventoryDetails inventoryDetails =
                inventoryChangeGroup.InventoryChangeGroupDetails.Single(i => i.PersonalInventoryBinId == 5);
            Assert.Equal("BIN001", inventoryDetails.PersonalBinName);
        }

        [Fact]
        public async Task InventoryInmateService_UpdateInventoryInmate()
        {
            //Arrange
            InventoryDetails inventoryDetails = new InventoryDetails
            {
                PersonalGroupName = "PG100023",
                InmateId = 100,
                PersonalInventoryGroupId = 5,
                PropertyGroupNotes = "COLLECTED ALL THINGS"
            };
            InventoryItemDetails inventoryItemDetails = new InventoryItemDetails
            {
                PersonalInventoryId = 5,
                PersonalInventoryBinId = 5,
                PersonalBinName = "BIN004"
            };
            List<InventoryItemDetails> lstitemdetails = new List<InventoryItemDetails> { inventoryItemDetails };
            InventoryChangeGroupVm inventoryChangeGroup = new InventoryChangeGroupVm
            {
                InventoryDetails = inventoryDetails,
                InventoryChangeGroupItemDetails = lstitemdetails
            };
            PersonalInventoryHistory personalInventory =
                _fixture.Db.PersonalInventoryHistory.SingleOrDefault(p => p.InventoryColor == "RED");
            Assert.Null(personalInventory);
            PersonalInventoryGroup personalInventoryGroup =
                _fixture.Db.PersonalInventoryGroup.SingleOrDefault(p => p.InmateId == 100);
            Assert.Null(personalInventoryGroup);
            //Act
            await _inventoryInmate.UpdateInventoryInmate(inventoryChangeGroup);
            //Assert
            personalInventory = _fixture.Db.PersonalInventoryHistory.Single(p => p.InventoryColor == "RED");
            Assert.Equal(1, personalInventory.InventoryArticles);
            Assert.Equal("PANTS", personalInventory.InventoryDescription);
            personalInventoryGroup = _fixture.Db.PersonalInventoryGroup.Single(p => p.InmateId == 100);
            Assert.Equal("COLLECTED ALL THINGS", personalInventoryGroup.GroupNote);
        }

        [Fact]
        public async Task InventoryInmateService_DeleteandUndoInventory()
        {
            //Arrange
            InventoryDetails inventoryDetails = new InventoryDetails
            {
                PersonalInventoryId = 8,
                DeleteReasonNote = "TEMPORARY RELEASE OF AN ACCUSED PERSON",
                DeleteReason = "RELEASE",
                DeleteFlag = 1,
                InmateId = 103
            };
            PersonalInventory personalInventory = _fixture.Db.PersonalInventory.Single(p => p.PersonalInventoryId == 8);
            Assert.Null(personalInventory.DeleteReason);
            Assert.Equal(0, personalInventory.DeleteFlag);
            //Act
            await _inventoryInmate.DeleteandUndoInventory(inventoryDetails);
            //Assert
            personalInventory = _fixture.Db.PersonalInventory.Single(p => p.PersonalInventoryId == 8);
            Assert.Equal("RELEASE", personalInventory.DeleteReason);
            Assert.Equal(1, personalInventory.DeleteFlag);
        }

        [Fact]
        public async Task InventoryInmateService_InsertInventoryAddItems()
        {
            //Arrange
            PersonalInventoryAddItems inventoryAddItems = new PersonalInventoryAddItems
            {
                InmateId = 104,
                InventoryAddItemsArticles = 1,
                InventoryAddItemsQuantity = 5,
                InventoryAddItemsColor = "WHITE",
                InventoryBinNumber = "BIN001",
                PersonalInventoryPrebookId = 5,
                PersonalInventoryBinId = 6,
                PersonalBinName = "BIN004"
            };
            List<PersonalInventoryAddItems> lstinventoryadditem = new List<PersonalInventoryAddItems>
            {
                inventoryAddItems
            };
            InventoryDetails inventoryDetails = new InventoryDetails
            {
                PersonalInventoryGroupId = 5,
                InmateId = 104,
                PersonalGroupName = "PG100020",
                PropertyGroupNotes = "SHARE BAG INTO SAME BIN",
                PersonalInventoryId = 6
            };
            PersonalInventoryPreBookVm personalInventoryPreBook = new PersonalInventoryPreBookVm
            {
                ImportFlag = 0,
                DeleteFlag = 0,
                PersonalInventoryAddItemsList = lstinventoryadditem,
                InventoryAddItems = InventoryAddItems.UseNewGroup,
                InventoryDetails = inventoryDetails
            };
            //Create new group
            PersonalInventory personalInventory =
                _fixture.Db.PersonalInventory.SingleOrDefault(p =>
                    p.InmateId == 104 && p.InventoryArticles == 1);
            Assert.Null(personalInventory);

            PersonalInventoryGroup personalinventorygroup =
                _fixture.Db.PersonalInventoryGroup.SingleOrDefault(p => p.InmateId == 104);
            Assert.Null(personalinventorygroup);
            //Act
            await _inventoryInmate.InsertInventoryAddItems(personalInventoryPreBook);
            //Assert
            personalInventory =
                _fixture.Db.PersonalInventory.Single(p =>
                    p.InmateId == 104 && p.InventoryArticles == 1);

            Assert.Equal("BIN004", personalInventory.InventoryBinNumber);

            personalinventorygroup = _fixture.Db.PersonalInventoryGroup.Single(p => p.InmateId == 104);
            Assert.Equal("SHARE BAG INTO SAME BIN", personalinventorygroup.GroupNote);
        }

        [Fact]
        public async Task InventoryInmateService_InsertInventoryMove()
        {
            //For Mail
            //Arrange
            MoveBinDetails moveBinDetails = new MoveBinDetails
            {
                MoveBinInventoryId = 8,
                MoveBinInmateId = 101
            };
            List<MoveBinDetails> lstmovebin = new List<MoveBinDetails> { moveBinDetails };
            MoveBinVm moveBin = new MoveBinVm
            {
                InmateBinEvents = InmateBinEvent.Mail,
                MoveBinDetails = lstmovebin
            };
            GenerateTables.Models.Inmate inmate = _fixture.Db.Inmate.Single(i => i.InmateId == 101);
            Assert.Null(inmate.InmatePersonalInventory);

            //For Move
            //Act
            await _inventoryInmate.InsertInventoryMove(moveBin);
            //Assert 
            Assert.Equal("BIN002", inmate.InmatePersonalInventory);

            //Arrange
            moveBinDetails = new MoveBinDetails
            {
                MoveBinInventoryId = 8,
                MoveBinInmateId = 103,
                MoveBinInventoryBinId = 7,
                MoveBinName = "BIN003"
            };
            lstmovebin = new List<MoveBinDetails> { moveBinDetails };
            moveBin = new MoveBinVm
            {
                InmateBinEvents = InmateBinEvent.Move,
                MoveBinDetails = lstmovebin
            };

            PersonalInventory personalInventory = _fixture.Db.PersonalInventory.Single(p => p.PersonalInventoryId == 8);
            Assert.Equal("BIN002", personalInventory.InventoryBinNumber);
            Assert.Equal(6, personalInventory.PersonalInventoryBinId);
            //Act
            await _inventoryInmate.InsertInventoryMove(moveBin);
            //BIN002 move to this bin BIN003 
            personalInventory = _fixture.Db.PersonalInventory.Single(p => p.PersonalInventoryId == 8);
            Assert.Equal("BIN003", personalInventory.InventoryBinNumber);


            //7856 Revision ==>checked (changed InventoryReturnDate)
            //Arrange
            moveBinDetails = new MoveBinDetails
            {
                MoveBinInventoryId = 9,
                MoveBinInmateId = 100,
                MoveBinInventoryBinId = 8,
                MoveBinFirstName = "BIN004"
            };
            lstmovebin = new List<MoveBinDetails> { moveBinDetails };
            moveBin = new MoveBinVm
            {
                InmateBinEvents = InmateBinEvent.Release,
                MoveBinDetails = lstmovebin,
                ReleaseDetails = new ReleaseDetails
                {
                    EvidenceAgencyId = 10,
                    InventoryReturnDate = DateTime.Now,
                    PersonAddress = "NEHRU STREET"
                }
            };
            personalInventory = _fixture.Db.PersonalInventory.Single(p => p.PersonalInventoryId == 9);
            Assert.Null(personalInventory.InventoryReturnDate);
            Assert.Equal("PILLARYAR KOVIL STREET", personalInventory.PersonAddress);

            await _inventoryInmate.InsertInventoryMove(moveBin);

            personalInventory = _fixture.Db.PersonalInventory.Single(p => p.PersonalInventoryId == 9);
            Assert.NotNull(personalInventory.InventoryReturnDate);
            Assert.Equal("NEHRU STREET", personalInventory.PersonAddress);
        }



    }
}
