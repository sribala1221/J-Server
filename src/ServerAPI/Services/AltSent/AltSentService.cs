using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GenerateTables.Models;
using ServerAPI.ViewModels;
using ServerAPI.Utilities;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Microsoft.EntityFrameworkCore;

// ReSharper disable once CheckNamespace
namespace ServerAPI.Services
{
    public class AltSentService : IAltSentService
    {
        private readonly AAtims _context;
        private readonly ICommonService _commonService;
        private readonly IPersonService _personService;
        private readonly int _personnelId;
        private readonly IPhotosService _photos;


        public AltSentService(AAtims context, ICommonService commonService, IHttpContextAccessor httpContextAccessor,
            IPhotosService photosService, IPersonService personService)
        {
            _context = context;
            _commonService = commonService;
            _personnelId = Convert.ToInt32(httpContextAccessor.HttpContext.User.FindFirst("personnelId")?.Value);
            _personService = personService;
            _photos = photosService;
        }

        /// <summary>
        /// Get alt sentence request grid
        /// </summary>
        /// <param name="facilityId"></param>
        /// <param name="scheduleDate"></param>
        /// <returns></returns>
        public AltSentRequestDetails LoadAltSentRequestDetails(int facilityId, DateTime? scheduleDate)
        {
            // Get request grid based on facility Id and schedule date (optional)
            List<AltSentenceRequest> requestDetails = LoadActiveRequest(facilityId, scheduleDate);
            AltSentRequestDetails details = new AltSentRequestDetails
            {
                RequestGrid = requestDetails
            };
            if (!scheduleDate.HasValue)
            {
                // Get active request grid
                details.StatusGrid = GetAltSentStatus(requestDetails);
                details.ElapseGrid = GetAltSentElapsed(requestDetails);
                details.ProgramGrid = GetAltSentencePrograms(requestDetails, facilityId);
            }
            else
            {
                // Get scheduled request grid
                details.ScheduledGrid = requestDetails.GroupBy(a => a.ScheduleDate)
                    .Select(a => new KeyValuePair<string, int>(a.Key?.TimeOfDay.ToString(), a.Count())).ToList();
            }

            return details;
        }

        /// <summary>
        /// Get facility list
        /// </summary>
        /// <returns></returns>
        public IEnumerable<KeyValuePair<int, string>> GetFacilityList()
        {
            return _commonService.GetFacilities().Where(a => a.AltSentFlag == 1)
                .OrderBy(a => a.FacilityAbbr)
                .Select(a => new KeyValuePair<int, string>(a.FacilityId, a.FacilityAbbr));
        }

        /// <summary>
        /// get alt sent program list
        /// </summary>
        /// <param name="facilityId"></param>
        /// <returns></returns>
        public IEnumerable<KeyValuePair<int, string>> GetAltSentProgramList(int facilityId)
        {
            return _context.AltSentProgram.Where(a =>
                    a.FacilityId == facilityId && (!a.InactiveFlag.HasValue || a.InactiveFlag == 0)
                                               && (!a.DoNotAllowRequest.HasValue || a.DoNotAllowRequest == 0))
                .Select(a => new KeyValuePair<int, string>(a.AltSentProgramId, a.AltSentProgramAbbr));
        }

        /// <summary>
        /// Get alt sentence request status grid
        /// </summary>
        /// <param name="requestList"></param>
        /// <returns></returns>
        private List<KeyValuePair<AltSentStatus, int>> GetAltSentStatus(List<AltSentenceRequest> requestList)
        {
            List<KeyValuePair<AltSentStatus, int>> altSentStatus = new List<KeyValuePair<AltSentStatus, int>>
            {
                // Get all status count
                new KeyValuePair<AltSentStatus, int>(AltSentStatus.All, requestList.Count),
                // Get pending status count
                new KeyValuePair<AltSentStatus, int>(AltSentStatus.Pending, requestList.Count(a => !a.ApproveFlag)),
                // Get approved status count
                new KeyValuePair<AltSentStatus, int>(AltSentStatus.Approved,
                    requestList.Count(a => a.ApproveFlag && !a.ScheduleDate.HasValue)),
                // Get scheduled status count
                new KeyValuePair<AltSentStatus, int>(AltSentStatus.Scheduled,
                    requestList.Count(a => a.ApproveFlag && a.ScheduleDate.HasValue))
            };
            return altSentStatus;
        }

        /// <summary>
        /// Get alt sentence request program grid
        /// </summary>
        /// <param name="requestList"></param>
        /// <param name="facilityId"></param>
        /// <returns></returns>
        private List<AltSentProgramGrid> GetAltSentencePrograms(List<AltSentenceRequest> requestList, int facilityId)
        {
            // Get all programs from request list
            IEnumerable<KeyValuePair<int, string>> allPrograms = requestList.SelectMany(a => a.Programs).ToList();
            // Get each program count from all programs
            List<AltSentProgramGrid> programGrid = allPrograms.GroupBy(a => new {a.Key, a.Value}).Select(a =>
                new AltSentProgramGrid
                {
                    AltSentProgramAbbr = a.Key.Value,
                    AltSentProgramId = a.Key.Key,
                    Cnt = a.Count()
                }).ToList();
            // Get remaining program count from alt sentence program table
            programGrid.AddRange(
                _context.AltSentProgram.Where(a =>
                        !programGrid.Select(p => p.AltSentProgramId).Contains(a.AltSentProgramId)
                        && a.FacilityId == facilityId)
                    .Select(a => new AltSentProgramGrid
                    {
                        AltSentProgramId = a.AltSentProgramId,
                        AltSentProgramAbbr = a.AltSentProgramAbbr
                    })
            );
            return programGrid.OrderBy(a => a.AltSentProgramId).ToList();
        }

        /// <summary>
        /// Get active alt sentence request list according to facility
        /// </summary>
        /// <param name="facilityId"></param>
        /// <param name="scheduleDate"></param>
        /// <returns></returns>
        private List<AltSentenceRequest> LoadActiveRequest(int facilityId, DateTime? scheduleDate)
        {
            List<AltSentenceRequest> requests = _context.AltSentRequest.Where(a =>
                    a.FacilityId == facilityId && !a.AltSentId.HasValue
                                               && !a.RejectFlag.HasValue && !a.DeleteFlag.HasValue
                                               && a.Inmate.InmateActive == 1 && (!scheduleDate.HasValue ||
                                                                                 a.SchedReqBookDate.HasValue &&
                                                                                 a.SchedReqBookDate.Value.Date ==
                                                                                 scheduleDate))
                .OrderByDescending(a => a.SchedReqBookDate)
                .ThenBy(a => a.ApproveDate).ThenBy(a => a.RequestDate)
                .Select(request => new AltSentenceRequest
                {
                    AltSentRequestId = request.AltSentRequestId,
                    PersonnelId = request.RequestPersonnelId,
                    ScheduleDate = request.SchedReqBookDate,
                    ScheduleNote = request.SchedReqBookNotes,
                    RequestDate = request.RequestDate,
                    ApprovedDate = request.ApproveDate,
                    InmateId = request.InmateId,
                    RequestNote = request.RequestNote,
                    DeleteFlag = request.DeleteFlag > 0,
                    ApproveFlag = request.ApproveFlag > 0,
                    RejectFlag = request.RejectFlag > 0,
                    InmateInfo = new PersonVm
                    {
                        InmateId = request.InmateId,
                        PersonId = request.Inmate.PersonId,
                        PersonFirstName = request.Inmate.Person.PersonFirstName,
                        PersonMiddleName = request.Inmate.Person.PersonMiddleName,
                        PersonLastName = request.Inmate.Person.PersonLastName,
                        InmateNumber = request.Inmate.InmateNumber
                    },
                    HousingDetail = new HousingDetail
                    {
                        HousingUnitLocation = request.Inmate.HousingUnitId > 0
                            ? request.Inmate.HousingUnit.HousingUnitLocation
                            : default,
                        HousingUnitNumber = request.Inmate.HousingUnitId > 0
                            ? request.Inmate.HousingUnit.HousingUnitNumber
                            : default,
                        HousingUnitBedLocation = request.Inmate.HousingUnitId > 0
                            ? request.Inmate.HousingUnit.HousingUnitBedLocation
                            : default,
                        HousingUnitBedNumber = request.Inmate.HousingUnitId > 0
                            ? request.Inmate.HousingUnit.HousingUnitBedNumber
                            : default,
                        FacilityAbbr = request.Inmate.HousingUnitId > 0
                            ? request.Inmate.HousingUnit.Facility.FacilityAbbr
                            : default
                    },
                    OfficerLastName = request.RequestPersonnel.PersonNavigation.PersonLastName,
                    OfficerFirstName = request.RequestPersonnel.PersonNavigation.PersonFirstName,
                    OfficerMiddleName = request.RequestPersonnel.PersonNavigation.PersonMiddleName,
                    OfficerBadgeNumber = request.RequestPersonnel.OfficerBadgeNum,
                    PhotoFilePath = _photos.GetPhotoByPerson(request.Inmate.Person)
                }).ToList();
            List<KeyValuePair<int, int>> requestProgramId = _context.AltSentRequestProgram
                .Where(a => requests.Select(x => x.AltSentRequestId).Contains(a.AltSentRequestId))
                .Select(x => new KeyValuePair<int, int>(x.AltSentRequestId, x.AltSentProgramId)).ToList();
            List<KeyValuePair<int, string>> requestPrograms = _context.AltSentProgram
                .Where(a => requestProgramId.Select(x => x.Value).Contains(a.AltSentProgramId))
                .Select(a => new KeyValuePair<int, string>(a.AltSentProgramId, a.AltSentProgramAbbr)).ToList();
            requests.ForEach(a =>
            {
                a.Programs = requestPrograms.Where(x => requestProgramId.Where(y => y.Key == a.AltSentRequestId)
                        .Select(y => y.Value).Contains(x.Key))
                    .Select(x => new KeyValuePair<int, string>(x.Key, x.Value)).ToList();
            });
            return requests;
        }

        /// <summary>
        /// Get alt sentence elapsed grid
        /// </summary>
        /// <param name="requestList"></param>
        /// <returns></returns>
        private List<KeyValuePair<AltSentElapsed, int>> GetAltSentElapsed(List<AltSentenceRequest> requestList)
        {
            List<KeyValuePair<AltSentElapsed, int>> elapsed = new List<KeyValuePair<AltSentElapsed, int>>
            {
                // Get all count
                new KeyValuePair<AltSentElapsed, int>(AltSentElapsed.All, requestList.Count),
                // 01 - 24 Hours count
                new KeyValuePair<AltSentElapsed, int>(AltSentElapsed.Hours0124,
                    requestList.Count(a => a.RequestDate != null &&
                                           (Math.Abs(DateTime.Now.Subtract(a.RequestDate.Value).TotalDays) > 0
                                            && Math.Abs(DateTime.Now.Subtract((DateTime) a.RequestDate).TotalDays) <
                                            1))),
                // 25 - 48 Hours count
                new KeyValuePair<AltSentElapsed, int>(AltSentElapsed.Hours2548, requestList.Count(a =>
                    a.RequestDate != null && (Math.Abs(DateTime.Now.Subtract((DateTime) a.RequestDate).TotalDays) > 1
                                              && Math.Abs(DateTime.Now.Subtract((DateTime) a.RequestDate).TotalDays) <
                                              2))),
                // Get 49 - 72 Hours count
                new KeyValuePair<AltSentElapsed, int>(AltSentElapsed.Hours4972, requestList.Count(a =>
                    a.RequestDate != null && (Math.Abs(DateTime.Now.Subtract((DateTime) a.RequestDate).TotalDays) > 2
                                              && Math.Abs(DateTime.Now.Subtract((DateTime) a.RequestDate).TotalDays) <
                                              3))),
                // Get 73 + Hours count
                new KeyValuePair<AltSentElapsed, int>(AltSentElapsed.Hours73,
                    requestList.Count(a =>
                        a.RequestDate != null &&
                        Math.Abs(DateTime.Now.Subtract((DateTime) a.RequestDate).TotalDays) > 3))
            };
            return elapsed;
        }

        /// <summary>
        /// Insert or update alt sentence request details
        /// </summary>
        /// <param name="altSentenceRequest"></param>
        /// <param name="altSentRequestId"></param>
        /// <returns></returns>
        public async Task<int> InsertUpdateAltSentRequest(AltSentenceRequest altSentenceRequest,
            int altSentRequestId = 0)
        {
            AltSentRequest altSentRequest;
            if (altSentRequestId > 0)
            {
                // Update alt sentence request
                altSentRequest = _context.AltSentRequest
                    .Single(a => a.AltSentRequestId == altSentRequestId);
                altSentRequest.InmateId = altSentenceRequest.InmateId;
                altSentRequest.FacilityId = altSentenceRequest.FacilityId;
                altSentRequest.RequestNote = altSentenceRequest.RequestNote;
                altSentRequest.RequestDate = altSentenceRequest.RequestDate;
                altSentRequest.RequestPersonnelId = altSentenceRequest.PersonnelId;
                altSentRequest.UpdateBy = _personnelId;
                altSentRequest.UpdateDate = DateTime.Now;
                // Delete alt sentence request program according to alt sentence request
                _context.AltSentRequestProgram.RemoveRange(
                    _context.AltSentRequestProgram.Where(a => a.AltSentRequestId == altSentRequest.AltSentRequestId));
            }
            else
            {
                // Insert alt sentence request
                altSentRequest = new AltSentRequest
                {
                    InmateId = altSentenceRequest.InmateId,
                    FacilityId = altSentenceRequest.FacilityId,
                    RequestNote = altSentenceRequest.RequestNote,
                    RequestDate = altSentenceRequest.RequestDate,
                    RequestPersonnelId = altSentenceRequest.PersonnelId,
                    CreateBy = _personnelId,
                    CreateDate = DateTime.Now
                };
                _context.AltSentRequest.Add(altSentRequest);
            }

            // Add alt sentence request program
            _context.AltSentRequestProgram.AddRange(altSentenceRequest.Programs
                .Select(a => new AltSentRequestProgram
                {
                    AltSentRequestId = altSentRequest.AltSentRequestId,
                    AltSentProgramId = a.Key
                }));
            // create alt sentence request history based on alt sentence request
            CreateAltSentRequestHistory(altSentRequest.AltSentRequestId, altSentenceRequest.HistoryList);
            return await _context.SaveChangesAsync();
        }

        /// <summary>
        /// check altSent request exists or not? based on inmate and facility
        /// </summary>
        /// <param name="inmateId"></param>
        /// <param name="facilityId"></param>
        /// <returns></returns>
        public bool CheckAltSentRequestExists(int inmateId, int facilityId)
        {
            return _context.AltSentRequest.Any(a => a.InmateId == inmateId && a.FacilityId == facilityId
                                                                           && !a.AltSentId.HasValue &&
                                                                           !a.RejectFlag.HasValue &&
                                                                           !a.DeleteFlag.HasValue &&
                                                                           a.Inmate.InmateActive == 1);
        }

        /// <summary>
        /// Get alt sentence request based on facility and request id
        /// </summary>
        /// <param name="altSentRequestId"></param>
        /// <returns></returns>
        private AltSentenceRequest GetAltSentRequest(int altSentRequestId)
        {
            AltSentenceRequest altSentRequest = _context.AltSentRequest
                .Where(a => a.AltSentRequestId == altSentRequestId).Select(request => new AltSentenceRequest
                {
                    AltSentRequestId = request.AltSentRequestId,
                    FacilityId = request.FacilityId,
                    FacilityName = request.Facility.FacilityName,
                    RequestDate = request.RequestDate,
                    InmateId = request.InmateId,
                    InmateInfo = new PersonVm
                    {
                        InmateId = request.InmateId,
                        PersonId = request.Inmate.PersonId,
                        PersonFirstName = request.Inmate.Person.PersonFirstName,
                        PersonMiddleName = request.Inmate.Person.PersonMiddleName,
                        PersonLastName = request.Inmate.Person.PersonLastName,
                        InmateNumber = request.Inmate.InmateNumber
                    },
                    HousingDetail = new HousingDetail
                    {
                        HousingUnitLocation = request.Inmate.HousingUnitId > 0
                            ? request.Inmate.HousingUnit.HousingUnitLocation
                            : default,
                        HousingUnitNumber = request.Inmate.HousingUnitId > 0
                            ? request.Inmate.HousingUnit.HousingUnitNumber
                            : default,
                        HousingUnitBedLocation = request.Inmate.HousingUnitId > 0
                            ? request.Inmate.HousingUnit.HousingUnitBedLocation
                            : default,
                        HousingUnitBedNumber = request.Inmate.HousingUnitId > 0
                            ? request.Inmate.HousingUnit.HousingUnitBedNumber
                            : default,
                        FacilityAbbr = request.Inmate.HousingUnitId > 0
                            ? request.Inmate.HousingUnit.Facility.FacilityAbbr
                            : default
                    },
                    OfficerLastName = request.RequestPersonnel.PersonNavigation.PersonLastName,
                    PersonnelNumber = request.RequestPersonnel.PersonnelNumber,
                    RequestNote = request.RequestNote,
                    ApprovalNote = request.ApprovalNote,
                    DeleteFlag = request.DeleteFlag > 0,
                    ApproveFlag = request.ApproveFlag > 0,
                    RejectFlag = request.RejectFlag > 0,
                    Programs = request.AltSentRequestProgram.Select(a =>
                        new KeyValuePair<int, string>
                            (a.AltSentProgramId, a.AltSentProgram.AltSentProgramAbbr)).ToList(),
                    CreatedById = request.CreateBy,
                    CreatedByLastName = request.CreateByNavigation.PersonNavigation.PersonLastName,
                    CreatedByFirstName = request.CreateByNavigation.PersonNavigation.PersonFirstName,
                    CreatedDate = request.CreateDate,
                    UpdatedById = request.UpdateBy,
                    UpdatedByLastName = request.UpdateBy.HasValue
                        ? request.UpdateByNavigation.PersonNavigation.PersonLastName
                        : default,
                    UpdatedByFirstName = request.UpdateBy.HasValue
                        ? request.UpdateByNavigation.PersonNavigation.PersonFirstName
                        : default,
                    UpdatedDate = request.UpdateDate,
                    DeletedById = request.DeletedBy,
                    ApprovedById = request.ApprovedBy,
                    ApprovedByLastName = request.ApprovedBy.HasValue
                        ? request.ApprovedByNavigation.PersonNavigation.PersonLastName
                        : default,
                    ApprovedPersonnelNumber =
                        request.ApprovedBy.HasValue ? request.ApprovedByNavigation.PersonnelNumber : default,
                    RejectedById = request.RejectedBy,
                    RejectedByLastName = request.RejectedBy.HasValue
                        ? request.RejectedByNavigation.PersonNavigation.PersonLastName
                        : default,
                    RejectedPersonnelNumber =
                        request.RejectedBy.HasValue ? request.RejectedByNavigation.PersonnelNumber : default,
                    DeletedDate = request.DeleteDate,
                    DeletedByLastName = request.DeletedBy.HasValue
                        ? request.DeletedByNavigation.PersonNavigation.PersonLastName
                        : default,
                    DeletedPersonnelNumber =
                        request.DeletedBy.HasValue ? request.DeletedByNavigation.PersonnelNumber : default,
                    ApprovedDate = request.ApproveDate,
                    RejectedDate = request.RejectDate,
                    PersonnelId = request.RequestPersonnelId
                }).Single();
            return altSentRequest;
        }

        /// <summary>
        /// Delete alt sentence active request
        /// </summary>
        /// <param name="altSentRequestId"></param>
        /// <param name="history"></param>
        /// <param name="deleteFlag"></param>
        /// <returns></returns>
        public async Task<int> DeleteAltSentActiveRequest(int altSentRequestId, string history, bool deleteFlag)
        {
            AltSentRequest altSentRequest =
                _context.AltSentRequest.Single(a => a.AltSentRequestId == altSentRequestId);
            if (deleteFlag)
            {
                // Delete alt sentence request
                altSentRequest.DeleteFlag = 1;
                altSentRequest.DeletedBy = _personnelId;
                altSentRequest.DeleteDate = DateTime.Now;
            }
            else
            {
                // Undo alt sentence request
                altSentRequest.DeleteFlag = null;
                altSentRequest.DeletedBy = null;
                altSentRequest.DeleteDate = null;
            }

            altSentRequest.UpdateDate = DateTime.Now;
            altSentRequest.UpdateBy = _personnelId;
            // Create alt sentence request history
            CreateAltSentRequestHistory(altSentRequest.AltSentRequestId, history);
            return await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Get altSent histories based on request
        /// </summary>
        /// <param name="altSentRequestId"></param>
        /// <returns></returns>
        public List<HistoryVm> GetAltSentHistories(int altSentRequestId)
        {
            List<HistoryVm> altSentHistories = _context.AltSentRequestHistory
                .Where(a => a.AltSentRequestId == altSentRequestId)
                .OrderByDescending(a => a.CreateDate).Select(history => new HistoryVm
                {
                    HistoryId = history.AltSentRequestHistoryId,
                    CreateDate = history.CreateDate,
                    PersonId = history.Personnel.PersonId,
                    OfficerBadgeNumber = history.Personnel.OfficerBadgeNum,
                    PersonLastName = history.Personnel.PersonNavigation.PersonLastName,
                    HistoryList = history.AltSentHistoryList
                }).ToList();
            altSentHistories.Where(a => !string.IsNullOrEmpty(a.HistoryList)).ToList().ForEach(a =>
            {
                a.Header = JsonConvert.DeserializeObject<Dictionary<string, string>>(a.HistoryList)
                    .Select(x => new PersonHeader
                    {
                        Header = x.Key,
                        Detail = x.Value
                    }).ToList();
            });
            return altSentHistories;
        }

        /// <summary>
        /// Approve altSent request
        /// </summary>
        /// <param name="altSentRequestId"></param>
        /// <param name="approveRequest"></param>
        /// <returns></returns>
        public async Task<int> ApproveAltSentRequest(int altSentRequestId, ApproveRequest approveRequest)
        {
            AltSentRequest altSentRequest =
                _context.AltSentRequest.Single(a => a.AltSentRequestId == altSentRequestId);
            altSentRequest.ApprovalNote = approveRequest.ApprovalNote;
            altSentRequest.UpdateDate = DateTime.Now;
            altSentRequest.UpdateBy = _personnelId;
            if (approveRequest.Approveflag)
            {
                // Approve altSent request
                altSentRequest.ApproveFlag = 1;
                altSentRequest.ApprovedBy = _personnelId;
                altSentRequest.ApproveDate = DateTime.Now;
                altSentRequest.RejectFlag = null;
                altSentRequest.RejectDate = null;
                altSentRequest.RejectedBy = null;
                // insert altSent request approval history
                InsertAltSentRequestApprovalHistory(altSentRequestId, approveRequest.ApprovalNote,
                    AltSentDetailConstants.Approved);
            }

            if (approveRequest.RejectFlag)
            {
                // Reject altSent request
                altSentRequest.RejectFlag = 1;
                altSentRequest.RejectedBy = _personnelId;
                altSentRequest.RejectDate = DateTime.Now;
                altSentRequest.ApproveFlag = null;
                altSentRequest.ApproveDate = null;
                altSentRequest.ApprovedBy = null;
                // Insert alt sentence reject history
                InsertAltSentRequestApprovalHistory(altSentRequestId, approveRequest.ApprovalNote,
                    AltSentDetailConstants.Rejected);
            }

            if (approveRequest.IsDelete)
            {
                // Delete alt sentence active request
                altSentRequest.DeleteFlag = 1;
                altSentRequest.DeletedBy = _personnelId;
                altSentRequest.DeleteDate = DateTime.Now;
            }
            else
            {
                if (!string.IsNullOrEmpty(approveRequest.StrPersonnelDelete))
                {
                    // Undo alt sentence request delete
                    altSentRequest.DeleteFlag = null;
                    altSentRequest.DeletedBy = null;
                    altSentRequest.DeleteDate = null;
                }
            }

            // To create alt sentence request history
            approveRequest.HistoryList.ForEach(history => { CreateAltSentRequestHistory(altSentRequestId, history); });
            return await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Get schedule details based on facility, date, inmate and request
        /// </summary>
        /// <param name="scheduleDetails"></param>
        /// <returns></returns>
        public ScheduleDetails GetScheduleDetails(ScheduleDetails scheduleDetails)
        {
            scheduleDetails.FacilityList = GetFacilityList();
            // To get scheduled time details based on facility and schedule date
            scheduleDetails.ScheduledTimes = _context.AltSentRequest.Where(a =>
                    a.FacilityId == scheduleDetails.FacilityId
                    && !a.DeleteFlag.HasValue && a.SchedReqBookDate.HasValue && a.Inmate.InmateActive == 1
                    && !a.AltSentId.HasValue &&
                    a.SchedReqBookDate.Value.Date == scheduleDetails.ScheduleDateTime.Value.Date)
                .GroupBy(a => a.SchedReqBookDate)
                .Select(a => new KeyValuePair<TimeSpan, int>(a.Key.Value.TimeOfDay, a.Count())).ToList();
            if (scheduleDetails.IsPreviousNextDate)
            {
                LoadScheduledInmateDetails(scheduleDetails); // To get inmate details
            }
            else
            {
                // To check whether the schedule time is matched or not?
                if (scheduleDetails.ScheduledTimes.Any(a => a.Key == scheduleDetails.ScheduleDateTime?.TimeOfDay))
                {
                    LoadScheduledInmateDetails(scheduleDetails); // To get inmate details
                }
            }

            return scheduleDetails;
        }

        /// <summary>
        /// Schedule alt sentence request
        /// </summary>
        /// <param name="altSentRequestId"></param>
        /// <param name="schedule"></param>
        /// <returns></returns>
        public async Task<int> ScheduleAltSentRequest(int altSentRequestId, SaveSchedule schedule)
        {
            AltSentRequest altSentRequest =
                _context.AltSentRequest.Single(a => a.AltSentRequestId == altSentRequestId);
            altSentRequest.SchedReqBookDate = schedule.ScheduleDateTime;
            altSentRequest.SchedReqBookDateBy = _personnelId;
            altSentRequest.SchedReqBookNotes = schedule.ScheduleNote;
            // Create alt sentence request history
            CreateAltSentRequestHistory(altSentRequest.AltSentRequestId, schedule.HistoryList);
            return await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Create alt sentence request history based on request
        /// </summary>
        /// <param name="altSentRequestId"></param>
        /// <param name="historyList"></param>
        private void CreateAltSentRequestHistory(int altSentRequestId, string historyList)
        {
            AltSentRequestHistory requestHistory = new AltSentRequestHistory
            {
                AltSentRequestId = altSentRequestId,
                PersonnelId = _personnelId,
                CreateDate = DateTime.Now,
                AltSentHistoryList = historyList
            };
            _context.AltSentRequestHistory.Add(requestHistory);
        }

        /// <summary>
        /// Get alt sentence appeal history based on request Id
        /// </summary>
        /// <param name="requestId"></param>
        /// <returns></returns>
        private List<AppealHistory> LoadAltSentAppealHistory(int requestId)
        {
            return _context.AltSentRequestAppeal
                .Where(a => a.AltSentRequestId == requestId)
                .OrderByDescending(a => a.AppealDate).Select(history => new AppealHistory
                {
                    AltSentRequestAppealId = history.AltSentRequestAppealId,
                    Date = history.AppealDate,
                    Status = history.ApproveFlag.HasValue
                        ? AltSentDetailConstants.Approved
                        : history.RejectFlag.HasValue
                            ? AltSentDetailConstants.Rejected
                            : history.DeleteFlag.HasValue
                                ? AltSentDetailConstants.Deleted
                                : AltSentDetailConstants.None,
                    PersonnelId = history.AltSentRequest.RequestPersonnelId,
                    PersonnelLastName = history.AltSentRequest.RequestPersonnel.PersonNavigation.PersonLastName,
                    PersonnelNumber = history.AltSentRequest.RequestPersonnel.PersonnelNumber,
                    Notes = history.AppealNote
                }).ToList();
        }

        /// <summary>
        /// To check alt sentence program exists or not?
        /// </summary>
        /// <param name="inmateId"></param>
        /// <returns></returns>
        public bool CheckAltSentPgmExists(int inmateId)
        {
            return _context.AltSent.Any(a => a.Incarceration.InmateId == inmateId && !a.AltSentClearFlag.HasValue);
        }

        /// <summary>
        /// Insert alt sentence request approval history based on request
        /// </summary>
        /// <param name="requestId"></param>
        /// <param name="approvalNote"></param>
        /// <param name="approveStatus"></param>
        private void InsertAltSentRequestApprovalHistory(int requestId, string approvalNote, string approveStatus)
        {
            AltSentRequestApprovalHistory approvalHistory = new AltSentRequestApprovalHistory
            {
                AltSentRequestId = requestId,
                ApprovalNote = approvalNote,
                CreateDate = DateTime.Now,
                CreateBy = _personnelId,
                UpdateDate = DateTime.Now,
                UpdateBy = _personnelId
            };
            if (AltSentDetailConstants.Approved == approveStatus)
            {
                // Approve
                approvalHistory.ApproveFlag = 1;
                approvalHistory.ApproveDate = DateTime.Now;
                approvalHistory.ApprovedBy = _personnelId;
            }
            else
            {
                // Reject
                approvalHistory.AltSentRequestId = requestId;
                approvalHistory.RejectFlag = 1;
                approvalHistory.RejectDate = DateTime.Now;
                approvalHistory.RejectedBy = _personnelId;
            }

            _context.AltSentRequestApprovalHistory.Add(approvalHistory);
        }

        /// <summary>
        /// Get flag value for alt sentence arrest not allowed based on incarceration Id
        /// </summary>
        /// <param name="arrestId"></param>
        /// <returns></returns>
        public bool GetAltSentArrestNotAllowedFlag(int arrestId)
        {
            return _context.IncarcerationArrestXref.Where(a => !a.ReleaseDate.HasValue && a.Arrest.ArrestId == arrestId)
                .Select(a => a.Arrest.ArrestSentenceAltSentNotAllowed > 0).SingleOrDefault();
        }

        /// <summary>
        /// Get approve request details based on request Id
        /// </summary>
        /// <param name="requestId"></param>
        /// <returns></returns>
        public ApproveRequestDetails GetApproveRequestInfo(int requestId)
        {
            ApproveRequestDetails details = new ApproveRequestDetails
            {
                RequestInfo = GetAltSentRequest(requestId), // Getting alt sentence request details.
                AppealHistory = LoadAltSentAppealHistory(requestId) // Getting alt sentence appeal history.
            };
            return details;
        }

        /// <summary>
        /// Get alt sentence request info for add, edit and view based on facility Id and request Id
        /// </summary>
        /// <param name="facilityId"></param>
        /// <param name="requestId"></param>
        /// <returns></returns>
        public AltSentenceRequest GetAltSentRequestInfo(int facilityId, int? requestId)
        {
            AltSentenceRequest requestInfo = new AltSentenceRequest
            {
                RequestDate = DateTime.Now,
                PersonnelId = _personnelId
            };
            if (requestId > 0)
            {
                requestInfo = GetAltSentRequest(requestId.Value);
            }

            requestInfo.FacilityList = _commonService.GetFacilities()
                .Where(a => (requestId > 0 || a.AltSentFlag == 1) &&
                (requestId <= 0 || a.AltSentFlag == 1 || a.FacilityId == requestInfo.FacilityId))
                .Select(a => new KeyValuePair<int, string>(a.FacilityId, a.FacilityAbbr)).ToList();
            requestInfo.ProgramList = GetAltSentProgramList(facilityId);
            return requestInfo;
        }

        /// <summary>
        /// To get inmate name and number based on inmate id
        /// </summary>
        /// <param name="scheduleDetails"></param>
        private void LoadScheduledInmateDetails(ScheduleDetails scheduleDetails)
        {
            // To check whether the request is matched with scheduled date or not?
            if (!_context.AltSentRequest.Any(a =>
                a.AltSentRequestId == scheduleDetails.RequestId && a.InmateId == scheduleDetails.InmateId &&
                a.SchedReqBookDate.HasValue &&
                a.SchedReqBookDate.Value.Date == scheduleDetails.ScheduleDateTime.Value.Date)) return;
            PersonVm personInfo = _personService.GetInmateDetails(scheduleDetails.InmateId);
            scheduleDetails.PersonLastName = personInfo.PersonLastName;
            scheduleDetails.PersonFirstName = personInfo.PersonFirstName;
            scheduleDetails.Number = personInfo.InmateNumber;
        }

        public List<ProgramTypes> LoadProgramList(int inmateId, int altSentId = 0) => _context.AltSent.Where(a =>
            a.Incarceration.Inmate.InmateId == inmateId
            && !a.AltSentProgram.InactiveFlag.HasValue
            && (altSentId == 0 || a.AltSentId == altSentId)).OrderByDescending(a => a.AltSentId).Select(a =>
            new ProgramTypes
            {
                AltSentId = a.AltSentId,
                AltSentStart = a.AltSentStart,
                IncarcerationId = a.IncarcerationId,
                FacilityAbbr = a.AltSentProgram.Facility.FacilityAbbr,
                AltSentProgramAbbr = a.AltSentProgram.AltSentProgramAbbr,
                AltSentThru = a.AltSentThru,
                AltSentProgramId = a.AltSentProgram.AltSentProgramId,
                AltSentTotalAttend = a.AltSentTotalAttend,
                AltSentNote = a.AltSentNote
            }).ToList();

        public AltSentScheduleVm GetAltSentSchedule(int facilityId, int programId)
        {
            AltSentScheduleVm altSentSchedule = new AltSentScheduleVm
            {
                FacilityList = GetFacilityList().ToList(),
                ProgramList = GetAltSentProgramList(facilityId).ToList(),

                lstAltSentScheduleSites = _context.AltSentSite.Where(alt => alt.AltSentProgramId == programId).Select(
                    alt =>
                        new AltSentSchedule
                        {
                            AltSentSiteId = alt.AltSentSiteId,
                            AltSentSiteName = alt.AltSentSiteName,
                            AltSentWorkSite = alt.AltSentSiteFlagAsWorkSite,
                            AltSentTrainingSite = alt.AltSentSiteFlagAsTrainingSite,
                            AltSentReportingSite = alt.AltSentSiteFlagAsReportingSite,
                            lstAltSentSiteSchedule = _context.AltSentSiteSchd
                                .Where(sch => sch.AltSentSiteId == 1).Select(sch =>
                                    new AltSentSiteSchedule
                                    {
                                        AltSentSiteId = sch.AltSentSiteId,
                                        AltSentSiteScheduleId = sch.AltSentSiteSchdId,
                                        ScheduleDay = sch.AltSentSiteSchdDayOfWeek,
                                        ScheduleTimeFrom = sch.AltSentSiteSchdTimeFrom,
                                        ScheduleTimeThru = sch.AltSentSiteSchdTimeThru,
                                        ScheduleCapacity = sch.AltSentSiteSchdCapacity,
                                        ScheduleDescription = sch.AltSentSiteSchdDescription,
                                        ScheduleAdditionalEmail = sch.AltSentSiteSchdAdditionalEmail,
                                        ScheduleAdditionalContact = sch.AltSentSiteSchdAdditionalContact,
                                        ScheduleInmateInstructions = sch.AltSentSiteSchdInmateInstructions,
                                        InactiveFlag = sch.InactiveFlag == 1,
                                        AssignCount = sch.AltSentSiteSchdDayOfWeek.HasValue
                                            ? Check(sch.AltSentSiteSchdDayOfWeek.Value, sch.AltSentSiteSchdId)
                                            : 0
                                    }).ToList()
                        }).ToList()
            };


            return altSentSchedule;
        }

        double Check(int dayOfWeek, int altSentSiteScheduleId)
        {
            switch (dayOfWeek)
            {
                case 1:
                    return _context.AltSent.Count(alt =>
                        alt.DefaultSunAltSentSiteAssignId == altSentSiteScheduleId && alt.AltSentClearFlag != 1 &&
                        alt.PrimaryAltSentSiteId > 0);
                case 2:
                    return _context.AltSent.Count(alt =>
                        alt.DefaultMonAltSentSiteAssignId == altSentSiteScheduleId && alt.AltSentClearFlag != 1 &&
                        alt.PrimaryAltSentSiteId > 0);
                case 3:
                    return _context.AltSent.Count(alt =>
                        alt.DefaultTueAltSentSiteAssignId == altSentSiteScheduleId && alt.AltSentClearFlag != 1 &&
                        alt.PrimaryAltSentSiteId > 0);
                case 4:
                    return _context.AltSent.Count(alt =>
                        alt.DefaultWedAltSentSiteAssignId == altSentSiteScheduleId && alt.AltSentClearFlag != 1 &&
                        alt.PrimaryAltSentSiteId > 0);
                case 5:
                    return _context.AltSent.Count(alt =>
                        alt.DefaultThuAltSentSiteAssignId == altSentSiteScheduleId && alt.AltSentClearFlag != 1 &&
                        alt.PrimaryAltSentSiteId > 0);
                case 6:
                    return _context.AltSent.Count(alt =>
                        alt.DefaultFriAltSentSiteAssignId == altSentSiteScheduleId && alt.AltSentClearFlag != 1 &&
                        alt.PrimaryAltSentSiteId > 0);
                case 7:
                    return _context.AltSent.Count(alt =>
                        alt.DefaultSatAltSentSiteAssignId == altSentSiteScheduleId && alt.AltSentClearFlag != 1 &&
                        alt.PrimaryAltSentSiteId > 0);
            }

            return 0;
        }

        public List<AltSentQueue> GetAltSentQueueDetails(int facilityId)
        {
            List<HousingDetail> housingDetail = _context.HousingUnit
                .Select(hu => new HousingDetail
                {
                    HousingUnitLocation = hu.HousingUnitLocation,
                    HousingUnitNumber = hu.HousingUnitNumber,
                    //FacilityAbbr = hu.Facility.FacilityAbbr,
                }).ToList();

            List<AltSentQueue> lstAltSentQueue = _context.Incarceration.Include(i => i.CurrentAltSent).Where(i =>
                    i.Inmate.FacilityId == facilityId
                    && i.Inmate.InmateActive == 1
                    && !i.ReleaseOut.HasValue
                    && !i.CurrentAltSentId.HasValue
                    || i.CurrentAltSent.AltSentProgramId == 0
                )
                .Select(i => new AltSentQueue
                {
                    PersonLastName = i.Inmate.Person.PersonLastName,
                    PersonFirstName = i.Inmate.Person.PersonFirstName,
                    Number = i.Inmate.InmateNumber,
                    DateIn = i.DateIn,
                    HousingDetail = housingDetail.SingleOrDefault(h => h.HousingUnitId == i.Inmate.HousingUnitId)
                }).ToList();

            return lstAltSentQueue;
        }
    }
}