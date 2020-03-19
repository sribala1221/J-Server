using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using ServerAPI.Services;
using ServerAPI.Tests;
using ServerAPI.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using GenerateTables.Models;
using ServerAPI.JMS.Tests.Helper;
using Moq;
using Microsoft.AspNetCore.Identity;
using JwtDb.Models;
using Microsoft.Extensions.Caching.Memory;

// ReSharper disable once CheckNamespace
namespace ServerAPI.JMS.Tests
{

    [Collection("Database collection")]
    public class CellServiceTest
    {
        private readonly CellService _cellService;
        private readonly IConfigurationRoot _configuration = MockHelpers.GetIConfigurationRoot();
        private readonly Mock<IMemoryCache> _memory = new Mock<IMemoryCache>();
        public CellServiceTest(DbInitialize fixture)
        {
            DbInitialize fixture1 = fixture;
            PersonService personService = new PersonService(fixture.Db);
            AppletsSavedService appletsSavedService = new AppletsSavedService(fixture.Db, _configuration);
            PhotosService photosService = new PhotosService(fixture.Db, _configuration);
            Mock<UserManager<AppUser>> userMgr = MockHelpers.MockUserManager<AppUser>();
            HttpContextAccessor httpContext = new HttpContextAccessor { HttpContext = fixture.Context.HttpContext };

            InterfaceEngineService interfaceEngineService = new InterfaceEngineService(fixture.Db,
                _configuration, _memory.Object);

            CommonService commonService = new CommonService(fixture1.Db, _configuration,  httpContext,personService,appletsSavedService,interfaceEngineService);
            FacilityPrivilegeService facilityPrivilegeService = new FacilityPrivilegeService(fixture.Db,
                httpContext,personService);
            FacilityHousingService facilityHousingService = new FacilityHousingService(fixture1.Db, 
                _configuration, httpContext);

            _cellService = new CellService(fixture.Db, commonService, httpContext,
                facilityPrivilegeService, facilityHousingService, fixture1.JwtDb, userMgr.Object);
        }


        [Fact]
        public void GetFloorNotesDetails()
        {
            //Arrange
            MyLogRequestVm myLogRequest = new MyLogRequestVm
            {
                PersonnelId = 11,
                Hours = 12,
                Keyword = "VISITOR FOR JAIL INMATE"
            };
            //Act
            IQueryable<FloorNotes> floor = _cellService.GetFloorNotesDetails(myLogRequest);
            //Assert
            FloorNotes floorNotes = floor.Single(f => f.FloorNoteId == 20);
            Assert.Equal("CHENNAI", floorNotes.FloorNoteLocation);

        }

        [Fact]
        public void GetHousingGroup()
        {
            //Act
            List<HousingUnitGroupVm> lstHousingGroup = _cellService.GetHousingGroup(1);
            //Assert
            HousingUnitGroupVm housingUnitGroup = lstHousingGroup.Single(h => h.HousingUnitGroupId == 10);
            Assert.Equal("HOUSING A BLOCK", housingUnitGroup.GroupName);
            Assert.Equal("'CHENNAI','MADURAI','TRICHY','COIMBATORE','PUDUKKOTTAI'", housingUnitGroup.LocationString);
            Assert.Equal("'FLOOR1 DOWN-A','FLOOR1 UP-A','FLOOR2 DOWN-A','FLOOR1 UP-B'", housingUnitGroup.GroupString);
        }


        [Fact]
        public void GetPrivilegesDetails()
        {
            //Arrange
            MyLogRequestVm myLogRequest = new MyLogRequestVm
            {
                Location = "FLOOR1",
                Number = "UP-A"
            };
            //Act
            IQueryable<Privileges> lstprivileges = _cellService.GetPrivilegesDetails(myLogRequest);

            //Assert
            Privileges privileges = lstprivileges.Single(p => p.PrivilegeId == 5);
            Assert.Equal("CHENNAI", privileges.PrivilegeDescription);
            Assert.Equal("REVOKE", privileges.PrivilegeType);
        }

        [Fact]
        public void GetMyLogDetails()
        {
            //Arrange
            MyLogRequestVm myLogRequest = new MyLogRequestVm
            {
                PersonnelId = 11,
                FacilityId = 2,
                IsLogSearch = new LogSettingDetails()
            };
            //Act
            MylogDetailsVm mylogDetails = _cellService.GetMyLogDetails(myLogRequest);
            //Assert
            HousingDetail housingDetail = mylogDetails.HousingLst.Single(x => x.HousingUnitListId == 11);
            Assert.Equal("DOWN-A", housingDetail.HousingUnitNumber);
        }

        [Fact]
        public void GetMyLogDetailsCount()
        {
            //Arrange
            MyLogRequestVm myLogRequest = new MyLogRequestVm
            {
                FromDate = DateTime.Now.Date.AddDays(-3),
                ToDate = DateTime.Now,
                IsLogSearch = new LogSettingDetails
                {
                    IsSetLocation = true
                }
            };
            //Act
            MyLogCountDetailsVm myLogCountDetails = _cellService.GetMyLogDetailsCount(myLogRequest);
            //Assert
            CellLogDetailsVm cellLogDetails = myLogCountDetails.CellLogDetailsLst.Single(c => c.CellLogId == 17);
            Assert.Equal("CHENNAI", cellLogDetails.LastLocTrack);

        }
    }
}
