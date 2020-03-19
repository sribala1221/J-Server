using GenerateTables.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using ServerAPI.Services;
using ServerAPI.Tests;
using ServerAPI.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Xunit;
using ServerAPI.JMS.Tests.Helper;
using Moq;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.AspNetCore.Identity;
using JwtDb.Models;

// ReSharper disable once CheckNamespace
namespace ServerAPI.JMS.Tests
{
    [Collection("Database collection")]
    public class InventoryLostFoundServiceTest
    {
        private readonly InventoryLostFoundService _inventoryLostFoundService;
        private readonly IConfigurationRoot _configuration = MockHelpers.GetIConfigurationRoot();
        private readonly DbInitialize _fixture;
        private readonly Mock<IMemoryCache> _memory = new Mock<IMemoryCache>();

        public InventoryLostFoundServiceTest(DbInitialize fixture)
        {
            _fixture = fixture;
            PersonService personService = new PersonService(_fixture.Db);
            Mock<UserManager<AppUser>> userMgr = MockHelpers.MockUserManager<AppUser>();
            PhotosService photosService = new PhotosService(_fixture.Db, _configuration);
            InterfaceEngineService interfaceEngineService = new InterfaceEngineService(fixture.Db, _configuration,
                _memory.Object);
            HttpContextAccessor httpContext = new HttpContextAccessor { HttpContext = fixture.Context.HttpContext };
            AppletsSavedService appletsSavedService = new AppletsSavedService(_fixture.Db, _configuration);
            CommonService commonService = new CommonService(fixture.Db, _configuration,
                httpContext, personService,appletsSavedService,interfaceEngineService);
            FacilityHousingService facilityHousingService = new FacilityHousingService(fixture.Db, _configuration, httpContext);
            FormsService formsService = new FormsService(_fixture.Db, httpContext, commonService, personService,
                interfaceEngineService);

            InventoryInmateService inventoryInmateService =
                new InventoryInmateService(_fixture.Db, commonService, httpContext, formsService,
                    _configuration, personService, photosService, facilityHousingService, interfaceEngineService);
            _inventoryLostFoundService =
                new InventoryLostFoundService(_fixture.Db, httpContext, inventoryInmateService, photosService);
        }

        [Fact]
        public void GetLostFoundInventory()
        {
            //Arrange
            SearchOptionsView searchOptions = new SearchOptionsView
            {
                FacilityId = 1,
                GlobalFacilityId = 1,
                GroupId = 6,
                DeleteFlag = true,
                FoundPersonalId = 11,
                DispositionCode = 3,
                BinId = 5,
                PageIntialize = true
            };

            //Act
            LostFoundInventory lostFound = _inventoryLostFoundService.GetLostFoundInventory(searchOptions);

            //Assert
            InventoryDetails inventoryLostFound = lostFound.InventoryGrid.Single(i => i.PersonalGroupName == "PG100022");
            Assert.Equal("DRESS WITH THE BIN001", inventoryLostFound.PropertyGroupNotes);

        }

        [Fact]
        public void ItemList()
        {
            //Act
            InventoryLostFoundVm inventoryLostFound = _inventoryLostFoundService.ItemList(8);

            //Assert
            Assert.Equal("FOUNDED BY OTHERS", inventoryLostFound.LostFoundByOther);

        }

        [Fact]
        public async Task InsertInventory()
        {
            //Arrange
            IdentitySave identitySave = new IdentitySave
            {
                PersonalInventoryGroupId = 6,
                AppliedInmateId = 140,
                PersonalInventoryId = new List<int>
                {
                    12,
                    11
                }
            };
            PersonalInventory personalInventory =
                _fixture.Db.PersonalInventory.Single(p => p.PersonalInventoryId == 12);
            Assert.Equal(130, personalInventory.InmateId);

            //Act
            await _inventoryLostFoundService.InsertInventory(identitySave);

            //Assert
            personalInventory = _fixture.Db.PersonalInventory.Single(p => p.PersonalInventoryId == 12);
            Assert.Equal(140, personalInventory.InmateId);
        }

        [Fact]
        public async Task DeleteOrUndoLostFound()
        {
            //Arrange
            InventoryDetails inventoryDetails = new InventoryDetails
            {
                PersonalInventoryId = 15,
                UpdateFlag = true,
                DeleteReason = "OTHER REASON",
                DeleteFlag = 1
            };

            PersonalInventory personalInventory =
                _fixture.Db.PersonalInventory.Single(p => p.PersonalInventoryId == 15);
            Assert.Equal(0, personalInventory.DeleteFlag);

            //Act
            await _inventoryLostFoundService.DeleteOrUndoLostFound(inventoryDetails);

            //Arrange
            personalInventory = _fixture.Db.PersonalInventory.Single(p => p.PersonalInventoryId == 15);
            Assert.Equal(1, personalInventory.DeleteFlag);
            Assert.Equal("OTHER REASON", personalInventory.DeleteReason);

        }

        [Fact]
        public void InventoryPdfGet()
        {
            //Act
            InventoryLostFoundVm inventoryLostFound = _inventoryLostFoundService.InventoryPdfGet(5);

            //Assert
            Assert.NotNull(inventoryLostFound);
        }

        [Fact]
        public void GetHousingUnitNumberDetails()
        {
            //Act
            List<HousingDetail> lstHousingDetails = _inventoryLostFoundService.GetHousingUnitNumberDetails(1, "FLOOR2");

            //Assert
            HousingDetail housingDetails = lstHousingDetails.Single(h => h.HousingUnitNumber == "UP-A");
            Assert.NotNull(housingDetails);
        }
        //8559 SVN revision
        //Issue checked damaged flag and found by. 
        [Fact]
        public void HistoryDetails()
        {
            HistorySearch historySearch = new HistorySearch
            {
                FacilityId = 2,
                DispositionCode = 3,
                Deleted = false,
                Damaged = true,
                Color = "BLACK",
                Keyword1 = "S",
                Keyword2 = "SS",
                TopRecords = 1,
                FoundBy = true,
                PropertyGroup = "PG100022"
            };

            List<InventoryLostFoundHistory> inventoryLostFoundHistory = _inventoryLostFoundService.
                HistoryDetails(historySearch);
            InventoryLostFoundHistory inventoryLost = inventoryLostFoundHistory.Single(h => h.InmateId == 125);
            Assert.Equal("PG100022", inventoryLost.GroupNumber);
            Assert.Equal("BIN001", inventoryLost.Bin);
        }

    }
}
