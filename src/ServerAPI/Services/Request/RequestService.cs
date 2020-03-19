﻿using GenerateTables.Models;
using Microsoft.AspNetCore.Http;
using ServerAPI.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using ServerAPI.ViewModels;
using Microsoft.AspNetCore.Identity;
using JwtDb.Models;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

// ReSharper disable once CheckNamespace
namespace ServerAPI.Services
{
    public class RequestService : IRequestService
    {
        private readonly AAtims _context;
        private readonly int _personnelId;
        private readonly string _userName;
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IPersonService _iPersonService;
        private readonly IInmateService _inmateService;
        private readonly IFacilityHousingService _facilityHousingService;
		private readonly IAtimsHubService _atimsHubService;

        public RequestService(AAtims context, IHttpContextAccessor httpContextAccessor,
            UserManager<AppUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IPersonService personService,
            IInmateService inmateService,
            IFacilityHousingService facilityHousingService,
			IAtimsHubService atimsHubService)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _userName = httpContextAccessor.HttpContext.User.Claims.First(s => s.Type == ClaimTypes.NameIdentifier).Value;
            _personnelId =
                Convert.ToInt32(httpContextAccessor.HttpContext.User.FindFirst(AuthDetailConstants.PERSONNELID)?.Value);
            _iPersonService = personService;
            _inmateService = inmateService;
            _facilityHousingService = facilityHousingService;
			_atimsHubService = atimsHubService;
        }

		//Save Request and Request_track Details
		public async Task<int> SaveRequestDetail(RequestVm requestDetails)
		{
			//Insert into Request table
			if (!requestDetails.ActionLookupId.HasValue) return await _context.SaveChangesAsync();
			Request req = new Request
			{
				RequestActionLookupId = requestDetails.ActionLookupId.Value,
				InmateId = requestDetails.InmateId,
				RequestNote = requestDetails.Note,
				RequestDate = DateTime.Now,
				RequestedBy = _personnelId,
				CreateDate = DateTime.Now,
				CreatedBy = _personnelId,
				UpdatedBy = _personnelId,
				UpdateDate = DateTime.Now,
				RequestHousingLocation = requestDetails.HousingLocation,
				RequestHousingNumber = requestDetails.HousingNumber,
				HousingUnitListId = requestDetails.ReqHousingUnitListId,

				//Insert into Request_Track table
				RequestTrack = new List<RequestTrack>
				{
					new RequestTrack
					{
						RequestTrackDate = DateTime.Now,
						RequestTrackBy = _personnelId,
						RequestTrackCategory = RequestTrackCategory.NEWREQUEST,
						RequestActionLookupId = requestDetails.ActionLookupId.Value,
						RequestTrackNote = requestDetails.Note
					}
				}
			};
			_context.Request.Add(req);
			var res = await _context.SaveChangesAsync();
			await _atimsHubService.GetRequestCount();
			return res;

            //*******Update
            //update Request set Request_Note ='" & Request_Note & "' where Request_id=" & Request_id
        }

        // To Assign, Undo and Clear Request Details
		public async Task<int> UpdateRequestDetails(RequestOperations requests)
		{
			Request request = _context.Request.Single(i => i.RequestId == requests.RequestId);
			RequestTrack requestTrack = new RequestTrack
			{
				RequestId = requests.RequestId,
				RequestTrackBy = _personnelId,
				RequestTrackDate = DateTime.Now
			};
			switch (requests.RequestStatus)
			{
				case RequestStatus.Accepted:
					request.PendingBy = _personnelId;
					request.PendingDate = DateTime.Now;
					request.ClearedBy = null;
					request.ClearedDate = null;
					requestTrack.RequestTrackCategory = RequestTrackCategory.REQUESTACCEPTED;
					break;
				case RequestStatus.Pending:
					request.PendingBy = null;
					request.PendingDate = null;
					request.ClearedBy = null;
					request.ClearedDate = null;
					requestTrack.RequestTrackCategory = RequestTrackCategory.UNDOREQUESTACCEPTED;
					break;
				case RequestStatus.Cleared:
					request.PendingBy = _personnelId;
                    request.PendingDate = DateTime.Now;
					request.ClearedBy = _personnelId;
					request.ClearedDate = DateTime.Now;
					requestTrack.RequestTrackCategory = RequestTrackCategory.REQUESTCLEARED;
					break;
                case RequestStatus.UndoCleared:
                    requestTrack.RequestTrackCategory = RequestTrackCategory.UNDOREQUESTCLEARED;
                    request.ClearedBy = null;
                    request.ClearedDate = null;
                    break;
			}
			request.UpdateDate = DateTime.Now;
			request.UpdatedBy = _personnelId;
			_context.Request.Update(request);
			_context.RequestTrack.Add(requestTrack);
			var res = await _context.SaveChangesAsync();
			await _atimsHubService.GetRequestCount();
			return res;
		}   

		//Method for Transfer,Note,Disposition,Clear and Response
		public async Task<int> SaveRequestTransfer(RequestTransfer transNote)
		{
			Request request = _context.Request.Single(i => i.RequestId == transNote.RequestId);
			RequestTrack requestTrack = new RequestTrack
			{
				RequestId = transNote.RequestId,
				RequestTrackBy = _personnelId,
				RequestTrackNote = transNote.Note,
				RequestTrackDate = DateTime.Now
			};

			switch (transNote.RequestStatus)
			{
				case RequestDetailsStatus.Transfer:
					if (transNote.ReqActionLookupId.HasValue && transNote.ReqActionLookupId != 0)
					{
						request.RequestActionLookupId = transNote.ReqActionLookupId.Value;
						requestTrack.RequestActionLookupId = transNote.ReqActionLookupId;
					}
					else
					{
						request.PendingBy = transNote.PersonnelId;
						request.PendingDate = DateTime.Now;
					}
					requestTrack.RequestTrackCategory = transNote.ReqTransferchk == 0
						? RequestTrackCategory.TRANSFERPERSONNEL
						: RequestTrackCategory.TRANSFERACTION;
					_context.Request.Update(request);
					break;
				case RequestDetailsStatus.Note:
					requestTrack.RequestTrackCategory = RequestTrackCategory.NOTE;
					requestTrack.ResponseInmateFlag = 1;
					requestTrack.ResponseInmateReadFlag = 0;
					break;
				case RequestDetailsStatus.Response:
					requestTrack.RequestTrackCategory = RequestTrackCategory.INMATERESPONSE;
					requestTrack.ResponseInmateFlag = 0;
					requestTrack.ResponseInmateReadFlag = 1;
					break;
				case RequestDetailsStatus.Dispo:
					RequestActionLookup requestLookUp = _context.RequestActionLookup.SingleOrDefault(
						i => i.RequestActionLookupId == request.RequestActionLookupId);
					
					request.ClearedBy = transNote.ClearFlag ? _personnelId : new int?();
					request.ClearedDate = transNote.ClearFlag ? DateTime.Now : new DateTime?();
					requestTrack.RequestTrackCategory = transNote.ClearFlag ? RequestTrackCategory.REQUESTCLEARED
						: RequestTrackCategory.DISPOSITION;
					request.ResponseDisposition = transNote.DispoCategory;
					requestTrack.ResponseDisposition = transNote.DispoCategory;
					requestTrack.ResponseInmateFlag = requestLookUp != null && requestLookUp.ReturnDispoToInmate ? 1 : 0;
					_context.Request.Update(request);
					break;
			}

			_context.RequestTrack.Add(requestTrack);
			return await _context.SaveChangesAsync();
		}

		public List<RequestVm> GetRequestActionList(RequestDetails request)
        {     
			List<HousingDetail> lstHousingDetail = new List<HousingDetail>();

            if (request.FacilityId > 0 && request.RequestType != RequestTypeEnum.Visit)
            {
                lstHousingDetail = _context.HousingUnit
                    .Where(x => x.FacilityId == request.FacilityId
								&& (!x.HousingUnitInactive.HasValue || x.HousingUnitInactive == 0))
                    .GroupBy(g => new {g.HousingUnitListId, g.HousingUnitLocation, g.HousingUnitNumber})
                    .Select(y => new HousingDetail
                    {
                        HousingUnitLocation = y.Key.HousingUnitLocation,
                        HousingUnitNumber = y.Key.HousingUnitNumber,
                        HousingUnitListId = y.Key.HousingUnitListId
                    }).ToList();
            }

            List<RequestVm> lstRequestVm = _context.RequestActionLookup
                .Where(x => !x.InactiveFlag
                            && (x.PendingAllFacilityFlag || !x.RequestFacilityId.HasValue || request.FacilityId <= 0 || x.RequestFacilityId == request.FacilityId)
                            && (request.RequestType != RequestTypeEnum.Visit || x.ShowInFlag == 4))
                .Select(y => new RequestVm
                {
                    RequestByInmate = y.RequestByInmate,
                    ActionLookupId = y.RequestActionLookupId,
                    FacilityAbbr = y.Facility.FacilityAbbr,
                    Action = y.ActionLookup,
                    ShowInFlag = y.ShowInFlag,
                    LstHousingDetail = y.ShowInFlag == 1 ? lstHousingDetail : new List<HousingDetail>(),
					RequestDepartment = y.RequestDepartment,
					RequestPosition = y.RequestPosition,
					HousingLocation = y.RequestHousingLocation,
					HousingNumber = y.RequestHousingNumber,
					RequestLocation = y.RequestLocation,
					RequestProgram = y.RequestProgram,
					RequestWorkCrew = y.RequestWorkCrew
                }).OrderBy(d => d.Action).ToList();

			switch (request.RequestType)
			{
				case RequestTypeEnum.Program:
					lstRequestVm = lstRequestVm.Where(r => r.RequestProgram).ToList();
					break;
				case RequestTypeEnum.WorkCrew:
					lstRequestVm = lstRequestVm.Where(r => r.RequestWorkCrew).ToList();
					break;
				case RequestTypeEnum.Location:
					lstRequestVm = lstRequestVm.Where(r => request.HousingLocation == r.RequestLocation).ToList();
					break;
				case RequestTypeEnum.Housing:
					lstRequestVm = lstRequestVm.Where(r => request.HousingLocation == r.HousingLocation 
					&& request.HousingNumber == r.HousingNumber).ToList();
					break;
			}

            return lstRequestVm;
        }

		public async Task<ReqResponsibilityVm> GetPenRequestDetails(int facilityId, int[] housingGroup, string fromScreen)
		{
			ReqResponsibilityVm pendingReqDetails = new ReqResponsibilityVm();
			List<string> userRoles = await GetUserRoleIds();
			List<RequestVm> pendingReq = _context.Request.SelectMany(rq =>
			_context.RequestActionUserGroup.Where(ug => 
			 !string.IsNullOrEmpty(ug.RoleId) && ug.DeleteFlag == 0 && ug.RequestActionLookupId == rq.RequestActionLookupId
								 && userRoles.Contains(ug.RoleId) &&
				(facilityId > 0 ? rq.RequestActionLookup.RequestFacilityId == facilityId || !rq.RequestActionLookup.FacilityId.HasValue
                                                                                         || rq.RequestActionLookup.PendingAllFacilityFlag
				: !rq.RequestActionLookup.PendingAllFacilityFlag)
				&& !rq.ClearedBy.HasValue && !rq.PendingBy.HasValue && !rq.RequestActionLookup.InactiveFlag
				&& (fromScreen != "console" || rq.RequestActionLookup.ShowInFlag == 1)),
				(rqs, ug) => new RequestVm {
					RequestId = rqs.RequestId,
					InmateId = rqs.InmateId,
					RequestDate = rqs.RequestDate,
					RequestNote = rqs.RequestNote,
					Disposition = rqs.ResponseDisposition,
					ActionLookupId = rqs.RequestActionLookupId,
					HousingLocation = rqs.RequestHousingLocation,
					HousingNumber = rqs.RequestHousingNumber,
					RequestedById = rqs.RequestedByNavigation.PersonId,
					PersonnelBadgeNumber = rqs.RequestedByNavigation.OfficerBadgeNum,
					Action = rqs.RequestActionLookup.ActionLookup,
					FacilityId = rqs.RequestActionLookup.FacilityId,
					AppSubModuleId = rqs.RequestActionLookup.AppAoSubModuleId
				}).Distinct().ToList();

			int[] inmateIds =
				_context.Inmate.Where(inm => inm.InmateActive == 1).Select(inm => inm.InmateId).ToArray();
			pendingReq = pendingReq.Where(per => !per.InmateId.HasValue || inmateIds.Any(i=> i==per.InmateId)).ToList();
			pendingReqDetails.Responsibilities = new List<FloorNoteTypeCount> {
				new FloorNoteTypeCount{
				Name = "All",
				Count = pendingReq.Count}
			};

			if (pendingReq.Any()) {
				List<int> inmates = pendingReq.Where(r => r.InmateId > 0).Select(pr => pr.InmateId ?? 0).ToList();
				List<InmateDetail> inmateDetails = _context.Inmate.Include(i=>i.Person)
                    .Where(inm => inmates.Contains(inm.InmateId))
					.Select(inm => new InmateDetail {
						InmateId = inm.InmateId,
						Person = new PersonVm {
							PersonFirstName = inm.Person.PersonFirstName,
							PersonLastName = inm.Person.PersonLastName,
						},
						InmateNumber = inm.InmateNumber,
						HousingUnit = new HousingDetail {
							HousingUnitNumber = inm.HousingUnit.HousingUnitNumber,
							HousingUnitLocation = inm.HousingUnit.HousingUnitLocation,
							HousingUnitBedNumber = inm.HousingUnit.HousingUnitBedNumber,
							HousingUnitBedLocation = inm.HousingUnit.HousingUnitBedLocation,
						}
					}).ToList();
				
				List<int> reqByIds = pendingReq.Select(de => de.RequestedById ?? 0).Distinct().ToList();
				List<PersonnelVm> dbPerson = _context.Person.Where(pr => reqByIds.Contains(pr.PersonId))
					.Select(de => new PersonnelVm
					{
						PersonId = de.PersonId,
						PersonLastName = de.PersonLastName,
						PersonFirstName = de.PersonFirstName,
						PersonMiddleName = de.PersonMiddleName
					}).ToList();

				pendingReq.ForEach(de =>
				{
					InmateDetail dbInmate = inmateDetails.SingleOrDefault(i => i.InmateId == de.InmateId);
					de.RequestBy = dbPerson.Single(p => p.PersonId == de.RequestedById);
					if (dbInmate != null) {
						de.InmateHousingDetail = dbInmate.HousingUnit;
						de.PersonFirstName = dbInmate.Person.PersonFirstName;
						de.PersonLastName = dbInmate.Person.PersonLastName;
						de.InmateNumber = dbInmate.InmateNumber;
                    }});

				pendingReqDetails.Responsibilities.AddRange(pendingReq.GroupBy(p => new { p.ActionLookupId, p.Action })
					.Select(r => new FloorNoteTypeCount {
						Id = r.Key.ActionLookupId ?? 0,
						Name = r.Key.Action,
						Count = r.Count()
					}).ToList());
				pendingReqDetails.RequestDetailLst = pendingReq;
			}
			return pendingReqDetails;
		}

		public async Task<ReqResponsibilityVm> GetAssignRequest(int inmateId, int facilityId, string fromScreen)
		{
			ReqResponsibilityVm assignReqDetails = new ReqResponsibilityVm();
			List<string> userRoles = await GetUserRoleIds();
			List<RequestVm> aReqDetails = _context.Request.SelectMany(r =>
			_context.RequestActionUserGroup.Where(ug =>
			 !string.IsNullOrEmpty(ug.RoleId) && ug.DeleteFlag == 0 && ug.RequestActionLookupId == r.RequestActionLookupId
								 && userRoles.Contains(ug.RoleId)
										&& (facilityId == 0 || r.RequestActionLookup.FacilityId == facilityId || !r.RequestActionLookup.FacilityId.HasValue)
										&& !r.ClearedBy.HasValue && r.PendingBy.HasValue
										&& !r.RequestActionLookup.InactiveFlag
										&& (fromScreen != "console" || r.RequestActionLookup.ShowInFlag == 1)
										&& (fromScreen != "status" || r.InmateId > 0)),
			   (rs, ug) => new RequestVm {
				   RequestId = rs.RequestId,
				   ActionLookupId = rs.RequestActionLookupId,
				   RequestDate = rs.RequestDate,
				   RequestedById = rs.RequestedByNavigation.PersonId,
				   RequestNote = rs.RequestNote,
				   Disposition = rs.ResponseDisposition,
				   InmateId = rs.InmateId,				  
				   Action = rs.RequestActionLookup.ActionLookup,
				   PendingDate = rs.PendingDate,
				   PersonnelBadgeNumber = rs.RequestedByNavigation.OfficerBadgeNum,
				   RequestDepartment = rs.RequestActionLookup.RequestDepartment,
				   RequestPosition = rs.RequestActionLookup.RequestPosition,
				   HousingLocation = rs.RequestActionLookup.RequestLocation,
				   HousingNumber = rs.RequestActionLookup.RequestHousingNumber,
				   CreateProgramRequest = rs.RequestActionLookup.RequestProgram ? 1 : 0
			   }).Distinct().ToList();
            
			int[] inmateIdLst =
				_context.Inmate.Where(inm => inm.InmateActive == 1).Select(inm => inm.InmateId).ToArray();

			aReqDetails = aReqDetails.Where(per => !per.InmateId.HasValue || inmateIdLst.Any(i => i == per.InmateId)).ToList();

			assignReqDetails.Responsibilities = new List<FloorNoteTypeCount> {
					new FloorNoteTypeCount{
					Name = "All",
				Count = aReqDetails.Count}
			};

			if (aReqDetails.Any())
			{
				List<int> inmateIds = aReqDetails.Where(r => r.InmateId > 0).Select(pr => pr.InmateId ?? 0).ToList();
				List<InmateDetail> inmateDetails = _context.Inmate.Include(i => i.Person)
                    .Where(inm => inmateIds.Contains(inm.InmateId))
					.Select(inm => new InmateDetail {
						InmateId = inm.InmateId,
						Person = new PersonVm {
							PersonFirstName = inm.Person.PersonFirstName,
							PersonLastName = inm.Person.PersonLastName,
						},
						InmateNumber = inm.InmateNumber,
						HousingUnit = new HousingDetail {
							HousingUnitNumber = inm.HousingUnit.HousingUnitNumber,
							HousingUnitLocation = inm.HousingUnit.HousingUnitLocation,
							HousingUnitBedNumber = inm.HousingUnit.HousingUnitBedNumber
						}}).ToList();
				List<int> requestedById = aReqDetails.Select(im => im.RequestedById ?? 0).Distinct().ToList();
				List<PersonnelVm> dbPerson = _context.Person.Where(pr => requestedById.Contains(pr.PersonId))
						.Select(de => new PersonnelVm {
							PersonId = de.PersonId,
							PersonLastName = de.PersonLastName,
							PersonFirstName = de.PersonFirstName,
							PersonMiddleName = de.PersonMiddleName
						}).ToList();
				aReqDetails.ForEach(de => {
					InmateDetail dbInmate = inmateDetails.SingleOrDefault(i => i.InmateId == de.InmateId);
					de.RequestBy = dbPerson.Single(p => p.PersonId == de.RequestedById);
					if (dbInmate != null) {
						de.InmateHousingDetail = dbInmate.HousingUnit;
						de.PersonFirstName = dbInmate.Person.PersonFirstName;
						de.PersonLastName = dbInmate.Person.PersonLastName;
						de.InmateNumber = dbInmate.InmateNumber;
					}});

				assignReqDetails.Responsibilities.AddRange(aReqDetails.GroupBy(p => new { p.ActionLookupId, p.Action })
				.Select(r => new FloorNoteTypeCount {
					Id = r.Key.ActionLookupId ?? 0,
					Name = r.Key.Action,
					Count = r.Count()
				}).ToList());
				assignReqDetails.AssignedLst = new List<FloorNoteTypeCount>
			{
				new FloorNoteTypeCount
				{
					Name = "All",
					Count = aReqDetails.Count
				}
			};
				assignReqDetails.AssignedLst.AddRange(aReqDetails.GroupBy(p => new { p.InmateId, p.PersonLastName })
				.Select(r => new FloorNoteTypeCount
				{
					Id = r.Key.InmateId ?? 0,
					Name = r.Key.PersonLastName,
					Count = r.Count()
				}).ToList());
				
				assignReqDetails.RequestDetailLst = aReqDetails;
			}
			return assignReqDetails;
		}

		private async Task<List<string>> GetUserRoleIds()
		{
			AppUser userToVerify = await _userManager.FindByNameAsync(_userName);
			IList<string> roles = await _userManager.GetRolesAsync(userToVerify);
			return roles.Select(async s => await _roleManager.FindByNameAsync(s)).Select(t => t.Result.Id).ToList();
		}
		
		//GetRequestCount => key: Pending Request; value: Assigned Request
		public async Task<KeyValuePair<int,int>> GetRequestCount(int facilityId)
		{
			List<string> userRoles = await GetUserRoleIds();
			int[] inmateIdLst = _context.Inmate.Where(i => i.InmateActive == 1).Select(i => i.InmateId).ToArray();

            List<RequestVm> requestDetails = _context.Request.Where(w =>
                    !w.ClearedBy.HasValue && !w.RequestActionLookup.InactiveFlag
                      && (!w.InmateId.HasValue || inmateIdLst.Any(a => a == w.InmateId.Value))
                      && (w.RequestActionLookup.RequestActionUserGroup.Any(r =>
                          r.RequestActionLookupId == w.RequestActionLookupId &&
                          (r.RequestActionLookup.RequestFacilityId == facilityId
                           || !r.RequestActionLookup.RequestFacilityId.HasValue)
                          && userRoles.Any(s => s == r.RoleId))))
                .Select(s => new RequestVm
                {
                    RequestId = s.RequestId,
                    PendingBy = s.PendingBy,
                    RequestedById = s.RequestedBy
                }).Distinct().ToList();
            KeyValuePair<int, int> requestCount = new KeyValuePair<int, int>(requestDetails.Where(r =>
                         !r.PendingBy.HasValue).Distinct().Count(),
                requestDetails.Where(r => r.PendingBy.HasValue).Distinct().Count());
            return requestCount;
		}

		public async Task<ReqResponsibilityVm> GetRequestStatus(int facilityId)
		{
			RequestValues values = new RequestValues
            {
				FacilityId = facilityId,
				Top = 1000
			};
            ReqResponsibilityVm requestStatus = new ReqResponsibilityVm
            {
                RequestDetailLst = await GetRequestSearch(values)
            };

            requestStatus.RequestDetailLst = requestStatus.RequestDetailLst.Where(r => !r.ClearedBy.HasValue || r.ClearedBy == 0).ToList();
			requestStatus.RequestDetailLst.ForEach(de =>
            {
                de.ElapsedStatus = de.Elapsed / 60 >= 36
                    ? ElapsedHours.NONE
                    : de.Elapsed / 60 < 36 && de.Elapsed / 60 >= 24
                        ? ElapsedHours.THIRTYSIXHOURS
                        : de.Elapsed / 60 < 24 && de.Elapsed / 60 >= 12
                            ? ElapsedHours.TWENTYFOURHOURS
                            : ElapsedHours.TWELVEHOURS;
            });

			requestStatus.RequestStatusLst = requestStatus.RequestDetailLst.GroupBy(p => new { p.ActionLookupId, p.Action, p.PendingBy, p.ElapsedStatus })
					.Select(r => new RequestStatusVm
					{
						Assigned = r.Where(de=>de.PendingBy == r.Key.PendingBy 
								&& !de.ClearedBy.HasValue).Select(d=>d.PendingPerson).FirstOrDefault(),
						ActionId=r.Key.ActionLookupId ?? 0,
						ActionName = r.Key.Action,
						Elapsed = r.Key.ElapsedStatus,
						Count = r.Count()
					}).OrderByDescending(o=> o.Elapsed)
                    .ThenBy(t=> t.ActionName).ToList();
            return requestStatus;
		}

		#region RequestSearch

		public async Task<List<RequestVm>> GetRequestSearch(RequestValues values)
        {
            List<RequestVm> requestDetails = _context.Request
                .Where(r => (values.ActionLookupId == 0 || values.ActionLookupId == r.RequestActionLookupId)
                            && (r.RequestActionLookup.FacilityId == values.FacilityId || !r.RequestActionLookup.FacilityId.HasValue)
                            && !r.RequestActionLookup.InactiveFlag
                            && (!values.InmateId.HasValue || r.InmateId == values.InmateId)
                            && (values.RequestStatus == RequestStatus.None
                            || (values.RequestStatus == RequestStatus.Pending
                            ? !r.PendingBy.HasValue && !r.ClearedBy.HasValue
                            : values.RequestStatus == RequestStatus.Assigned
                            ? r.PendingBy > 0 && !r.ClearedBy.HasValue : r.ClearedBy > 0))
                            && (string.IsNullOrEmpty(values.Disposition) ||
                                r.ResponseDisposition == values.Disposition))
                .Select(y => new RequestVm
                {
                    ActionLookupId = y.RequestActionLookupId,
                    RequestDate = y.RequestDate,
                    Action = y.RequestActionLookup.ActionLookup,
                    FacilityId = y.RequestActionLookup.RequestFacilityId,
                    InmateId = y.InmateId,
                    Note = y.RequestNote,
                    RequestId = y.RequestId,
                    ClearedBy = y.ClearedBy,
                    ClearedDate = y.ClearedDate,
                    ClearedPerson = new PersonnelVm
                    {
                        PersonFirstName = y.ClearedByNavigation.PersonNavigation.PersonFirstName,
                        PersonLastName = y.ClearedByNavigation.PersonNavigation.PersonLastName,
                        OfficerBadgeNumber = y.ClearedByNavigation.OfficerBadgeNum
                    },
                    PendingBy = y.PendingBy,
                    PendingDate = y.PendingDate,
                    PendingPerson = new PersonnelVm
                    {
                        PersonnelId = y.PendingBy ?? 0,
                        PersonFirstName = y.PendingByNavigation.PersonNavigation.PersonFirstName,
                        PersonLastName = y.PendingByNavigation.PersonNavigation.PersonLastName,
                        OfficerBadgeNumber = y.PendingByNavigation.OfficerBadgeNum
                    },
                    Disposition = y.ResponseDisposition,
                    RequestedById = y.RequestedBy,
                    RequestBy = new PersonnelVm
                    {
                        PersonFirstName = y.RequestedByNavigation.PersonNavigation.PersonFirstName,
                        PersonLastName = y.RequestedByNavigation.PersonNavigation.PersonLastName,
                        OfficerBadgeNumber = y.RequestedByNavigation.OfficerBadgeNum
                    },
                    RequestDepartment = y.RequestActionLookup.RequestDepartment,
                    RequestPosition = y.RequestActionLookup.RequestPosition,
                    UpdatedBy = y.UpdatedBy,
                    Elapsed = DateTimeCalc(y.UpdateDate)
                }).ToList();

                switch (values.RequestPersonnel) {
                    case RequestStatus.None:
                        requestDetails = requestDetails.Where(w => (!values.FromDate.HasValue || !values.ToDate.HasValue 
                                || (w.ClearedDate.HasValue ? (values.FromDate <= w.ClearedDate.Value.Date && w.ClearedDate.Value.Date <= values.ToDate)
                                    : values.FromDate <= DateTime.Now.Date) 
								&& w.RequestDate.HasValue && (values.FromDate <= w.RequestDate.Value.Date && w.RequestDate.Value.Date <= values.ToDate))
                                && (!values.RequestedBy.HasValue || w.PendingBy == values.RequestedBy || w.RequestedById == values.RequestedBy 
                                || w.ClearedBy == values.RequestedBy || w.UpdatedBy == values.RequestedBy))
                            .ToList();
                        break;
                    case RequestStatus.Assigned:
                        requestDetails = requestDetails.Where(w => (!values.FromDate.HasValue || !values.ToDate.HasValue 
                            || w.PendingDate.HasValue && values.FromDate <= w.PendingDate.Value.Date 
                            && w.PendingDate.Value.Date <= values.ToDate) 
                            && (!values.RequestedBy.HasValue || w.PendingBy == values.RequestedBy)
                            && w.PendingBy.HasValue).ToList();
                        break;
                    case RequestStatus.Cleared:
                        requestDetails = requestDetails.Where(w => (!values.FromDate.HasValue || !values.ToDate.HasValue 
                            || w.ClearedDate.HasValue && values.FromDate <= w.ClearedDate.Value.Date 
                                                      && w.ClearedDate.Value.Date <= values.ToDate)
                            && (!values.RequestedBy.HasValue || w.ClearedBy == values.RequestedBy) 
                            && w.ClearedBy.HasValue).ToList();
                        break;
                    case RequestStatus.Requested:
                        requestDetails = requestDetails.Where(w => (!values.FromDate.HasValue || !values.ToDate.HasValue
                            || w.RequestDate.HasValue && values.FromDate <= w.RequestDate.Value.Date && w.RequestDate.Value.Date <= values.ToDate)
                            && (!values.RequestedBy.HasValue || w.RequestedById == values.RequestedBy)
                            && w.RequestedById.HasValue).ToList();
                        break; }
                if (requestDetails.Count > 0) {
                    List<int> inmateIds = requestDetails.Where(s => s.InmateId.HasValue)
                        .Select(s => s.InmateId ?? 0).ToList();
                    IQueryable<Inmate> inmateLst = _context.Inmate.Include(de => de.Person)
                        .Include(h => h.HousingUnit).Include(f => f.Facility)
                        .Where(w => inmateIds.Contains(w.InmateId));
                    List<RequestOperations> requestActionLst = await RequestAction(values.FacilityId);
                    requestDetails.ForEach(item => {
                        item.IsView = requestActionLst.Count(s => s.RequestId == item.ActionLookupId) == 0;
                        if (item.InmateId.HasValue && item.InmateId > 0) {
                            Inmate inmate = inmateLst.Single(s => s.InmateId == item.InmateId);
                            item.PersonDetails = new PersonVm {
                                InmateNumber = inmate.InmateNumber,
                                PersonId = inmate.PersonId,
                                PersonFirstName = inmate.Person.PersonFirstName,
                                PersonLastName = inmate.Person.PersonLastName
                            };
                            item.InmateHousingDetail = new HousingDetail {
                                HousingUnitLocation = inmate.HousingUnit?.HousingUnitLocation,
                                HousingUnitNumber = inmate.HousingUnit?.HousingUnitNumber,
                                HousingUnitBedNumber = inmate.HousingUnit?.HousingUnitBedNumber,
                                FacilityAbbr = inmate.Facility.FacilityAbbr
                            };
                        }
                    });
                }
                return requestDetails.OrderByDescending(o => o.RequestDate).Take(values.Top).ToList();
        }
        public async Task<List<RequestOperations>> RequestAction(int facilityId)
        {
            List<string> userRoles = await GetUserRoleIds();
            List<RequestOperations> lstActionLookups = _context.Request
                .SelectMany(w => _context.RequestActionUserGroup
                        .Where(u => !string.IsNullOrEmpty(u.RoleId) && userRoles.Contains(u.RoleId)
                                    && !w.ClearedBy.HasValue && !w.PendingBy.HasValue
                                    && u.RequestActionLookupId == w.RequestActionLookupId
                                    && u.DeleteFlag == 0
                                    && (facilityId <= 0 || !w.RequestActionLookup.RequestFacilityId.HasValue 
                                                        || w.RequestActionLookup.RequestFacilityId == facilityId)
                                    && !w.RequestActionLookup.InactiveFlag ),
                    (rs, ug) => new RequestOperations {
                        RequestId = rs.RequestActionLookup.RequestActionLookupId,
                        ActionLookup = rs.RequestActionLookup.ActionLookup
                    }).Distinct().ToList();

            return lstActionLookups;
        }
           
        #endregion

        #region Booking Request

        // Get all pending request in a facility
        public List<RequestTypes> GetBookingPendingReq(int facilityId, int showInFlag)
        {
            IQueryable<Incarceration> lstIncarcerations =
                _context.Incarceration.Where(inc => !inc.ReleaseOut.HasValue && inc.InmateId.HasValue);
            IEnumerable<RequestDetails> lstRequest = _context.Request.Where(req =>
                !req.PendingBy.HasValue && !req.ClearedBy.HasValue &&
                !req.RequestActionLookup.InactiveFlag &&
                req.RequestActionLookup.ShowInFlag == showInFlag &&
                (req.RequestActionLookup.FacilityId == facilityId ||
                 req.RequestActionLookup.PendingAllFacilityFlag)).Select(req =>
                new RequestDetails {
                    InmateId = req.InmateId.Value,
                    ActionLookupId = req.RequestActionLookupId,
                    Action = req.RequestActionLookup.ActionLookup,
                    RequestDate = req.RequestDate.Value,
                    CreateDate = req.CreateDate.Value
                }).ToList();

            List<RequestTypes> requestTypes = (from inc in lstIncarcerations
                from req in lstRequest
                where (showInFlag == 3 ? req.RequestDate > inc.DateIn : req.CreateDate > inc.DateIn)
                      && req.InmateId == inc.InmateId && req.ActionLookupId.HasValue
                select new {
                    req.ActionLookupId,
                    req.Action
                }).GroupBy(ins => new
            {
                ins.ActionLookupId,
                ins.Action
            }).Select(inx => new RequestTypes
            {
                RequestCount = inx.Count(),
                RequestLookupId = inx.Key.ActionLookupId.Value,
                RequestLookupName = inx.Key.Action
            }).ToList();

            return requestTypes;
        }

        // Get all assigned request
        public List<RequestTypes> GetBookingAssignedReq(int showInFlag)
        {
            List<RequestTypes> requestTypes =
				_context.Request.Where(req =>
				!req.RequestActionLookup.InactiveFlag &&
				req.RequestActionLookup.ShowInFlag == showInFlag &&
				(showInFlag == 3 ? req.PendingBy == _personnelId : req.PendingBy.HasValue)
				&& !req.ClearedBy.HasValue &&
				req.PendingBy == _personnelId).Select(req => new
				{
					req.RequestActionLookupId,
					req.RequestActionLookup.ActionLookup
				}).GroupBy(ins => new
			 {
                ins.RequestActionLookupId,
                ins.ActionLookup
            }).ToList().Select(lpr => new RequestTypes
            {
                RequestCount = lpr.ToList().Count,
                RequestLookupId = lpr.Key.RequestActionLookupId,
                RequestLookupName = lpr.Key.ActionLookup
            }).ToList();

            return requestTypes;
        }

        // Get pending request details
        public List<RequestVm> GetBookingPendingReqDetail(int facilityId, int requestLookupId, int showInFlag)
        {
            IQueryable<Incarceration> lstIncarcerations =
                _context.Incarceration.Where(inc => !inc.ReleaseOut.HasValue && inc.InmateId.HasValue);

            IQueryable<RequestVm> lstRequest = _context.Request.Where(req =>
                    !req.PendingBy.HasValue && !req.ClearedBy.HasValue &&
                    !req.RequestActionLookup.InactiveFlag &&
                    req.RequestActionLookup.ShowInFlag == showInFlag &&
                    (req.RequestActionLookup.FacilityId == facilityId ||
                     req.RequestActionLookup.PendingAllFacilityFlag)
                    && req.RequestActionLookupId == requestLookupId)
                .Select(req => new RequestVm
                {
                    InmateId = req.InmateId.Value,
                    RequestId = req.RequestId,
                    ActionLookupId = req.RequestActionLookupId,
                    Action = req.RequestActionLookup.ActionLookup,
                    AppSubModuleId = req.RequestActionLookup.AppAoSubModuleId,
                    FacilityId = req.RequestActionLookup.FacilityId,
                    RequestNote = req.RequestNote,
                    RequestDisposition = req.ResponseDisposition,
                    RequestDate = req.RequestDate,
                    RequestedById = req.RequestedBy,
                    CreateDate = req.CreateDate
                });

            List<RequestVm> lstPendingRequests = lstIncarcerations.SelectMany(inc =>
                lstRequest.Where(req => req.CreateDate > inc.DateIn && req.InmateId == inc.InmateId)).ToList();

            List<int> officerIds =
                lstPendingRequests.Where(i => i.RequestedById.HasValue).Select(i => i.RequestedById.Value).ToList();

            List<PersonnelVm> requestedOfficer =
                _iPersonService.GetPersonNameList(officerIds.ToList());

            List<int> inmateIds =
                lstPendingRequests.Where(i => i.InmateId.HasValue).Select(i => i.InmateId.Value).ToList();

            List<PersonVm> lstPersonDetails = _inmateService.GetInmateDetails(inmateIds);

            List<int> housingDetailIds =
                lstPersonDetails.Where(i => i.HousingUnitId.HasValue).Select(i => i.HousingUnitId.Value).ToList();

            List<HousingDetail> lstHousingDetail = _facilityHousingService.GetHousingList(housingDetailIds);

            lstPendingRequests.ForEach(lpr =>
            {
                if (lpr.RequestedById.HasValue)
                {
                    lpr.RequestBy = requestedOfficer.Single(ao => ao.PersonnelId == lpr.RequestedById.Value);
                }

                if (!lpr.InmateId.HasValue) return;
                lpr.PersonDetails = lstPersonDetails.Single(lpd => lpd.InmateId == lpr.InmateId.Value);

                if (lpr.PersonDetails.HousingUnitId.HasValue)
                {
                    lpr.InmateHousingDetail = lstHousingDetail
                        .SingleOrDefault(lpd => lpd.HousingUnitId == lpr.PersonDetails.HousingUnitId.Value);
                }
            });

            return lstPendingRequests;
        }

        // Get assigned request details
        public List<RequestVm> GetBookingAssignedReqDetail(int requestLookupId, int showInFlag)
        {
            List<RequestVm> lstAssignedRequests = _context.Request.Where(req =>
                    !req.RequestActionLookup.InactiveFlag &&
                    req.RequestActionLookup.ShowInFlag == showInFlag &&
                    req.PendingBy.HasValue && !req.ClearedBy.HasValue &&
                    req.RequestActionLookupId == requestLookupId && req.PendingBy == _personnelId)
                .Select(req => new RequestVm
                {
                    InmateId = req.InmateId.Value,
                    RequestId = req.RequestId,
                    ActionLookupId = req.RequestActionLookupId,
                    Action = req.RequestActionLookup.ActionLookup,
                    AppSubModuleId = req.RequestActionLookup.AppAoSubModuleId,
                    FacilityId = req.RequestActionLookup.FacilityId,
                    FacilityAbbr = req.RequestActionLookup.Facility.FacilityAbbr,
                    RequestDepartment = req.RequestActionLookup.RequestDepartment,
                    RequestPosition = req.RequestActionLookup.RequestPosition,
                    RequestNote = req.RequestNote,
                    RequestDisposition = req.ResponseDisposition,
                    RequestDate = req.RequestDate,
                    RequestedById = req.RequestedBy,
                    CreateDate = req.CreateDate,
                    PendingDate = req.PendingDate
                }).ToList();


            List<int> officerIds =
                lstAssignedRequests.Where(i => i.RequestedById.HasValue).Select(i => i.RequestedById.Value).ToList();

            List<PersonnelVm> requestedOfficer = _iPersonService.GetPersonNameList(officerIds.ToList());

            List<int> inmateIds =
                lstAssignedRequests.Where(i => i.InmateId.HasValue).Select(i => i.InmateId.Value).ToList();

            List<PersonVm> lstPersonDetails = _inmateService.GetInmateDetails(inmateIds);

            List<int> housingDetailIds =
                lstPersonDetails.Where(i => i.HousingUnitId.HasValue).Select(i => i.HousingUnitId.Value).ToList();

            List<HousingDetail> lstHousingDetail = _facilityHousingService.GetHousingList(housingDetailIds);

            lstAssignedRequests.ForEach(lpr =>
            {
                if (lpr.RequestedById.HasValue)
                {
                    lpr.RequestBy = requestedOfficer.Single(ao => ao.PersonnelId == lpr.RequestedById.Value);
                }


                if (!lpr.InmateId.HasValue) return;
                lpr.PersonDetails = _iPersonService.GetInmateDetails(lpr.InmateId.Value);

                if (lpr.PersonDetails.HousingUnitId.HasValue)
                {
                    lpr.InmateHousingDetail = lstHousingDetail
                        .SingleOrDefault(lpd => lpd.HousingUnitId == lpr.PersonDetails.HousingUnitId.Value);
                }
            });

            return lstAssignedRequests.ToList();
        }

		#endregion

		#region Inmate Request
		//Inmate Request summary
        public IEnumerable<InmateRequestVm> GetInmateRequest(int inmateId, int? requestId, int? actionId)
        {
            List<InmateRequestVm> inmateRequest = _context.Request
                .Where(x => x.InmateId == inmateId
                            && (!requestId.HasValue || requestId.Value > 0 &&
                                x.RequestId == requestId)
                            && (!actionId.HasValue || actionId.Value > 0 &&
                                x.RequestActionLookup.RequestActionLookupId == actionId)
                            && x.RequestActionLookup.RequestByInmate
                            && !x.RequestActionLookup.InactiveFlag)
                .Select(r => new InmateRequestVm {
                    RequestId = r.RequestId,
                    RequestActionLookupId = r.RequestActionLookupId,
                    Date = r.RequestDate,
                    Note = r.RequestNote,
                    AssignedDate = r.CreateDate,
                    ClearedDate = r.ClearedDate,
                    Action = r.RequestActionLookup.ActionLookup,
                    Facility = r.RequestActionLookup.Facility.FacilityAbbr
                }).ToList();
            return inmateRequest;
        }

        //Get Request Tracking details based on Request Id
		public RequestTrackVm GetRequestDetailsById(int requestId)
		{
			RequestVm requestDetails = _context.Request
				.Where(x => x.RequestId == requestId)
				.Select(y => new RequestVm {
					ActionLookupId = y.RequestActionLookupId,
					RequestDate = y.RequestDate,
					Elapsed = DateTimeCalc(y.RequestDate),
					Action = y.RequestActionLookup.ActionLookup,
					InmateId = y.InmateId,
					Note = y.RequestNote,
					RequestId = y.RequestId,
					ClearedBy = y.ClearedBy,
					ClearedDate = y.ClearedDate,
					ClearedPerson = new PersonnelVm {
						PersonFirstName = y.ClearedByNavigation.PersonNavigation.PersonFirstName,
						PersonLastName = y.ClearedByNavigation.PersonNavigation.PersonLastName,
						OfficerBadgeNumber = y.ClearedByNavigation.OfficerBadgeNum
					},
					PendingBy = y.PendingBy,
					PendingDate = y.PendingDate,
					PendingPerson = new PersonnelVm {
						PersonFirstName = y.PendingByNavigation.PersonNavigation.PersonFirstName,
						PersonLastName = y.PendingByNavigation.PersonNavigation.PersonLastName,
						OfficerBadgeNumber = y.PendingByNavigation.OfficerBadgeNum
					},
					Disposition = y.ResponseDisposition,
					AllowActionTransfer = y.RequestActionLookup.AllowActionTransfer ? 1 : 0,
					RequestBy = new PersonnelVm {
						PersonFirstName = y.RequestedByNavigation.PersonNavigation.PersonFirstName,
						PersonLastName = y.RequestedByNavigation.PersonNavigation.PersonLastName,
						OfficerBadgeNumber = y.RequestedByNavigation.OfficerBadgeNum
					},
					RequestByInmate = y.RequestActionLookup.RequestByInmate,
					ReturnDispoToInmate = y.RequestActionLookup.ReturnDispoToInmate,					
					NewActionRequest = y.RequestActionLookup.ActionResponseNewRequest ? 1 : 0,
					CreateAppointment = y.RequestActionLookup.ActionResponseCreateAppointment ? 1 : 0,
					CreateProgramRequest = y.RequestActionLookup.ActionResponseCreateProgramRequest ? 1 : 0,
					SendInternalEmail = y.RequestActionLookup.ActionResponseSendInternalEmail ? 1 : 0,
					SendEmail = y.RequestActionLookup.ActionResponseSendEmail ? 1 : 0,
					CopyNote = y.RequestActionLookup.ActionResponseCopyNote,
					OpenInmateFile = y.RequestActionLookup.ActionResponseOpenInmateFile ? 1 : 0
				}).SingleOrDefault();

			if (requestDetails != null)
			{
				Inmate inmate = _context.Inmate.SingleOrDefault(s => requestDetails.InmateId > 0 && s.InmateId == requestDetails.InmateId);
				if (inmate != null) {
					requestDetails.InmateNumber = inmate.InmateNumber;
					requestDetails.PersonId = inmate.PersonId;
					requestDetails.InmateHousingDetail = new HousingDetail {
						HousingUnitId = inmate.HousingUnitId ?? 0
					};

					HousingUnit housingUnit =
						_context.HousingUnit.SingleOrDefault(s => s.HousingUnitId == requestDetails.InmateHousingDetail.HousingUnitId);
					if (housingUnit != null) {
						requestDetails.InmateHousingDetail.HousingUnitLocation = housingUnit.HousingUnitLocation;
						requestDetails.InmateHousingDetail.HousingUnitNumber = housingUnit.HousingUnitNumber;
						requestDetails.InmateHousingDetail.HousingUnitBedNumber = housingUnit.HousingUnitBedNumber;
					}
				}
				Person person = _context.Person.SingleOrDefault(s => requestDetails.PersonId > 0 && s.PersonId == requestDetails.PersonId);
				if (person != null) {
					requestDetails.PersonLastName = person.PersonLastName;
					requestDetails.PersonFirstName = person.PersonFirstName;
				}

				requestDetails.IsRequestForm = _context.FormTemplates.Any(f =>
				f.RequestActionLookupId == requestDetails.ActionLookupId && f.Inactive != 1);
			}

			List<RequestVm> lstRequestTracking = _context.RequestTrack
				.Where(y => y.RequestId == requestId)
				.Select(y => new RequestVm {
					RequestId = y.RequestTrackId,
					RequestDate = y.RequestTrackDate,
					ActionLookupId = y.RequestActionLookupId,
					TrackBy = y.RequestTrackBy,
					RequestCategory = y.RequestTrackCategory,
					Disposition = y.ResponseDisposition,
					Note = y.RequestTrackNote,
					ReqResInmateFlag = y.ResponseInmateFlag,
					ReadFlag = y.ResponseInmateReadFlag,
					PersonFirstName = y.RequestTrackByNavigation.PersonNavigation.PersonFirstName,
					PersonLastName = y.RequestTrackByNavigation.PersonNavigation.PersonLastName,
					PersonnelNumber = y.RequestTrackByNavigation.PersonnelNumber,
					PersonnelBadgeNumber = y.RequestTrackByNavigation.OfficerBadgeNum,
					Action = _context.RequestActionLookup.Where(x => x.RequestActionLookupId == y.RequestActionLookupId)
							.Select(z => z.ActionLookup).SingleOrDefault()
				}).ToList();
			RequestTrackVm requestTrackVm = new RequestTrackVm {
				RequestDetails = requestDetails,
				LstRequestDetails = lstRequestTracking
			};

			return requestTrackVm;
		}

		//Request Count based on Action Type
        public InmateRequestCount GetInmateRequestCount(int inmateId, int? requestId)
        {
            InmateRequestCount inmateRequestCount = new InmateRequestCount
            {
                AllType = _context.Request.Count(x => x.InmateId == inmateId && x.RequestActionLookup.RequestByInmate
                          && !x.RequestActionLookup.InactiveFlag),
                ListInmateRequestType = _context.Request.Where(x => x.InmateId == inmateId
                                         && (!requestId.HasValue || requestId.Value > 0 && x.RequestId == requestId)
                                         && x.RequestActionLookup.RequestByInmate 
                                         && !x.RequestActionLookup.InactiveFlag)
                    .GroupBy(y => new {y.RequestActionLookup.ActionLookup, y.RequestActionLookup.RequestActionLookupId})
                    .Select(z => new FloorNoteTypeCount {
                        Id = z.Key.RequestActionLookupId,
                        Name = z.Key.ActionLookup,
                        Count = z.Count()
                    }).ToList()
            };
            return inmateRequestCount;
        }

        //TODO ???
        private static long DateTimeCalc(DateTime? requestDate) =>
			requestDate.HasValue ? (int)((DateTime.Now - requestDate.Value).TotalMinutes) : 0;
		
		#endregion
	}
}
