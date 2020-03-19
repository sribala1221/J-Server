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
    public class GrievanceAppealsService : IGrievanceAppealsService
    {
        private readonly AAtims _context;
        private readonly int _personnelId;
        private readonly ICommonService _commonService;
        private readonly IGrievanceService _grievanceService;
        private readonly IPhotosService _photoService;
        private readonly IAppletsSavedService _appletsSavedService;

        public GrievanceAppealsService(AAtims context, ICommonService commonService,
            IGrievanceService grievanceService, IPhotosService photoService,
            IHttpContextAccessor httpContextAccessor, IAppletsSavedService appletsSavedService)
        {
            _context = context;
            _commonService = commonService;
            _grievanceService = grievanceService;
            _photoService = photoService;
            _personnelId = Convert.ToInt32(httpContextAccessor.HttpContext.User
                .FindFirst(AuthDetailConstants.PERSONNELID)?.Value);
            _appletsSavedService = appletsSavedService;
        }

        public GrievanceAppealsVm GetGrievanceAppealDetails(int facilityId, bool showReviewed, bool showDeleted)
        {
            GrievanceAppealsVm grievanceAppeals = new GrievanceAppealsVm
            {
                GrievanceAppealList = GetGrievanceAppeals(facilityId, showReviewed, showDeleted)
            };
            grievanceAppeals.ElapsedCount = GetElapsedCounts(grievanceAppeals
                .GrievanceAppealList.Where(a => a.CreateDate.HasValue).ToList());
            grievanceAppeals.CategoryCount = grievanceAppeals.GrievanceAppealList
                .Where(a=>!string.IsNullOrEmpty(a.Category)).GroupBy(a => a.Category)
                .Select(a => new KeyValuePair<string, int>(a.Key, a.Count())).ToList();
            grievanceAppeals.TypeCount = grievanceAppeals.GrievanceAppealList
                .Where(a => !string.IsNullOrEmpty(a.Type)).GroupBy(a => a.Type)
                .Select(a => new KeyValuePair<string, int>(a.Key, a.Count())).ToList();
            grievanceAppeals.LocationCount = grievanceAppeals.GrievanceAppealList
                .Where(a => !string.IsNullOrEmpty(a.Location)).GroupBy(a => a.Location)
                .Select(a => new KeyValuePair<string, int>(a.Key, a.Count())).ToList();
            return grievanceAppeals;
        }

        private List<GrievanceAppealDetails> GetGrievanceAppeals(int facilityId, bool showReviewed, bool showDeleted)
        {
            List<LookupVm> lookups =
                   _commonService.GetLookups(new[] { LookupConstants.GRVAPPEALTYPE, LookupConstants.GRIEVTYPE });

            List<GrievanceAppealDetails> grievanceAppealDetails = _context.GrievanceAppeal
                .Where(w => w.Grievance.FacilityId == facilityId && (showReviewed ||
                !w.ReviewDate.HasValue) && (showDeleted || !w.DeleteFlag.HasValue ||
                w.DeleteFlag == 0)).Select(s => new GrievanceAppealDetails
                {
                    CreateDate = s.CreateDate,
                    AppealDate = s.AppealDate,
                    Location = s.Grievance.GrievanceLocation,
                    AppealCategoryLookup = s.AppealCategoryLookup,
                    Category = lookups.Where(a => a.LookupIndex == s.AppealCategoryLookup
                                                  && a.LookupType == LookupConstants.GRVAPPEALTYPE)
                          .Select(a => a.LookupDescription).FirstOrDefault(),
                    AppealNote = s.AppealNote,
                    PersonInfo = new PersonInfoVm
                    {
                        PersonId = s.Grievance.Inmate.PersonId,
                        PersonLastName = s.Grievance.Inmate.Person.PersonLastName,
                        PersonFirstName = s.Grievance.Inmate.Person.PersonFirstName,
                        PersonMiddleName = s.Grievance.Inmate.Person.PersonMiddleName,
                        PersonSuffix = s.Grievance.Inmate.Person.PersonSuffix,
                        InmateId = s.Grievance.InmateId,
                        InmateNumber = s.Grievance.Inmate.InmateNumber
                    },
                    GrievanceNumber = s.Grievance.GrievanceNumber,
                    ReviewDate = s.ReviewDate,
                    GrievanceId = s.GrievanceId,
                    GrievanceCreateDate = s.Grievance.CreateDate,
                    GrievanceType = s.Grievance.GrievanceType,
                    Type = lookups.Where(a => a.LookupIndex == s.Grievance.GrievanceType
                                              && a.LookupType == LookupConstants.GRIEVTYPE)
                          .Select(a => a.LookupDescription).FirstOrDefault(),
                    AppealByLastName = s.AppealByNavigation.PersonNavigation.PersonLastName,
                    AppealByBadgeNumber = s.AppealByNavigation.OfficerBadgeNum,
                    GrievanceAppealId = s.GrievanceAppealId,
                    DeleteFlag = s.DeleteFlag,
                    GrievanceDisposition = s.Grievance.GrievanceDispositionLookup,
                    DateOccured = s.Grievance.DateOccured,
                    GrievanceSummary = s.Grievance.GrievanceSummary,
                    AppealCount = s.Grievance.GrievanceAppeal.Count(),
                    OverrideReason = s.OverrideReason,
                    SensitiveMaterial = s.Grievance.SensitiveMaterial
                }).ToList();
            return grievanceAppealDetails;
        }

        private List<GrievanceAppealDetails> GetAppealCount(int? facilityId, List<GrievanceAppealDetails> lstDetails)
        {
            int[] grievanceIds = lstDetails.Select(s => s.GrievanceId).ToArray();

            List<KeyValuePair<int, int>> appealCounts = _context.GrievanceAppeal
                .Where(w => grievanceIds.Contains(w.GrievanceId) && (!facilityId.HasValue || w.Grievance.FacilityId == facilityId))
                .GroupBy(g => g.GrievanceId)
                .Select(s => new KeyValuePair<int, int>(s.Key, s.Count())).ToList();

            lstDetails.ForEach(item =>
            {
                item.AppealCount = appealCounts.SingleOrDefault(s => s.Key == item.GrievanceId).Value;
            });

            return lstDetails;
        }

        public List<GrievanceAppealDetails> GetGrievanceDetails(int? grievanceId)
        {
            List<GrievanceAppealDetails> lstGrievanceDetails = _context.Grievance.Where(w =>
                    w.DeleteFlag == 0 && (grievanceId.HasValue ? w.GrievanceId == grievanceId : w.SetReview == 1))
                .OrderByDescending(o => o.DateOccured)
                .Select(s => new GrievanceAppealDetails
                {
                    GrievanceId = s.GrievanceId,
                    PersonInfo = new PersonInfoVm
                    {
                        InmateId = s.InmateId,
                        PersonLastName = s.Inmate.Person.PersonLastName,
                        PersonFirstName = s.Inmate.Person.PersonFirstName,
                        InmateNumber = s.Inmate.InmateNumber
                    },
                    DateOccured = s.DateOccured,
                    GrievanceNumber = s.GrievanceNumber,
                    Type = _context.Lookup.Where(l => l.LookupIndex == s.GrievanceType && l.LookupInactive == 0
                                                                                       && l.LookupType == LookupConstants.GRIEVTYPE)
                        .Select(a => a.LookupDescription).FirstOrDefault(),
                    AppealBy = s.AppealBy,
                    AppealByLastName = s.AppealByNavigation.PersonNavigation.PersonLastName,
                    AppealByBadgeNumber = s.AppealByNavigation.OfficerBadgeNum,
                    AppealDate = s.AppealDate
                }).ToList();

            return grievanceId.HasValue ? lstGrievanceDetails : GetAppealCount(null, lstGrievanceDetails);
        }

        public async Task<int> InsertGrievanceAppeal(GrievanceAppealParam grivAppealParam)
        {
            GrievanceAppeal grievanceAppeal = new GrievanceAppeal
            {
                GrievanceId = grivAppealParam.GrievanceId,
                AppealCategoryLookup = grivAppealParam.AppealCategory,
                AppealNote = grivAppealParam.AppealNote,
                CreatedBy = _personnelId,
                CreateDate = DateTime.Now,
                AppealBy = _personnelId,
                AppealDate = DateTime.Now,
                OverrideReason = grivAppealParam.OverrideReason
            };

            _context.GrievanceAppeal.Add(grievanceAppeal);

            return await _context.SaveChangesAsync();
        }

        public async Task<int> UpdateGrievanceAppeal(int grievanceAppealId, GrievanceAppealParam grivAppealParam)
        {
            GrievanceAppeal grievanceAppeal =
                _context.GrievanceAppeal.Single(w => w.GrievanceAppealId == grievanceAppealId);

            grievanceAppeal.AppealNote = grivAppealParam.AppealNote;
            grievanceAppeal.ReviewNote = grivAppealParam.ReviewNote;
            grievanceAppeal.AppealCategoryLookup = grivAppealParam.AppealCategory;
            grievanceAppeal.InmateResponseNote = grivAppealParam.InmateResponse;
            grievanceAppeal.AppealDisposition = grivAppealParam.Disposition;
            grievanceAppeal.UpdateBy = _personnelId;
            grievanceAppeal.UpdateDate = DateTime.Now;
            grievanceAppeal.OverrideReason = grivAppealParam.OverrideReason;
            if (!string.IsNullOrEmpty(grivAppealParam.Disposition))
            {
                grievanceAppeal.ReviewBy = _personnelId;
                grievanceAppeal.ReviewDate = DateTime.Now;
            }

            return await _context.SaveChangesAsync();
        }

        public async Task<int> DeleteGrievanceAppeal(int grievanceAppealId, bool deleteFlag)
        {
            GrievanceAppeal grievanceAppeal =
                _context.GrievanceAppeal.Single(w => w.GrievanceAppealId == grievanceAppealId);

            if (deleteFlag)
            {
                // Delete Grievance Appeal
                grievanceAppeal.DeleteFlag = 1;
                grievanceAppeal.DeleteBy = _personnelId;
                grievanceAppeal.DeleteDate = DateTime.Now;
            }
            else
            {
                // Undo Delete Grievance Appeal
                grievanceAppeal.DeleteFlag = null;
                grievanceAppeal.DeleteBy = null;
                grievanceAppeal.DeleteDate = null;
            }

            return await _context.SaveChangesAsync();
        }

        public GrievanceAppealDetails GetAppealDetailsForPdf(int grievanceId)
        {
            GrievanceAppealDetails grvAppealDetails = _context.GrievanceAppeal.Where(w => w.GrievanceId == grievanceId)
                .Select(s => new GrievanceAppealDetails
                {
                    AppealNote = s.AppealNote,
                    AppealDate = s.AppealDate,
                    Category = _context.Lookup.SingleOrDefault(w =>
                        w.LookupInactive == 0 && w.LookupType == LookupConstants.GRVAPPEALTYPE
                                              && w.LookupIndex == s.AppealCategoryLookup).LookupDescription,
                    ReviewDate = s.ReviewDate,
                    InmateResponseNote = s.InmateResponseNote,
                    ReviewByLastName = s.ReviewByNavigation.PersonNavigation.PersonLastName,
                    ReviewByBadgeNumber = s.ReviewByNavigation.OfficerBadgeNum,
                    ReviewNote = s.ReviewNote,
                    CreateDate = s.CreateDate,
                    AppealByLastName = s.AppealByNavigation.PersonNavigation.PersonLastName,
                    AppealByBadgeNumber = s.AppealByNavigation.OfficerBadgeNum
                }).Single();

            return grvAppealDetails;
        }

        private List<KeyValuePair<string, int>> GetElapsedCounts(List<GrievanceAppealDetails> appealDetails)
        {
            return new List<KeyValuePair<string, int>>() {
                new KeyValuePair<string, int>("73 + Hours", appealDetails
                .Count(a=>DateTime.Now.Subtract(a.CreateDate.Value).TotalHours>=73)),
                new KeyValuePair<string, int>("49 - 72 Hours",appealDetails
                .Count(a=>DateTime.Now.Subtract(a.CreateDate.Value).TotalHours>=49 &&
                DateTime.Now.Subtract(a.CreateDate.Value).TotalHours<73)),
                new KeyValuePair<string, int>("25 - 48 Hours",appealDetails
                .Count(a=>DateTime.Now.Subtract(a.CreateDate.Value).TotalHours>=25 &&
                DateTime.Now.Subtract(a.CreateDate.Value).TotalHours<49)),
                new KeyValuePair<string, int>("00 - 24 Hours",appealDetails
                .Count(a=>DateTime.Now.Subtract(a.CreateDate.Value).TotalHours<25))
            };
        }

        public List<GrievanceAppealSearch> SearchGrievances(string grievanceNumber, int inmateId)
        {
            List<Lookup> lookups = _context.Lookup.Where(a => a.LookupType == LookupConstants.GRIEVTYPE).ToList();
            List<GrievanceAppealSearch> grievances = _context.Grievance
                .Where(g => g.SetReview == 1 && g.DeleteFlag == 0 && (string.IsNullOrEmpty(grievanceNumber) ||
                g.GrievanceNumber.Contains(grievanceNumber)) && (inmateId <= 0 || g.InmateId == inmateId))
                .OrderByDescending(g => g.DateOccured)
                .Select(g => new GrievanceAppealSearch
                {
                    GrievanceId = g.GrievanceId,
                    InmateId = g.InmateId,
                    GrievanceNumber = g.GrievanceNumber,
                    DateOccured = g.DateOccured,
                    Type = lookups.Where(l => l.LookupIndex == g.GrievanceType)
                    .Select(l => l.LookupDescription).SingleOrDefault(),
                    ReportingInmate = new PersonInfo
                    {
                        PersonLastName = g.Inmate.Person.PersonLastName,
                        PersonFirstName = g.Inmate.Person.PersonFirstName,
                        InmateNumber = g.Inmate.InmateNumber
                    },
                    AppealCount = g.GrievanceAppeal.Count(),
                    AppealBy = g.AppealBy,
                    AppealPersonnel = new PersonnelVm
                    {
                        PersonLastName = g.AppealByNavigation.PersonNavigation.PersonLastName,
                        OfficerBadgeNumber = g.AppealByNavigation.OfficerBadgeNumber
                    },
                    AppealDate = g.AppealDate
                }).ToList();
            return grievances;
        }

        public bool CheckReviewFlagAndClear(int grievanceId, int facilityId, int grievanceAppealId)
        {
            return _context.GrievanceAppeal.Count(a => (grievanceAppealId <= 0 ||
            a.GrievanceAppealId != grievanceAppealId) && a.GrievanceId == grievanceId &&
            !a.ReviewDate.HasValue && (!a.DeleteFlag.HasValue || a.DeleteFlag == 0) &&
            a.Grievance.FacilityId == facilityId) > 0;
        }

        public GrievanceAppealsReport GetGrievanceAppealsReport(int grievanceId)
        {
            GrievanceAppealsReport grievanceAppealsReport = new GrievanceAppealsReport
            {
                Grievance = _grievanceService.GetGrievance(grievanceId),
                GrievanceAppeal = _context.GrievanceAppeal.Where(a => a.GrievanceId == grievanceId)
                .Select(a => new GrievanceAppealDetails
                {
                    GrievanceAppealId = a.GrievanceAppealId,
                    AppealDate = a.AppealDate,
                    AppealCategoryLookup = a.AppealCategoryLookup,
                    Category = _context.Lookup.SingleOrDefault(w =>
                          w.LookupInactive == 0 && w.LookupType == LookupConstants.GRVAPPEALTYPE
                                                && w.LookupIndex == a.AppealCategoryLookup)
                                               .LookupDescription,
                    ReviewDate = a.ReviewDate,
                    InmateResponseNote = a.InmateResponseNote,
                    ReviewByLastName = a.ReviewByNavigation.PersonNavigation.PersonLastName,
                    ReviewByBadgeNumber = a.ReviewByNavigation.OfficerBadgeNum,
                    ReviewNote = a.ReviewNote,
                    CreateDate = a.CreateDate,
                    AppealByLastName = a.AppealByNavigation.PersonNavigation.PersonLastName,
                    AppealByBadgeNumber = a.AppealByNavigation.OfficerBadgeNum
                }).FirstOrDefault(),
                GrievanceForms = _grievanceService.GetGrievanceForms(grievanceId),
                AttachmentDetails = _context.AppletsSaved.Where(a => a.GrievanceId == grievanceId)
                    .Select(a => new AttachmentDetails
                    {
                        CreateDate = a.CreateDate,
                        Title = a.AppletsSavedTitle,
                        SavedId = a.AppletsSavedId,
                        DeleteFlag = a.AppletsDeleteFlag,
                        GrievanceId = a.GrievanceId,
                        FilePath = a.AppletsSavedIsExternal ? _appletsSavedService.GetExternalPath() + a.AppletsSavedPath : a.AppletsSavedPath,
                        Type = a.AppletsSavedType
                    }).ToList(),
                AgencyName = _context.Agency.Where(a => a.AgencyJailFlag).FirstOrDefault()?.AgencyName,
                GrievanceFlags = _context.GrievanceFlag
                 .Where(g => g.GrievanceId == grievanceId && g.DeleteFlag != 1)
                 .Select(g => g.GrievanceFlagText).ToArray()
            };
            grievanceAppealsReport.PhotoFilePath = _photoService
                .GetPhotoByPersonId(grievanceAppealsReport.Grievance.PersonId);
            grievanceAppealsReport.InmateClassifications = _context.InmateClassification
                .Where(a => a.InmateNavigation.PersonId == grievanceAppealsReport
                .Grievance.PersonId && a.InmateNavigation.InmateActive == 1)
                .Select(a => a.InmateClassificationReason).ToArray();
            return grievanceAppealsReport;
        }
    }
}
