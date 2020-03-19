using GenerateTables.Models;
using JwtDb.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Moq;
using ServerAPI.JMS.Tests.Helper;
using ServerAPI.Services;
using ServerAPI.Tests;
using ServerAPI.ViewModels;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

// ReSharper disable once CheckNamespace
namespace ServerAPI.JMS.Tests
{
    [Collection("Database collection")]
    public class InmateCellServiceTest
    {
        private readonly InmateCellService _inmateCellService;
        private readonly DbInitialize _fixture;
        private readonly IConfigurationRoot _configuration = MockHelpers.GetIConfigurationRoot();
        private readonly Mock<IMemoryCache> _memory = new Mock<IMemoryCache>();
        public InmateCellServiceTest(DbInitialize fixture)
        {
            _fixture = fixture;
            Mock<UserManager<AppUser>> userMgr = MockHelpers.MockUserManager<AppUser>();
            HttpContextAccessor httpContext = new HttpContextAccessor { HttpContext = fixture.Context.HttpContext };
            PersonService personService = new PersonService(_fixture.Db);
            PhotosService photosService = new PhotosService(_fixture.Db, _configuration);
            InterfaceEngineService interfaceEngineService = new InterfaceEngineService(fixture.Db, _configuration,
                _memory.Object);
            AppletsSavedService appletsSavedService = new AppletsSavedService(fixture.Db, _configuration);
            CommonService commonService = new CommonService(_fixture.Db, _configuration,
                 httpContext, personService,appletsSavedService,interfaceEngineService);
            FacilityPrivilegeService facilityPrivilegeService = new FacilityPrivilegeService(_fixture.Db, httpContext,
                personService);
          
            InmateService inmateService = new InmateService(_fixture.Db, commonService, personService, httpContext,
                facilityPrivilegeService, photosService);
            HousingConflictService housingConflictService = new HousingConflictService(_fixture.Db, inmateService,
                photosService);
            LockdownService lockdownService = new LockdownService(_fixture.Db, httpContext);
            HousingService housingService = new HousingService(_fixture.Db, housingConflictService, httpContext, photosService, facilityPrivilegeService,
                lockdownService, interfaceEngineService);
            _inmateCellService = new InmateCellService(fixture.Db, httpContext, housingService, photosService);
        }


        [Fact]
        public void GetInmateCell()
        {
            //Arrange
            CellInmateInputs cellInmateInputs = new CellInmateInputs
            {
                FacilityId = 1,
                HousingUnitListId = 6,
                TabId = 0
            };
            //Act
            InmateCellVm inmateCell = _inmateCellService.GetInmateCell(cellInmateInputs);
            //Assert
            Assert.True(inmateCell.HousingVisitationDetails.Count > 0);

            //Arrange
            cellInmateInputs = new CellInmateInputs
            {
                FacilityId = 1,
                FromDate = DateTime.Now,
                ThruDate = DateTime.Now,
                TabId = 1,
                HousingUnitListId = 5
            };
            //Act
            inmateCell = _inmateCellService.GetInmateCell(cellInmateInputs);
            //Assert
            Assert.True(inmateCell.HousingInmateHistoryList.Count > 0);
        }

        [Fact]
        public void GetCellLog()
        {
            //Act
            CellLogVm cellLog = _inmateCellService.GetCellLog(21, 1);

            //Assert
            Assert.Equal(6, cellLog.CellLogDetail.HousingUnitListId);
            Assert.True(cellLog.HousingDetail.Count > 0);
        }

        [Fact]
        public async Task InmateCellLog()
        {
            //Arrange
            InmateCellLogDetailsVm inmateCellLogDetails = new InmateCellLogDetailsVm
            {
                FacilityId = 2,
                HousingUnitListId = 7,
                Mode = InputMode.Add,
                Note = "CELLLOG COUNT IS MISSING",
                Count = 5,
                LogDate = DateTime.Now,
                LogTime = "6",
                NoteType = "MISC"
            };

            CellLog cellLog = _fixture.Db.CellLog.SingleOrDefault(c => c.HousingUnitListId == 7);
            Assert.Null(cellLog);

            //Act
            await _inmateCellService.InmateCellLog(inmateCellLogDetails);

            //Assert
            cellLog = _fixture.Db.CellLog.SingleOrDefault(c => c.HousingUnitListId == 7);
            Assert.NotNull(cellLog);
            Assert.Equal(5, cellLog.CellLogCount);

        }

    }
}
