using GenerateTables.Models;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using ServerAPI.Utilities;
using ServerAPI.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ServerAPI.Services
{
    public class InmateVisitationService : IInmateVisitationService
    {
        #region Properties

        private readonly AAtims _context;
        private readonly ICommonService _commonService;
        private readonly IPersonIdentityService _personIdentityService;
        private readonly IPersonAddressService _personAddressService;
        private List<VisitorInfo> _personalVistorInfoList;
        private readonly int _personnelId;

        #endregion

        #region Constructor

        public InmateVisitationService(AAtims context, ICommonService commonService,
            IPersonIdentityService personIdentityService, IPersonAddressService personAddressService,
            IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _commonService = commonService;
            _personIdentityService = personIdentityService;
            _personAddressService = personAddressService;
            _personnelId = Convert.ToInt32(httpContextAccessor.HttpContext.User
                .FindFirst("personnelId")?.Value);
        }

        #endregion

        #region Methods

        private List<Lookup> GetLookupValue() => _context.Lookup.Where(
            x =>
                (x.LookupType.Contains(LookupConstants.STATE) ||
                 x.LookupType.Contains(LookupConstants.RELATIONS) ||
                 x.LookupType.Contains(LookupConstants.VISPERIDTYPE) ||
                 x.LookupType.Contains(LookupConstants.VISPROFIDTYPE) ||
                 x.LookupType.Contains(LookupConstants.VISTYPE)) && x.LookupInactive == 0).ToList();

        public VisitorListVm GetDefaultVisitorInfo(SearchVisitorList searchVisitorList)
        {
            VisitorListVm visitorList = GetDefaultInfo(searchVisitorList);
            return visitorList;
        }

        public VisitorListVm GetVisitorList(SearchVisitorList searchVisitorList)
        {
            VisitorListVm visitorList = GetDefaultInfo(searchVisitorList);

            visitorList.VisitorInfo = GetVisitationSearchList(searchVisitorList);

            return visitorList;
        }

        private VisitorCountDetails GetVisitorCountDetails()
        {
            //To get Visitor List Count 

            VisitorCountDetails visitorCount = new VisitorCountDetails();
            return visitorCount;
        }

        public List<VisitorInfo> GetVisitationSearchList(SearchVisitorList searchVisitor)
        {
            //To get Visitor List Based On Search Condition
            IQueryable<VisitorToInmate> visitorInmate = _context.VisitorToInmate;

            if (searchVisitor.InmateId > 0)
            {
                visitorInmate = visitorInmate.Where(v =>
                    v.InmateId == searchVisitor.InmateId);
            }

            if (!string.IsNullOrEmpty(searchVisitor.VisitorFirstName))
            {
                visitorInmate = visitorInmate.Where(v =>
                    v.Visitor.PersonFirstName.StartsWith(searchVisitor.VisitorFirstName));
            }

            if (!string.IsNullOrEmpty(searchVisitor.VisitorLastName))
            {
                visitorInmate = visitorInmate.Where(v =>
                    v.Visitor.PersonLastName.StartsWith(searchVisitor.VisitorLastName));
            }

            if (!string.IsNullOrEmpty(searchVisitor.VisitorMiddleName))
            {
                visitorInmate = visitorInmate.Where(v =>
                    v.Visitor.PersonMiddleName.StartsWith(searchVisitor.VisitorMiddleName));
            }

            if (searchVisitor.VisitorDob.HasValue)
            {
                visitorInmate = visitorInmate.Where(v =>
                    v.Visitor.PersonDob.HasValue);

                visitorInmate = visitorInmate.Where(v =>
                    Convert.ToDateTime(v.Visitor.PersonDob.Value.ToShortDateString()) ==
                    Convert.ToDateTime(searchVisitor.VisitorDob.Value.ToShortDateString()));
            }

			if (searchVisitor.RejectSpecificInmate)
            {
                visitorInmate =
                    visitorInmate.Where(v => v.VisitorNotAllowedFlag == 1);
            }

            if (searchVisitor.VisitorRelationshipId.HasValue)
            {
                visitorInmate = visitorInmate.Where(v =>
                    v.VisitorRelationship == searchVisitor.VisitorRelationshipId);
            }

            _personalVistorInfoList = visitorInmate.Where(l =>
                    l.VisitorId > 0 //&& (searchVisitor.IsActiveVisitor || !l.Visitor.InactiveFlag)
					)
                .Select(x => new VisitorInfo
                {
                    PersonId = x.Visitor.PersonId,
                    VisitorNotes = x.Visitor.VisitorNotes,
                    VistorRejectSpecificInmate = x.VisitorNotAllowedFlag,
                    PersonDetails = _context.Person.Where(p =>
                        p.PersonId == x.Visitor.PersonId).Select(p =>
                        new PersonInfo
                        {
                            PersonFirstName = p.PersonFirstName,
                            PersonLastName = p.PersonLastName,
                            PersonMiddleName = p.PersonMiddleName,
                            PersonSuffix = p.PersonSuffix,
                            PersonSexLast = p.PersonSexLast,
                            PersonDlNumber = p.PersonDlNumber,
                            PersonPhone = p.PersonPhone,
                            PersonDob = p.PersonDob,
                            PersonId = p.PersonId
                        }).FirstOrDefault(),
                }).GroupBy(g=>g.VisitorListId).Select(s=>s.First()).ToList();

            List<Lookup> lstProfType = _context.Lookup.Where(l => l.LookupType == LookupConstants.VISTYPE).ToList();
            List<Lookup> lstSex = _commonService.GetLookupList(LookupConstants.SEX);
            _personalVistorInfoList.ForEach(item =>
            {
                item.VisitorCount = _context.VisitToVisitor.Where(c =>
                    c.PersonId == item.PersonId
                    && !c.Visit.DeleteFlag)
                    .Select(v => v.ScheduleId).Count();

                item.VisitorInmateCount = _context.VisitorToInmate.Count(i =>
                    i.VisitorId == item.VisitorListId
					&& (i.DeleteFlag == 0 || !i.DeleteFlag.HasValue));

                item.VisitorGender = item.PersonDetails.PersonSexLast.HasValue
                    ? lstSex.SingleOrDefault(look => 
                        look.LookupIndex == item.PersonDetails.PersonSexLast.Value)?.LookupDescription
                    : null;

                if (item.AddressId.HasValue)
                {
                    item.VisitorAddress = _context.Address.Where(a =>
                        a.AddressId == item.AddressId).Select(x =>
                        new VisitorAddress
                        {
                            AddressNumber = x.AddressNumber,
                            AddressDirection = x.AddressDirection,
                            AddressStreet = x.AddressStreet,
                            AddressSuffix = x.AddressSuffix,
                            AddressDirectionSuffix = x.AddressDirectionSuffix,
                            AddressUnitType = x.AddressUnitType,
                            AddressUnitNumber = x.AddressUnitNumber
                        }).Single();
                }

                if (item.ProfessionalTypeId.HasValue)
                {
                    item.ProfessionalType = lstProfType.Single(l =>
                        l.LookupIndex == item.ProfessionalTypeId.Value).LookupDescription;
                }
            });
            return _personalVistorInfoList.OrderBy(x => x.PersonDetails.InmateId).ToList();
        }

        public List<AssignedInmateList> GetAssignedInmateList(int visitorListId, bool isActiveInmate)
        {
            //To Get Inmate List Based On the Visitor
            List<AssignedInmateList> assignedInmateList = _context.VisitorToInmate.Where(v =>
                    v.VisitorId == visitorListId && (!isActiveInmate
                        ? (v.DeleteFlag ?? 0) == 0
                        : v.DeleteFlag.HasValue || (v.DeleteFlag ?? 0) == 0))
                .Select(x => new AssignedInmateList
                {
                    VisitorListToInmateId = x.VisitorToInmateId,
                    VisitorCount = _context.VisitToVisitor.Where(y =>
                            y.Visit.InmateId == x.InmateId //&& y.ScheduleVisit.StartDate.HasValue
                            && !y.Visit.DeleteFlag)
                        .Select(y => y.ScheduleId).Count(),
                    InmateId = x.InmateId,
                    PersonId = x.Inmate.PersonId,
                    InmateNumber = x.Inmate.InmateNumber,
                    InmateNotes = x.VisitorNote,
                    RelationShip = _commonService.GetLookupList(LookupConstants.RELATIONS).Where(l =>
                        l.LookupIndex == x.VisitorRelationship).Select(y => y.LookupDescription).SingleOrDefault(),
                    HousingUnitId = x.Inmate.HousingUnitId,
                    Location = x.Inmate.InmateCurrentTrack,
                    InmateDeleteFlag = x.DeleteFlag,
                    RejectSpecificInmate = x.VisitorNotAllowedFlag
                }).ToList();

            assignedInmateList.ForEach(item =>
            {
                item.PersonDetails = _context.Person.Where(p =>
                    p.PersonId == item.PersonId).Select(x => new PersonVm
                {
                    PersonFirstName = x.PersonFirstName,
                    PersonLastName = x.PersonLastName,
                    PersonMiddleName = x.PersonMiddleName,
                    PersonSuffix = x.PersonSuffix,
                    PersonId = x.PersonId
                }).Single();

                HousingUnit housingUnit = 
                    _context.HousingUnit.SingleOrDefault(h => h.HousingUnitId == item.HousingUnitId);
                if (housingUnit is null) return;
                item.HousingUnitLocation = housingUnit.HousingUnitLocation;
                item.HousingUnitNumber = housingUnit.HousingUnitNumber;
            });
            return assignedInmateList;
        }

        public async Task<int> InsertUpdatePersonDetails(PersonalVisitorDetails personalVisitor)
        {
            //To Insert and Update the Person Details
            VisitorPersonal visitInfo = new VisitorPersonal();

            int personId = await _personIdentityService.InsertUpdatePersonDetails(personalVisitor.PersonIdentity);

            if (personalVisitor.VisitorInfo.VisitorListId.HasValue)
            {
                visitInfo.VisitorNotAllowedFlag = personalVisitor.VisitorRejectDetails.RejectAll == 1 ;
                visitInfo.UpdateDate = DateTime.Now;
                visitInfo.PersonOfInterestFlag = personalVisitor.PersonOfInterestDetails.PersonOfInterest == 1;
                visitInfo.VisitorNotAllowedNote =
                    personalVisitor.VisitorRejectDetails.VisitorNotAllowedNote;
                visitInfo.VisitorNotAllowedReason =
                    personalVisitor.VisitorRejectDetails.VisitorNotAllowedReason;
                visitInfo.PersonOfInterestNote =
                    personalVisitor.PersonOfInterestDetails.PersonOfInterestNote;
                visitInfo.PersonOfInterestReason =
                    personalVisitor.PersonOfInterestDetails.PersonOfInterestReason;
                visitInfo.VisitorNotAllowedExpire =
                    personalVisitor.VisitorRejectDetails.VisitorNotAllowedExpireDate;
                visitInfo.PersonOfInterestExpire =
                    personalVisitor.PersonOfInterestDetails.PersonOfInterestExpire;
            }
            else
            {
                visitInfo = personalVisitor.VisitorInfo.IsPersonnel
                    ? _context.VisitorPersonal.SingleOrDefault(v => v.PersonId == personId)
                    : _context.VisitorPersonal.SingleOrDefault(v => v.PersonId == personId);

                if (visitInfo == null)
                {
                    visitInfo = new VisitorPersonal();
                }
                else
                {
                    personalVisitor.VisitorInfo.PersonId = visitInfo.PersonId;
                }
            }

            visitInfo.PersonId = personId;
            visitInfo.VisitorNotes = personalVisitor.VisitorInfo.VisitorNotes;
            visitInfo.CreateBy = _personnelId;

            if (!personalVisitor.VisitorInfo.VisitorListId.HasValue)
            {
                visitInfo.CreateDate = DateTime.Now;
                _context.Visitor.Add(visitInfo);
            }

            if (personalVisitor.VisitorRejectDetails.RejectAll is 1 ||
                !string.IsNullOrEmpty(personalVisitor.VisitorRejectDetails.VisitorNotAllowedNote) ||
                !string.IsNullOrEmpty(personalVisitor.VisitorRejectDetails.VisitorNotAllowedReason) ||
                personalVisitor.VisitorRejectDetails.VisitorNotAllowedExpireDate.HasValue)
            {
                VisitorRejectSaveHistory visitorRejectSaveHistory = new VisitorRejectSaveHistory
                {
                    SaveDate = DateTime.Now,
                    SaveBy = _personnelId,
                    VisitorNotAllowedFlag = personalVisitor.VisitorRejectDetails.RejectAll,
                    VisitorNotAllowedNote =
                        personalVisitor.VisitorRejectDetails.VisitorNotAllowedNote,
                    VisitorNotAllowedReason =
                        personalVisitor.VisitorRejectDetails.VisitorNotAllowedReason,
                    VisitorNotAllowedExpire =
                        personalVisitor.VisitorRejectDetails.VisitorNotAllowedExpireDate
                };
                _context.VisitorRejectSaveHistory.Add(visitorRejectSaveHistory);
            }


            if (personalVisitor.PersonOfInterestDetails.PersonOfInterest is 1 ||
                !string.IsNullOrEmpty(personalVisitor.PersonOfInterestDetails.PersonOfInterestNote) ||
                !string.IsNullOrEmpty(personalVisitor.PersonOfInterestDetails.PersonOfInterestReason) ||
                personalVisitor.PersonOfInterestDetails.PersonOfInterestExpire.HasValue)
            {
                VisitorPersonIntSaveHistory visitorPersonIntSaveHistory = new VisitorPersonIntSaveHistory
                {
                    SaveDate = DateTime.Now,
                    SaveBy = _personnelId,
                    PersonOfInterestFlag = personalVisitor.PersonOfInterestDetails.PersonOfInterest,
                    PersonOfInterestNote =
                        personalVisitor.PersonOfInterestDetails.PersonOfInterestNote,
                    PersonOfInterestReason =
                        personalVisitor.PersonOfInterestDetails.PersonOfInterestReason,
                    PersonOfExpiryDate =
                        personalVisitor.PersonOfInterestDetails.PersonOfInterestExpire
                };
                _context.VisitorPersonIntSaveHistory.Add(visitorPersonIntSaveHistory);
            }

            await _context.SaveChangesAsync();
            return visitInfo.PersonId;
        }

        public async Task<bool> InsertInmateToVisitor(InmateToVisitorInfo inmateToVisitorInfo)
        {
            //To save the Inmate Details
            if (!inmateToVisitorInfo.VisitorListToInmateId.HasValue)
            {
                int visitorToInmateId = inmateToVisitorInfo.IsPersonnel
                    ? _context.VisitorToInmate.Where(v =>
                                v.VisitorId == inmateToVisitorInfo.VisitorListId &&
                                v.InmateId == inmateToVisitorInfo.InmateId &&
                                (v.DeleteFlag == 0 || !v.DeleteFlag.HasValue)
                        )
                        .Select(x => x.VisitorToInmateId).SingleOrDefault()
                    : _context.VisitorToInmate.Where(v =>
                                v.VisitorId == inmateToVisitorInfo.VisitorListId &&
                                v.InmateId == inmateToVisitorInfo.InmateId &&
                                (v.DeleteFlag == 0 || !v.DeleteFlag.HasValue)
                        )
                        .Select(x => x.VisitorToInmateId).SingleOrDefault();

                if (visitorToInmateId > 0)
                {
                    return false;
                }
            }

            VisitorToInmate visitorToInmate = new VisitorToInmate();

            if (inmateToVisitorInfo.VisitorListToInmateId.HasValue)
            {
                visitorToInmate = _context.VisitorToInmate.Single(v =>
                    v.VisitorToInmateId == inmateToVisitorInfo.VisitorListToInmateId.Value);
            }

            visitorToInmate.VisitorId = inmateToVisitorInfo.VisitorListId;
            visitorToInmate.InmateId = inmateToVisitorInfo.InmateId;
            visitorToInmate.VisitorRelationship = inmateToVisitorInfo.VisitorRelationshipId;
            visitorToInmate.VisitorNotAllowedFlag = inmateToVisitorInfo.VisitorRejectDetails.RejectAll;
            visitorToInmate.VisitorNote = inmateToVisitorInfo.VisitorNotes;
            visitorToInmate.VisitorNotAllowedNote =
                inmateToVisitorInfo.VisitorRejectDetails.VisitorNotAllowedNote;
            visitorToInmate.VisitorNotAllowedReason =
                inmateToVisitorInfo.VisitorRejectDetails.VisitorNotAllowedReason;
            visitorToInmate.VisitorNotAllowedExpire =
                inmateToVisitorInfo.VisitorRejectDetails.VisitorNotAllowedExpireDate;
            visitorToInmate.CreateBy = 1;
            visitorToInmate.CreateDate = DateTime.Now;

            if (!inmateToVisitorInfo.VisitorListToInmateId.HasValue)
            {
                _context.VisitorToInmate.Add(visitorToInmate);
            }

            VisitorToInmateHistory visitorToInmateHistory = new VisitorToInmateHistory
            {
                VisitorToInmateId = visitorToInmate.VisitorToInmateId,
                PersonnelId = _personnelId,
                CreateDate = DateTime.Now,
                VisitorToInmateHistoryList = inmateToVisitorInfo.VisitorInmateToHisitoryList
            };
            _context.VisitorToInmateHistory.Add(visitorToInmateHistory);

            if (inmateToVisitorInfo.VisitorRejectDetails.RejectAll.HasValue)
            {
                VisitorRejectSaveHistory visitorRejectSaveHistory = new VisitorRejectSaveHistory
                {
                    VisitorId = inmateToVisitorInfo.VisitorListId ?? 0,
                    VisitorToInmateId = visitorToInmate.VisitorToInmateId,
                    SaveDate = DateTime.Now,
                    SaveBy = _personnelId,
                    VisitorNotAllowedFlag = inmateToVisitorInfo.VisitorRejectDetails.RejectAll,
                    VisitorNotAllowedNote =
                        inmateToVisitorInfo.VisitorRejectDetails.VisitorNotAllowedNote,
                    VisitorNotAllowedReason =
                        inmateToVisitorInfo.VisitorRejectDetails.VisitorNotAllowedReason,
                    VisitorNotAllowedExpire =
                        inmateToVisitorInfo.VisitorRejectDetails.VisitorNotAllowedExpireDate
                };
                _context.VisitorRejectSaveHistory.Add(visitorRejectSaveHistory);
            }

            await _context.SaveChangesAsync();
            return true;
        }

        //To Get RejectAll reason
        public List<KeyValuePair<int, string>> GetRejectReason() =>
            _commonService.GetLookupKeyValuePairs(LookupConstants.VISREJECTREAS);

        //To Get PersonOfInterest Reason
        public List<KeyValuePair<int, string>> GetPersonOfInterest() =>
            _commonService.GetLookupKeyValuePairs(LookupConstants.VISPERSINTREAS);

        public PersonalVisitorDetails GetPersonalVisitorDetails(int visitorListId)
        {
            //To Get the Personal Visitor Details
            Visitor visitor = _context.Visitor.Single(v => v.PersonId == visitorListId);
            PersonalVisitorDetails personalVisitorDetails = new PersonalVisitorDetails
            {
                VisitorInfo = new SearchVisitorList
                {
                    //  VisitorListId = visitor.VisitorId,
                    VisitorNotes = visitor.VisitorNotes,
                },
                PersonIdentity = _personIdentityService.GetPersonDetails(visitor.PersonId),
                PersonAddressDetails = _personAddressService.GetPersonAddressDetails(visitor.PersonId)
            };

            //To get the Person Identity Details

            //To get the Person Address Details
            return personalVisitorDetails;
        }

        public async Task<int> DeletePersonalVisitorDetails(int visitorListId)
        {
            return await _context.SaveChangesAsync();
        }

        public async Task<int> UndoPersonalVisitorDetails(int visitorListId)
        {
            return await _context.SaveChangesAsync();
        }

        public List<VisitorListSaveHistory> GetVisitorListSaveHistory(int visitorListId)
        {
            //To get Visitation Save History
            List<VisitorListSaveHistory> visitationHistoryList = _context.VisitorHistory
                .Where(vlh => vlh.VisitorId == visitorListId).Select(x => new VisitorListSaveHistory
                {
                    VisitorListHistoryId = x.VisitorHistoryId,
                    CreateDate = x.CreateDate,
                    PersonLastName = x.Personnel.PersonNavigation.PersonLastName,
                    OfficerBadgeNumber = x.Personnel.OfficerBadgeNum,
                    VisitorHisitoryList = x.VisitorHistoryList
                }).OrderByDescending(vlh => vlh.CreateDate).ToList();

            //To Get Json Result Into Dictionary
            visitationHistoryList.ForEach(item =>
            {
                if (item.VisitorHisitoryList == null) return;
                Dictionary<string, string> appHistoryList =
                    JsonConvert.DeserializeObject<Dictionary<string, string>>(item.VisitorHisitoryList);
                item.VisitationHeader = appHistoryList.Select(ah =>
                        new VisitationHeader {Header = ah.Key, Detail = ah.Value}).ToList();
            });
            return visitationHistoryList;
        }

        public HistoryList GetHistoryList(SearchVisitorHistoryList searchVisitationHistory)
        {
            //To get the History List
            HistoryList historyList = new HistoryList
            {
                VisitorFlagList = new List<string>
                {
                    VisitorAlerts.REVOKEDVISIT,
                    VisitorAlerts.VISITWEEK,
                    VisitorAlerts.KEEPSEPREGISTER,
                    VisitorAlerts.GENDERREGISTER,
                    VisitorAlerts.EXCLUDECLASSIFY,
                    VisitorAlerts.BYPASSSCHEDULE,
                    VisitorAlerts.CAPACITYALERT,
                    VisitorAlerts.SCHEDULECONFLICT,
                    VisitorAlerts.REJECTALL,
                    VisitorAlerts.REJECTFORINMATE,
                    VisitorAlerts.PERSONOFINTEREST,
                    VisitorAlerts.GANGKEEPSEP,
                    VisitorAlerts.VICTIMCONTACT,
                    VisitorAlerts.ADDITIONALVISITOR,
                    VisitorAlerts.CUSTODY
                },

                VisitorDenyReasonList = _commonService.GetLookupKeyValuePairs(LookupConstants.VISITDENYREAS)
                    .OrderBy(x => x.Value)
                    .ThenBy(x => x.Key).ToList(),

                HistoryInfoList = GetVisitationHistorySearchList(searchVisitationHistory)
            };
            return historyList;
        }

        public List<HistoryInfo> GetVisitationHistorySearchList(SearchVisitorHistoryList searchHistoryList)
        {
            //To get the Visitation History Based on Search Condition
            List<VisitorProfessional> visitorList = _context.VisitorProfessional.ToList();

			IQueryable<VisitToVisitor> visit = _context.VisitToVisitor.Where(w =>
				w.Visit.InmateId == searchHistoryList.InmateId &&
				(searchHistoryList.DateFrom.HasValue && searchHistoryList.DateTo.HasValue
					? w.Visit.StartDate.Date >= searchHistoryList.DateFrom &&
                      w.Visit.StartDate.Date <= searchHistoryList.DateTo
					: w.Visit.StartDate.Date == DateTime.Now.Date)
				&& visitorList.Select(l => l.PersonId).Contains(w.PersonId));

			if (searchHistoryList.DeniedVisit)
            {
                visit = visit.Where(v => v.Visit.VisitDenyFlag == 1);
            }

            if (!string.IsNullOrEmpty(searchHistoryList.DenyReason))
            {
                visit = visit.Where(v => v.Visit.VisitDenyReason == searchHistoryList.DenyReason);
            }

            if (!string.IsNullOrEmpty(searchHistoryList.VisitFlag))
            {
                visit = visit.Where(v =>
					v.Visit.VisitSystemFlagString.ToLower().Contains(searchHistoryList.VisitFlag.ToLower()));
            }


            List <HistoryInfo> historyInfoList = visit.Select(v => new HistoryInfo
            {
                // VisitorId = v.VisitId,
                PersonId = v.PersonId,
                InmateId = v.Visit.InmateId ?? 0,
                VisitorDate = v.Visit.StartDate,
                VisitorTimeIn = v.Visit.StartDate.ToString(DateConstants.HOURSMINUTES),
                VisitorTimeOut = v.Visit.EndDate.HasValue ? v.Visit.EndDate.Value.ToString(DateConstants.HOURSMINUTES) : string.Empty,
                VisitorLocation = v.Visit.LocationDetail,
                //Reason = v.Visit.ReasonId,
                VisitorNotes = v.Visit.Notes,
                VisitorDenyFlag = v.Visit.VisitDenyFlag,
                VisitorDenyReason = v.Visit.VisitDenyReason,
                VisitorSystemAlerts = v.Visit.VisitSystemFlagString,
                VisitorDenyNotes = v.Visit.VisitDenyNote,
                VisitorFirstName = v.Visitor.PersonFirstName,
                VisitorLastName = v.Visitor.PersonLastName,
                VisitorMiddleName = v.Visitor.PersonMiddleName,
                VisitorSuffix = v.Visitor.PersonSuffix,
                VisitorDeleteFlag = v.Visit.DeleteFlag ? 1 : 0,
                //VisitorProfFlag = v.VisitProfessionalFlag
            }).OrderBy(v => v.VisitorDate).ToList();

            historyInfoList.ForEach(item =>
            {
                VisitorProfessional visitordetails = visitorList.FirstOrDefault(l =>
                    l.PersonId == item.PersonId);// && l.InmateId == searchHistoryList.InmateId);

                if (!(visitordetails is null))
                {
                    item.VisitorType = visitordetails.VisitorType.HasValue
                        ? _context.Lookup.SingleOrDefault(l =>
                                l.LookupType == LookupConstants.VISTYPE &&
                                l.LookupIndex == visitordetails.VisitorType.Value)
                            ?.LookupDescription
                        : null;
                    //item.VistorNotAllowedFlag = visitordetails.VisitorNotAllowedFlag ? 1 : 0;
                }
            });
            return historyInfoList;
		}

        public List<VisitationHistory> GetVisitorHistory(int inmateId, int personId)
        {

            IQueryable<VisitToVisitor> visit =
                _context.VisitToVisitor.Where(v => !v.Visit.DeleteFlag && (inmateId > 0
                                                ? v.Visit.InmateId == inmateId : v.PersonId == personId));

            List<VisitationHistory> visitationHistoryList = visit.Select(v => new VisitationHistory
            {
				VisitorDateIn = v.Visit.StartDate,
				VisitorDateOut = v.Visit.EndDate,
				VisitorInfo = new PersonInfo
                {
                    PersonFirstName = v.Visitor.PersonFirstName,
                    PersonLastName = v.Visitor.PersonLastName,
                    PersonMiddleName = v.Visitor.PersonMiddleName,
                    PersonSuffix = v.Visitor.PersonSuffix
                },
                InmateInfo = new PersonInfo
                {
                    PersonFirstName = v.Visitor.PersonFirstName,
                    PersonLastName = v.Visitor.PersonLastName,
                    PersonMiddleName = v.Visitor.PersonMiddleName,
                    PersonSuffix = v.Visitor.PersonSuffix
                },
                VisitorNotes = v.Visit.Notes,
                VisitorDenyFlag = v.Visit.VisitDenyFlag,
                VisitorDenyReason = v.Visit.VisitDenyReason,
                VisitorSystemFlagString = v.Visit.VisitSystemFlagString,
                VisitorDenyNote = v.Visit.VisitDenyNote,
                VisitorCreatedDate = v.Visit.CreateDate
            }).OrderByDescending(v => v.VisitorCreatedDate).ToList();
            return visitationHistoryList;
        }

        public async Task<int> DeleteAssignedInmateDetails(int visitorListId)
        {
            //To delete the Assigned Inmate
            VisitorToInmate visitorToInmate =
                _context.VisitorToInmate.Single(v => v.VisitorToInmateId == visitorListId);
            visitorToInmate.DeleteFlag = 1;
            visitorToInmate.DeleteBy = _personnelId;
            visitorToInmate.DeleteDate = DateTime.Now;
            return await _context.SaveChangesAsync();
        }

        public async Task<int> UndoAssignedInmateDetails(int visitorListId)
        {
            //To undo the Assigned Inmate
            VisitorToInmate visitorToInmate =
                _context.VisitorToInmate.Single(v => v.VisitorToInmateId == visitorListId);
            visitorToInmate.DeleteFlag = 0;
            visitorToInmate.DeleteBy = _personnelId;
            visitorToInmate.DeleteDate = DateTime.Now;
            return await _context.SaveChangesAsync();
        }

        public async Task<int> DeleteVisitationHistory(int visitorId)
        {
			VisitToVisitor visit = _context.VisitToVisitor.SingleOrDefault(v => v.ScheduleId == visitorId);
            if (visit == null) return -1;
            visit.Visit.DeleteFlag = true;
            visit.Visit.DeleteBy = _personnelId;
            visit.Visit.DeleteDate = DateTime.Now;

            return await _context.SaveChangesAsync();
        }

        public List<VisitorListSaveHistory> GetVisitorAssignedListSaveHistory(int visitorId)
        {
            //To get Visitation Save History
            List<VisitorListSaveHistory> visitationHistoryList = _context.VisitorToInmateHistory
                .Where(vlh => vlh.VisitorToInmateId == visitorId).Select(x => new VisitorListSaveHistory
                {
                    VisitorListHistoryId = x.VisitorToInmateHistoryId,
                    CreateDate = x.CreateDate,
                    PersonLastName = x.Personnel.PersonNavigation.PersonLastName,
                    OfficerBadgeNumber = x.Personnel.OfficerBadgeNum,
                    VisitorHisitoryList = x.VisitorToInmateHistoryList
                }).OrderByDescending(x => x.CreateDate).ToList();

            //To Get Json Result Into Dictionary
            visitationHistoryList.ForEach(item =>
            {
                if (item.VisitorHisitoryList == null) return;
                Dictionary<string, string> appHistoryList =
                    JsonConvert.DeserializeObject<Dictionary<string, string>>(item.VisitorHisitoryList);
                item.VisitationHeader = appHistoryList.Select(ah =>
                        new VisitationHeader {Header = ah.Key, Detail = ah.Value}).ToList();
            });
            return visitationHistoryList;
        }

        public InmateToVisitorInfo GetInmateVisitorDetails(int visitorId)
        {
            VisitorToInmate visitorToInmate =
                _context.VisitorToInmate.FirstOrDefault(v => v.VisitorToInmateId == visitorId);
            InmateToVisitorInfo visitorDetails = new InmateToVisitorInfo();
            if (visitorToInmate == null) return visitorDetails;
            visitorDetails.VisitorListToInmateId = visitorToInmate.VisitorToInmateId;
            visitorDetails.VisitorListId = visitorToInmate.VisitorId;
            visitorDetails.InmateId = visitorToInmate.InmateId;
            visitorDetails.VisitorRelationshipId = visitorToInmate.VisitorRelationship;
            visitorDetails.VisitorNotes = visitorToInmate.VisitorNote;
            visitorDetails.VisitorRejectDetails = new VisitorRejectDetails
            {
                VisitorNotAllowedNote = visitorToInmate.VisitorNotAllowedNote,
                VisitorNotAllowedReason = visitorToInmate.VisitorNotAllowedReason,
                VisitorNotAllowedExpireDate = visitorToInmate.VisitorNotAllowedExpire,
                RejectAll = visitorToInmate.VisitorNotAllowedFlag
            };

            return visitorDetails;
        }

        public List<RejectInmateHistory> GetVisitorRejectHistoryList(int visitorId, VisitorRejectFlag visitorFlag, int visitorListId)
        {
            List<RejectInmateHistory> rejectHistoryList = new List<RejectInmateHistory>();

            switch (visitorFlag)
            {
                case VisitorRejectFlag.RejectSpecificInmate:
                    rejectHistoryList = _context.VisitorRejectSaveHistory.Where(v =>
                        visitorId > 0 ? v.VisitorToInmateId == visitorId
                            : v.VisitorId == visitorListId).Select(x => new RejectInmateHistory
                    {
                        SaveDate = x.SaveDate,
                        VisitorRejectExpireDate = x.VisitorNotAllowedExpire,
                        VisitorRejectFlag = x.VisitorNotAllowedFlag,
                        VisitorRejectNote = x.VisitorNotAllowedNote,
                        VisitorRejectReason = x.VisitorNotAllowedReason,
                        PersonLastName = x.SaveByNavigation.PersonNavigation.PersonLastName,
                        OfficerBadgeNumber = x.SaveByNavigation.OfficerBadgeNum
                    }).OrderByDescending(x=> x.SaveDate).ToList();
                    break;
                case VisitorRejectFlag.RejectAll:
                    rejectHistoryList = _context.VisitorRejectSaveHistory.Where(v =>
                        v.VisitorId == visitorId).Select(x => new RejectInmateHistory
                    {
                        SaveDate = x.SaveDate,
                        VisitorRejectExpireDate = x.VisitorNotAllowedExpire,
                        VisitorRejectFlag = x.VisitorNotAllowedFlag,
                        VisitorRejectNote = x.VisitorNotAllowedNote,
                        VisitorRejectReason = x.VisitorNotAllowedReason,
                        PersonLastName = x.SaveByNavigation.PersonNavigation.PersonLastName,
                        OfficerBadgeNumber = x.SaveByNavigation.OfficerBadgeNum
                    }).OrderByDescending(x => x.SaveDate).ToList();
                    break;
                case VisitorRejectFlag.PersonOfInterest:
                    rejectHistoryList = _context.VisitorPersonIntSaveHistory.Where(v =>
                        v.VisitorId == visitorId).Select(x => new RejectInmateHistory
                    {
                        SaveDate = x.SaveDate,
                        VisitorRejectExpireDate = x.PersonOfExpiryDate,
                        VisitorRejectFlag = x.PersonOfInterestFlag,
                        VisitorRejectNote = x.PersonOfInterestNote,
                        VisitorRejectReason = x.PersonOfInterestReason,
                        PersonLastName = x.SaveByNavigation.PersonNavigation.PersonLastName,
                        OfficerBadgeNumber = x.SaveByNavigation.OfficerBadgeNum
                    }).OrderByDescending(x => x.SaveDate).ToList();
                    break;
            }

            return rejectHistoryList;
        }

        #endregion

        #region Common methods

        private VisitorListVm GetDefaultInfo(SearchVisitorList searchVisitorList)
        {
            //To get Visitor List
            List<Lookup> lstLookUp = GetLookupValue();

            VisitorListVm visitorList = new VisitorListVm
            {
                VisitorStateList = lstLookUp.Where(x => x.LookupType == LookupConstants.STATE)
                    .Select(x => new KeyValuePair<int, string>(x.LookupIndex,
                        x.LookupName)).OrderBy(x => x.Value).ToList()
            };

            if (searchVisitorList.IsPersonnel)
            {
                visitorList.VisitorIdTypeList = lstLookUp.Where(x => x.LookupType == LookupConstants.VISPERIDTYPE)
                    .Select(x => new KeyValuePair<int, string>(x.LookupIndex,
                        x.LookupDescription)).OrderBy(x => x.Value).ToList();

                visitorList.VisitorRelationshipList = lstLookUp.Where(x => x.LookupType == LookupConstants.RELATIONS)
                    .Select(x => new KeyValuePair<int, string>(x.LookupIndex,
                        x.LookupDescription)).OrderBy(x => x.Value).ToList();

                visitorList.VisitorCountDetails =
                    GetVisitorCountDetails();
            }
            else
            {
                visitorList.VisitorIdTypeList = lstLookUp.Where(x => x.LookupType == LookupConstants.VISPROFIDTYPE)
                    .Select(x => new KeyValuePair<int, string>(x.LookupIndex,
                        x.LookupDescription)).OrderBy(x => x.Value).ToList();

                visitorList.ProfessionalTypeList = lstLookUp.Where(x => x.LookupType == LookupConstants.VISTYPE)
                    .Select(x => new KeyValuePair<int, string>(x.LookupIndex,
                        x.LookupDescription)).OrderBy(x => x.Value).ToList();
            }

            return visitorList;
        }

        #endregion
    }
}
