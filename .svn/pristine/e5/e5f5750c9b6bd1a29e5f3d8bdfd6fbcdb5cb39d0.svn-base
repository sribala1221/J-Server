using System;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Moq;
using ServerAPI.JMS.Tests.Helper;
using ServerAPI.Services;
using ServerAPI.Tests;
using ServerAPI.ViewModels;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Xunit;
using GenerateTables.Models;
using JwtDb.Models;
using Microsoft.Extensions.Caching.Memory;

// ReSharper disable once CheckNamespace
namespace ServerAPI.JMS.Tests
{
    [Collection("Database collection")]
    public class TempHoldServiceTest
    {

        private readonly TempHoldService _tempHoldService;
        private readonly IConfigurationRoot _configuration = MockHelpers.GetIConfigurationRoot();
        private readonly DbInitialize _fixture;
        private readonly Mock<IMemoryCache> _memory = new Mock<IMemoryCache>();

        public TempHoldServiceTest(DbInitialize fixture)
        {
            _fixture = fixture;
            Mock<UserManager<AppUser>> userMgr = MockHelpers.MockUserManager<AppUser>();
            AppletsSavedService appletsSavedService = new AppletsSavedService(_fixture.Db, _configuration);
            PersonService personService = new PersonService(_fixture.Db);
            PhotosService photosService = new PhotosService(fixture.Db, _configuration);
            HttpContextAccessor httpContext = new HttpContextAccessor { HttpContext = fixture.Context.HttpContext };
            InterfaceEngineService interfaceEngineService = new InterfaceEngineService(_fixture.Db,
                _configuration, _memory.Object);
            CommonService commonService = new CommonService(fixture.Db, _configuration, httpContext, personService,appletsSavedService,interfaceEngineService);

            _tempHoldService = new TempHoldService(fixture.Db, commonService, httpContext);
        }


        [Fact]
        public void GetIntakeTempHoldDetails()
        {
            //Arrange
            PersonnelSearchVm personnelSearch = new PersonnelSearchVm
            {
                PersonnelId = 11,
                PersonDetail = new PersonVm
                {
                    FacilityId = 1
                },
                PersonnelSearchText = new string[]
                {


                }
            };

            //Act
            IntakeTempHoldVm inatakeTempHold = _tempHoldService.GetIntakeTempHoldDetails(personnelSearch);

            //Assert
            Assert.True(inatakeTempHold.LstTempHoldLocation.Count > 0);
            Assert.True(inatakeTempHold.LstTempHoldType.Count > 1);


        }

        [Fact]
        public void GetTempHoldCompleteStepLookup()
        {
            //Act
            TempHoldCompleteStepLookup tempHoldCompleteStep = _tempHoldService.GetTempHoldCompleteStepLookup(11);

            //Assert
            Assert.True(tempHoldCompleteStep.ListDisposition.Count > 0);
            Assert.True(tempHoldCompleteStep.ListLocation.Count > 0);
            Assert.True(tempHoldCompleteStep.ListType.Count > 0);
            Assert.Equal("RELEASE OUT", tempHoldCompleteStep.PrebookComplete.TempHoldCompleteNote);

        }

        ////Due to rewrite SaveIntakeTempHold method
        //[Fact]
        //public async Task SaveIntakeTempHold()
        //{
        //    IntakeTempHoldParam intakeTempHoldParam = new IntakeTempHoldParam
        //    {
        //        TempHoldLocationId = 7,
        //        FacilityId = 1,
        //        ReceivingOfficerId = 11,
        //        CreateBy = 11,
        //        InmatePrebookId = 15,
        //        TempHoldNote = "RELEASE AND HOLD",
        //        TempHoldLocation = null,
        //        TempHoldTypeId = 5
        //    };

        //    await _tempHoldService.SaveIntakeTempHold(intakeTempHoldParam);

        //}

        [Fact]
        public async Task UpdateTempHold()
        {
            //Arrange
            PrebookCompleteVm prebookComplete = new PrebookCompleteVm
            {
                TempHoldId = 12,
                TempHoldNote = "RELEASE IN",
                TempHoldLocationId = 5,
                TempHoldLocation = "CHENNAI",
                TempHoldCompleteFlag = 1,
                BookingRequired = true,
                InmatePrebookId = 15,
                TempHoldCompleteNote = "testing note",
                TempHoldDisposition = 10
            };

            //Before Update
            InmatePrebook inmatePrebook = _fixture.Db.InmatePrebook.Single(i => i.InmatePrebookId == 15);
            Assert.Equal(12, inmatePrebook.TempHoldId);

            //Act
            await _tempHoldService.UpdateTempHold(prebookComplete);

            //Assert
            //After Update
            inmatePrebook = _fixture.Db.InmatePrebook.Single(i => i.InmatePrebookId == 15);
            Assert.Null(inmatePrebook.TempHoldId);
        }

        [Fact]
        public void GetTempHoldDetails()
        {
            //Arrange
            TempHoldDetailsVm tempHoldDetails = new TempHoldDetailsVm
            {
                FacilityId = 1,
                TempHoldId = 5,
                Disposition = 4,
                FromDate = DateTime.Now.AddDays(-1),
                ToDate = DateTime.Now
            };

            //Act
            TempHoldVm tempHold = _tempHoldService.GetTempHoldDetails(tempHoldDetails);

            //Assert
            Assert.True(tempHold.TempHoldTypeCnt.Count>0);
            Assert.True(tempHold.DispositionCnt.Count > 0);
            Assert.True(tempHold.TempHoldDetails.Count > 0);
        }
    }
}
