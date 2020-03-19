using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using GenerateTables.Models;
using ServerAPI.ViewModels;
using ServerAPI.Utilities;

namespace ServerAPI.Services
{
    public class IncidentAppealsService : IIncidentAppealsService
    {
        private readonly AAtims _context;
        private readonly int _personnelId;
        private readonly ICommonService _commonService;

        public IncidentAppealsService(AAtims context, ICommonService commonService, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _commonService = commonService;
            _personnelId = Convert.ToInt32(httpContextAccessor.HttpContext.User
                .FindFirst(AuthDetailConstants.PERSONNELID)?.Value);
        }

        public List<KeyValuePair<string, int>> GetIncidentAppealsCount(int facilityId)
        {
            List<KeyValuePair<string, int>> appealsCount = new List<KeyValuePair<string, int>>();

            //To get DisciplinaryIncident details
            IQueryable<DisciplinaryIncident> lstDisciplinaryIncident =
                _context.DisciplinaryIncident.Where(w => facilityId == 0 || w.FacilityId == facilityId);

            //To get DisciplinaryInmateAppeal details
            IQueryable<DisciplinaryInmateAppeal> lstDisciplinaryInmateAppeal = _context.DisciplinaryInmateAppeal;

            #region Appealable Incidents Count

            //To get DisciplinaryInmate details for Appealable Incidents
            IQueryable<DisciplinaryInmate> lstDispInmateAppealable =
                _context.DisciplinaryInmate.Where(w => w.AppealDueDate.HasValue
                                                       && w.AppealDueDate >= DateTime.Now.Date
                                                       && w.DisciplinaryInmateBypassHearing != 1);

            //To get DisciplinaryInmateAppeal details for Appealable Incidents
            IQueryable<DisciplinaryInmateAppeal> lstDispInmateAppealAppealable =
                lstDisciplinaryInmateAppeal.Where(w => w.ReviewComplete == 0);

            int incidentsCount = GetAppealsCount(lstDisciplinaryIncident, lstDispInmateAppealable,
                lstDispInmateAppealAppealable);

            appealsCount.Add(new KeyValuePair<string, int>(AppealConstants.APPEALABLEINCIDENTS, incidentsCount));

            #endregion

            #region Active Appeals Count

            //To get DisciplinaryInmateAppeal details for Active Appeals
            IQueryable<DisciplinaryInmateAppeal> lstDispInmateAppealActive = lstDisciplinaryInmateAppeal.Where(
                w => w.ReviewComplete == 0 && w.SendForReview == 0);

            //To get DisciplinaryInmate details for Active Appeals
            int[] dispActiveInmateIds = lstDispInmateAppealActive.Select(s => s.DisciplinaryInmateId).ToArray();

            IQueryable<DisciplinaryInmate> lstDispInmateActive = _context.DisciplinaryInmate.Where(w =>
                dispActiveInmateIds.Contains(w.DisciplinaryInmateId));

            int activeCount =
                GetAppealsCount(lstDisciplinaryIncident, lstDispInmateActive, lstDispInmateAppealActive);

            appealsCount.Add(new KeyValuePair<string, int>(AppealConstants.ACTIVEAPPEALS, activeCount));

            #endregion

            #region Appeals for Review Count

            //To get DisciplinaryInmateAppeal details for Appeals for Review
            IQueryable<DisciplinaryInmateAppeal> lstDispInmateAppealReview = lstDisciplinaryInmateAppeal.Where(
                w => w.ReviewComplete == 0 && w.SendForReview == 1);

            //To get DisciplinaryInmate details for Appeals for Review
            int[] dispReviewInmateIds = lstDispInmateAppealReview.Select(s => s.DisciplinaryInmateId).ToArray();

            IQueryable<DisciplinaryInmate> lstDispInmateReview = _context.DisciplinaryInmate.Where(w =>
                dispReviewInmateIds.Contains(w.DisciplinaryInmateId));

            int reviewCount =
                GetAppealsCount(lstDisciplinaryIncident, lstDispInmateReview, lstDispInmateAppealReview);

            appealsCount.Add(new KeyValuePair<string, int>(AppealConstants.APPEALSFORREVIEW, reviewCount));

            #endregion

            #region Appeal History Count

            //To get DisciplinaryInmate details for Appeal History
            int[] dispHistoryInmateIds = lstDisciplinaryInmateAppeal.Select(s => s.DisciplinaryInmateId).ToArray();

            IQueryable<DisciplinaryInmate> lstDispInmateHistory = _context.DisciplinaryInmate.Where(w =>
                dispHistoryInmateIds.Contains(w.DisciplinaryInmateId));

            int historyCount =
                GetAppealsCount(lstDisciplinaryIncident, lstDispInmateHistory, lstDisciplinaryInmateAppeal, true);

            appealsCount.Add(new KeyValuePair<string, int>(AppealConstants.APPEALHISTORY, historyCount));

            #endregion

            return appealsCount;
        }

        private int GetAppealsCount(IQueryable<DisciplinaryIncident> lstDisciplinaryIncident,
            IQueryable<DisciplinaryInmate> lstDisciplinaryInmate,
            IQueryable<DisciplinaryInmateAppeal> lstDisciplinaryInmateAppeal, bool appealHistory = false)
        {
            List<DisciplinaryInmate> lstIncidentAndInmate = lstDisciplinaryIncident.SelectMany(dinc =>
                    lstDisciplinaryInmate.Where(dinm => dinm.DisciplinaryIncidentId == dinc.DisciplinaryIncidentId)).ToList();

            int count = lstIncidentAndInmate.SelectMany(incinm =>
                lstDisciplinaryInmateAppeal.Where(dia => dia.DisciplinaryInmateId == incinm.DisciplinaryInmateId)
                    .DefaultIfEmpty()).Count();

            return appealHistory && count > 100 ? 100 : count;
        }

        public List<Appeals> GetIncidentAppeals(AppealsParam objAppealsParam)
        {
            //To get DisciplinaryIncident details by Facility Id
            IQueryable<DisciplinaryIncident> lstDisciplinaryIncident =
                _context.DisciplinaryIncident.Where(w =>
                    (!objAppealsParam.FacilityId.HasValue || w.FacilityId == objAppealsParam.FacilityId)
                    && (objAppealsParam.DateSelection != DateSelection.ByDateRange || !objAppealsParam.DateFrom.HasValue || 
                        !objAppealsParam.DateTo.HasValue || !w.DisciplinaryIncidentDate.HasValue ||
                         (w.DisciplinaryIncidentDate.Value.Date >= objAppealsParam.DateFrom.Value.Date &&
                          w.DisciplinaryIncidentDate.Value.Date <= objAppealsParam.DateTo.Value.Date))
                    && (string.IsNullOrEmpty(objAppealsParam.IncidentNumber) ||
                        w.DisciplinaryNumber.Contains(objAppealsParam.IncidentNumber)));

            //To get DisciplinaryInmateAppeal details
            IQueryable<DisciplinaryInmateAppeal> lstDisciplinaryInmateAppeal = _context.DisciplinaryInmateAppeal.Where(
                w => (objAppealsParam.AppealType == AppealType.AppealHistory ||
                      w.ReviewComplete == 0 && (objAppealsParam.AppealType == AppealType.ActiveAppeals
                          ? w.SendForReview == 0 : objAppealsParam.AppealType != AppealType.AppealsForReview || w.SendForReview == 1))
                     && (!objAppealsParam.OfficerId.HasValue || w.ReportedBy == objAppealsParam.OfficerId ||
                         w.CreateBy == objAppealsParam.OfficerId || w.ReviewBy == objAppealsParam.OfficerId)
                     && (!objAppealsParam.AppealReason.HasValue || w.AppealReason == objAppealsParam.AppealReason)
                     && (!objAppealsParam.AppealDisposition.HasValue || w.ReviewDispo == objAppealsParam.AppealDisposition));

            //To get DisciplinaryInmate details
            IQueryable<DisciplinaryInmate> lstDisciplinaryInmate;

            if (objAppealsParam.AppealType == AppealType.AppealableIncidents)
            {
                lstDisciplinaryInmate = _context.DisciplinaryInmate.Where(w => w.AppealDueDate.HasValue
                                           && w.AppealDueDate >= DateTime.Now.Date && w.DisciplinaryInmateBypassHearing != 1
                                           && (!objAppealsParam.InmateId.HasValue || w.InmateId == objAppealsParam.InmateId));
            }
            else
            {
                int[] disciplinaryInmateIds = lstDisciplinaryInmateAppeal.Select(s => s.DisciplinaryInmateId).ToArray();

                lstDisciplinaryInmate = _context.DisciplinaryInmate.Where(w =>
                    disciplinaryInmateIds.Contains(w.DisciplinaryInmateId)
                    && (!objAppealsParam.InmateId.HasValue || w.InmateId == objAppealsParam.InmateId));
            }

            //To get Incident Appeals list
            List<Appeals> lstAppeals = GetAppeals(objAppealsParam, lstDisciplinaryIncident, lstDisciplinaryInmate,
                lstDisciplinaryInmateAppeal);

            //Applying Order by for AppealHistory
            if (objAppealsParam.AppealType == AppealType.AppealHistory)
            {
                lstAppeals = lstAppeals.OrderByDescending(o => o.AppealDateTime).ToList();
            }
            
            //Taking last 100 records
            lstAppeals = objAppealsParam.DateSelection == DateSelection.Last100
                ? lstAppeals.Take(100).ToList() : lstAppeals;

            //To get Inmate and Person details
            int[] inmateIds = lstAppeals.Select(s => s.InmateId ?? 0).ToArray();

            List<PersonInfoVm> lstPersonInfo = GetPersonInfo(inmateIds);

            //To get Personnel details
            int[] reviewByIds = null;

            int[] reportedByIds = lstAppeals.Select(s => s.ReportedBy).ToArray();

            //To get Reviewer details for Appeal History
            if (objAppealsParam.AppealType == AppealType.AppealHistory)
            {
                reviewByIds = lstAppeals.Select(s => s.ReviewBy ?? 0).ToArray();
            }

            List<PersonDetailVM> lstPersonnelDetails = GetPersonnelDetails(reviewByIds, reportedByIds);


            return AssignPersonAndPersonnelDetails(objAppealsParam.AppealType, lstAppeals,
                lstPersonInfo, lstPersonnelDetails);
        }

        private List<Appeals> GetAppeals(AppealsParam objAppealsParam,
            IQueryable<DisciplinaryIncident> lstDisciplinaryIncident,
            IQueryable<DisciplinaryInmate> lstDisciplinaryInmate,
            IQueryable<DisciplinaryInmateAppeal> lstDisciplinaryInmateAppeal)
        {
            //Join DisciplinaryIncident details with DisciplinaryInmate details
            List<Appeals> lstIncidentAndInmate = lstDisciplinaryIncident.SelectMany(dinc =>
                    lstDisciplinaryInmate.Where(dinm => dinm.DisciplinaryIncidentId == dinc.DisciplinaryIncidentId),
                (sdinc, sdinm) => new Appeals
                {
                    IncidentId = sdinc.DisciplinaryIncidentId,
                    DisciplinaryActive = sdinc.DisciplinaryActive,
                    DisciplinaryInmateId = sdinm.DisciplinaryInmateId,
                    InmateId = sdinm.InmateId,
                    IncidentNumber = sdinc.DisciplinaryNumber,
                    IncidentDateTime = sdinc.DisciplinaryIncidentDate,
                    IncidentType = (objAppealsParam.AppealType == AppealType.AppealHistory)
                        ? _context.Lookup.SingleOrDefault(w => w.LookupInactive == 0 && w.LookupType == LookupConstants.DISCTYPE
                                                  && w.LookupIndex == sdinc.DisciplinaryType).LookupDescription : null,
                                                  DisciplinaryAppealRoute=sdinm.DisciplinaryAppealRoute
                }).ToList();

            List<LookupVm> lookups = _commonService.GetLookups(new[]
                {LookupConstants.INCAPPEALREAS, LookupConstants.INCAPPEALDISPO});

            //Join DisciplinaryIncident and DisciplinaryInmate details with DisciplinaryInmateAppeal details
            List<Appeals> lstAppeals = lstIncidentAndInmate.SelectMany(incinm =>
                    lstDisciplinaryInmateAppeal.Where(dia => dia.DisciplinaryInmateId == incinm.DisciplinaryInmateId).DefaultIfEmpty(),
                (sincinm, sdia) => new Appeals
                {
                    DisciplinaryInmateAppealId = sdia?.DisciplinaryInmateAppealId,
                    IncidentId = sincinm.IncidentId,
                    DisciplinaryActive = sincinm.DisciplinaryActive,
                    DisciplinaryInmateId = sincinm.DisciplinaryInmateId,
                    AppealDateTime = sdia?.AppealDate,
                    AppealReasonId = sdia?.AppealReason,
                    AppealReason = lookups.Where(a => a.LookupIndex == sdia?.AppealReason
                                                      && a.LookupType == LookupConstants.INCAPPEALREAS)
                                          .Select(a => a.LookupDescription).FirstOrDefault(),
                    AppealNote = sdia?.AppealNote,
                    CreateDate = sdia?.CreateDate,
                    InmateId = sincinm.InmateId,
                    SendForReview = sdia?.SendForReview,
                    ReviewComplete = sdia?.ReviewComplete,
                    IncidentNumber = sincinm.IncidentNumber,
                    IncidentDateTime = sincinm.IncidentDateTime,
                    DispoDateTime = sdia?.ReviewDate,
                    ReviewDispoId = sdia?.ReviewDispo,
                    ReviewDispo = lookups.Where(a => a.LookupIndex == sdia?.ReviewDispo
                                                     && a.LookupType == LookupConstants.INCAPPEALDISPO)
                                         .Select(a => a.LookupDescription).FirstOrDefault(),
                    ReviewNote = sdia?.ReviewNote,
                    ReviewInmateResponse = sdia?.ReviewInmateResponse,
                    ReviewDiscDaysPrior = sdia?.ReviewDiscDaysPrior,
                    ReviewDiscDaysNew = sdia?.ReviewDiscDaysNew,
                    ReportedBy = sdia?.ReportedBy ?? 0,
                    ReviewBy = objAppealsParam.AppealType == AppealType.AppealHistory ? sdia?.ReviewBy : null,
                    IncidentType = sincinm.IncidentType,
                    DisciplinaryAppealRoute=sincinm.DisciplinaryAppealRoute
                }).ToList();

            return lstAppeals;
        }

        private List<PersonInfoVm> GetPersonInfo(int[] inmateIds)
        {
            List<PersonInfoVm> lstPersonInfo = _context.Inmate.Where(w => inmateIds.Contains(w.InmateId))
                .Select(s => new PersonInfoVm
                {
                    InmateId = s.InmateId,
                    PersonId = s.PersonId,
                    PersonLastName = s.Person.PersonLastName,
                    PersonFirstName = s.Person.PersonFirstName,
                    PersonMiddleName = s.Person.PersonMiddleName,
                    InmateNumber = s.InmateNumber
                }).ToList();

            return lstPersonInfo;
        }

        private List<PersonDetailVM> GetPersonnelDetails(int[] reviewByIds, int[] reportedByIds) =>
            _context.Personnel.Where(w => reviewByIds == null || reviewByIds.Contains(w.PersonnelId)
                || reportedByIds.Contains(w.PersonnelId))
                .Select(s => new PersonDetailVM
                {
                    PersonnelId = s.PersonnelId,
                    LastName = s.PersonNavigation.PersonLastName,
                    FirstName = s.PersonNavigation.PersonFirstName,
                    OfficerBadgeNumber = s.OfficerBadgeNum,
                }).ToList();

        private List<Appeals> AssignPersonAndPersonnelDetails(AppealType appealType, List<Appeals> lstAppeals,
            List<PersonInfoVm> lstPersonInfo, List<PersonDetailVM> lstPersonnelDetails)
        {
            lstAppeals.ForEach(item =>
            {
                if (item.InmateId > 0)
                {
                    //Assigning Inmate and Person details
                    PersonInfoVm personInfo = lstPersonInfo.Single(s => s.InmateId == item.InmateId);

                    item.PersonInfo = new PersonInfoVm
                    {
                        InmateId = personInfo.InmateId,
                        PersonId = personInfo.PersonId,
                        PersonLastName = personInfo.PersonLastName,
                        PersonFirstName = personInfo.PersonFirstName,
                        PersonMiddleName = personInfo.PersonMiddleName,
                        InmateNumber = personInfo.InmateNumber
                    };
                }

                //Assigning Reported By Personnel details
                PersonDetailVM reportedByDetails =
                    lstPersonnelDetails.SingleOrDefault(s => s.PersonnelId == item.ReportedBy);

                if (reportedByDetails != null)
                {
                    item.ReportedByLastName = reportedByDetails.LastName;
                    item.ReportedByFirstName = reportedByDetails.FirstName;
                    item.ReportedByBadgeNumber = reportedByDetails.OfficerBadgeNumber;
                }

                //Assigning Reviewer details if AppealType is AppealHistory
                if (appealType != AppealType.AppealHistory) return;
                PersonDetailVM reviewerDetails =
                    lstPersonnelDetails.SingleOrDefault(s => s.PersonnelId == item.ReviewBy);

                if (reviewerDetails == null) return;
                item.ReviewerLastName = reviewerDetails.LastName;
                item.ReviewerFirstName = reviewerDetails.FirstName;
                item.ReviewerBadgeNumber = reviewerDetails.OfficerBadgeNumber;
            });

            if (appealType != AppealType.AppealHistory)
            {
                lstAppeals = lstAppeals.OrderBy(o => appealType == AppealType.AppealsForReview
                            ? o.AppealDateTime : o.IncidentDateTime).ThenBy(t => t.IncidentNumber)
                    .ThenBy(l => l.PersonInfo?.PersonLastName).ThenBy(f => f.PersonInfo?.PersonFirstName).ToList();
            }

            return lstAppeals;
        }

        public async Task<int> InsertDisciplinaryInmateAppeal(DispInmateAppeal inmateAppeal)
        {
            DisciplinaryInmateAppeal dispInmateAppeal = new DisciplinaryInmateAppeal
            {
                AppealDate = inmateAppeal.AppealDate,
                AppealReason = inmateAppeal.AppealReason,
                AppealNote = inmateAppeal.AppealNote,
                ReportedBy = inmateAppeal.ReportedBy,
                SendForReview = inmateAppeal.SendForReview,
                DisciplinaryInmateId = inmateAppeal.DisciplinaryInmateId,
                CreateBy = _personnelId,
                CreateDate = DateTime.Now  
            };

            _context.DisciplinaryInmateAppeal.Add(dispInmateAppeal);
               DisciplinaryInmate disciplinaryInmate=_context.DisciplinaryInmate
            .Find(inmateAppeal.DisciplinaryInmateId);            
             disciplinaryInmate.DisciplinaryAppealRoute = inmateAppeal.DisciplinaryAppealRoute;

            return await _context.SaveChangesAsync();
        }

        public async Task<int> UpdateDisciplinaryInmateAppeal(int dispInmateAppealId, DispInmateAppeal inmateAppeal)
        {
            DisciplinaryInmateAppeal dispInmateAppeal = _context.DisciplinaryInmateAppeal.Single(w =>
                w.DisciplinaryInmateAppealId == dispInmateAppealId);
            if (dispInmateAppeal.SendForReview == 1)
            {
                dispInmateAppeal.ReviewBy = _personnelId;
                dispInmateAppeal.ReviewDate = DateTime.Now;
            }
            dispInmateAppeal.AppealDate = inmateAppeal.AppealDate;
            dispInmateAppeal.AppealReason = inmateAppeal.AppealReason;
            dispInmateAppeal.AppealNote = inmateAppeal.AppealNote;
            dispInmateAppeal.ReportedBy = inmateAppeal.ReportedBy;
            dispInmateAppeal.SendForReview = inmateAppeal.SendForReview;
            dispInmateAppeal.ReviewComplete = inmateAppeal.ReviewComplete;
            dispInmateAppeal.ReviewDiscDaysPrior = inmateAppeal.ReviewDiscDaysPrior;
            dispInmateAppeal.ReviewDiscDaysNew = inmateAppeal.ReviewDiscDaysNew;
            dispInmateAppeal.ReviewDispo = inmateAppeal.ReviewDispo;
            dispInmateAppeal.ReviewNote = inmateAppeal.ReviewNote;
            dispInmateAppeal.ReviewInmateResponse = inmateAppeal.ReviewInmateResponse;
            dispInmateAppeal.UpdateBy = _personnelId;
            dispInmateAppeal.UpdateDate = DateTime.Now;
            DisciplinaryInmate disciplinaryInmate=_context.DisciplinaryInmate
            .Find(dispInmateAppeal.DisciplinaryInmateId);            
             disciplinaryInmate.DisciplinaryAppealRoute = inmateAppeal.DisciplinaryAppealRoute;
            return await _context.SaveChangesAsync();
        }

		public int GetAppealsCountByIncidentId(int incidentId) => _context.DisciplinaryInmateAppeal
				   .Count(d => (d.DisciplinaryInmate.DisciplinaryIncident.DisciplinaryActive == 0 ||
								 !d.DisciplinaryInmate.DisciplinaryIncident.DisciplinaryActive.HasValue)
								&& d.DisciplinaryInmate.DisciplinaryIncidentId == incidentId);
	}
}