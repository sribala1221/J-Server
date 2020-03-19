using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GenerateTables.Models;
using ServerAPI.ViewModels;
using ServerAPI.Utilities;
using Microsoft.AspNetCore.Http;

namespace ServerAPI.Services
{
    public class RecordsCheckService : IRecordsCheckService
    {
        private readonly AAtims _context;
        private readonly ICommonService _commonService;
        private readonly RecordsCheckHistoryVm ObRecordsCheckHistory = new RecordsCheckHistoryVm();
        private readonly int _personnelId;

        public RecordsCheckService(AAtims context, ICommonService commonService,
            IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _commonService = commonService;
            _personnelId = Convert.ToInt32(httpContextAccessor.HttpContext.User.FindFirst("personnelId")?.Value);
        }

        public RecordsCheckHistoryVm GetRecordHistroy(int personId)
        {
            ObRecordsCheckHistory.LstRecordHisty = (from r in _context.RecordsCheckRequest
                where r.PersonId == personId
                select new RecordsCheckVm
                {
                    RecordsCheckRequestId = r.RecordsCheckRequestId,
                    RequestType = r.RequestType,
                    RequestNote = r.RequestNote,
                    BypassNote = r.BypassNotes,
                    ResponseNote = r.ResponseNote,
                    ClearNote = r.ClearNote,
                    RequestDate = r.RequestDate,
                    ByPassFlag = r.BypassFlag.HasValue,
                    DeleteFlag = r.DeleteFlag == 1,
                    ResponseDate = r.ResponseDate,
                    ByPassDate = r.BypassDate,
                    ResponseFlag = r.ResponseFlag.HasValue,
                    ClearFlag = r.ClearFlag.HasValue,
                    SupervisorReviewFlag = r.SupervisorReviewFlag.HasValue,
                    ClearDate = r.ClearDate,
                    RequestFlag = r.RequestFlag.HasValue,
                    SupervisorReviewDate = r.SupervisorReviewDate,
                    RequestBy = r.RequestBy,
                    ResponseBy = r.ResponseBy,
                    ClearBy = r.ClearBy,
                    SupervisorBy = r.SupervisorReviewBy,
                    ByPassBy = r.BypassBy,
                    InmateNumber = _context.Inmate.Single(i => i.PersonId == personId).InmateNumber,
                    PersonSuffix = r.Person.PersonSuffix,
                    PersonLastName = r.Person.PersonLastName,
                    PersonFirstName = r.Person.PersonFirstName,
                    PersonMiddleName = r.Person.PersonMiddleName,
                    PersonId = personId,
                    RequestFacilityId = r.RequestFacilityId,
                    BypassReason = r.BypassReason,
                    BypassDate = r.BypassDate
                }).OrderByDescending(o => o.RecordsCheckRequestId).ToList();

            if (ObRecordsCheckHistory.LstRecordHisty != null)
            {
                List<int> personIds = ObRecordsCheckHistory.LstRecordHisty.Select(i => new[]
                    {
                        i.RequestBy, i.ResponseBy, i.ClearBy,
                        i.SupervisorBy, i.ByPassBy
                    })
                    .SelectMany(i => i)
                    .Where(i => i.HasValue)
                    .Select(i => i.Value)
                    .ToList();

                List<RecordsCheckVm> lstPersonDet = (from per in _context.Personnel
                    where personIds.Contains(per.PersonnelId)
                    select new RecordsCheckVm
                    {
                        Personneld = per.PersonnelId,
                        PersonLastName = per.PersonNavigation.PersonLastName,
                        OfficerBadgeNumber = per.OfficerBadgeNum

                    }).ToList();

                ObRecordsCheckHistory.LstRecordHisty.ForEach(item =>
                {
                    //Get Actions of Recordscheck
                    item.RequestAction = _context.RecordsCheckRequestActions.Where(i =>
                            i.RecordsCheckRequestId == item.RecordsCheckRequestId)
                        .Select(i => i.RecordsCheckRequestAction)
                        .ToArray();
                    item.ResponseAction = _context.RecordsCheckResponseAlerts.Where(i =>
                            i.RecordsCheckRequestId == item.RecordsCheckRequestId)
                        .Select(i => i.ResponseAlert)
                        .ToArray();
                    item.ClearAction = _context.RecordsCheckClearActions.Where(i =>
                            i.RecordsCheckRequestId == item.RecordsCheckRequestId)
                        .Select(i => i.RecordsCheckAction)
                        .ToArray();

                    RecordsCheckVm personnelDetails;
                    if (item.RequestBy > 0)
                    {
                        personnelDetails = lstPersonDet.Single(p => p.Personneld == item.RequestBy);
                        item.RequestOfficer = new PersonnelVm
                        {
                            PersonLastName = personnelDetails.PersonLastName,
                            OfficerBadgeNumber = personnelDetails.OfficerBadgeNumber
                        };
                    }

                    if (item.ResponseBy > 0)
                    {
                        personnelDetails = lstPersonDet.Single(p => p.Personneld == item.ResponseBy);
                        item.ResponseOfficer = new PersonnelVm
                        {
                            PersonLastName = personnelDetails.PersonLastName,
                            OfficerBadgeNumber = personnelDetails.OfficerBadgeNumber
                        };
                    }

                    if (item.ClearBy > 0)
                    {
                        personnelDetails = lstPersonDet.Single(p => p.Personneld == item.ClearBy);
                        item.ClearOfficer = new PersonnelVm
                        {
                            PersonLastName = personnelDetails.PersonLastName,
                            OfficerBadgeNumber = personnelDetails.OfficerBadgeNumber
                        };
                    }

                    if (item.ByPassBy > 0)
                    {
                        personnelDetails = lstPersonDet.Single(p => p.Personneld == item.ByPassBy);
                        item.ByPassOfficer = new PersonnelVm
                        {
                            PersonLastName = personnelDetails.PersonLastName,
                            OfficerBadgeNumber = personnelDetails.OfficerBadgeNumber
                        };
                    }

                    if (item.SupervisorBy > 0)
                    {
                        personnelDetails = lstPersonDet.Single(p => p.Personneld == item.SupervisorBy);
                        item.SuperviseOfficer = new PersonnelVm
                        {
                            PersonLastName = personnelDetails.PersonLastName,
                            OfficerBadgeNumber = personnelDetails.OfficerBadgeNumber
                        };
                    }
                });
            }

            ObRecordsCheckHistory.RecordCheckType = _commonService.GetLookupList(LookupConstants.RECCHKREQTYPE);

            ObRecordsCheckHistory.FacilityList = _commonService.GetFacilities();

            ObRecordsCheckHistory.ActionList = GetActionList(LookupConstants.RECCHKREQACTION);

            return ObRecordsCheckHistory;
        }

        public RecordsCheckVm GetRecordCheck(int formRecordId)
        {
            RecordsCheckVm recordCheck = _context.RecordsCheckRequest
                .Where(rec => rec.RecordsCheckRequestId == formRecordId)
                .Select(r => new RecordsCheckVm
                {
                    RecordsCheckRequestId = r.RecordsCheckRequestId,
                    RequestType = r.RequestType,
                    RequestNote = r.RequestNote,
                    BypassNote = r.BypassNotes,
                    ResponseNote = r.ResponseNote,
                    ClearNote = r.ClearNote,
                    RequestDate = r.RequestDate,
                    ByPassFlag = r.BypassFlag.HasValue,
                    DeleteFlag = r.DeleteFlag == 1,
                    ResponseDate = r.ResponseDate,
                    ByPassDate = r.BypassDate,
                    ResponseFlag = r.ResponseFlag.HasValue,
                    ClearFlag = r.ClearFlag.HasValue,
                    SupervisorReviewFlag = r.SupervisorReviewFlag.HasValue,
                    ClearDate = r.ClearDate,
                    RequestFlag = r.RequestFlag.HasValue,
                    SupervisorReviewDate = r.SupervisorReviewDate,
                    RequestBy = r.RequestBy,
                    ResponseBy = r.ResponseBy,
                    ClearBy = r.ClearBy,
                    SupervisorBy = r.SupervisorReviewBy,
                    ByPassBy = r.BypassBy,
                    InmateNumber = _context.Inmate.Single(i => i.PersonId == r.PersonId).InmateNumber,
                    PersonLastName = r.Person.PersonLastName,
                    PersonFirstName = r.Person.PersonFirstName,
                    PersonId = r.PersonId,
                    RequestFacilityId = r.RequestFacilityId
                }).Single();

            List<RecordsCheckVm> lstPersonDet = (from per in _context.Personnel
                select new RecordsCheckVm
                {
                    Personneld = per.PersonnelId,
                    PersonLastName = per.PersonNavigation.PersonLastName,
                    OfficerBadgeNumber = per.OfficerBadgeNum
                }).ToList();


            recordCheck.RequestAction = _context.RecordsCheckRequestActions.Where(i =>
                    i.RecordsCheckRequestId == recordCheck.RecordsCheckRequestId)
                .Select(i => i.RecordsCheckRequestAction)
                .ToArray();
            recordCheck.ResponseAction = _context.RecordsCheckResponseAlerts.Where(i =>
                    i.RecordsCheckRequestId == recordCheck.RecordsCheckRequestId)
                .Select(i => i.ResponseAlert)
                .ToArray();
            recordCheck.ClearAction = _context.RecordsCheckClearActions.Where(i =>
                    i.RecordsCheckRequestId == recordCheck.RecordsCheckRequestId)
                .Select(i => i.RecordsCheckAction)
                .ToArray();

            RecordsCheckVm personnelDetails;
            if (recordCheck.RequestBy > 0)
            {
                personnelDetails = lstPersonDet.Single(p => p.Personneld == recordCheck.RequestBy);
                recordCheck.RequestOfficer = new PersonnelVm
                {
                    PersonLastName = personnelDetails.PersonLastName,
                    OfficerBadgeNumber = personnelDetails.OfficerBadgeNumber
                };
            }

            if (recordCheck.ResponseBy > 0)
            {
                personnelDetails = lstPersonDet.Single(p => p.Personneld == recordCheck.ResponseBy);
                recordCheck.ResponseOfficer = new PersonnelVm
                {
                    PersonLastName = personnelDetails.PersonLastName,
                    OfficerBadgeNumber = personnelDetails.OfficerBadgeNumber
                };
            }

            if (recordCheck.ClearBy > 0)
            {
                personnelDetails = lstPersonDet.Single(p => p.Personneld == recordCheck.ClearBy);
                recordCheck.ClearOfficer = new PersonnelVm
                {
                    PersonLastName = personnelDetails.PersonLastName,
                    OfficerBadgeNumber = personnelDetails.OfficerBadgeNumber
                };
            }

            if (recordCheck.ByPassBy > 0)
            {
                personnelDetails = lstPersonDet.Single(p => p.Personneld == recordCheck.ByPassBy);
                recordCheck.ByPassOfficer = new PersonnelVm
                {
                    PersonLastName = personnelDetails.PersonLastName,
                    OfficerBadgeNumber = personnelDetails.OfficerBadgeNumber
                };
            }

            if (recordCheck.SupervisorBy > 0)
            {
                personnelDetails = lstPersonDet.Single(p => p.Personneld == recordCheck.SupervisorBy);
                recordCheck.SuperviseOfficer = new PersonnelVm
                {
                    PersonLastName = personnelDetails.PersonLastName,
                    OfficerBadgeNumber = personnelDetails.OfficerBadgeNumber
                };
            }

            return recordCheck;
        }

        public List<KeyValuePair<int, string>> GetActionList(string lookupType)
            => _commonService.GetLookupKeyValuePairs(lookupType);

        public async Task<RecordsCheckVm> InsertRecordsCheck(RecordsCheckVm obSaveRecordsCheck)
        {

            if (obSaveRecordsCheck.RecordsCheckRequestId == 0)
            {
                RecordsCheckRequest recordscheck = new RecordsCheckRequest
                {
                    PersonId = obSaveRecordsCheck.PersonId,
                    RequestType = obSaveRecordsCheck.RequestType,
                    RequestBy = _personnelId,
                    RequestDate = DateTime.Now,
                    RequestNote = obSaveRecordsCheck.Note,
                    RequestFacilityId = obSaveRecordsCheck.RequestFacilityId
                };
                _context.RecordsCheckRequest.Add(recordscheck);


                List<RecordsCheckRequestActions> reqAction = obSaveRecordsCheck.Action.Select(rc =>
                    new RecordsCheckRequestActions
                    {
                        RecordsCheckRequestId = recordscheck.RecordsCheckRequestId,
                        RecordsCheckRequestAction = rc
                    }).ToList();

                _context.RecordsCheckRequestActions.AddRange(reqAction);

            }
            else
            {
                RecordsCheckRequest updateRecordsCheck = _context.RecordsCheckRequest.Single(r =>
                    r.RecordsCheckRequestId == obSaveRecordsCheck.RecordsCheckRequestId);
                switch (obSaveRecordsCheck.RecordsStatus)
                {
                    case RecordsCheckStatus.Request:
                        updateRecordsCheck.RequestNote = obSaveRecordsCheck.Note;
                        updateRecordsCheck.RequestBy = _personnelId;
                        updateRecordsCheck.RequestDate = DateTime.Now;
                        updateRecordsCheck.RequestFacilityId = obSaveRecordsCheck.RequestFacilityId;

                        List<RecordsCheckRequestActions> reqActions = _context.RecordsCheckRequestActions.Where(r =>
                            r.RecordsCheckRequestId == obSaveRecordsCheck.RecordsCheckRequestId).ToList();

                        _context.RecordsCheckRequestActions.RemoveRange(reqActions);


                        List<RecordsCheckRequestActions> reqAction = obSaveRecordsCheck.Action.Select(asd =>
                            new RecordsCheckRequestActions
                            {
                                RecordsCheckRequestId = obSaveRecordsCheck.RecordsCheckRequestId,
                                RecordsCheckRequestAction = asd
                            }).ToList();

                        _context.RecordsCheckRequestActions.AddRange(reqAction);


                        break;
                    case RecordsCheckStatus.Response:
                        updateRecordsCheck.ResponseNote = obSaveRecordsCheck.Note;
                        updateRecordsCheck.ResponseBy = _personnelId;
                        updateRecordsCheck.ResponseDate = DateTime.Now;

                        List<RecordsCheckResponseAlerts> resActions = _context.RecordsCheckResponseAlerts.Where(r =>
                            r.RecordsCheckRequestId == obSaveRecordsCheck.RecordsCheckRequestId).ToList();

                        _context.RecordsCheckResponseAlerts.RemoveRange(resActions);

                        List<RecordsCheckResponseAlerts> objInsrtRespose = obSaveRecordsCheck.Action.Select(asd =>
                            new RecordsCheckResponseAlerts
                            {
                                RecordsCheckRequestId = obSaveRecordsCheck.RecordsCheckRequestId,
                                ResponseAlert = asd
                            }).ToList();

                        _context.RecordsCheckResponseAlerts.AddRange(objInsrtRespose);

                        break;
                    default:
                        updateRecordsCheck.ClearNote = obSaveRecordsCheck.Note;
                        updateRecordsCheck.ClearBy = _personnelId;
                        updateRecordsCheck.ClearDate = DateTime.Now;

                        List<RecordsCheckClearActions> clearActions = _context.RecordsCheckClearActions.Where(r =>
                            r.RecordsCheckRequestId == obSaveRecordsCheck.RecordsCheckRequestId).ToList();

                        _context.RecordsCheckClearActions.RemoveRange(clearActions);


                        List<RecordsCheckClearActions> clrAction = obSaveRecordsCheck.Action.Select(asd =>
                            new RecordsCheckClearActions
                            {
                                RecordsCheckRequestId = obSaveRecordsCheck.RecordsCheckRequestId,
                                RecordsCheckAction = asd
                            }).ToList();

                        _context.RecordsCheckClearActions.AddRange(clrAction);

                        break;
                }
            }

            await _context.SaveChangesAsync();

            return obSaveRecordsCheck;
        }


        public Task<int> InsertBypass(RecordsCheckVm objSaveRecordsCheck)
        {

            RecordsCheckRequest recordscheck = new RecordsCheckRequest
            {
                PersonId = objSaveRecordsCheck.PersonId,
                BypassBy = _personnelId,
                BypassFlag = 1,
                BypassDate = DateTime.Now,
                BypassNotes = objSaveRecordsCheck.Note,
                //BypassReason = objSaveRecordsCheck.Action
            };
            _context.RecordsCheckRequest.Add(recordscheck);

            return _context.SaveChangesAsync();

        }

        public async Task<RecordsCheckVm> SendResponseRecordsCheck(RecordsCheckVm obSaveRecordsCheck)
        {
            RecordsCheckRequest updateRecordsCheck = _context.RecordsCheckRequest.Single(r =>
                r.RecordsCheckRequestId == obSaveRecordsCheck.RecordsCheckRequestId);


            switch (obSaveRecordsCheck.RecordsStatus)
            {
                case RecordsCheckStatus.Request:
                    updateRecordsCheck.RequestBy = _personnelId;
                    updateRecordsCheck.RequestDate = DateTime.Now;
                    updateRecordsCheck.RequestFlag = 1;
                    break;
                case RecordsCheckStatus.Response:
                    updateRecordsCheck.ResponseFlag = 1;
                    updateRecordsCheck.ResponseBy = _personnelId;
                    updateRecordsCheck.ResponseDate = DateTime.Now;
                    break;
                case RecordsCheckStatus.Supervisor:
                    updateRecordsCheck.SupervisorReviewFlag = 1;
                    updateRecordsCheck.SupervisorReviewNote = obSaveRecordsCheck.Note;
                    updateRecordsCheck.SupervisorReviewBy = _personnelId;
                    updateRecordsCheck.SupervisorReviewDate = DateTime.Now;
                    break;
                default:
                    updateRecordsCheck.ClearFlag = 1;
                    updateRecordsCheck.ClearBy = _personnelId;
                    updateRecordsCheck.ClearDate = DateTime.Now;
                    break;
            }

            await _context.SaveChangesAsync();

            return obSaveRecordsCheck;
        }

        public Task<int> DeleteRecordsCheck(int recordCheckId)
        {


            RecordsCheckRequest updateRecordsCheck =
                _context.RecordsCheckRequest.Single(r => r.RecordsCheckRequestId == recordCheckId);
            updateRecordsCheck.DeleteFlag = 1;
            updateRecordsCheck.DeleteBy = _personnelId;
            updateRecordsCheck.DeleteByNavigation = _context.Personnel.Single(per => per.PersonnelId == _personnelId);

            return _context.SaveChangesAsync();
        }

        public async Task<int> InsertFormRecords(FormRecordVm obInsert)
        {
            FormRecord objFormRec = _context.FormRecord.SingleOrDefault(i => i.FormRecordId == obInsert.FormRecordId);
            if (objFormRec != null)
            {
                objFormRec.XmlData = obInsert.XmlData;
                objFormRec.UpdateBy = 1;
                objFormRec.UpdateDate = DateTime.Now;
            }

            FormRecordSaveHistory obj = new FormRecordSaveHistory
            {
                XmlData = obInsert.XmlData,
                FormRecordId = obInsert.FormRecordId,
                SaveBy = 1,
                SaveDate = DateTime.Now
            };
            _context.FormRecordSaveHistory.Add(obj);
            _context.SaveChanges();

            return await _context.SaveChangesAsync();
        }

        public List<FormRecordVm> FormRecordHist(int formRecordId) =>
            (from i in _context.FormRecordSaveHistory
                where i.FormRecordId == formRecordId
                select new FormRecordVm
                {
                    FormRecordId = formRecordId,
                    FormRecordSaveHistoryId = i.FormRecordSaveHistoryId,
                    XmlData = i.XmlData,
                    SaveDate = i.SaveDate,
                    SaveBy = i.SaveBy,
                    OfficerBadgeNumber = i.SaveByNavigation.PersonnelNumber,
                    HtmlFileName = i.FormRecord.FormTemplates.HtmlFileName,
                    FormNotes = i.FormNotes
                }).ToList();

        // Records check response count
        public IQueryable<RecordsCheckRequest> GetRecordsCheckResponseCount(int facilityId) =>
            _context.RecordsCheckRequest.Where(rcr => rcr.ResponseFlag.HasValue && rcr.ResponseFlag == 1
                && rcr.DeleteFlag == 0 && !rcr.ClearFlag.HasValue &&
                rcr.RequestFacilityId.HasValue && rcr.RequestFacilityId == facilityId);

        // Get Record check response details.
        public List<RecordsCheckVm> GetRecordsCheckResponse(int facilityId)
        {
            List<RecordsCheckVm> lstRecordsCheckResponse =
                GetRecordsCheckResponseCount(facilityId).Select(rcr => new RecordsCheckVm
                {
                    RecordsCheckRequestId = rcr.RecordsCheckRequestId,
                    RequestType = rcr.RequestType,
                    RequestNote = rcr.RequestNote,
                    BypassNote = rcr.BypassNotes,
                    ResponseNote = rcr.ResponseNote,
                    ClearNote = rcr.ClearNote,
                    RequestDate = rcr.RequestDate,
                    ByPassFlag = rcr.BypassFlag.HasValue,
                    DeleteFlag = rcr.DeleteFlag == 1,
                    ResponseDate = rcr.ResponseDate,
                    ByPassDate = rcr.BypassDate,
                    ResponseFlag = rcr.ResponseFlag.HasValue,
                    ClearFlag = rcr.ClearFlag.HasValue,
                    SupervisorReviewFlag = rcr.SupervisorReviewFlag.HasValue,
                    ClearDate = rcr.ClearDate,
                    RequestFlag = rcr.RequestFlag.HasValue,
                    SupervisorReviewDate = rcr.SupervisorReviewDate,
                    RequestBy = rcr.RequestBy,
                    ResponseBy = rcr.ResponseBy,
                    ClearBy = rcr.ClearBy,
                    SupervisorBy = rcr.SupervisorReviewBy,
                    ByPassBy = rcr.BypassBy,
                    InmateNumber = _context.Inmate.Single(i => i.PersonId == rcr.PersonId).InmateNumber,
                    PersonLastName = rcr.Person.PersonLastName,
                    PersonFirstName = rcr.Person.PersonFirstName,
                    PersonId = rcr.PersonId,
                    RequestFacilityId = rcr.RequestFacilityId
                }).ToList();

            List<int> personIds = lstRecordsCheckResponse.Select(i => new[]
                {
                    i.RequestBy, i.ResponseBy, i.ClearBy,
                    i.SupervisorBy, i.ByPassBy
                })
                .SelectMany(i => i)
                .Where(i => i.HasValue)
                .Select(i => i.Value)
                .ToList();

            List<RecordsCheckVm> lstPersonDet = (from per in _context.Personnel
                where personIds.Contains(per.PersonnelId)
                select new RecordsCheckVm
                {
                    Personneld = per.PersonnelId,
                    PersonLastName = per.PersonNavigation.PersonLastName,
                    OfficerBadgeNumber = per.OfficerBadgeNum
                }).ToList();

            lstRecordsCheckResponse.ForEach(item =>
            {
                //Get Actions of Records check
                item.RequestAction = _context.RecordsCheckRequestActions.Where(i =>
                        i.RecordsCheckRequestId == item.RecordsCheckRequestId)
                    .Select(i => i.RecordsCheckRequestAction).ToArray();
                item.ResponseAction = _context.RecordsCheckResponseAlerts.Where(i =>
                        i.RecordsCheckRequestId == item.RecordsCheckRequestId)
                    .Select(i => i.ResponseAlert).ToArray();
                item.ClearAction = _context.RecordsCheckClearActions.Where(i =>
                        i.RecordsCheckRequestId == item.RecordsCheckRequestId)
                    .Select(i => i.RecordsCheckAction).ToArray();

                RecordsCheckVm personnelDetails;
                if (item.RequestBy > 0)
                {
                    personnelDetails = lstPersonDet.Single(p => p.Personneld == item.RequestBy);
                    item.RequestOfficer = new PersonnelVm
                    {
                        PersonLastName = personnelDetails.PersonLastName,
                        OfficerBadgeNumber = personnelDetails.OfficerBadgeNumber
                    };
                }

                if (item.ResponseBy > 0)
                {
                    personnelDetails = lstPersonDet.Single(p => p.Personneld == item.ResponseBy);
                    item.ResponseOfficer = new PersonnelVm
                    {
                        PersonLastName = personnelDetails.PersonLastName,
                        OfficerBadgeNumber = personnelDetails.OfficerBadgeNumber
                    };
                }

                if (item.ClearBy > 0)
                {
                    personnelDetails = lstPersonDet.Single(p => p.Personneld == item.ClearBy);
                    item.ClearOfficer = new PersonnelVm
                    {
                        PersonLastName = personnelDetails.PersonLastName,
                        OfficerBadgeNumber = personnelDetails.OfficerBadgeNumber
                    };
                }

                if (item.ByPassBy > 0)
                {
                    personnelDetails = lstPersonDet.Single(p => p.Personneld == item.ByPassBy);
                    item.ByPassOfficer = new PersonnelVm
                    {
                        PersonLastName = personnelDetails.PersonLastName,
                        OfficerBadgeNumber = personnelDetails.OfficerBadgeNumber
                    };
                }

                if (!item.SupervisorBy.HasValue || item.SupervisorBy ==0) return;
                personnelDetails = lstPersonDet.Single(p => p.Personneld == item.SupervisorBy);
                item.SuperviseOfficer = new PersonnelVm
                {
                    PersonLastName = personnelDetails.PersonLastName,
                    OfficerBadgeNumber = personnelDetails.OfficerBadgeNumber
                };
            });

            return lstRecordsCheckResponse;
        }
    }
}
