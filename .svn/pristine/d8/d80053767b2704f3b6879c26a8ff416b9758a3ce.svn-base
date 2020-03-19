using System;
using System.Collections.Generic;
using System.Linq;
using GenerateTables.Models;
using ServerAPI.ViewModels;
using ServerAPI.Utilities;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using JwtDb.Models;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

// ReSharper disable once CheckNamespace
namespace ServerAPI.Services
{
    public class ClassifyViewerService : IClassifyViewerService
    {
        private readonly AAtims _context;
        private readonly ICommonService _commonService;
        private IQueryable<Inmate> _inmate;
        private readonly ClassLogDetails _classLogDetails = new ClassLogDetails();
        private readonly ClassifyCount _classifyCount = new ClassifyCount();
        private ClassLogInputs _classLogInputs;
        private IQueryable<ClassLog> _lstIncarcerationIntake;
        private IQueryable<ClassLog> _lstIncarcerationRelease;
        private readonly int _personnelId;
        private readonly IPersonService _personService;
        private readonly JwtDbContext _jwtDbContext;
        private readonly UserManager<AppUser> _userManager;
        private readonly IAppletsSavedService _appletsSavedService;

        public ClassifyViewerService(AAtims context, ICommonService commonService,
            IHttpContextAccessor httpContextAccessor, IPersonService personService, JwtDbContext jwtDbContext,
            UserManager<AppUser> userManager, IAppletsSavedService appletsSavedService)
        {
            _context = context;
            _commonService = commonService;
            _jwtDbContext = jwtDbContext;
            _personnelId = Convert.ToInt32(httpContextAccessor.HttpContext.User.FindFirst("personnelId")?.Value);
            _personService = personService;
            _userManager = userManager;
            _appletsSavedService = appletsSavedService;
        }

        public async Task<ClassLogDetails> GetClassifyLog(int facilityId)
        {
            ClassLogInputs inputs = new ClassLogInputs
            {
                LogParameters = await GetClassifySettings(),
                Hours = 12,
                FacilityId = facilityId
            };
            //For Get Classify setting
            _classLogDetails.GetClassifySettings = inputs.LogParameters;
            //For classification Drop Down          
            _classLogDetails.GetClassify = _commonService.GetLookupKeyValuePairs(LookupCategory.CLASREAS);
            return GetClassLog(inputs);
        }

        public ClassLogDetails GetClassLog(ClassLogInputs classLogInputs)
        {
            if (classLogInputs.FacilityId <= 0) return _classLogDetails;
            _classLogInputs = classLogInputs;
            //searching in Classification Viewer Class Log 
            DateTime fromDate;
            DateTime toDate;
            //Searching on Selection of dates
            if (classLogInputs.Hours > 0)
            {
                fromDate = DateTime.Now.AddHours(-classLogInputs.Hours);
                toDate = DateTime.Now;
            }
            else
            {
                fromDate = classLogInputs.Fromdate;
                toDate = classLogInputs.Todate.AddDays(1).AddTicks(-1);
            }
            _inmate = _context.Inmate.Where(i => i.FacilityId == classLogInputs.FacilityId
                                     && (classLogInputs.InmateId == 0 || !classLogInputs.InmateId.HasValue
                                         || i.InmateId == classLogInputs.InmateId)
                                     && (!classLogInputs.Active || i.InmateActive == 1)
                                     && (!classLogInputs.HousingUnitListId.HasValue
                                        || classLogInputs.HousingUnitListId == 0
                                        || (i.HousingUnit.HousingUnitListId == classLogInputs.HousingUnitListId))
                                     && (String.IsNullOrEmpty(classLogInputs.ClassType)
                                         || i.InmateClassification.InmateClassificationReason == classLogInputs.ClassType));
            _classLogDetails.ClassLog = new List<ClassLog>();
            _classLogDetails.ClassifyCount = new ClassifyCount();
            //For Housing Drop Down
            _classLogDetails.GetHousing = GetHousing(classLogInputs.FacilityId);

            if (!_inmate.Any()) return _classLogDetails;

            // To Get Classification Details
            GetClassification(fromDate, toDate);
            if (_inmate.Any())
            {
                // To Get Classification Narrative Details
                GetClassificationNarrative(fromDate, toDate);
                // To Get form Details
                GetForm(fromDate, toDate);
                // To Get attach Details
                GetAttach(fromDate, toDate);
                // To Get Link Details
                GetLink(fromDate, toDate);
                // To get Intake and Release details
                GetIntakeAndRelease(fromDate, toDate);
                // To Get Housing Details
                GetHousing(fromDate, toDate);
                // To get note details
                GetNote(fromDate, toDate);
                // To Get Incident Details
                GetIncident(fromDate, toDate);
                // To Get Message Details
                GetMessage(fromDate, toDate);
                // To get Flag details
                GetFlag(fromDate, toDate);
                // To get Keep separate details
                GetKeepSep(fromDate, toDate);
                // To Get Association Details
                GetAssoc(fromDate, toDate);
                // To Get Privilege Details
                GetPrivilege(fromDate, toDate);
            }
            _classLogDetails.ClassifyCount = _classifyCount;

            return _classLogDetails;
        }

        private List<HousingDetail> GetHousing(int facilityId) =>
           _context.HousingUnit.Where(h => h.FacilityId == facilityId &&
                                           (h.HousingUnitInactive == 0 || h.HousingUnitInactive == null))
               .GroupBy(g => new
               {
                   g.HousingUnitLocation,
                   g.HousingUnitNumber,
                   g.HousingUnitListId
               })
               .Select(h => new HousingDetail
               {
                   HousingUnitLocation = h.Key.HousingUnitLocation,
                   HousingUnitNumber = h.Key.HousingUnitNumber,
                   HousingUnitListId = h.Key.HousingUnitListId
               }).ToList();

        private void GetClassification(DateTime fromDate, DateTime toDate)
        {
            if (_classLogInputs.Flag && (_classLogInputs.LogParameters is null ||
                 !_classLogInputs.LogParameters.Initial && !_classLogInputs.LogParameters.ReClassify)) return;
            //To get InmateClassification Details
            IQueryable<InmateClassification> inmateClassification = _context.InmateClassification.Where(i =>
                i.InmateDateAssigned.HasValue && i.InmateDateAssigned >= fromDate &&
                i.InmateDateAssigned <= toDate && _inmate.Any(c => c.InmateId == i.InmateId)
                  && (String.IsNullOrEmpty(_classLogInputs.ClassType) || i.InmateClassificationReason == _classLogInputs.ClassType)
                && (!_classLogInputs.PersonnelId.HasValue || _classLogInputs.PersonnelId == 0
                                                        || i.ClassificationOfficerId == _classLogInputs.PersonnelId));
            if (!inmateClassification.Any()) return;
            // if (!_classLogInputs.Flag)
            // {
            _classifyCount.Initial = inmateClassification.Count(i =>
                i.InmateClassificationType == ClassTypeConstants.INITIAL);
            _classifyCount.ReClassify = inmateClassification.Count(i =>
                i.InmateClassificationType == ClassTypeConstants.RECLASSIFICATION);
            // }
            if (_classLogInputs.LogParameters is null ||
                !_classLogInputs.LogParameters.Initial && !_classLogInputs.LogParameters.ReClassify) return;

            _classLogDetails.ClassLog = inmateClassification.Where(i =>
                _classLogInputs.LogParameters.Initial && i.InmateClassificationType == ClassTypeConstants.INITIAL ||
                _classLogInputs.LogParameters.ReClassify &&
                i.InmateClassificationType == ClassTypeConstants.RECLASSIFICATION).Select(ic => new ClassLog
                {
                    Id = ic.InmateClassificationId,
                    ClassType = ic.InmateClassificationType,
                    ClassDate = ic.InmateDateAssigned,
                    ClassNarrative = ic.InmateOverrideNarrative,
                    Reason = ic.InmateClassificationReason,
                    OfficerId = ic.ClassificationOfficerId,
                    PersonDetails = new PersonInfo
                    {
                        InmateId = ic.InmateId,
                        PersonId = ic.InmateNavigation.PersonId,
                        PersonFirstName = ic.InmateNavigation.Person.PersonFirstName,
                        PersonMiddleName = ic.InmateNavigation.Person.PersonMiddleName,
                        PersonLastName = ic.InmateNavigation.Person.PersonLastName,
                        InmateNumber = ic.InmateNavigation.InmateNumber,
                        HousingUnitId = ic.InmateNavigation.HousingUnitId ?? 0,
                        LastReviewDate = ic.InmateNavigation.LastClassReviewDate,
                        InmateClassificationId = ic.InmateNavigation.InmateClassificationId
                    },
                    OfficerDetails = new PersonnelVm
                    {
                        PersonFirstName = ic.ClassificationOfficer.PersonNavigation.PersonFirstName,
                        PersonLastName = ic.ClassificationOfficer.PersonNavigation.PersonLastName,
                        PersonMiddleName = ic.ClassificationOfficer.PersonNavigation.PersonMiddleName,
                        OfficerBadgeNumber = ic.ClassificationOfficer.OfficerBadgeNum,
                    }
                }).ToList();
        }

        private void GetClassificationNarrative(DateTime fromDate, DateTime toDate)
        {
            if (_classLogInputs.Flag && (_classLogInputs.LogParameters is null ||
                                         !_classLogInputs.LogParameters.ClassNote &&
                                         !_classLogInputs.LogParameters.Review)) return;
            IQueryable<InmateClassificationNarrative> lstInmateNarrative =
                _context.InmateClassificationNarrative.Where(ic =>
                    ic.CreateDate.HasValue && ic.CreateDate >= fromDate && ic.CreateDate <= toDate &&
                    _inmate.Any(c => c.InmateId == ic.InmateId) && ic.Narrative != null
                      && (!_classLogInputs.PersonnelId.HasValue || _classLogInputs.PersonnelId == 0
                                                              || ic.CreatedBy == _classLogInputs.PersonnelId)
                    && (_classLogInputs.DeleteFlag || !(ic.DeleteFlag.HasValue) || ic.DeleteFlag == 0));
            if (!lstInmateNarrative.Any()) return;
            //to get count of ClassNote and Review
            _classifyCount.ClassNote = lstInmateNarrative.Count(n => n.ReviewFlag == 0);
            _classifyCount.Review = lstInmateNarrative.Count(n => n.ReviewFlag == 1);
            //Review Flag 0 means ClassNote and Flag 1 means Review            
            if (_classLogInputs.LogParameters is null ||
                !_classLogInputs.LogParameters.ClassNote && !_classLogInputs.LogParameters.Review) return;
            //To get InmateClassificationNarrative Details
            _classLogDetails.ClassLog.AddRange(lstInmateNarrative.Where(ic =>
                    _classLogInputs.LogParameters.ClassNote &&
                    (!ic.ReviewFlag.HasValue || ic.ReviewFlag == 0) ||
                    _classLogInputs.LogParameters.Review &&
                    (!ic.ReviewFlag.HasValue || ic.ReviewFlag == 1))
                .Select(icn => new ClassLog
                {
                    Id = icn.InmateClassificationNarrativeId,
                    OfficerId = icn.CreatedBy,
                    ClassDate = icn.CreateDate,
                    ClassNarrative = icn.Narrative,
                    ClassType = ClassTypeConstants.CLASSNOTE,
                    ReviewFlag = icn.ReviewFlag ?? 0,
                    PersonDetails = new PersonInfo
                    {
                        InmateId = icn.InmateId,
                        PersonId = icn.Inmate.PersonId,
                        PersonFirstName = icn.Inmate.Person.PersonFirstName,
                        PersonMiddleName = icn.Inmate.Person.PersonMiddleName,
                        PersonLastName = icn.Inmate.Person.PersonLastName,
                        InmateNumber = icn.Inmate.InmateNumber,
                        HousingUnitId = icn.Inmate.HousingUnitId ?? 0,
                        LastReviewDate = icn.Inmate.LastClassReviewDate,
                        InmateClassificationId = icn.Inmate.InmateClassificationId
                    },
                    OfficerDetails = new PersonnelVm
                    {
                        PersonFirstName = icn.CreatedByNavigation.PersonNavigation.PersonFirstName,
                        PersonLastName = icn.CreatedByNavigation.PersonNavigation.PersonLastName,
                        PersonMiddleName = icn.CreatedByNavigation.PersonNavigation.PersonMiddleName,
                        OfficerBadgeNumber = icn.CreatedByNavigation.OfficerBadgeNum,
                    }
                }));
        }

        private void GetForm(DateTime fromDate, DateTime toDate)
        {
            if (_classLogInputs.Flag &&
                (_classLogInputs.LogParameters is null || !_classLogInputs.LogParameters.Form)) return;
            //To get FormRecord Details
            IQueryable<FormRecord> lstFormRecord =
                _context.FormRecord.Where(f =>
                    f.CreateDate <= toDate && f.CreateDate >= fromDate &&
                     _inmate.Any(c => c.InmateId == f.ClassificationFormInmateId)
                    && (!_classLogInputs.PersonnelId.HasValue || _classLogInputs.PersonnelId == 0
                                                              || f.CreateBy == _classLogInputs.PersonnelId)
                    && (_classLogInputs.DeleteFlag || f.DeleteFlag == 0));
            // _inmate.Select(a => a.InmateId).Contains(f.ClassificationFormInmateId ?? 0));
            if (!lstFormRecord.Any()) return;
            //to get count of Form details
            _classifyCount.Form = lstFormRecord.Count();
            //To get Form Details           
            if (_classLogInputs.LogParameters is null || !_classLogInputs.LogParameters.Form) return;

            _classLogDetails.ClassLog.AddRange(lstFormRecord
                .SelectMany(f => _inmate.Where(w => w.InmateId == f.ClassificationFormInmateId),
                    (f, a) => new ClassLog
                    {
                        Id = f.FormRecordId,
                        ClassDate = f.CreateDate,
                        ClassType = ClassTypeConstants.FORM,
                        ClassNarrative = f.FormNotes,
                        OfficerId = f.CreateBy,
                        DeleteFlag = f.DeleteFlag == 1,
                        PersonDetails = new PersonInfo
                        {
                            InmateId = a.InmateId,
                            PersonId = a.PersonId,
                            PersonFirstName = a.Person.PersonFirstName,
                            PersonMiddleName = a.Person.PersonMiddleName,
                            PersonLastName = a.Person.PersonLastName,
                            InmateNumber = a.InmateNumber,
                            HousingUnitId = a.HousingUnitId ?? 0,
                            LastReviewDate = a.LastClassReviewDate,
                            InmateClassificationId = a.InmateClassificationId
                        },
                        OfficerDetails = new PersonnelVm
                        {
                            PersonFirstName = f.CreateByNavigation.PersonNavigation.PersonFirstName,
                            PersonLastName = f.CreateByNavigation.PersonNavigation.PersonLastName,
                            PersonMiddleName = f.CreateByNavigation.PersonNavigation.PersonMiddleName,
                            OfficerBadgeNumber = f.CreateByNavigation.OfficerBadgeNum,
                        }
                    }));
        }

        private void GetAttach(DateTime fromDate, DateTime toDate)
        {
            if (_classLogInputs.Flag &&
                (_classLogInputs.LogParameters is null || !_classLogInputs.LogParameters.Attach)) return;
            //To get AppletsSaved Details
            IQueryable<AppletsSaved> lstAppletsSaved = _context.AppletsSaved.Where(a =>
                a.CreateDate.HasValue && a.CreateDate <= toDate && a.CreateDate >= fromDate &&
                (a.InmateId.HasValue ? _inmate.Any(c => c.InmateId == a.InmateId) : default)
                && (!_classLogInputs.PersonnelId.HasValue || _classLogInputs.PersonnelId == 0
                                                          || a.CreatedBy == _classLogInputs.PersonnelId)
                && (_classLogInputs.DeleteFlag || a.AppletsDeleteFlag == 0));
            // _inmate.Select(l => l.InmateId).Contains(a.InmateId ?? 0));
            if (!lstAppletsSaved.Any()) return;
            //To get count of Attach
            _classifyCount.Attach = lstAppletsSaved.Count();
            //To get Attach Details
            if (_classLogInputs.LogParameters is null || !_classLogInputs.LogParameters.Attach) return;

            _classLogDetails.ClassLog.AddRange(lstAppletsSaved.Select(a => new ClassLog
            {
                Id = a.AppletsSavedId,
                ClassDate = a.CreateDate,
                ClassType = ClassTypeConstants.ATTACH,
                Type = a.AppletsSavedType,
                Note = a.AppletsSavedTitle,
                PathName = a.AppletsSavedIsExternal ? _appletsSavedService.GetExternalPath() + a.AppletsSavedPath 
                    : a.AppletsSavedPath,
                OfficerId = a.CreatedBy,
                DeleteFlag = a.AppletsDeleteFlag == 1,
                PersonDetails = new PersonInfo
                {
                    InmateId = a.InmateId ?? 0,
                    PersonId = a.Inmate.PersonId,
                    PersonFirstName = a.Inmate.Person.PersonFirstName,
                    PersonMiddleName = a.Inmate.Person.PersonMiddleName,
                    PersonLastName = a.Inmate.Person.PersonLastName,
                    InmateNumber = a.Inmate.InmateNumber,
                    HousingUnitId = a.Inmate.HousingUnitId ?? 0,
                    LastReviewDate = a.Inmate.LastClassReviewDate,
                    InmateClassificationId = a.Inmate.InmateClassificationId
                },
                OfficerDetails = new PersonnelVm
                {
                    PersonFirstName = a.CreatedByNavigation.PersonNavigation.PersonFirstName,
                    PersonLastName = a.CreatedByNavigation.PersonNavigation.PersonLastName,
                    PersonMiddleName = a.CreatedByNavigation.PersonNavigation.PersonMiddleName,
                    OfficerBadgeNumber = a.CreatedByNavigation.OfficerBadgeNum,
                }
            }));
        }

        private void GetLink(DateTime fromDate, DateTime toDate)
        {
            if (_classLogInputs.Flag &&
                (_classLogInputs.LogParameters is null || !_classLogInputs.LogParameters.Link)) return;
            //To get InmateClassificationLinkXref Details
            IQueryable<InmateClassificationLinkXref> lstLink = _context.InmateClassificationLinkXref
                .Where(xref => xref.InmateClassificationLink.CreateDate.HasValue &&
                               xref.InmateClassificationLink.CreateDate <= toDate &&
                               xref.InmateClassificationLink.CreateDate >= fromDate &&
                               _inmate.Any(c => c.InmateId == xref.InmateId)
                               && (!_classLogInputs.PersonnelId.HasValue || _classLogInputs.PersonnelId == 0
                                                                         || xref.CreateBy == _classLogInputs.PersonnelId)
                               && (_classLogInputs.DeleteFlag || ((!xref.DeleteFlag.HasValue
                                                                  && !xref.InmateClassificationLink.DeleteFlag.HasValue))
                                   || (xref.DeleteFlag == 0
                                       && xref.InmateClassificationLink.DeleteFlag == 0)));
            if (!lstLink.Any()) return;
            //To get Count of Link details
            _classifyCount.Link = lstLink.Count();
            //To get Link Details           
            if (_classLogInputs.LogParameters is null || !_classLogInputs.LogParameters.Link) return;

            _classLogDetails.ClassLog.AddRange(lstLink.Select(xref => new ClassLog
            {
                Id = xref.InmateClassificationLink.InmateClassificationLinkId,
                ClassDate = xref.InmateClassificationLink.CreateDate,
                ClassType = ClassTypeConstants.LINK,
                Type = xref.InmateClassificationLink.LinkType.HasValue.ToString(),
                Note = xref.InmateClassificationLink.LinkNote,
                OfficerId = xref.InmateClassificationLink.CreateBy,
                DeleteFlag = xref.DeleteFlag == 1,
                PersonDetails = new PersonInfo
                {
                    InmateId = xref.InmateId,
                    PersonId = xref.Inmate.PersonId,
                    PersonFirstName = xref.Inmate.Person.PersonFirstName,
                    PersonMiddleName = xref.Inmate.Person.PersonMiddleName,
                    PersonLastName = xref.Inmate.Person.PersonLastName,
                    InmateNumber = xref.Inmate.InmateNumber,
                    HousingUnitId = xref.Inmate.HousingUnitId ?? 0,
                    LastReviewDate = xref.Inmate.LastClassReviewDate,
                    InmateClassificationId = xref.Inmate.InmateClassificationId
                },
                OfficerDetails = new PersonnelVm
                {
                    PersonFirstName = xref.InmateClassificationLink.CreateByNavigation.PersonNavigation.PersonFirstName,
                    PersonLastName = xref.InmateClassificationLink.CreateByNavigation.PersonNavigation.PersonLastName,
                    PersonMiddleName = xref.InmateClassificationLink.CreateByNavigation.PersonNavigation.PersonMiddleName,
                    OfficerBadgeNumber = xref.InmateClassificationLink.CreateByNavigation.OfficerBadgeNum,
                }
            }));
        }

        private void GetIntakeAndRelease(DateTime fromDate, DateTime toDate)
        {
            if (_classLogInputs.Flag && (_classLogInputs.LogParameters is null ||
                                         !_classLogInputs.LogParameters.Intake &&
                                         !_classLogInputs.LogParameters.Release)) return;

            //To get Incarceration Details 
            IQueryable<ClassLog> lstIncarceration = _context.Incarceration.Where(c =>
              (c.InmateId.HasValue ? _inmate.Any(f => f.InmateId == c.InmateId) : default)
              && (!_classLogInputs.PersonnelId.HasValue || _classLogInputs.PersonnelId == 0
              || c.InOfficerId == _classLogInputs.PersonnelId)).Select(c => new ClassLog
              {
                  Id = c.IncarcerationId,
                  ClassDate = c.DateIn,
                  OfficerId = c.InOfficerId,
                  ReleaseOut = c.ReleaseOut,
                  PersonDetails = new PersonInfo
                  {
                      InmateId = c.InmateId ?? 0,
                      PersonId = c.Inmate.PersonId,
                      PersonFirstName = c.Inmate.Person.PersonFirstName,
                      PersonMiddleName = c.Inmate.Person.PersonMiddleName,
                      PersonLastName = c.Inmate.Person.PersonLastName,
                      InmateNumber = c.Inmate.InmateNumber,
                      HousingUnitId = c.Inmate.HousingUnitId ?? 0,
                      LastReviewDate = c.Inmate.LastClassReviewDate,
                      InmateClassificationId = c.Inmate.InmateClassificationId
                  },
                  Count = c.IncarcerationArrestXref.Count,
                  ArrestDetails = new ArrestIntake
                  {
                      BookingNumber = c.IncarcerationArrestXref.FirstOrDefault().Arrest.ArrestBookingNo,
                      Description = c.IncarcerationArrestXref.FirstOrDefault().ReleaseReason,
                      ArrestType = c.IncarcerationArrestXref.FirstOrDefault().Arrest.ArrestType
                  },
                  OfficerDetails = new PersonnelVm
                  {
                      PersonFirstName = c.InOfficer.PersonNavigation.PersonFirstName,
                      PersonLastName = c.InOfficer.PersonNavigation.PersonLastName,
                      PersonMiddleName = c.InOfficer.PersonNavigation.PersonMiddleName,
                      OfficerBadgeNumber = c.InOfficer.OfficerBadgeNum,
                  }
              });
            if (!lstIncarceration.Any()) return;
            _lstIncarcerationIntake = lstIncarceration.Where(c => c.ClassDate <= toDate && c.ClassDate >= fromDate);
            _classifyCount.Intake = _lstIncarcerationIntake.Count();
            //To get Release Details
            _lstIncarcerationRelease = lstIncarceration.Where(c =>
                c.ReleaseOut.HasValue && c.ReleaseOut <= toDate && c.ReleaseOut >= fromDate);
            //To get count Release Details
            _classifyCount.Release = _lstIncarcerationRelease.Count();
            if (_classLogInputs.LogParameters is null ||
                !_classLogInputs.LogParameters.Intake && !_classLogInputs.LogParameters.Release) return;
            //To get Intake details
            if (_classLogInputs.LogParameters.Intake)
            {
                _classLogDetails.ClassLog.AddRange(_lstIncarcerationIntake.Select(c => new ClassLog
                {
                    Id = c.Id,
                    ClassDate = c.ClassDate,
                    OfficerId = c.OfficerId,
                    ClassType = ClassTypeConstants.INTAKE,
                    PersonDetails = c.PersonDetails,
                    Count = c.Count,
                    ArrestDetails = new ArrestIntake
                    {
                        Description = c.ArrestDetails.ArrestType,
                        BookingNumber = c.ArrestDetails.BookingNumber
                    },
                    OfficerDetails = c.OfficerDetails
                }));
            }
            //To get Release Details
            if (!_classLogInputs.LogParameters.Release) return;
            _classLogDetails.ClassLog.AddRange(_lstIncarcerationRelease.Select(c => new ClassLog
            {
                Id = c.Id,
                ClassDate = c.ReleaseOut,
                OfficerId = c.OfficerId,
                ClassType = ClassTypeConstants.RELEASE,
                PersonDetails = c.PersonDetails,
                Count = c.Count,
                ArrestDetails = new ArrestIntake
                {
                    Description = c.ArrestDetails.Description,
                    BookingNumber = c.ArrestDetails.BookingNumber
                },
                OfficerDetails = c.OfficerDetails
            }));
        }

        private void GetHousing(DateTime fromDate, DateTime toDate)
        {
            if (_classLogInputs.Flag &&
                (_classLogInputs.LogParameters is null || !_classLogInputs.LogParameters.Housing)) return;
            //To get HousingUnitMoveHistory Details
            IQueryable<HousingUnitMoveHistory> lstHousingMoveHistory = _context.HousingUnitMoveHistory.Where(hum =>
                hum.CreateDate <= toDate && hum.CreateDate >= fromDate && hum.HousingUnitToId.HasValue &&
                _inmate.Any(c => c.InmateId == hum.InmateId)
              && (!_classLogInputs.PersonnelId.HasValue || _classLogInputs.PersonnelId == 0
              || hum.MoveOfficerId == _classLogInputs.PersonnelId));
            if (!lstHousingMoveHistory.Any()) return;
            //To get Count of Housing Details
            _classifyCount.Housing = lstHousingMoveHistory.Count();
            //To get Housing Details           
            if (_classLogInputs.LogParameters is null || !_classLogInputs.LogParameters.Housing) return;
            _classLogDetails.ClassLog.AddRange(lstHousingMoveHistory
                .Select(hum => new ClassLog
                {
                    Id = hum.HousingUnitMoveHistoryId,
                    ClassDate = hum.MoveDate,
                    OfficerId = hum.MoveOfficerId,
                    ClassType = ClassTypeConstants.HOUSING,
                    HousingUnitId = hum.HousingUnitToId,
                    Reason = hum.MoveReason,
                    PersonDetails = new PersonInfo
                    {
                        InmateId = hum.InmateId,
                        PersonId = hum.Inmate.PersonId,
                        PersonFirstName = hum.Inmate.Person.PersonFirstName,
                        PersonMiddleName = hum.Inmate.Person.PersonMiddleName,
                        PersonLastName = hum.Inmate.Person.PersonLastName,
                        InmateNumber = hum.Inmate.InmateNumber,
                        HousingUnitId = hum.Inmate.HousingUnitId ?? 0,
                        LastReviewDate = hum.Inmate.LastClassReviewDate,
                        InmateClassificationId = hum.Inmate.InmateClassificationId
                    },
                    HousingDetails = new HousingDetail
                    {
                        HousingUnitLocation = hum.HousingUnitTo.HousingUnitLocation,
                        HousingUnitNumber = hum.HousingUnitTo.HousingUnitNumber,
                        FacilityAbbr = hum.HousingUnitTo.Facility.FacilityAbbr
                    },
                    OfficerDetails = new PersonnelVm
                    {
                        PersonFirstName = hum.MoveOfficer.PersonNavigation.PersonFirstName,
                        PersonLastName = hum.MoveOfficer.PersonNavigation.PersonLastName,
                        PersonMiddleName = hum.MoveOfficer.PersonNavigation.PersonMiddleName,
                        OfficerBadgeNumber = hum.MoveOfficer.OfficerBadgeNum,
                    }
                }));
        }

        private void GetNote(DateTime fromDate, DateTime toDate)
        {
            if (_classLogInputs.Flag &&
                (_classLogInputs.LogParameters is null || !_classLogInputs.LogParameters.Note)) return;
            //To get FloorNoteXref table details
            IQueryable<FloorNoteXref> floorNoteXref = _context.FloorNoteXref.Where(fnote =>
                fnote.FloorNote.FloorNoteDate <= toDate && fnote.FloorNote.FloorNoteDate >= fromDate &&
                _inmate.Any(c => c.InmateId == fnote.InmateId)
               && (!_classLogInputs.PersonnelId.HasValue || _classLogInputs.PersonnelId == 0
              || fnote.FloorNote.FloorNoteOfficerId == _classLogInputs.PersonnelId)
                 && (_classLogInputs.DeleteFlag
              || !fnote.FloorNote.FloorDeleteFlag.HasValue
               || fnote.FloorNote.FloorDeleteFlag == 0));
            if (!floorNoteXref.Any()) return;
            //To get Count of Note Details
            _classifyCount.Note = floorNoteXref.Count();
            //To get Note Details         
            if (_classLogInputs.LogParameters is null || !_classLogInputs.LogParameters.Note) return;

            _classLogDetails.ClassLog.AddRange(floorNoteXref.Select(noteXref => new ClassLog
            {
                Id = noteXref.FloorNoteId,
                ClassDate = noteXref.FloorNote.FloorNoteDate,
                OfficerId = noteXref.FloorNote.FloorNoteOfficerId,
                ClassType = ClassTypeConstants.NOTE,
                Type = noteXref.FloorNote.FloorNoteType,
                Note = noteXref.FloorNote.FloorNoteNarrative,
                DeleteFlag = noteXref.FloorNote.FloorDeleteFlag == 1,
                PersonDetails = new PersonInfo
                {
                    InmateId = noteXref.InmateId,
                    PersonId = noteXref.Inmate.PersonId,
                    PersonFirstName = noteXref.Inmate.Person.PersonFirstName,
                    PersonMiddleName = noteXref.Inmate.Person.PersonMiddleName,
                    PersonLastName = noteXref.Inmate.Person.PersonLastName,
                    InmateNumber = noteXref.Inmate.InmateNumber,
                    HousingUnitId = noteXref.Inmate.HousingUnitId ?? 0,
                    LastReviewDate = noteXref.Inmate.LastClassReviewDate,
                    InmateClassificationId = noteXref.Inmate.InmateClassificationId
                },
                OfficerDetails = new PersonnelVm
                {
                    PersonFirstName = noteXref.FloorNote.FloorNoteOfficer.PersonNavigation.PersonFirstName,
                    PersonLastName = noteXref.FloorNote.FloorNoteOfficer.PersonNavigation.PersonLastName,
                    PersonMiddleName = noteXref.FloorNote.FloorNoteOfficer.PersonNavigation.PersonMiddleName,
                    OfficerBadgeNumber = noteXref.FloorNote.FloorNoteOfficer.OfficerBadgeNum,
                }
            }));
        }

        private void GetIncident(DateTime fromDate, DateTime toDate)
        {
            if (_classLogInputs.Flag &&
                (_classLogInputs.LogParameters is null || !_classLogInputs.LogParameters.Incident)) return;
            //To get DisciplinaryInmate details
            IQueryable<DisciplinaryInmate> inmateDisciplinary = _context.DisciplinaryInmate.Where(dinmate =>
                dinmate.DisciplinaryIncident.DisciplinaryReportDate <= toDate &&
                dinmate.DisciplinaryIncident.DisciplinaryReportDate >= fromDate &&
                (dinmate.InmateId.HasValue ? _inmate.Any(c => c.InmateId == dinmate.InmateId) : default)
                && (_classLogInputs.DeleteFlag || !dinmate.DeleteFlag.HasValue || dinmate.DeleteFlag == 0)
                && (!_classLogInputs.PersonnelId.HasValue || _classLogInputs.PersonnelId == 0
              || dinmate.DisciplinaryIncident.DisciplinaryOfficerId == _classLogInputs.PersonnelId));
            if (!inmateDisciplinary.Any()) return;
            //To get Count of Incident Details
            _classifyCount.Incident = inmateDisciplinary.Count();
            //To get Incident Details           
            if (_classLogInputs.LogParameters is null || !_classLogInputs.LogParameters.Incident) return;

            _classLogDetails.ClassLog.AddRange(inmateDisciplinary.Select(dinmate => new ClassLog
            {
                Id = dinmate.DisciplinaryIncident.DisciplinaryIncidentId,
                ClassDate = dinmate.DisciplinaryIncident.DisciplinaryReportDate,
                OfficerId = dinmate.DisciplinaryIncident.DisciplinaryOfficerId,
                ClassType = ClassTypeConstants.INCIDENT,
                IncidentNarrative = new IncidentNarrative
                {
                    DisciplinarySynopsis = dinmate.DisciplinaryIncident.DisciplinarySynopsis,
                    DisciplinaryNumber = dinmate.DisciplinaryIncident.DisciplinaryNumber,
                    InmatelookupDescription = dinmate.DisciplinaryInmateType.ToString(),
                    IncidentlookupDescription = dinmate.DisciplinaryIncident.DisciplinaryType.ToString()
                },
                DeleteFlag = dinmate.DeleteFlag == 1,
                PersonDetails = new PersonInfo
                {
                    InmateId = dinmate.InmateId ?? 0,
                    PersonId = dinmate.Inmate.PersonId,
                    PersonFirstName = dinmate.Inmate.Person.PersonFirstName,
                    PersonMiddleName = dinmate.Inmate.Person.PersonMiddleName,
                    PersonLastName = dinmate.Inmate.Person.PersonLastName,
                    InmateNumber = dinmate.Inmate.InmateNumber,
                    HousingUnitId = dinmate.Inmate.HousingUnitId ?? 0,
                    LastReviewDate = dinmate.Inmate.LastClassReviewDate,
                    InmateClassificationId = dinmate.Inmate.InmateClassificationId
                },
                OfficerDetails = new PersonnelVm
                {
                    PersonFirstName = dinmate.DisciplinaryIncident.DisciplinaryOfficer.PersonNavigation.PersonFirstName,
                    PersonLastName = dinmate.DisciplinaryIncident.DisciplinaryOfficer.PersonNavigation.PersonLastName,
                    PersonMiddleName = dinmate.DisciplinaryIncident.DisciplinaryOfficer.PersonNavigation.PersonMiddleName,
                    OfficerBadgeNumber = dinmate.DisciplinaryIncident.DisciplinaryOfficer.OfficerBadgeNum,

                }
            }));
        }

        private void GetMessage(DateTime fromDate, DateTime toDate)
        {
            if (_classLogInputs.Flag &&
                (_classLogInputs.LogParameters is null || !_classLogInputs.LogParameters.Message)) return;
            //To get PersonAlert Details
            IQueryable<ClassLog> personAlert = _context.PersonAlert.Where(alert =>
                alert.CreateDate <= toDate && alert.CreateDate >= fromDate &&
                _inmate.Any(c => c.PersonId == alert.PersonId)
                 && (!_classLogInputs.PersonnelId.HasValue || _classLogInputs.PersonnelId == 0
              || alert.CreatedBy == _classLogInputs.PersonnelId)
              && (_classLogInputs.DeleteFlag || alert.ActiveAlertFlag == 1
            ))
                .Select(alert => new ClassLog
                {
                    Id = alert.PersonAlertId,
                    PersonId = alert.PersonId,
                    ClassDate = alert.CreateDate,
                    OfficerId = alert.CreatedBy,
                    ClassType = ClassTypeConstants.MESSAGE,
                    DeleteFlag = !alert.ActiveAlertFlag.HasValue || alert.ActiveAlertFlag == 0,
                    ClassNarrative = alert.Alert,
                    OfficerDetails = new PersonnelVm
                    {
                        PersonFirstName = alert.CreatedByNavigation.PersonNavigation.PersonFirstName,
                        PersonLastName = alert.CreatedByNavigation.PersonNavigation.PersonLastName,
                        PersonMiddleName = alert.CreatedByNavigation.PersonNavigation.PersonMiddleName,
                        OfficerBadgeNumber = alert.CreatedByNavigation.OfficerBadgeNum,
                    }
                });
            if (!personAlert.Any()) return;
            //To get Count of Message details
            _classifyCount.Message = personAlert.Count();
            //To get Message Details
            if (_classLogInputs.LogParameters is null || !_classLogInputs.LogParameters.Message) return;

            _classLogDetails.ClassLog.AddRange(personAlert
                .SelectMany(alert => _inmate.Where(w => w.PersonId == alert.PersonId),
                    (alert, a) => new ClassLog
                    {
                        Id = alert.Id,
                        ClassDate = alert.ClassDate,
                        OfficerId = alert.OfficerId,
                        ClassType = ClassTypeConstants.MESSAGE,
                        DeleteFlag = alert.DeleteFlag,
                        ClassNarrative = alert.ClassNarrative,
                        PersonDetails = new PersonInfo
                        {
                            InmateId = a.InmateId,
                            PersonId = a.PersonId,
                            PersonFirstName = a.Person.PersonFirstName,
                            PersonMiddleName = a.Person.PersonMiddleName,
                            PersonLastName = a.Person.PersonLastName,
                            InmateNumber = a.InmateNumber,
                            HousingUnitId = a.HousingUnitId ?? 0,
                            LastReviewDate = a.LastClassReviewDate,
                            InmateClassificationId = a.InmateClassificationId
                        },
                        OfficerDetails = alert.OfficerDetails
                    }));
        }

        private void GetFlag(DateTime fromDate, DateTime toDate)
        {
            if (_classLogInputs.Flag &&
                (_classLogInputs.LogParameters is null || !_classLogInputs.LogParameters.Flag)) return;
            //To get PersonFlag Details
            IQueryable<PersonFlag> personFlag = _context.PersonFlag.Where(flag =>
                flag.CreateDate <= toDate && flag.CreateDate >= fromDate &&
                _inmate.Any(c => c.PersonId == flag.PersonId) && flag.MedicalFlagIndex == null
                 && (_classLogInputs.DeleteFlag || flag.DeleteFlag == 0)
                   && (!_classLogInputs.PersonnelId.HasValue || _classLogInputs.PersonnelId == 0
              || flag.CreateBy == _classLogInputs.PersonnelId));
            if (!personFlag.Any()) return;
            //To get count of Flag Details
            _classifyCount.Flag = personFlag.Count();
            // To get Flag Details
            if (_classLogInputs.LogParameters is null || !_classLogInputs.LogParameters.Flag) return;

            _classLogDetails.ClassLog.AddRange(personFlag
                .SelectMany(pf => _inmate.Where(w => w.PersonId == pf.PersonId),
                    (pf, a) => new ClassLog
                    {
                        Id = pf.PersonFlagId,
                        ClassDate = pf.CreateDate,
                        OfficerId = pf.CreateBy,
                        ClassType = ClassTypeConstants.FLAG,
                        DeleteFlag = pf.DeleteFlag == 1,
                        PersonFlagIndex = pf.PersonFlagIndex ?? 0,
                        InmateFlagIndex = pf.InmateFlagIndex ?? 0,
                        DietFlagIndex = pf.DietFlagIndex ?? 0,
                        PersonDetails = new PersonInfo
                        {
                            InmateId = a.InmateId,
                            PersonId = a.PersonId,
                            PersonFirstName = a.Person.PersonFirstName,
                            PersonMiddleName = a.Person.PersonMiddleName,
                            PersonLastName = a.Person.PersonLastName,
                            InmateNumber = a.InmateNumber,
                            HousingUnitId = a.HousingUnitId,
                            LastReviewDate = a.LastClassReviewDate,
                            InmateClassificationId = a.InmateClassificationId
                        },
                        OfficerDetails = new PersonnelVm
                        {
                            PersonFirstName = pf.CreateByNavigation.PersonNavigation.PersonFirstName,
                            PersonLastName = pf.CreateByNavigation.PersonNavigation.PersonLastName,
                            PersonMiddleName = pf.CreateByNavigation.PersonNavigation.PersonMiddleName,
                            OfficerBadgeNumber = pf.CreateByNavigation.OfficerBadgeNum,
                        }
                    }));
        }

        private void GetKeepSep(DateTime fromDate, DateTime toDate)
        {
            if (_classLogInputs.Flag &&
                (_classLogInputs.LogParameters is null || !_classLogInputs.LogParameters.KeepSep)) return;
            //To get Keep inmate
            //To get KeepSeparate Details      
            IQueryable<KeepSeparate> listKeep =
                _context.KeepSeparate.Where(k => k.KeepSeparateDate <= toDate
                && k.KeepSeparateDate >= fromDate
                && (_classLogInputs.DeleteFlag || k.InactiveFlag == 0)
                  && (!_classLogInputs.PersonnelId.HasValue || _classLogInputs.PersonnelId == 0
              || k.KeepSeparateOfficerId == _classLogInputs.PersonnelId));
            if (listKeep.Any())
            {
                IQueryable<KeepSeparate> lstKeep1 =
                    listKeep.Where(k => _inmate.Any(c => c.InmateId == k.KeepSeparateInmate1Id));
                _classifyCount.KeepSep = lstKeep1.Count();
                IQueryable<KeepSeparate> lstKeep2 =
                    listKeep.Where(k => _inmate.Any(c => c.InmateId == k.KeepSeparateInmate2Id));
                _classifyCount.KeepSep = _classifyCount.KeepSep + lstKeep2.Count();
                if (_classLogInputs.LogParameters != null && _classLogInputs.LogParameters.KeepSep)
                {
                    //To get KeepSeparate Details             
                    _classLogDetails.ClassLog.AddRange(lstKeep1.Select(k => new ClassLog
                    {
                        Id = k.KeepSeparateId,
                        ClassDate = k.KeepSeparateDate,
                        OfficerId = k.KeepSeparateOfficerId,
                        ClassType = ClassTypeConstants.KEEPSEPINMATE,
                        KeepSeparateNarrative = new KeepSeparateNarrative
                        {
                            KeepSepReason = k.KeepSeparateReason,
                            InmateActive = k.KeepSeparateInmate1.InmateActive == 1,
                            KeepSepNote = k.KeepSeparateNote,
                            KeepSepInmate2Id = k.KeepSeparateInmate2Id,
                            KeepSepType = k.KeepSeparateType,
                        },
                        DeleteFlag = k.InactiveFlag == 1,
                        PersonDetails = new PersonInfo
                        {
                            InmateId = k.KeepSeparateInmate1Id,
                            PersonId = k.KeepSeparateInmate1.PersonId,
                            PersonFirstName = k.KeepSeparateInmate1.Person.PersonFirstName,
                            PersonMiddleName = k.KeepSeparateInmate1.Person.PersonMiddleName,
                            PersonLastName = k.KeepSeparateInmate1.Person.PersonLastName,
                            InmateNumber = k.KeepSeparateInmate1.InmateNumber,
                            HousingUnitId = k.KeepSeparateInmate1.HousingUnitId ?? 0,
                            LastReviewDate = k.KeepSeparateInmate1.LastClassReviewDate,
                            InmateClassificationId = k.KeepSeparateInmate1.InmateClassificationId
                        },
                        HousingDetails = new HousingDetail
                        {
                            HousingUnitLocation = k.KeepSeparateInmate1.HousingUnit != null
                                 ? k.KeepSeparateInmate1.HousingUnit.HousingUnitLocation : "",
                            HousingUnitNumber = k.KeepSeparateInmate1.HousingUnit != null
                                 ? k.KeepSeparateInmate1.HousingUnit.HousingUnitNumber : "",
                            HousingUnitBedLocation = k.KeepSeparateInmate1.HousingUnit != null
                                 ? k.KeepSeparateInmate1.HousingUnit.HousingUnitBedLocation : "",
                            HousingUnitBedNumber = k.KeepSeparateInmate1.HousingUnit != null
                                 ? k.KeepSeparateInmate1.HousingUnit.HousingUnitBedLocation : ""
                        },
                        OfficerDetails = new PersonnelVm
                        {
                            PersonFirstName = k.KeepSeparateOfficer.PersonNavigation.PersonFirstName,
                            PersonLastName = k.KeepSeparateOfficer.PersonNavigation.PersonLastName,
                            PersonMiddleName = k.KeepSeparateOfficer.PersonNavigation.PersonMiddleName,
                            OfficerBadgeNumber = k.KeepSeparateOfficer.OfficerBadgeNum,
                        }
                    }));
                    _classLogDetails.ClassLog.AddRange(lstKeep2.Select(k => new ClassLog
                    {
                        Id = k.KeepSeparateId,
                        ClassDate = k.KeepSeparateDate,
                        OfficerId = k.KeepSeparateOfficerId,
                        ClassType = ClassTypeConstants.KEEPSEPINMATE,
                        KeepSeparateNarrative = new KeepSeparateNarrative
                        {
                            KeepSepReason = k.KeepSeparateReason,
                            InmateActive = k.KeepSeparateInmate2.InmateActive == 1,
                            KeepSepNote = k.KeepSeparateNote,
                            KeepSepInmate2Id = k.KeepSeparateInmate1Id,
                            KeepSepType = k.KeepSeparateType,
                        },
                        DeleteFlag = k.InactiveFlag == 1,
                        PersonDetails = new PersonInfo
                        {
                            InmateId = k.KeepSeparateInmate2Id,
                            PersonId = k.KeepSeparateInmate2.PersonId,
                            PersonFirstName = k.KeepSeparateInmate2.Person.PersonFirstName,
                            PersonMiddleName = k.KeepSeparateInmate2.Person.PersonMiddleName,
                            PersonLastName = k.KeepSeparateInmate2.Person.PersonLastName,
                            InmateNumber = k.KeepSeparateInmate2.InmateNumber,
                            HousingUnitId = k.KeepSeparateInmate2.HousingUnitId ?? 0,
                            LastReviewDate = k.KeepSeparateInmate2.LastClassReviewDate,
                            InmateClassificationId = k.KeepSeparateInmate2.InmateClassificationId
                        },
                        HousingDetails = new HousingDetail
                        {
                            HousingUnitLocation = k.KeepSeparateInmate2.HousingUnit != null
                                ? k.KeepSeparateInmate2.HousingUnit.HousingUnitLocation : "",
                            HousingUnitNumber = k.KeepSeparateInmate2.HousingUnit != null
                                ? k.KeepSeparateInmate2.HousingUnit.HousingUnitNumber : "",
                            HousingUnitBedLocation = k.KeepSeparateInmate2.HousingUnit != null
                                ? k.KeepSeparateInmate2.HousingUnit.HousingUnitBedLocation : "",
                            HousingUnitBedNumber = k.KeepSeparateInmate2.HousingUnit != null
                                ? k.KeepSeparateInmate2.HousingUnit.HousingUnitBedLocation : ""
                        },
                        OfficerDetails = new PersonnelVm
                        {
                            PersonFirstName = k.KeepSeparateOfficer.PersonNavigation.PersonFirstName,
                            PersonLastName = k.KeepSeparateOfficer.PersonNavigation.PersonLastName,
                            PersonMiddleName = k.KeepSeparateOfficer.PersonNavigation.PersonMiddleName,
                            OfficerBadgeNumber = k.KeepSeparateOfficer.OfficerBadgeNum,
                        }
                    }));

                }
            }
            // To get Keep Assoc
            IQueryable<KeepSepAssocInmate> kAssoc =
                _context.KeepSepAssocInmate.Where(k =>
                    k.KeepSepDate <= toDate && k.KeepSepDate >= fromDate &&
                    _inmate.Any(c => c.InmateId == k.KeepSepInmate2Id)
                          && (_classLogInputs.DeleteFlag || k.DeleteFlag == 0)
                           && (!_classLogInputs.PersonnelId.HasValue || _classLogInputs.PersonnelId == 0
              || k.KeepSepOfficerId == _classLogInputs.PersonnelId));
            if (kAssoc.Any())
            {
                //count of keep assoc
                _classifyCount.KeepSep = _classifyCount.KeepSep + kAssoc.Count();
            }
            //To get Keep Subset
            IQueryable<KeepSepSubsetInmate> iKSubset =
                _context.KeepSepSubsetInmate.Where(k =>
                    k.KeepSepDate <= toDate && k.KeepSepDate >= fromDate &&
                    _inmate.Any(c => c.InmateId == k.KeepSepInmate2Id)
                    && (_classLogInputs.DeleteFlag || k.DeleteFlag == 0)
                     && (!_classLogInputs.PersonnelId.HasValue || _classLogInputs.PersonnelId == 0
              || k.KeepSepOfficerId == _classLogInputs.PersonnelId));
            //_inmate.Select(a => a.InmateId).Contains(k.KeepSepInmate2Id));
            if (iKSubset.Any())
            {
                //To get total count of Keep Sep
                _classifyCount.KeepSep = _classifyCount.KeepSep + iKSubset.Count();
            }
            if (_classLogInputs.LogParameters is null || !_classLogInputs.LogParameters.KeepSep) return;
            //To get Keep Assoc Details        
            List<Lookup> lookupslist = _context.Lookup.Where(w => w.LookupType == LookupConstants.CLASSGROUP).ToList();
            List<Lookup> lookupSubsetlist = _context.Lookup.Where(w => w.LookupType == LookupConstants.CLASSGROUPSUB).ToList();
            _classLogDetails.ClassLog.AddRange(kAssoc.Select(k => new ClassLog
            {
                Id = k.KeepSepAssocInmateId,
                ClassDate = k.CreateDate,
                OfficerId = k.KeepSepOfficerId,
                ClassType = ClassTypeConstants.KEEPSEPASSOC,
                KeepSeparateNarrative = new KeepSeparateNarrative
                {
                    KeepSepAssoc = lookupslist.Where(d => d.LookupIndex == k.KeepSepAssoc1Id)
                                    .Select(s => s.LookupDescription).SingleOrDefault(),
                    KeepSepAssoc1Id = k.KeepSepAssoc1Id,
                    KeepSepReason = k.KeepSepReason,
                    KeepSepNote = k.KeepSeparateNote,
                    KeepSepType = k.KeepSeparateType,
                },
                DeleteFlag = k.DeleteFlag == 1,
                PersonDetails = new PersonInfo
                {
                    InmateId = k.KeepSepInmate2Id,
                    PersonId = k.KeepSepInmate2.PersonId,
                    PersonFirstName = k.KeepSepInmate2.Person.PersonFirstName,
                    PersonMiddleName = k.KeepSepInmate2.Person.PersonMiddleName,
                    PersonLastName = k.KeepSepInmate2.Person.PersonLastName,
                    InmateNumber = k.KeepSepInmate2.InmateNumber,
                    HousingUnitId = k.KeepSepInmate2.HousingUnitId ?? 0,
                    LastReviewDate = k.KeepSepInmate2.LastClassReviewDate,
                    InmateClassificationId = k.KeepSepInmate2.InmateClassificationId
                },
                OfficerDetails = new PersonnelVm
                {
                    PersonFirstName = k.KeepSepOfficer.PersonNavigation.PersonFirstName,
                    PersonLastName = k.KeepSepOfficer.PersonNavigation.PersonLastName,
                    PersonMiddleName = k.KeepSepOfficer.PersonNavigation.PersonMiddleName,
                    OfficerBadgeNumber = k.KeepSepOfficer.OfficerBadgeNum,
                }
            }));
            //To get Keep Subset Details           
            _classLogDetails.ClassLog.AddRange(iKSubset.Select(k => new ClassLog
            {
                Id = k.KeepSepSubsetInmateId,
                ClassDate = k.CreateDate,
                OfficerId = k.KeepSepOfficerId,
                ClassType = ClassTypeConstants.KEEPSEPSUBSET,
                KeepSeparateNarrative = new KeepSeparateNarrative
                {
                    KeepSepAssoc = lookupslist.Where(f => f.LookupIndex == k.KeepSepAssoc1Id)
                                    .Select(s => s.LookupDescription).SingleOrDefault(),
                    KeepSepAssocSubset = lookupSubsetlist.Where(f => f.LookupIndex == k.KeepSepAssoc1SubsetId)
                                           .Select(s => s.LookupDescription).SingleOrDefault(),
                    KeepSepAssoc1Id = k.KeepSepAssoc1Id,
                    KeepSepAssoc1SubsetId = k.KeepSepAssoc1SubsetId,
                    KeepSepReason = k.KeepSepReason,
                    KeepSepNote = k.KeepSeparateNote,
                    KeepSepType = k.KeepSeparateType,
                },
                DeleteFlag = k.DeleteFlag == 1,
                PersonDetails = new PersonInfo
                {

                    InmateId = k.KeepSepInmate2Id,
                    PersonId = k.KeepSepInmate2.PersonId,
                    PersonFirstName = k.KeepSepInmate2.Person.PersonFirstName,
                    PersonMiddleName = k.KeepSepInmate2.Person.PersonMiddleName,
                    PersonLastName = k.KeepSepInmate2.Person.PersonLastName,
                    InmateNumber = k.KeepSepInmate2.InmateNumber,
                    HousingUnitId = k.KeepSepInmate2.HousingUnitId ?? 0,
                    LastReviewDate = k.KeepSepInmate2.LastClassReviewDate,
                    InmateClassificationId = k.KeepSepInmate2.InmateClassificationId
                },
                OfficerDetails = new PersonnelVm
                {
                    PersonFirstName = k.KeepSepOfficer.PersonNavigation.PersonFirstName,
                    PersonLastName = k.KeepSepOfficer.PersonNavigation.PersonLastName,
                    PersonMiddleName = k.KeepSepOfficer.PersonNavigation.PersonMiddleName,
                    OfficerBadgeNumber = k.KeepSepOfficer.OfficerBadgeNum,
                }
            }));
        }

        private void GetAssoc(DateTime fromDate, DateTime toDate)
        {
            if (_classLogInputs.Flag &&
                (_classLogInputs.LogParameters is null || !_classLogInputs.LogParameters.Assoc)) return;
            //To get PersonClassification Details     
            List<Lookup> lookupslist = _context.Lookup.Where(w => w.LookupType == LookupConstants.CLASSGROUP).ToList();
            List<Lookup> lookupSubsetlist = _context.Lookup.Where(w => w.LookupType == LookupConstants.CLASSGROUPSUB).ToList();
            IQueryable<PersonClassification> personClassification = _context.PersonClassification.Where(person =>
                person.CreateDate <= toDate && person.CreateDate >= fromDate &&
                _inmate.Any(c => c.PersonId == person.PersonId)
                 && (_classLogInputs.DeleteFlag || person.InactiveFlag == 0)
                  && (!_classLogInputs.PersonnelId.HasValue || _classLogInputs.PersonnelId == 0
              || person.CreatedByPersonnelId == _classLogInputs.PersonnelId));
            if (!personClassification.Any()) return;
            //To get Count of Assoc Details
            _classifyCount.Assoc = personClassification.Count();
            // To get Assoc Details
            if (_classLogInputs.LogParameters is null || !_classLogInputs.LogParameters.Assoc) return;

            _classLogDetails.ClassLog.AddRange(personClassification.SelectMany(
                p => _inmate.Where(a => a.PersonId == p.PersonId),
                (person, a) => new ClassLog
                {
                    Id = person.PersonClassificationId,
                    ClassDate = person.PersonClassificationDateFrom,
                    OfficerId = person.CreatedByPersonnelId,
                    ClassType = ClassTypeConstants.ASSOC,
                    DeleteFlag = person.InactiveFlag == 1,
                    AssocNarrative = new AssocNarrative
                    {
                        PersonClassificationType = lookupslist
                            .Where(f => f.LookupIndex == person.PersonClassificationTypeId).Select(s => s.LookupDescription).SingleOrDefault(),
                        PersonClassificationSubset = lookupSubsetlist
                            .Where(f => f.LookupIndex == person.PersonClassificationSubsetId)
                            .Select(s => s.LookupDescription).SingleOrDefault(),
                        PersonClassificationNotes = person.PersonClassificationNotes,
                        PersonClassificationStatus = person.PersonClassificationStatus,
                        PersonClassificationDateThru = person.PersonClassificationDateFrom,
                        PersonClassificationTypeId = person.PersonClassificationTypeId,
                        PersonClassificationSubsetId = person.PersonClassificationSubsetId,
                    },
                    PersonDetails = new PersonInfo
                    {
                        InmateId = a.InmateId,
                        PersonId = a.PersonId,
                        PersonFirstName = a.Person.PersonFirstName,
                        PersonMiddleName = a.Person.PersonMiddleName,
                        PersonLastName = a.Person.PersonLastName,
                        InmateNumber = a.InmateNumber,
                        HousingUnitId = a.HousingUnitId,
                        LastReviewDate = a.LastClassReviewDate,
                        InmateClassificationId = a.InmateClassificationId
                    },
                    OfficerDetails = new PersonnelVm
                    {
                        PersonFirstName = person.CreatedByPersonnel.PersonNavigation.PersonFirstName,
                        PersonLastName = person.CreatedByPersonnel.PersonNavigation.PersonLastName,
                        PersonMiddleName = person.CreatedByPersonnel.PersonNavigation.PersonMiddleName,
                        OfficerBadgeNumber = person.CreatedByPersonnel.OfficerBadgeNum,
                    }
                }));
        }

        private void GetPrivilege(DateTime fromDate, DateTime toDate)
        {
            if (_classLogInputs.Flag &&
                (_classLogInputs.LogParameters is null || !_classLogInputs.LogParameters.Privileges)) return;
            //To get InmatePrivilegeXref Details
            IQueryable<InmatePrivilegeXref> inmatePrivilegeXref = _context.InmatePrivilegeXref.Where(xref =>
                xref.CreateDate <= toDate && xref.CreateDate >= fromDate &&
                _inmate.Any(c => c.InmateId == xref.InmateId) && (!_classLogInputs.PersonnelId.HasValue
                || _classLogInputs.PersonnelId == 0 || xref.PrivilegeOfficerId == _classLogInputs.PersonnelId)
                && (_classLogInputs.DeleteFlag || xref.PrivilegeRemoveDatetime == null));
            if (!inmatePrivilegeXref.Any()) return;
            //To get Privileges Count
            _classifyCount.Privileges = inmatePrivilegeXref.Count();
            // To get Privileges Details
            if (_classLogInputs.LogParameters is null || !_classLogInputs.LogParameters.Privileges) return;

            _classLogDetails.ClassLog.AddRange(inmatePrivilegeXref.Select(xref => new ClassLog
            {
                Id = xref.InmatePrivilegeXrefId,
                ClassDate = xref.CreateDate,
                ClassType = ClassTypeConstants.PRIVILEGES,
                OfficerId = xref.PrivilegeOfficerId,
                PrivilegesNarrative = new PrivilegesNarrative
                {
                    PrivilegeRemoveOfficerId = xref.PrivilegeRemoveOfficerId,
                    PrivilegeDate = xref.PrivilegeDate,
                    PrivilegeNote = xref.PrivilegeNote,
                    PrivilegeExpires = xref.PrivilegeExpires,
                    PrivilegeId = xref.PrivilegeId,
                    PrivilegeDescription = xref.Privilege.PrivilegeDescription
                },
                DeleteFlag = xref.PrivilegeRemoveDatetime.HasValue,
                PersonDetails = new PersonInfo
                {
                    InmateId = xref.InmateId,
                    PersonId = xref.Inmate.PersonId,
                    PersonFirstName = xref.Inmate.Person.PersonFirstName,
                    PersonMiddleName = xref.Inmate.Person.PersonMiddleName,
                    PersonLastName = xref.Inmate.Person.PersonLastName,
                    InmateNumber = xref.Inmate.InmateNumber,
                    HousingUnitId = xref.Inmate.HousingUnitId ?? 0,
                    LastReviewDate = xref.Inmate.LastClassReviewDate,
                    InmateClassificationId = xref.Inmate.InmateClassificationId
                },
                OfficerDetails = new PersonnelVm
                {
                    PersonFirstName = xref.PrivilegeOfficer.PersonNavigation.PersonFirstName,
                    PersonLastName = xref.PrivilegeOfficer.PersonNavigation.PersonLastName,
                    PersonMiddleName = xref.PrivilegeOfficer.PersonNavigation.PersonMiddleName,
                    OfficerBadgeNumber = xref.PrivilegeOfficer.OfficerBadgeNum,
                }
            }));
        }

        //To get data from User Access Table for Classification Settings
        public async Task<LogParameters> GetClassifySettings()
        {
            string classifyDefault = DefaultClassifySettings();

            Claim claim = new Claim(CustomConstants.PERSONNELID, _personnelId.ToString());

            AppUser appUser = _userManager.GetUsersForClaimAsync(claim).Result.FirstOrDefault();

            IList<Claim> claims = _userManager.GetClaimsAsync(appUser).Result;

            Claim classifyClaim = claims.FirstOrDefault(f => f.Type == "Classify_Settings");

            if (classifyClaim == null)
            {
                await _userManager.AddClaimAsync(appUser, new Claim("Classify_Settings", classifyDefault));
            }

            IList<Claim> claims1 = _userManager.GetClaimsAsync(appUser).Result;

            string classifyClaimLog = claims1.FirstOrDefault(f => f.Type == "Classify_Settings")?.Value;

            LogParameters logParameters = JsonConvert.DeserializeObject<LogParameters>(classifyClaimLog);

            return logParameters;
        }

        private static string DefaultClassifySettings()
        {
            LogParameters lstClaimClassifySettings = new LogParameters
            {
                Initial = false,
                ReClassify = false,
                Review = false,
                ClassNote = false,
                Form = false,
                Attach = false,
                Link = false,
                Intake = false,
                Release = false,
                Housing = false,
                Note = false,
                Incident = false,
                Message = false,
                Flag = false,
                KeepSep = false,
                Assoc = false,
                Privileges = false,
            };
            string claimClassify = JsonConvert.SerializeObject(lstClaimClassifySettings);
            return claimClassify;
        }

        public async Task<IdentityResult> UpdateClassify(LogParameters logParameter)
        {
            string classifyDefault = DefaultClassifySettings();
            string userId = _jwtDbContext.UserClaims.Where(w => w.ClaimType == "personnel_id"
                                                               && w.ClaimValue == _personnelId.ToString())
                .Select(s => s.UserId).FirstOrDefault();

            AppUser appUser = _userManager.FindByIdAsync(userId).Result;

            IList<Claim> claims = await _userManager.GetClaimsAsync(appUser);

            Claim classifyClaim = claims.FirstOrDefault(f => f.Type == "Classify_Settings");
            IdentityResult result = classifyClaim == null
                ? await _userManager.AddClaimAsync(appUser, new Claim("Classify_Settings", classifyDefault))
                : await _userManager.ReplaceClaimAsync(appUser, classifyClaim,
                     new Claim("Classify_Settings", JsonConvert.SerializeObject(logParameter)));
            return result;
        }

        //  To get Details of Inmate Classification for tree plus click
        public List<ClassLog> GetInmateClassify(int inmateId)
        {
            // To Get InmateClassification details 
            List<ClassLog> lstInmateClassification =
                _context.InmateClassification.Where(i => i.InmateId == inmateId).Select(ic => new ClassLog
                {
                    ClassType = ic.InmateClassificationType,
                    ClassDate = ic.InmateDateAssigned,
                    ClassNarrative = ic.InmateClassificationReason,
                    OfficerId = ic.ClassificationOfficerId,
                    Id = ic.InmateClassificationId,
                    Reason = ic.InmateClassificationReason
                }).ToList();
            //To get InmateClassificationLinkXref details
            lstInmateClassification.AddRange(_context.InmateClassificationLinkXref.Where(i => i.InmateId == inmateId)
                .Select(xref => new ClassLog
                {
                    ClassDate = xref.InmateClassificationLink.CreateDate.Value,
                    ClassType = ClassTypeConstants.LINK,
                    Type = xref.InmateClassificationLink.LinkType.HasValue.ToString(),
                    Note = xref.InmateClassificationLink.LinkNote,
                    OfficerId = xref.InmateClassificationLink.CreateBy
                }));
            //To get List of Officer Ids
            List<int> officerIds = lstInmateClassification.GroupBy(g => g.OfficerId).Select(i => i.Key).ToList();
            //To get Officer Details List
            List<PersonnelVm> arrestOfficer = _personService.GetPersonNameList(officerIds.ToList());

            lstInmateClassification.ForEach(lstClassLog =>
            {
                if (lstClassLog.OfficerId > 0)
                {
                    //To Get Officer Details
                    lstClassLog.OfficerDetails =
                        arrestOfficer.Single(arr => arr.PersonnelId == lstClassLog.OfficerId);
                }
            });
            return lstInmateClassification;
        }
    }
}