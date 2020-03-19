using ServerAPI.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ServerAPI.Services
{
	public interface IRequestService
	{
		Task<KeyValuePair<int, int>> GetRequestCount(int facilityId);
		Task<int> SaveRequestDetail(RequestVm requestDetails);
		Task<int> UpdateRequestDetails(RequestOperations requests);
		Task<int> SaveRequestTransfer(RequestTransfer transNote);
		List<RequestVm> GetRequestActionList(RequestDetails request);
		Task<ReqResponsibilityVm> GetPenRequestDetails(int facilityId, int[] housingGroup, string fromScreen);
		Task<ReqResponsibilityVm> GetAssignRequest(int inmateId, int facilityId, string fromScreen);
        Task<List<RequestOperations>> RequestAction(int facilityId);
        Task<List<RequestVm>> GetRequestSearch(RequestValues values);
		Task<ReqResponsibilityVm> GetRequestStatus(int facilityId);
		List<RequestTypes> GetBookingPendingReq(int facilityId, int showInFlag);
	    List<RequestTypes> GetBookingAssignedReq(int showInFlag);
	    List<RequestVm> GetBookingPendingReqDetail(int facilityId, int requestLookupId, int showInFlag);
	    List<RequestVm> GetBookingAssignedReqDetail(int requestLookupId, int showInFlag);
		IEnumerable<InmateRequestVm> GetInmateRequest(int inmateId, int? requestId, int? actionId);
		InmateRequestCount GetInmateRequestCount(int inmateId, int? requestId);
		RequestTrackVm GetRequestDetailsById(int requestId);
	}
}
