using JwtDb.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Moq;
using ServerAPI.JMS.Tests.Helper;
using ServerAPI.Services;
using ServerAPI.Tests;
using ServerAPI.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using GenerateTables.Models;
using Microsoft.Extensions.Caching.Memory;

// ReSharper disable once CheckNamespace
namespace ServerAPI.JMS.Tests
{

    [Collection("Database collection")]
    public class RequestServiceTest
    {
        private readonly RequestService _requestService;
        private readonly DbInitialize _fixture;
        private readonly IConfigurationRoot _configuration = MockHelpers.GetIConfigurationRoot();
        private readonly Mock<IMemoryCache> _memory = new Mock<IMemoryCache>();

        public RequestServiceTest(DbInitialize fixture)
        {
            _fixture = fixture;
            AppUser appUser = new AppUser { UserName = "dssiadmin", Id = "11" };
            AppletsSavedService appletsSavedService = new AppletsSavedService(_fixture.Db, _configuration);
            Mock<UserManager<AppUser>> userMgr = MockHelpers.MockUserManager<AppUser>();

            userMgr.Setup(x => x.FindByNameAsync(It.Is<string>(u => u.Equals("dssiadmin")))).ReturnsAsync(appUser);

            userMgr.Setup(x => x.GetRolesAsync(It.Is<AppUser>(u => u.Equals(appUser))))
                .ReturnsAsync(new List<string> { "personnel_id" });

            var roleMgr = MockHelpers.MockRoleManager<IdentityRole>();

            var role = new IdentityRole
            {
                Id = "11",
                Name = "personnel_id",
                NormalizedName = "personnel_idr"
            };
            roleMgr.Setup(x => x.FindByNameAsync(It.Is<string>(u => u.Equals("personnel_id"))))
                .ReturnsAsync(role);
            HttpContextAccessor httpContext = new HttpContextAccessor { HttpContext = fixture.Context.HttpContext };
            PhotosService photosService = new PhotosService(fixture.Db, _configuration);
            PersonService personService = new PersonService(fixture.Db);
            AtimsHubService atimsHubService = new AtimsHubService(_fixture.HubContext.Object);
            InterfaceEngineService interfaceEngineService = new InterfaceEngineService(_fixture.Db,
                _configuration, _memory.Object);

            CommonService commonService = new CommonService(fixture.Db, _configuration,
                httpContext, personService,appletsSavedService,interfaceEngineService);
            FacilityPrivilegeService facilityPrivilegeService = new FacilityPrivilegeService(fixture.Db, httpContext,
                personService);

            InmateService inmateService = new InmateService(fixture.Db, commonService, personService, httpContext,
                facilityPrivilegeService, photosService);
            FacilityHousingService facilityHousingService = new FacilityHousingService(fixture.Db, _configuration,
                httpContext);
            _requestService = new RequestService(fixture.Db, httpContext, userMgr.Object, roleMgr.Object,
                personService, inmateService, facilityHousingService, atimsHubService);

        }

        [Fact]
        public async Task SaveRequestDetail()
        {
            //Arrange
            RequestVm requestVm = new RequestVm
            {
                ActionLookupId = 10,
                InmateId = 104,
                RequestNote = "NEW  REQUEST RAISE"

            };

            //Before Insert
            Request request = _fixture.Db.Request.SingleOrDefault(r => r.InmateId == 104);
            Assert.Null(request);

            //Act
            await _requestService.SaveRequestDetail(requestVm);

            //Assert
            //After Insert
            request = _fixture.Db.Request.Single(r => r.InmateId == 104);
            Assert.NotNull(request);
        }

        [Fact]
        public async Task UpdateRequestDetails()
        {
            //Arrange
            RequestOperations requestOperations = new RequestOperations
            {
                RequestId = 14,
                RequestStatus = RequestStatus.Pending
            };
            //Before Update
            Request request = _fixture.Db.Request.Single(r => r.RequestId == 14);
            Assert.Equal(13, request.UpdatedBy);

            //Before Insert
            RequestTrack requestTrack = _fixture.Db.RequestTrack.SingleOrDefault(r => r.RequestId == 14);
            Assert.Null(requestTrack);

            //Act
            await _requestService.UpdateRequestDetails(requestOperations);

            //Assert
            //After Update
            request = _fixture.Db.Request.Single(r => r.RequestId == 14);
            Assert.Equal(11, request.UpdatedBy);

            //After Insert
            requestTrack = _fixture.Db.RequestTrack.Single(r => r.RequestId == 14);
            Assert.NotNull(requestTrack);
        }

        [Fact]
        public async Task SaveRequestTransfer()
        {
            //Arrange
            RequestTransfer requestTransfer = new RequestTransfer
            {
                RequestId = 15,
                RequestStatus = RequestDetailsStatus.Note
            };

            //Before Insert
            RequestTrack requestTrack = _fixture.Db.RequestTrack.SingleOrDefault(r => r.RequestId == 15);
            Assert.Null(requestTrack);

            //Act
            await _requestService.SaveRequestTransfer(requestTransfer);

            //Assert
            //After Insert
            requestTrack = _fixture.Db.RequestTrack.Single(r => r.RequestId == 15);
            Assert.NotNull(requestTrack);
        }

        [Fact]
        public void GetRequestActionList()
        {
            //Arrange
            RequestDetails requestDetails = new RequestDetails
            {
                RequestType = RequestTypeEnum.Housing,
                FacilityId = 1
            };
            //Act
            List<RequestVm> lstRequest = _requestService.GetRequestActionList(requestDetails);

            //Assert
            Assert.True(lstRequest.Count > 3);
        }

        [Fact]
        public async Task GetPenRequestDetails()
        {
            //Arrange
            int[] groups = { };

            //Act
            ReqResponsibilityVm requestDetails = await _requestService.GetPenRequestDetails(1, groups, "status");

            //Assert
            Assert.True(requestDetails.RequestDetailLst.Count > 0);
            Assert.True(requestDetails.Responsibilities.Count > 1);
        }

        [Fact]

        public async Task GetAssignRequest()
        {
            //Act
            ReqResponsibilityVm reqResponsibility = await _requestService.GetAssignRequest(106, 1, "");

            //Assert
            Assert.True(reqResponsibility.AssignedLst.Count > 1);
            Assert.True(reqResponsibility.Responsibilities.Count > 1);
        }

        [Fact]
        public async Task GetRequestCount()
        {
            //Act
            KeyValuePair<int, int> result = await _requestService.GetRequestCount(1);

            //Assert
            Assert.Equal(3, result.Value);
        }


        [Fact]
        public async Task GetRequestStatus()
        {
            //Act
            ReqResponsibilityVm requestStatus = await _requestService.GetRequestStatus(2);

            //Assert
            Assert.True(requestStatus.RequestDetailLst.Count > 1);
        }

        [Fact]
        public async Task GetRequestSearch()
        {
            //Arrange
            RequestValues requestValues = new RequestValues
            {
                RequestStatus = RequestStatus.Assigned,
                InmateId = 110,
                FacilityId = 2,
                Top = 10,
                ActionLookupId = 19
            };

            //Act
            List<RequestVm> lstRequest = await _requestService.GetRequestSearch(requestValues);

            //Assert
            Assert.True(lstRequest.Count > 0);

        }

        [Fact]
        public async Task RequestAction()
        {
            //Act
            List<RequestOperations> lstRequestOperations = await _requestService.RequestAction(2);

            //Assert
            Assert.Equal(10, lstRequestOperations[0].RequestId);
            Assert.Equal("VISITOR REQUEST", lstRequestOperations[0].ActionLookup);
            Assert.Equal(RequestStatus.None, lstRequestOperations[0].RequestStatus);
        }
        [Fact]
        public void GetBookingPendingReq()
        {
            //Act
            List<RequestTypes> lstRequestTypes = _requestService.GetBookingPendingReq(1, 3);

            //Assert
            Assert.True(lstRequestTypes.Count > 0);
        }

        [Fact]
        public void GetBookingAssignedReq()
        {
            //Act
            List<RequestTypes> lstRequestTypes = _requestService.GetBookingAssignedReq(1);

            //Assert
            Assert.True(lstRequestTypes.Count > 0);

        }

        [Fact]
        public void GetBookingPendingReqDetail()
        {
            //Act
            List<RequestVm> lstRequest = _requestService.GetBookingPendingReqDetail(2, 16, 5);

            //Assert
            RequestVm request = lstRequest.Single(r => r.RequestId == 16);
            Assert.Equal(110, request.InmateId);
        }

        [Fact]
        public void GetBookingAssignedReqDetail()
        {
            //Act
            List<RequestVm> lstRequest = _requestService.GetBookingAssignedReqDetail(15, 7);

            //Assert
            Assert.True(lstRequest.Count > 1);
        }

        [Fact]
        public void GetInmateRequest()
        {
            //Act
            IEnumerable<InmateRequestVm> lstInmateRequest = _requestService.GetInmateRequest(106, null, null);

            //Assert
            Assert.True(lstInmateRequest.Count() > 2);
        }


        [Fact]
        public void GetRequestDetailsById()
        {
            //Act
            RequestTrackVm requestTrack = _requestService.GetRequestDetailsById(10);

            //Assert
            Assert.Equal("VISITATION REQUEST", requestTrack.RequestDetails.Action);
            Assert.Equal("CHS0010", requestTrack.RequestDetails.InmateNumber);
            Assert.True(requestTrack.LstRequestDetails.Count > 0);

        }

        [Fact]
        public void GetInmateRequestCount()
        {
            //Act
            InmateRequestCount inmateRequestCount = _requestService.GetInmateRequestCount(108, null);

            //Assert
            Assert.NotNull(inmateRequestCount);
        }


    }
}
