using GenerateTables.Models;
using System;
using ServerAPI.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ServerAPI.Utilities;
using Microsoft.AspNetCore.Http;

// ReSharper disable once CheckNamespace
namespace ServerAPI.Services
{
    public class SentenceAdjustmentService : ISentenceAdjustmentService
    {
        private readonly AAtims _context;
        private readonly int _personnelId;
        private readonly ICommonService _commonService;
        private readonly IInmateService _inmateService;
        public SentenceAdjustmentService(AAtims context, IHttpContextAccessor httpContextAccessor, ICommonService commonService,
        IInmateService inmateService)
        {
            _context = context;
            _personnelId =
                Convert.ToInt32(httpContextAccessor.HttpContext.User.FindFirst(AuthDetailConstants.PERSONNELID)?.Value);
            _commonService = commonService;
            _inmateService = inmateService;
        }

        public List<AttendanceVm> GetAttendeeList(DateTime dateValue, int inmateId)
        {
            List<AttendanceVm> attendanceList = _context.ArrestSentenceAttendance
                    .Where(a => a.ArrestSentenceAttendanceDay.AttendanceDate.HasValue &&
                                a.ArrestSentenceAttendanceDay.AttendanceDate.Value.Date == dateValue.Date && a.DeleteFlag == 0).Select(a =>
                        new AttendanceVm
                        {
                            ArrestSentenceAttendanceId = a.ArrestSentenceAttendanceId,
                            ArrestId = a.ArrestId,
                            ProgramFlag = a.ProgramFlag == 1,
                            WorkCrewFlag = a.WorkCrewFlag == 1,
                            AttendanceFlag = a.AttendFlag == 1,
                            NoDayForDayFlag = a.NoDayDayFlag == 1,
                            ReCalcComplete = a.ReCalcComplete == 1,
                            AttendanceCredit = a.AttendCredit??0,
                            Note = a.AttendNote,
                            InmateId = a.InmateId,
                            AttendanceDay = new AttendanceDayVm
                            {
                                ArrestSentenceAttendanceDayId=a.ArrestSentenceAttendanceDay.ArrestSentenceAttendanceDayId,
                                AttendanceDate=a.ArrestSentenceAttendanceDay.AttendanceDate
                            } ,
                            DeleteFlag = a.DeleteFlag == 1,
                        }).ToList();
            int[] inmateIds = attendanceList.Select(s => s.InmateId).ToArray();
            List<Incarceration> lstIncarcerations = _context.Incarceration
                .Where(x => inmateIds.Contains(x.InmateId??0)).ToList();

            List<InmateDetailsList> lstInmate = _context.Inmate.Where(x => inmateIds.Contains(x.InmateId))
                .Select(a => new InmateDetailsList
                {
                    InmateId = a.InmateId,
                    FacilityId = a.FacilityId,
                    InmateNumber = a.InmateNumber,
                    PersonFirstName = a.Person.PersonFirstName,
                    PersonLastName = a.Person.PersonLastName,
                    PersonMiddleName = a.Person.PersonMiddleName,
                    PersonDob = a.Person.PersonDob,
                    PersonId = a.PersonId,
                    InmateActive = a.InmateActive,
                }).ToList();

            attendanceList.ForEach(item =>
            {
                item.IncarcerationId = lstIncarcerations
                    .SingleOrDefault(s => s.InmateId == item.InmateId && !s.ReleaseOut.HasValue)?.IncarcerationId;
                item.ReleaseOut = lstIncarcerations.OrderBy(w => w.ReleaseOut)
                                      .FirstOrDefault(w => w.InmateId == item.InmateId)
                                      ?.ReleaseOut!=null;
                item.InmateDetails = lstInmate.SingleOrDefault(s=>s.InmateId==item.InmateId);
                item.NoDayForDaySentence =
                    GetNoDayForDaySentence(item.IncarcerationId ?? 0, item.InmateId);
            });

            attendanceList = inmateId > 0
                ? attendanceList.Where(x => x.InmateId == inmateId).ToList()
                : attendanceList.ToList();

            return attendanceList;
        }
        private bool GetNoDayForDaySentence(int incarcerationId, int inmateId)
        {
            List<NoDayForDayFlagVm> lstArrestSentence = _context.IncarcerationArrestXref
                .Where(w => w.IncarcerationId == incarcerationId)
                .Select(a => new NoDayForDayFlagVm
                {
                    InmateId = a.Incarceration.Inmate.InmateId,
                    DayForDayAllowed = a.Arrest.ArrestSentenceDayForDayAllowed > 0,
                    ReleaseDate = a.ReleaseDate,
                    NoDayForDay = a.Arrest.ArrestSentenceNoDayForDay > 0,
                    DayForDayFixed = a.Arrest.ArrestSentenceMethod.ArrestSentenceDdFixed ??0,
                    DayForDayFactor = a.Arrest.ArrestSentenceMethod.ArrestSentenceDdFactor ??0,
                    DayForDaySql = a.Arrest.ArrestSentenceMethod.ArrestSentenceDdSql
                }).ToList();

            bool isDayForDay =
                lstArrestSentence.Count(w => w.InmateId == inmateId && w.DayForDayAllowed
                                                                    && !w.ReleaseDate.HasValue && !w.NoDayForDay
                                                                    && (w.DayForDayFixed > 0 || w.DayForDayFactor > 0 ||(!string.IsNullOrEmpty(w.DayForDaySql) &&
                                                                        w.DayForDaySql.Length > 0))) > 0;
          return isDayForDay;
        }
        public AttendanceParam GetSentenceAdjDetail(int inmateId)
        {
            List<Lookup> lookups = _context.Lookup.Where(x =>
                x.LookupType == LookupConstants.ARRTYPE || x.LookupType == LookupConstants.BOOKSTAT).ToList();

            List<AttendanceSentenceVm> sentenceVms = _context.IncarcerationArrestXref
                .Where(x => x.Incarceration.Inmate.InmateId == inmateId &&
                            !x.Incarceration.ReleaseOut.HasValue &&
                            x.Incarceration.Inmate.InmateActive == 1).
                Select(x => new AttendanceSentenceVm
                {
                    ArrestId = x.Arrest.ArrestId,
                    InmateId = x.Arrest.InmateId,
                    BookingNumber = x.Arrest.ArrestBookingNo,
                    Type = lookups
                    .Where(l => l.LookupType == LookupConstants.ARRTYPE &&
                                l.LookupIndex == Convert.ToInt32(x.Arrest.ArrestType))
                    .Select(s => s.LookupDescription).FirstOrDefault(),
                    Status = lookups
                    .Where(l => l.LookupType == LookupConstants.BOOKSTAT &&
                                l.LookupIndex == x.Arrest.ArrestBookingStatus)
                    .Select(s => s.LookupName).FirstOrDefault(),
                    SentenceCode = x.Arrest.ArrestSentenceCode ?? 0,
                    ConsecutiveFlag = x.Arrest.ArrestSentenceConsecutiveFlag == 1,
                    StartDate = x.Arrest.ArrestSentenceStartDate,
                    DaysToServe = x.Arrest.ArrestSentenceDaysToServe ?? 0,
                    NoDayForDay = x.Arrest.ArrestSentenceNoDayForDay ?? 0,
                    DayForDayDays = x.Arrest.ArrestSentenceDayForDayDays ?? 0,
                    DayForDayAllowed = x.Arrest.ArrestSentenceDayForDayAllowed ?? 0,
                    ActualDaysToServe = x.Arrest.ArrestSentenceActualDaysToServe ?? 0,
                    ClearDate = x.Arrest.ArrestSentenceReleaseDate,
                    SentenceMethod = x.Arrest.ArrestSentenceMethod.MethodName,
                    ActiveBookingFlag = x.ReleaseDate == null,
                    DdFixed=x.Arrest.ArrestSentenceMethod.ArrestSentenceDdFixed??0,
                    DdFactor = x.Arrest.ArrestSentenceMethod.ArrestSentenceDdDFactor ??0,
                    DdSql = x.Arrest.ArrestSentenceMethod.ArrestSentenceDdSql,
                    ReleasedOut = x.Incarceration.ReleaseOut != null,
                }).ToList(); 

            //Adjustment detail
            List<AttendanceVm> sentenceAdjList = _context.ArrestSentenceAttendance
                .Where(x => x.InmateId == inmateId && x.DeleteFlag==0 && x.Inmate.InmateActive==1).Select(s =>
                      new AttendanceVm
                      {
                          ArrestSentenceAttendanceId = s.ArrestSentenceAttendanceId,
                          ArrestId = s.ArrestId,
                          InmateId = s.InmateId,
                          AttendanceCredit = s.AttendCredit??0,
                          BookingNumber = s.Arrest.ArrestBookingNo,
                          ProgramFlag = s.ProgramFlag == 1,
                          WorkCrewFlag = s.WorkCrewFlag == 1,
                          ReCalcComplete = s.ReCalcComplete == 1,
                          NoDayForDayFlag = s.NoDayDayFlag == 1,
                          DeleteFlag = s.DeleteFlag == 1,
                          AttendanceDay = new AttendanceDayVm
                          {
                              ArrestSentenceAttendanceDayId = s.ArrestSentenceAttendanceDay.ArrestSentenceAttendanceDayId,
                              AttendanceDate = s.ArrestSentenceAttendanceDay.AttendanceDate
                          }
                      }).ToList();
            int[] arrestAttendanceIds = sentenceAdjList.Select(x => x.ArrestSentenceAttendanceId).ToArray();
            List<Incarceration> lstIncarcerations = _context.Incarceration
                .Where(x => x.InmateId == inmateId).ToList();

            sentenceAdjList.AddRange(_context.ArrestSentenceAttendanceArrestXref
                .Where(a => a.Arrest.Inmate.InmateActive==1 &&
                arrestAttendanceIds.Contains(a.ArrestSentenceAttendanceId)).Select(d => new AttendanceVm
                {
                    ArrestSentenceAttendanceId = d.ArrestSentenceAttendanceId,
                    ArrestId = d.ArrestId,
                    InmateId = d.ArrestSentenceAttendance.InmateId,
                    AttendanceCredit = d.ArrestSentenceAttendance.AttendCredit??0,
                    BookingNumber = d.Arrest.ArrestBookingNo,
                    ProgramFlag = d.ArrestSentenceAttendance.ProgramFlag == 1,
                    WorkCrewFlag = d.ArrestSentenceAttendance.WorkCrewFlag == 1,
                    ReCalcComplete = d.ReCalcComplete == 1,
                    NoDayForDayFlag = d.ArrestSentenceAttendance.NoDayDayFlag == 1,
                    DeleteFlag = d.ArrestSentenceAttendance.DeleteFlag == 1,
                    AdditionalFlag = true
                }).ToList());

            sentenceAdjList.ForEach(item =>
            {
                item.IncarcerationId = lstIncarcerations
                    .SingleOrDefault(s => s.InmateId == item.InmateId && !s.ReleaseOut.HasValue)?.IncarcerationId;
                item.NoDayForDaySentence =
                    GetNoDayForDaySentence(item.IncarcerationId ?? 0, item.InmateId);
            });

            AttendanceParam attendance = new AttendanceParam
            {
                LstAttendanceSentenceVm = sentenceVms,
                LstAttendanceVms = sentenceAdjList
            };
            return attendance;
        }

        public async Task<int> UpdateNoDayForDay(int[] attendanceIds)
        {
            List<ArrestSentenceAttendance> sentenceAttendances = _context.ArrestSentenceAttendance
                .Where(x => attendanceIds.Contains(x.ArrestSentenceAttendanceId)).ToList();
            sentenceAttendances.ForEach(item =>
            {
                item.NoDayDayFlag = null;
                item.NoDayDayRemoveDate = DateTime.Now;
                item.NoDayDayRemoveBy = _personnelId;
            });

            return await _context.SaveChangesAsync();
        }
        public async Task<int> UpdateArrestAttendance(AttendanceParam attendanceParam)
        {
            List<ArrestSentenceAttendance> arrestSentenceAttendance = _context.ArrestSentenceAttendance
                .Where(x => attendanceParam.AttendanceIds.Contains(x.ArrestSentenceAttendanceId)).ToList();

            arrestSentenceAttendance.ForEach(item =>
            {
                item.ArrestId = attendanceParam.AttendanceVm.ArrestId;
                item.ArrestAppliedBy = _personnelId;
                item.ArrestAppliedDate = DateTime.Now;
            });

            attendanceParam.LstAttendanceVms.ForEach(item =>
            {
                ArrestSentenceAttendanceArrestXref attendanceArrestXref = new ArrestSentenceAttendanceArrestXref
                {
                    ArrestSentenceAttendanceId = item.ArrestSentenceAttendanceId,
                    ArrestId = item.ArrestId ?? 0,
                    ArrestAppliedBy = _personnelId,
                    ArrestAppliedDate = DateTime.Now
                };
                _context.ArrestSentenceAttendanceArrestXref.Add(attendanceArrestXref);
            });

            return await _context.SaveChangesAsync();
        }


        #region SentAdj_DiscDays
        public DiscDaysVm GetDiscDaysCount(bool showRemoved)
        {
            List<Incarceration> incarcerationList = _context.Incarceration.Where(w => !w.ReleaseOut.HasValue).ToList();
            DiscDaysVm discDaysDetails = new DiscDaysVm
            {
                ApplyListCount = GetCount(true, showRemoved, incarcerationList),
                RecalcListCount = GetCount(false, false, incarcerationList)
            };
            return discDaysDetails;
        }

        private int GetCount(bool isApply, bool showRemoved, List<Incarceration> incarcerationList)
        {
            List<int> discInmateIds = null;
            List<DisciplinaryDays> discInmates = new List<DisciplinaryDays>();

            if (!isApply)
            {
                string sentenceByCharge = _commonService.GetSiteOptionValue(string.Empty, SiteOptionsConstants.SENTENCEBYCHARGE);
                if (sentenceByCharge.ToUpper() == SiteOptionsConstants.ON)
                {
                    discInmateIds = _context.DisciplinarySentDayXref.Where(w =>
                    (w.Crime.ArrestSentenceDisciplinaryDaysFlag.HasValue
                    && w.Crime.ArrestSentenceDisciplinaryDaysFlag == 1)
                    && (!w.DeleteFlag.HasValue || w.DeleteFlag == 0))
                    .Select(s => s.DisciplinaryInmateId ?? 0).ToList();
                }
                else if (sentenceByCharge.ToUpper() == SiteOptionsConstants.OFF)
                {
                    IQueryable<Arrest> arrestList = _context.Arrest.Where(w =>
                    w.ArrestSentenceDisciplinaryDaysFlag == 1);
                    // Get sentDayXref list
                    List<DisciplinarySentDayXref> xrefList = _context.DisciplinarySentDayXref.Where(w =>
                    w.ArrestId.HasValue
                    && (!w.DeleteFlag.HasValue || w.DeleteFlag == 0)).ToList();

                    xrefList.ToList().ForEach(item => {
                        if (!arrestList.Any(a => a.ArrestId == item.ArrestId))
                        {
                            xrefList.Remove(item);
                        }
                    });
                    // Get disciplinary inmate ID from disciplinary sentday xref
                    discInmateIds = xrefList.Select(s => s.DisciplinaryInmateId ?? 0).ToList();
                }

                if (discInmateIds != null && discInmateIds.Count > 0)
                {
                    discInmates = _context.DisciplinaryInmate.Where(w =>
                    (w.DisciplinaryDaysSentFlag == 0
                    && w.DisciplinaryDaysRemoveFlag == 0
                    || (showRemoved && w.DisciplinaryDaysRemoveFlag == 1))
                    && discInmateIds.Contains(w.DisciplinaryInmateId)
                    && (w.InmateId.HasValue && w.Inmate.InmateActive == 1)
                    ).Select(s => new DisciplinaryDays
                    {
                        InmateId = (int)s.InmateId,
                        DateIn = incarcerationList.FirstOrDefault(f=>f.InmateId == s.InmateId).DateIn,
                        DiscIncidentId = s.DisciplinaryIncidentId
                    }).ToList();
                }
            }
            else
            {
                discInmates = _context.DisciplinaryInmate.Where(w =>
                (w.DisciplinaryDaysSentFlag == 1
                && w.DisciplinaryDaysRemoveFlag == 0
                || (showRemoved && w.DisciplinaryDaysRemoveFlag == 1))
                && (w.InmateId.HasValue && w.Inmate.InmateActive == 1)
                ).Select(s => new DisciplinaryDays
                {
                    InmateId = (int)s.InmateId,
                    DateIn = incarcerationList.FirstOrDefault(f => f.InmateId == s.InmateId).DateIn,
                    DiscIncidentId = s.DisciplinaryIncidentId
                }).ToList();
            }

            List<int> discIncidentIds = discInmates.Select(s => s.DiscIncidentId).ToList();
            List<DisciplinaryIncident> disciplinaryIncident = _context.DisciplinaryIncident
                .Where(w => discIncidentIds.Contains(w.DisciplinaryIncidentId))
                .Select(s => new DisciplinaryIncident
                {
                    DisciplinaryIncidentId = s.DisciplinaryIncidentId,
                    CreateDate = s.CreateDate
                }).ToList();
            discInmates.ForEach(item =>
            {
                item.CreateDate = disciplinaryIncident
                    .SingleOrDefault(s => s.DisciplinaryIncidentId == item.DiscIncidentId)
                    ?.CreateDate;
            });
            return discInmates.Count(w => w.CreateDate > w.DateIn && w.CreateDate < DateTime.Now);
        }

        public List<DisciplinaryDays> GetDiscDaysDetails(bool isApply, bool showRemoved)
        {
            List<int> discInmateIds = null;
            List<DisciplinaryDays> discInmates = new List<DisciplinaryDays>();
            List<Incarceration> incarcerationList = _context.Incarceration.Where(w => !w.ReleaseOut.HasValue).ToList();
            // Get lookup details for type discintype
            List<LookupVm> lookUpList = _commonService.GetLookups(
            new[] { LookupConstants.DISCINTYPE, LookupConstants.DISCTYPE }).ToList();
            // Get personnel details
            List<PersonnelVm> personnelList = _context.Personnel.Select(s => new PersonnelVm
            {
                PersonnelId = s.PersonnelId,
                PersonLastName = s.PersonNavigation.PersonLastName,
                OfficerBadgeNumber = s.OfficerBadgeNum
            }).ToList();

            if (!isApply)
            {
                string sentenceByCharge = _commonService.GetSiteOptionValue(string.Empty, SiteOptionsConstants.SENTENCEBYCHARGE);
                if (sentenceByCharge.ToUpper() == SiteOptionsConstants.ON)
                {
                    discInmateIds = _context.DisciplinarySentDayXref.Where(w =>
                    (w.Crime.ArrestSentenceDisciplinaryDaysFlag.HasValue
                    && w.Crime.ArrestSentenceDisciplinaryDaysFlag == 1)
                    && (!w.DeleteFlag.HasValue || w.DeleteFlag == 0))
                    .Select(s => s.DisciplinaryInmateId ?? 0).ToList();
                }
                else if (sentenceByCharge.ToUpper() == SiteOptionsConstants.OFF)
                {
                    IQueryable<Arrest> arrestList = _context.Arrest.Where(w =>
                    w.ArrestSentenceDisciplinaryDaysFlag == 1);
                    // Get sentDayXref list
                    List<DisciplinarySentDayXref> xrefList = _context.DisciplinarySentDayXref.Where(w =>
                    w.ArrestId.HasValue
                    && (!w.DeleteFlag.HasValue || w.DeleteFlag == 0)).ToList();

                    xrefList.ToList().ForEach(item => {
                        if (!arrestList.Any(a => a.ArrestId == item.ArrestId))
                        {
                            xrefList.Remove(item);
                        }
                    });
                    // Get disciplinary inmate ID from disciplinary sentday xref
                    discInmateIds = xrefList.Select(s => s.DisciplinaryInmateId ?? 0).ToList();
                }

                if (discInmateIds != null && discInmateIds.Count > 0)
                {
                    discInmates = _context.DisciplinaryInmate.Where(w =>
                    (w.DisciplinaryDaysSentFlag == 0
                    && w.DisciplinaryDaysRemoveFlag == 0
                    || (showRemoved && w.DisciplinaryDaysRemoveFlag == 1))
                    && discInmateIds.Contains(w.DisciplinaryInmateId)
                    && (w.InmateId.HasValue && w.Inmate.InmateActive == 1)
                    ).Select(s => new DisciplinaryDays
                    {
                        InmateId = (int)s.InmateId,
                        DiscInmateId = s.DisciplinaryInmateId,
                        RemoveFlag = s.DisciplinaryDaysRemoveFlag,
                        IncInvolvedParty = lookUpList.SingleOrDefault(w =>
                        w.LookupType == LookupConstants.DISCINTYPE
                        && Equals(w.LookupIndex, s.DisciplinaryInmateType))
                        .LookupDescription,
                        DiscDays = s.DisciplinaryDays,
                        RemovedBy = personnelList.SingleOrDefault(w => w.PersonnelId == s.DisciplinaryDaysRemoveBy),
                        RemoveDate = s.DisciplinaryDaysRemoveDate,
                        DateIn = incarcerationList.FirstOrDefault(f => f.InmateId == s.InmateId).DateIn,
                        DiscIncidentId = s.DisciplinaryIncidentId
                    }).ToList();
                }
            }
            else
            {
                discInmates = _context.DisciplinaryInmate.Where(w =>
                (w.DisciplinaryDaysSentFlag == 1
                && w.DisciplinaryDaysRemoveFlag == 0
                || (showRemoved && w.DisciplinaryDaysRemoveFlag == 1))
                && (w.InmateId.HasValue && w.Inmate.InmateActive == 1)
                ).Select(s => new DisciplinaryDays
                {
                    InmateId = (int)s.InmateId,
                    DiscInmateId = s.DisciplinaryInmateId,
                    RemoveFlag = s.DisciplinaryDaysRemoveFlag,
                    IncInvolvedParty = lookUpList.SingleOrDefault(w =>
                    w.LookupType == LookupConstants.DISCINTYPE
                    && Equals(w.LookupIndex, s.DisciplinaryInmateType))
                   .LookupDescription,
                    DiscDays = s.DisciplinaryDays,
                    RemovedBy = personnelList.SingleOrDefault(w => w.PersonnelId == s.DisciplinaryDaysRemoveBy),
                    RemoveDate = s.DisciplinaryDaysRemoveDate,
                    DateIn = incarcerationList.FirstOrDefault(f => f.InmateId == s.InmateId).DateIn,
                    DiscIncidentId = s.DisciplinaryIncidentId
                }).ToList();
            }

            List<int> discIncidentIds = discInmates.Select(s => s.DiscIncidentId).ToList();
            List<DisciplinaryIncident> disciplinaryIncident = _context.DisciplinaryIncident
                .Where(w => discIncidentIds.Contains(w.DisciplinaryIncidentId))
                .Select(s => new DisciplinaryIncident
                {
                    DisciplinaryNumber = s.DisciplinaryNumber,
                    DisciplinaryIncidentId = s.DisciplinaryIncidentId,
                    CreateDate = s.CreateDate,
                    DisciplinaryType = s.DisciplinaryType
                }).ToList();
            List<int> lstInmateId = discInmates.Select(s => s.InmateId).ToList();
            List<PersonVm> lstPersonDetails = _inmateService.GetInmateDetails(lstInmateId);
            discInmates.ForEach(item =>
            {
                item.InmateDetail = new PersonVm();
                item.InmateDetail = lstPersonDetails.SingleOrDefault(s => s.InmateId == item.InmateId);
                item.CreateDate = disciplinaryIncident
                    .SingleOrDefault(s => s.DisciplinaryIncidentId == item.DiscIncidentId)
                    ?.CreateDate;
                item.IncidentNumber = disciplinaryIncident
                    .SingleOrDefault(s => s.DisciplinaryIncidentId == item.DiscIncidentId)
                    ?.DisciplinaryNumber;
                item.DisciplinaryType = disciplinaryIncident
                    .SingleOrDefault(s => s.DisciplinaryIncidentId == item.DiscIncidentId)
                    ?.DisciplinaryType;
                item.IncidentType = lookUpList.SingleOrDefault(w =>
                    w.LookupType == LookupConstants.DISCTYPE
                    && Equals(w.LookupIndex, item.DisciplinaryType))
                    ?.LookupDescription;
            });
            discInmates = discInmates.Where(w => w.CreateDate > w.DateIn && w.CreateDate < DateTime.Now).ToList();
            return discInmates;
        }
        public int UpdateDiscInmateRemoveFlag(int discInmateId, bool isUndo)
        {
            DisciplinaryInmate discInmate = _context.DisciplinaryInmate.Find(discInmateId);
            discInmate.DisciplinaryDaysSentFlag = isUndo ? 1 : 0;
            discInmate.DisciplinaryDaysRemoveFlag = isUndo ? 0 : 1;
            discInmate.DisciplinaryDaysRemoveBy = isUndo ? null : (int?) _personnelId;
            discInmate.DisciplinaryDaysRemoveDate = isUndo ? null : (DateTime?) DateTime.Now;   
            return _context.SaveChanges();
        }

        public int GetIncarcerationForSentence(int inmateId, int arrestId)
        {
            return  _context.Incarceration.Single(s=>
            s.InmateId == inmateId && s.IncarcerationArrestXref.Any(a=>a.ArrestId == arrestId)).IncarcerationId;
        }
        #endregion

    }
}
