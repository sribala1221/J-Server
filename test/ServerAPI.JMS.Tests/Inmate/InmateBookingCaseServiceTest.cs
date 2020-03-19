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
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

// ReSharper disable once CheckNamespace
namespace ServerAPI.JMS.Tests
{

    [Collection("Database collection")]
    public class InmateBookingCaseServiceTest
    {
        private readonly InmateBookingCaseService _inmateBookingCase;

        private readonly IConfigurationRoot _configuration = MockHelpers.GetIConfigurationRoot();
        private readonly DbInitialize _fixture;
        private readonly Mock<IMemoryCache> _memory = new Mock<IMemoryCache>();

        public InmateBookingCaseServiceTest(DbInitialize fixture)
        {
            _fixture = fixture;
            HttpContextAccessor httpContext = new HttpContextAccessor { HttpContext = _fixture.Context.HttpContext };
            PersonService personService = new PersonService(fixture.Db);
            AppletsSavedService appletsSavedService = new AppletsSavedService(fixture.Db, _configuration);
            Mock<UserManager<AppUser>> userMgr = MockHelpers.MockUserManager<AppUser>();
            PhotosService photosService = new PhotosService(_fixture.Db, _configuration);
            InterfaceEngineService interfaceEngineService = new InterfaceEngineService(fixture.Db, _configuration,
                _memory.Object);
            CommonService commonService = new CommonService(_fixture.Db, _configuration, httpContext, personService,appletsSavedService,interfaceEngineService);
            FacilityHousingService facilityHousingService = new FacilityHousingService(_fixture.Db, _configuration, httpContext);
            FacilityPrivilegeService facilityPrivilegeService = new FacilityPrivilegeService(fixture.Db, httpContext,
                personService);
            RecordsCheckService recordsCheckService = new RecordsCheckService(_fixture.Db, commonService, httpContext);
            InmateService inmateService = new InmateService(_fixture.Db, commonService, personService,
                httpContext, facilityPrivilegeService, photosService);
            BookingReleaseService bookingReleaseService = new BookingReleaseService(_fixture.Db, commonService,
                httpContext, personService, inmateService, recordsCheckService, facilityHousingService,
                interfaceEngineService);
            _inmateBookingCase = new InmateBookingCaseService(_fixture.Db, httpContext, bookingReleaseService,
                commonService);
        }

        [Fact]
        public void GetInmateBookingBailDetails()
        {
            //Act
            BookingBailDetails bookingBail = _inmateBookingCase.GetInmateBookingBailDetails(14);

            //Assert
            Assert.Equal(5000, bookingBail.BailDetails.BailAmount);
            Assert.Equal("NO BAIL", bookingBail.BailDetails.BailType);
            Assert.True(bookingBail.BailTransactionDetails.Count > 0);
        }

        [Fact]
        public async Task DeleteUndoBailTransaction()
        {
            //Before Delete
            BailTransaction bailTransaction = _fixture.Db.BailTransaction.Single(b => b.BailTransactionId == 9);
            Assert.Null(bailTransaction.VoidFlag);
            Assert.Null(bailTransaction.VoidDate);

            //Act
            await _inmateBookingCase.DeleteUndoBailTransaction(9);

            //After Delete
            //Assert
            bailTransaction = _fixture.Db.BailTransaction.Single(b => b.BailTransactionId == 9);
            Assert.Equal(1, bailTransaction.VoidFlag);
            Assert.NotNull(bailTransaction.VoidDate);
        }

        [Fact]
        public void GetBailSaveHistory()
        {
            //Act
            List<BailSaveHistory2Vm> lstBailSaveHistory = _inmateBookingCase.GetBailSaveHistory(5);

            //Assert
            Assert.True(lstBailSaveHistory.Count > 1);
        }

        [Fact]
        public async Task UpdateBail()
        {
            //Act
            BailDetails bailDetails = new BailDetails
            {
                ArrestId = 15,
                BailAmount = 5000,
                BailType = "BAIL BOND"
            };
            //Before Update
            Arrest arrest = _fixture.Db.Arrest.Single(a => a.ArrestId == 15);
            Assert.Equal(1500, arrest.BailAmount);
            Assert.Equal("NO BAIL", arrest.BailType);

            //Before Insert History Table
            BailSaveHistory2 bailSaveHistory2 = _fixture.Db.BailSaveHistory2.SingleOrDefault(b => b.ArrestId == 15);
            Assert.Null(bailSaveHistory2);

            //Act
            await _inmateBookingCase.UpdateBail(bailDetails);

            //After Update
            //Assert
            arrest = _fixture.Db.Arrest.Single(a => a.ArrestId == 15);
            Assert.Equal(5000, arrest.BailAmount);
            Assert.Equal("BAIL BOND", arrest.BailType);

            //After Insert History Table
            bailSaveHistory2 = _fixture.Db.BailSaveHistory2.Single(b => b.ArrestId == 15);
            Assert.NotNull(bailSaveHistory2);
        }

        [Fact]
        public void GetBailCompanyDetails()
        {
            //Act
            BailCompanyDetails bailCompanyDetails = _inmateBookingCase.GetBailCompanyDetails();

            //Assert
            Assert.True(bailCompanyDetails.BailAgencies.Count > 1);
            Assert.True(bailCompanyDetails.BailCompanies.Count > 1);

        }

        [Fact]
        public void GetBailAgentHistoryDetails()
        {
            //Act
            List<HistoryVm> lstHistoryVms = _inmateBookingCase.GetBailAgentHistoryDetails(10);
            
            //Assert
            HistoryVm history = lstHistoryVms.Single(h => h.HistoryId == 10);
            Assert.Equal("SANGEETHA", history.PersonFirstName);
            Assert.Equal(50, history.PersonId);

        }

        [Fact]
        public async Task InsertUpdateBailAgent()
        {
            //Arrange
            BailAgentVm bailAgent = new BailAgentVm
            {
                BailAgentId = 12,
                BailAgentFirstName = "VEL",
                BailAgentMiddleName = "MUTHU",
                BailAgentLastName = "KUMAR",
                BailAgentLicenseNum = "147845",
                BailAgentHistoryList = @"{'LAST NAME':'VEL','FIRST NAME':'VEL','MIDDLE NAME':'MUTHU','LICENSE NUMBER':'147845','BAIL COMPANIES':'HIFI'}",
                BailCompanyIds = new List<KeyValuePair<int, bool>>
                {
                    new KeyValuePair<int, bool>
                  (11,true)
                }

            };
            //Before Insert
            BailAgent bail = _fixture.Db.BailAgent.Single(b => b.BailAgentId == 12);
            Assert.Null(bail.BailAgentLastName);
            Assert.Null(bail.BailAgentMiddleName);

            //Before Insert history table
            BailAgentHistory bailAgentHistory =
                _fixture.Db.BailAgentHistory.SingleOrDefault(bh => bh.BailAgentId == 12);
            Assert.Null(bailAgentHistory);

            await _inmateBookingCase.InsertUpdateBailAgent(bailAgent);

            //After Insert
            //Assert
            bail = _fixture.Db.BailAgent.Single(b => b.BailAgentId == 12);
            Assert.Equal("KUMAR", bail.BailAgentLastName);
            Assert.Equal("MUTHU", bail.BailAgentMiddleName);

            //After Insert history table
            bailAgentHistory =
                _fixture.Db.BailAgentHistory.Single(bh => bh.BailAgentId == 12);
            Assert.NotNull(bailAgentHistory);
        }

        [Fact]
        public async Task DeleteBailAgent()
        {
            //Before Delete
            BailAgent bailAgent = _fixture.Db.BailAgent.Single(ba => ba.BailAgentId == 11);
            Assert.Null(bailAgent.BailAgentDeleteFlag);

            //Act
            await _inmateBookingCase.DeleteBailAgent(11);

            //After Delete
            //Assert
            bailAgent = _fixture.Db.BailAgent.Single(ba => ba.BailAgentId == 11);
            Assert.Equal(1, bailAgent.BailAgentDeleteFlag);
        }

        [Fact]
        public async Task InsertBailTransaction()
        {
            //Arrange
            BailTransactionDetails bailTransaction = new BailTransactionDetails
            {
                BailCompanyId = 102,
                ArrestId = 7,
                AmountPosted = 1000,
                CreateDate = DateTime.Now.AddDays(-15),
                BailAgentId = 11,
                PaymentTypeLookup = "MISC"
            };
            //Before Insert
            BailTransaction bail = _fixture.Db.BailTransaction.SingleOrDefault(b => b.BailCompanyId == 102);
            Assert.Null(bail);

            //Act
            await _inmateBookingCase.InsertBailTransaction(bailTransaction);
        
            //Assert
            //After Insert
            bail = _fixture.Db.BailTransaction.SingleOrDefault(b => b.BailCompanyId == 102);
            Assert.NotNull(bail);
        }

        [Fact]
        public void GetBookingBailAmount()
        {
            //Act
            BailDetails bailDetails = _inmateBookingCase.GetBookingBailAmount(10, 105);
            
            //Assert
            Assert.Equal("BONDABLE", bailDetails.BailType);
            Assert.Equal(4500M,bailDetails.BailAmount);

        }


    }
}

