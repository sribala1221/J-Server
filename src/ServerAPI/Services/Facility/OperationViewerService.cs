﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using GenerateTables.Models;
using ServerAPI.ViewModels;
using ServerAPI.Utilities;
using Newtonsoft.Json;
using JwtDb.Models;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace ServerAPI.Services
{
    public class OperationViewerService : IOperationViewerService
    {
        private readonly ICommonService _commonService;
        private readonly AAtims _context;
        private readonly int _personnelId;
        private int _attendanceId;
        private FacilityViewerVm _objFacility;
        private List<HousingUnit> _housingUnitLst;
        private List<HousingUnit> _housingUnitLoc;
        private List<Privileges> _privilegesLst;
        private List<Lookup> _lookUp;
        private List<int> _housingUnitListIds;
        private readonly IPersonService _personService;
        private IQueryable<HousingUnitMoveHistory> _lstHousingUnitMoveHistory;
        private IQueryable<InmateTrak> _lstInmateTrack;
        private IQueryable<CellLog> _lstCellLog;
        private IQueryable<FloorNotes> _lstNotes;
        private IQueryable<FloorNoteXref> _lstFloorNoteXref;
        private IQueryable<SafetyCheck> _lstSafetyCheck;
        private readonly JwtDbContext _jwtDbContext;
        private readonly UserManager<AppUser> _userManager;

        private const int FALSE = 0;

        public OperationViewerService(AAtims context, ICommonService commonService,
            IHttpContextAccessor httpContextAccessor, IPersonService personService, JwtDbContext jwtDbContext,
            UserManager<AppUser> userManager)
        {
            _commonService = commonService;
            _context = context;
            _personnelId = Convert.ToInt32(httpContextAccessor.HttpContext.User.FindFirst("personnelId")?.Value);
            _personService = personService;
            _jwtDbContext = jwtDbContext;
            _userManager = userManager;
        }

        //PageLoad
        public async Task<FacilityViewerVm> GetFacilityViewerInit(int facilityId, bool isOperationViewer)
        {
            ViewerParameter objInput = new ViewerParameter
            {
                FacilityId = facilityId,
                Hours = 12,
                FilterSetting = await GetUserSettingDetails(),
                IsOperationViewer = isOperationViewer
            };
            if (objInput.FilterSetting == null)
            {
                objInput.FilterSetting = new ViewerFilter();
            }

            //Load Grid Details
            await GetFacilityViewerRefresh(objInput);

            return _objFacility;
        }

        //Refresh
        public async Task<FacilityViewerVm> GetFacilityViewerRefresh(ViewerParameter objInput)
        {
            _objFacility = new FacilityViewerVm
            {
                FilterSetting = objInput.FilterSetting ?? (objInput.IsCellViewerInit ?  await GetUserSettingDetails() : new ViewerFilter())
            };
            if (_objFacility.FilterSetting == null)
            {
                _objFacility.FilterSetting = new ViewerFilter();
            }
            if (objInput.FilterSetting == null)
            {
                objInput.FilterSetting = _objFacility.FilterSetting;
            }

            _housingUnitListIds = new List<int>();

            if (objInput.HousingUnitGroupId > 0)
            {
                _housingUnitListIds = _context.HousingGroupAssign.Where(h => h.HousingGroupId == objInput.HousingUnitGroupId && h.HousingUnitListId.HasValue)
                    .Select(s => s.HousingUnitListId ?? 0).ToList();
            }
            else
            {
                if (objInput.HousingUnitListId > 0)
                {
                    _housingUnitListIds.Add(objInput.HousingUnitListId);
                }

            }

            if (!objInput.All)
            {
                if (objInput.Hours > 0)
                {
                    objInput.FromDate = DateTime.Now.AddHours(-objInput.Hours);
                    objInput.ToDate = DateTime.Now;
                }
                else if (objInput.FromDate != null && objInput.ToDate != null)
                {
                    objInput.FromDate = objInput.FromDate.Value.Date;
                    objInput.ToDate = objInput.ToDate.Value.Date.AddDays(1).AddTicks(-1);
                }
            }

            //get attendance 
            if (objInput.OfficerId > 0)
            {
                _attendanceId = _context.AttendanceAo.Where(f => f.PersonnelId == objInput.OfficerId)
                    .Select(s => s.AttendanceAoId).DefaultIfEmpty().Max();
            }

            //Get FNotesRefID For filter

            //To get Lookup list 
            GetLookUpList();

            // Get DropDown Lookup
            GetLookUpDetails(objInput);
            // Get records and it's Count's 
            DatabaseCallSection(objInput);
            if (objInput.IsOperationViewer)
            {
                GetAttendanceDetails(objInput);
            }


            _objFacility.Option =
                _commonService.GetSiteOptionValue(string.Empty, SiteOptionsConstants.ALLOWNOTEEDIT);

            return _objFacility;

        }

        private void GetLookUpList() =>
            _lookUp = _context.Lookup.Where(x =>
                (x.LookupType == LookupConstants.NOTETYPECELL ||
                 x.LookupType == LookupConstants.NOTETYPEINMATE ||
                 x.LookupType == LookupConstants.NOTETYPELOC ||
                 x.LookupType == LookupConstants.NOTETYPEGEN ||
                 x.LookupType == "ATTENDSTAT") && x.LookupInactive == 0
            ).Select(s => new Lookup
            {
                LookupIndex = s.LookupIndex,
                LookupType = s.LookupType,
                LookupDescription = s.LookupDescription,
            }).ToList();

        #region DropDownsValues

        private void GetLookUpDetails(ViewerParameter filters)
        {
            FilterDropDown(filters);
            GridDropDown(filters);
        }

        private void FilterDropDown(ViewerParameter filters)
        {
            // Filters DD's
            // Housing Details
            IQueryable<HousingUnit> housingUnit =
                _context.HousingUnit.Where(hu => !hu.HousingUnitInactive.HasValue || hu.HousingUnitInactive == 0);
            _housingUnitLst = filters.FacilityId > 0
                ? housingUnit.Where(hu => hu.FacilityId == filters.FacilityId).ToList()
                : housingUnit.ToList();

            //location
            _objFacility.HousingUnitLoc = _housingUnitLst.Select(s => s.HousingUnitLocation.Trim()).OrderBy(o => o).Distinct().ToList();

            //number
            if (filters.HousingUnitLocation != null)
            {
                _housingUnitLst = _housingUnitLst.Where(hu =>
                    !string.IsNullOrWhiteSpace(hu.HousingUnitLocation) &&
                    hu.HousingUnitLocation.Trim() == filters.HousingUnitLocation).ToList();

                _objFacility.HousingUnitNum = _housingUnitLst
                .GroupBy(g => new { g.HousingUnitListId, g.HousingUnitLocation, g.HousingUnitNumber })
                .Select(s => new HousingDropDown
                {
                    HousingUnitListId = s.Key.HousingUnitListId,
                    HousingUnitLocation = s.Key.HousingUnitLocation,
                    HousingUnitNumber = s.Key.HousingUnitNumber
                }).OrderBy(o => o.HousingUnitLocation).ThenBy(t => t.HousingUnitNumber).Distinct().ToList();
            }

            //bed/cell number
            _housingUnitLoc = _housingUnitLst;
            if (filters.HousingUnitListId > 0)
            {
                _housingUnitLst = _housingUnitLst.Where(hu =>
                    hu.HousingUnitListId == filters.HousingUnitListId).ToList();

                _objFacility.HousingUnitBed = _housingUnitLst
                .Where(hu => !string.IsNullOrWhiteSpace(hu.HousingUnitBedNumber)).GroupBy(g =>
                    new { g.HousingUnitLocation, g.HousingUnitNumber, g.HousingUnitBedNumber })
                .Select(s => new HousingDropDown
                {
                    HousingUnitLocation = s.Key.HousingUnitLocation,
                    HousingUnitNumber = s.Key.HousingUnitNumber,
                    HousingUnitBedNumber = s.Key.HousingUnitBedNumber
                }).OrderBy(o => o.HousingUnitLocation).ThenBy(t => t.HousingUnitNumber).ThenBy(t => t.HousingUnitBedNumber).Distinct().ToList();
            }



            //Location Details
            IQueryable<Privileges> privileges =
                _context.Privileges.Where(p => p.InactiveFlag == 0 && p.RemoveFromTrackingFlag == 0);
            _privilegesLst = filters.FacilityId > 0 ? privileges.Where(p =>
                    p.FacilityId == filters.FacilityId || !p.FacilityId.HasValue || p.FacilityId == 0).ToList()
                : privileges.Where(p => !p.FacilityId.HasValue || p.FacilityId == 0 || true).ToList();
            _objFacility.LocationNoteType1 = _privilegesLst
                .Select(pl => new Privilege { Id = pl.PrivilegeId, Description = pl.PrivilegeDescription })
                .ToList();
        }

        private void GridDropDown(ViewerParameter filters)
        {
            //Grid DD's
            _objFacility.CellType =
                _lookUp.Where(l => l.LookupType == LookupConstants.NOTETYPECELL)
                    .Select(s => new KeyValuePair<int, string>(s.LookupIndex, s.LookupDescription)).ToList();

            //safety chk
            //housing
            _objFacility.SafetyChkHousing = _housingUnitLoc
                .Select(hsg => new KeyValuePair<int, string>(hsg.HousingUnitListId,
                    hsg.HousingUnitLocation + " " + hsg.HousingUnitNumber))
                .Distinct().ToList();

            //location
            if (filters.HousingUnitLocation != null && filters.HousingUnitListId == 0)
            {
                _privilegesLst = _privilegesLst.Where(p => p.HousingUnitLocation == filters.HousingUnitLocation)
                    .ToList();
            }
            if (filters.HousingUnitListId > 0)
            {
                _privilegesLst = _privilegesLst.Where(p => p.HousingUnitListId == filters.HousingUnitListId).ToList();
            }

            _objFacility.SafetyChkLocation = _privilegesLst
                .Select(sp => new KeyValuePair<int, string>(sp.PrivilegeId, sp.PrivilegeDescription)).Distinct()
                .ToList();
            //safety chk

            _objFacility.InmateNoteType =
                _lookUp.Where(l => l.LookupType == LookupConstants.NOTETYPEINMATE)
                    .Select(s => new KeyValuePair<int, string>(s.LookupIndex, s.LookupDescription)).ToList();

            _objFacility.LocationNoteType2 =
                _lookUp.Where(l => l.LookupType == LookupConstants.NOTETYPELOC)
                    .Select(s => new KeyValuePair<int, string>(s.LookupIndex, s.LookupDescription)).ToList();

            _objFacility.GeneralNoteType =
                _lookUp.Where(l => l.LookupType == LookupConstants.NOTETYPEGEN)
                    .Select(s => new KeyValuePair<int, string>(s.LookupIndex, s.LookupDescription)).ToList();
        }

        #endregion

        #region Operations

        //DB call section
        private void DatabaseCallSection(ViewerParameter objInput)
        {
            _lstHousingUnitMoveHistory =
                _context.HousingUnitMoveHistory.Where(hmh => hmh.HousingUnitToId.HasValue); //Housing In and Out
            _lstInmateTrack = _context.InmateTrak; //Tracking In and Out
            _lstFloorNoteXref = _context.FloorNoteXref; //Inmate Note
            if (objInput.InmateId == 0)
            {
                _lstCellLog = _context.CellLog; //cell log and head count
                //_lstNotes = _context.FloorNotes.Where(fn =>
                // !_context.FloorNoteXref.Any(y => y.FloorNoteId == fn.FloorNoteId) &&
                //    //temporarily removing this condition
                //    //!_floorNotesXrefList.Contains(fn.FloorNoteId) && //Location and General Note
                //    (!string.IsNullOrWhiteSpace(fn.FloorNoteNarrative) ||
                //        !string.IsNullOrWhiteSpace(fn.FloorNoteType))  );

                //_lstNotes = _lstNotes.Where(s => !_context.FloorNoteXref.Any(x => x.FloorNoteId == s.FloorNoteId));

                _lstNotes = _context.FloorNotes.Where(x => !_context.FloorNoteXref.Any(y => y.FloorNoteId == x.FloorNoteId));

                _lstSafetyCheck = _context.SafetyCheck; //Safety Check
            }

            CommonFilterSection(objInput);
        }

        //common filter section
        private void CommonFilterSection(ViewerParameter filters)
        {

            if (filters.FacilityId > 0)
            {
                _lstHousingUnitMoveHistory =
                    _lstHousingUnitMoveHistory.Where(ff => ff.HousingUnitTo.FacilityId == filters.FacilityId);
                _lstInmateTrack = _lstInmateTrack.Where(ff => ff.Inmate.FacilityId == filters.FacilityId);
                _lstCellLog = _lstCellLog?.Where(cl => cl.FacilityId == filters.FacilityId);
                _lstFloorNoteXref = _lstFloorNoteXref.Where(ff => ff.Inmate.FacilityId == filters.FacilityId);
                _lstSafetyCheck = _lstSafetyCheck?.Where(sc => sc.FacilityId == filters.FacilityId);
            }
            if (filters.OfficerId > 0)
            {
                _lstHousingUnitMoveHistory =
                    _lstHousingUnitMoveHistory.Where(of => of.MoveOfficerId == filters.OfficerId);
                _lstCellLog =
                    _lstCellLog?.Where(cl => cl.CellLogOfficerId == filters.OfficerId);
                _lstNotes = _lstNotes?.Where(of => of.FloorNoteOfficerId == filters.OfficerId);
                _lstFloorNoteXref =
                    _lstFloorNoteXref.Where(ff => ff.FloorNote.FloorNoteOfficerId == filters.OfficerId);
                _lstSafetyCheck = _lstSafetyCheck?.Where(sc => sc.CreateBy == filters.OfficerId);
            }
            if (filters.InmateId > 0)
            {
                _lstHousingUnitMoveHistory = _lstHousingUnitMoveHistory.Where(inf => inf.InmateId == filters.InmateId);
                _lstInmateTrack = _lstInmateTrack.Where(inf => inf.InmateId == filters.InmateId);
                _lstFloorNoteXref = _lstFloorNoteXref.Where(ff => ff.InmateId == filters.InmateId);
            }
            if (!string.IsNullOrWhiteSpace(filters.Keyword))
            {
                _lstHousingUnitMoveHistory = _lstHousingUnitMoveHistory.Where(inf =>
                    (inf.HousingUnitTo.HousingUnitLocation.Trim().Contains(filters.Keyword) ||
                     inf.HousingUnitTo.HousingUnitNumber.Trim().Contains(filters.Keyword) ||
                     inf.HousingUnitTo.HousingUnitBedLocation.Trim().Contains(filters.Keyword) ||
                     inf.HousingUnitTo.HousingUnitBedNumber.Trim().Contains(filters.Keyword)));
                _lstInmateTrack = _lstInmateTrack.Where(inf => inf.InmateTrakLocation.Trim().Contains(filters.Keyword));
                _lstCellLog = _lstCellLog?.Where(cl => cl.CellLogComments.Trim().Contains(filters.Keyword));
                _lstNotes = _lstNotes?.Where(ff => ff.FloorNoteNarrative.Trim().Contains(filters.Keyword));
                _lstFloorNoteXref =
                    _lstFloorNoteXref.Where(ff => ff.FloorNote.FloorNoteNarrative.Trim().Contains(filters.Keyword));
                _lstSafetyCheck = _lstSafetyCheck?.Where(sc => sc.SafetyCheckNote.Trim().Contains(filters.Keyword));
            }

            if (_housingUnitListIds.Count > 0)
            {
                _lstHousingUnitMoveHistory = _lstHousingUnitMoveHistory.Where(ff =>
                  _housingUnitListIds.Contains(ff.HousingUnitTo.HousingUnitListId));
                _lstInmateTrack = _lstInmateTrack.Where(ff =>
                 _housingUnitListIds.Contains(ff.Inmate.HousingUnit.HousingUnitListId));
                _lstCellLog = _lstCellLog?.Where(cl => _housingUnitListIds.Contains(cl.HousingUnitListId ?? 0));
                _lstFloorNoteXref = _lstFloorNoteXref.Where(ff =>
                  _housingUnitListIds.Contains(ff.Inmate.HousingUnit.HousingUnitListId));
            }

            GetOperations(filters);

        }

        private void GetOperations(ViewerParameter filters)
        {
            _objFacility.GetCount = new ViewerCount();
            _objFacility.ListViewerDetails = new List<ViewerDetails>();

            //Act
            if (_lstHousingUnitMoveHistory.Any())
            {
                GetHousingIn(filters, _lstHousingUnitMoveHistory);
                GetHousingOut(filters, _lstHousingUnitMoveHistory);
            }
            if (_lstInmateTrack.Any())
            {
                GetTrackingIn(filters, _lstInmateTrack);
                GetTrackingOut(filters, _lstInmateTrack);
            }
            if (_lstFloorNoteXref.Any())
            {
                GetInmateNote(filters, _lstFloorNoteXref);
            }
            if (filters.InmateId == 0)
            {
                if (_lstCellLog.Any())
                {
                    GetCellLogHeadCount(filters, _lstCellLog);
                }
                GetFloorNote(filters, _lstNotes);
                if (_lstSafetyCheck.Any())
                {
                    GetSafetyCheck(filters, _lstSafetyCheck);
                }

            }
            //    GetOfficer();
        }

        private void GetOfficer()
        {
            // To get List of Officer Ids
            List<int> officerIds = _objFacility.ListViewerDetails.Where(c => c.OfficerId > 0).Select(i => i.OfficerId)
                .Distinct().ToList();
            //To get Officer Details List
            List<PersonnelVm> officer = _personService.GetPersonNameList(officerIds.ToList());

            _objFacility.ListViewerDetails.Where(c => c.OfficerId > 0).ToList().ForEach(item =>
            {
                //To get Officer Details for all operation's
                item.Officer = officer
                    .Where(arr => arr.PersonnelId == item.OfficerId).Select(p => new PersonnelVm
                    {
                        PersonLastName = p.PersonLastName,
                        OfficerBadgeNumber = p.OfficerBadgeNumber
                    }).SingleOrDefault();
            });
        }

        #region Housing In and Out

        private void GetHousingIn(ViewerParameter filters, IQueryable<HousingUnitMoveHistory> housingUnitMoveHistoryIn)
        {

            if (!filters.All)
            {
                housingUnitMoveHistoryIn = housingUnitMoveHistoryIn.Where(df =>
                    df.MoveDate >= filters.FromDate && df.MoveDate <= filters.ToDate);
            }

            //count
            _objFacility.GetCount.HousingInCount = housingUnitMoveHistoryIn.Count();

            if (!filters.FilterSetting.HousingIn) return;

            //distinct rows - Get
            List<ViewerDetails> housingInRecords = housingUnitMoveHistoryIn
                .Select(hmh => new ViewerDetails
                {
                    Id = hmh.HousingUnitToId.Value,
                    ViewerType = ViewerType.HOUSINGIN,
                    Date = hmh.MoveDate,
                    Location = hmh.Inmate.InmateCurrentTrack,
                    //narrative
                    HousingUnits = new HousingDetail
                    {
                        HousingUnitListId = hmh.HousingUnitTo.HousingUnitListId,
                        HousingUnitLocation = hmh.HousingUnitTo.HousingUnitLocation,
                        HousingUnitNumber = hmh.HousingUnitTo.HousingUnitNumber,
                        HousingUnitBedNumber = hmh.HousingUnitTo.HousingUnitBedNumber,
                        HousingUnitBedLocation = hmh.HousingUnitTo.HousingUnitBedLocation,
                    },
                    //narrative
                    InmateId = hmh.InmateId,
                    Inmate = new PersonDetailVM
                    {
                        FirstName = hmh.Inmate.Person.PersonFirstName,
                        MiddleName = hmh.Inmate.Person.PersonMiddleName,
                        LastName = hmh.Inmate.Person.PersonLastName
                    },
                    //officer
                    OfficerId = hmh.MoveOfficerId,
                    Officer = new PersonnelVm
                    {
                        PersonLastName = hmh.MoveOfficer.PersonNavigation.PersonLastName,
                        OfficerBadgeNumber = hmh.MoveOfficer.OfficerBadgeNumber
                    },
                    //officer
                    InmateNumber = hmh.Inmate.InmateNumber
                }).Distinct().ToList();

         

            //record
            _objFacility.ListViewerDetails.AddRange(housingInRecords);
        }

        private void GetHousingOut(ViewerParameter filters,
            IQueryable<HousingUnitMoveHistory> housingUnitMoveHistoryOut)
        {
            housingUnitMoveHistoryOut = housingUnitMoveHistoryOut.Where(d => d.MoveDateThru.HasValue);
            if (!filters.All)
            {
                housingUnitMoveHistoryOut = housingUnitMoveHistoryOut.Where(d =>
                    d.MoveDateThru >= filters.FromDate && d.MoveDateThru <= filters.ToDate);
            }

            //count
            _objFacility.GetCount.HousingOutCount = housingUnitMoveHistoryOut.Count();

            if (!filters.FilterSetting.HousingOut) return;

            //distinct rows - Get
            List<ViewerDetails> housingOutRecords = housingUnitMoveHistoryOut
                .Select(hmh => new ViewerDetails
                {
                    Id = hmh.HousingUnitToId.Value,
                    ViewerType = ViewerType.HOUSINGOUT,
                    Date = hmh.MoveDateThru,
                    Location = hmh.Inmate.InmateCurrentTrack,
                    //narrative
                    HousingUnits = new HousingDetail
                    {
                        HousingUnitListId = hmh.HousingUnitTo.HousingUnitListId,
                        HousingUnitLocation = hmh.HousingUnitTo.HousingUnitLocation,
                        HousingUnitNumber = hmh.HousingUnitTo.HousingUnitNumber,
                        HousingUnitBedNumber = hmh.HousingUnitTo.HousingUnitBedNumber,
                        HousingUnitBedLocation = hmh.HousingUnitTo.HousingUnitBedLocation,
                    },
                    //narrative
                    Inmate = new PersonDetailVM
                    {
                        FirstName = hmh.Inmate.Person.PersonFirstName,
                        MiddleName = hmh.Inmate.Person.PersonMiddleName,
                        LastName = hmh.Inmate.Person.PersonLastName
                    },
                    InmateId = hmh.InmateId,
                    //officer
                    OfficerId = hmh.MoveThruBy ?? 0,
                    Officer = new PersonnelVm
                    {
                        PersonLastName = hmh.MoveThruByNavigation.PersonNavigation.PersonLastName,
                        OfficerBadgeNumber = hmh.MoveOfficer.OfficerBadgeNumber
                    },
                    //officer
                    InmateNumber = hmh.Inmate.InmateNumber
                }).Distinct().ToList();

           

            //record
            _objFacility.ListViewerDetails.AddRange(housingOutRecords);
        }

        #endregion

        #region Tracking In and Out

        private void GetTrackingIn(ViewerParameter filters, IQueryable<InmateTrak> lstInmateTrackIn)
        {
            lstInmateTrackIn = lstInmateTrackIn.Where(d => d.InPersonnelId.HasValue);
            lstInmateTrackIn = lstInmateTrackIn.Where(d => d.InmateTrakDateIn.HasValue && d.InmateTrakDateOut.HasValue);
            if (!filters.All)
            {
                lstInmateTrackIn = lstInmateTrackIn.Where(df =>
                    df.InmateTrakDateIn >= filters.FromDate && df.InmateTrakDateIn <= filters.ToDate);
            }
            if (filters.OfficerId > 0)
            {
                lstInmateTrackIn =
                    lstInmateTrackIn.Where(of => of.InPersonnelId == filters.OfficerId);
            }

            if (!filters.FilterSetting.TrackingIn)
            {
                //count
                _objFacility.GetCount.TrackingInCount = lstInmateTrackIn.Count();
            }
            else
            {
                //record
                List<ViewerDetails> trackInRange = lstInmateTrackIn
                    .Select(it => new ViewerDetails
                    {
                        Id = it.InmateTrakId,
                        ViewerType = ViewerType.TRACKINGIN,
                        Date = it.InmateTrakDateIn,
                        Location = it.Inmate.InmateCurrentTrack,
                        //narrative
                        LocTrack = it.InmateTrakLocation,
                        Reason = it.InmateRefused ? it.InmateRefusedReason : string.Empty,
                        //narrative
                        Inmate = new PersonDetailVM
                        {
                            FirstName = it.Inmate.Person.PersonFirstName,
                            MiddleName = it.Inmate.Person.PersonMiddleName,
                            LastName = it.Inmate.Person.PersonLastName
                        },
                        InmateId = it.InmateId,
                        //officer
                        OfficerId = it.InPersonnelId ?? 0,
                        Officer = new PersonnelVm
                        {
                            PersonLastName = it.InPersonnel.PersonNavigation.PersonLastName,
                            OfficerBadgeNumber = it.InPersonnel.OfficerBadgeNumber
                        },
                        //officer
                        InmateNumber = it.Inmate.InmateNumber,
                        PersonId = it.Inmate.PersonId,
                    }).ToList();
                _objFacility.ListViewerDetails.AddRange(trackInRange);

                _objFacility.GetCount.TrackingInCount = trackInRange.Count();
            }

        }

        private void GetTrackingOut(ViewerParameter filters, IQueryable<InmateTrak> lstInmateTrackOut)
        {

            lstInmateTrackOut = lstInmateTrackOut.Where(d =>
                d.InmateTrakDateOut.HasValue && !d.InmateTrakDateIn.HasValue || d.InmateTrakDateIn.HasValue);

            if (!filters.All)
            {
                lstInmateTrackOut = lstInmateTrackOut.Where(df =>
                    df.InmateTrakDateOut >= filters.FromDate && df.InmateTrakDateOut <= filters.ToDate);
            }
            if (filters.OfficerId > 0)
            {
                lstInmateTrackOut = lstInmateTrackOut.Where(of => of.OutPersonnelId == filters.OfficerId);
            }

            if (!filters.FilterSetting.TrackingOut)
            {
                //count
                _objFacility.GetCount.TrackingOutCount = lstInmateTrackOut.Count();
            }
            else
            {
                //record
                List<ViewerDetails> trackOutRange = lstInmateTrackOut
                     .Select(it => new ViewerDetails
                     {
                         Id = it.InmateTrakId,
                         ViewerType = ViewerType.TRACKINGOUT,
                         Date = it.InmateTrakDateOut,
                         Location = it.Inmate.InmateCurrentTrack,
                         //narrative
                         LocTrack = it.InmateTrakLocation,
                         Reason = it.InmateRefused ? it.InmateRefusedReason : string.Empty,
                         //narrative
                         Inmate = new PersonDetailVM
                         {
                             FirstName = it.Inmate.Person.PersonFirstName,
                             MiddleName = it.Inmate.Person.PersonMiddleName,
                             LastName = it.Inmate.Person.PersonLastName
                         },
                         InmateId = it.InmateId,
                         //officer
                         OfficerId = it.OutPersonnelId,
                         Officer = new PersonnelVm
                         {
                             PersonLastName = it.OutPersonnel.PersonNavigation.PersonLastName,
                             OfficerBadgeNumber = it.OutPersonnel.OfficerBadgeNumber
                         },
                         //officer
                         InmateNumber = it.Inmate.InmateNumber,
                         PersonId = it.Inmate.PersonId
                     }).ToList();
                _objFacility.ListViewerDetails.AddRange(trackOutRange);

                _objFacility.GetCount.TrackingOutCount =
                   trackOutRange.Count();
            }

        }

        #endregion

        #region Cell Log and Head Count

        private void GetCellLogHeadCount(ViewerParameter filters, IQueryable<CellLog> cellLog)
        {
            if (filters.LocationId > 0)
            {
                cellLog = cellLog.Where(cl => cl.CellLogLocationId == filters.LocationId);
            }
            cellLog = cellLog.Where(d => filters.DeleteFlag || !d.CellLogDeleteFlag.HasValue || d.CellLogDeleteFlag == FALSE);

            GetCellLog(filters, cellLog);
            GetHeadCount(filters, cellLog);

            if (filters.FilterSetting.CellLog || filters.FilterSetting.HeadCount)
            {
                // GetUpdateBy();
            }
        }

        //Cell Log
        private void GetCellLog(ViewerParameter filters, IQueryable<CellLog> cellLog)
        {

            //Second level filtering
            cellLog = cellLog.Where(cl => !cl.CellLogHeadcountId.HasValue);
            if (!string.IsNullOrWhiteSpace(filters.HousingUnitBedNumber))
            {
                cellLog = cellLog.Where(cl => cl.HousingUnitBedNumber == filters.HousingUnitBedNumber);
            }
            if (!string.IsNullOrWhiteSpace(filters.CellNoteType))
            {
                cellLog = cellLog.Where(cl => cl.CellLogLocation == filters.CellNoteType);
            }
            if (!filters.All)
            {
                cellLog = cellLog.Where(df => df.CellLogDate >= filters.FromDate && df.CellLogDate <= filters.ToDate);
            }

            if (!filters.FilterSetting.CellLog)
            {
                //count
                _objFacility.GetCount.CellLogCount = cellLog.Count();
            }
            else
            {
                //record
                List<ViewerDetails> celllogRange = cellLog.Select(cl => new ViewerDetails
                {
                    Id = cl.CellLogId,
                    ViewerType = ViewerType.CELL,
                    Date = cl.CellLogDate,
                    UpdateDate = cl.UpdateDate,
                    //narrative
                    NoteType = cl.CellLogNoteType,
                    HousingUnits = new HousingDetail
                    {
                        HousingUnitListId = cl.HousingUnitListId,
                        HousingUnitLocation = cl.HousingUnitLocation,
                        HousingUnitNumber = cl.HousingUnitNumber,
                    },
                    Comment = cl.CellLogComments,
                    //narrative
                    //no inmate
                    DeleteFlag = cl.CellLogDeleteFlag == 1,
                    //officer
                    OfficerId = cl.CellLogOfficerId,
                    //officer
                    UpdatedById = cl.UpdateBy ?? 0,
                    UpdatedBy = new PersonnelVm
                    {
                        PersonLastName = cl.UpdateByNavigation.PersonNavigation.PersonLastName,
                        OfficerBadgeNumber = cl.UpdateByNavigation.OfficerBadgeNumber
                    },
                    Officer = new PersonnelVm
                    {
                        PersonLastName = cl.CellLogOfficer.PersonNavigation.PersonLastName,
                        OfficerBadgeNumber = cl.CellLogOfficer.OfficerBadgeNumber
                    },
                }).ToList();
                _objFacility.ListViewerDetails.AddRange(celllogRange);

                _objFacility.GetCount.CellLogCount = celllogRange.Count();
            }

        }

        //Head Count
        private void GetHeadCount(ViewerParameter filters, IQueryable<CellLog> headCount)
        {

            //Second level filtering
            headCount = headCount.Where(cl => cl.CellLogHeadcountId.HasValue);

            if (!filters.All)
            {
                headCount = filters.Hours > 0
                    ? headCount.Where(df => df.UpdateDate >= filters.FromDate
                                            && df.UpdateDate <= filters.ToDate)
                    : headCount.Where(df => df.CellLogDate >= filters.FromDate
                                            && df.CellLogDate <= filters.ToDate);
            }

            if (!filters.FilterSetting.HeadCount)
            {
                //count
                _objFacility.GetCount.HeadCount = headCount.Count();
            }
            else
            {
                //record
                List<ViewerDetails> headcountRange = headCount.Select(cl => new ViewerDetails
                {
                    Id = cl.CellLogId,
                    ViewerType = ViewerType.HEADCOUNT,
                    Date = cl.CreateDate,
                    UpdateDate = cl.UpdateDate,
                    //narrative
                    Comment = cl.CellLogComments,
                    HousingUnits = new HousingDetail
                    {
                        HousingUnitListId = cl.HousingUnitListId,
                        HousingUnitLocation = cl.HousingUnitLocation,
                        HousingUnitNumber = cl.HousingUnitNumber,
                    },
                    LocationId = cl.CellLogLocationId ?? 0,
                    Location = cl.CellLogLocation,
                    Count = cl.CellLogCount ?? 0,
                    //narrative
                    //no inmate
                    DeleteFlag = cl.CellLogDeleteFlag == 1,
                    //officer
                    OfficerId = cl.CellLogOfficerId,
                    //officer
                    ClearedBy = cl.CellLogHeadcount.ClearedBy ?? 0,
                    UpdatedById = cl.UpdateBy ?? 0,
                    UpdatedBy = new PersonnelVm
                    {
                        PersonLastName = cl.UpdateByNavigation.PersonNavigation.PersonLastName,
                        OfficerBadgeNumber = cl.UpdateByNavigation.OfficerBadgeNumber
                    },
                    Officer = new PersonnelVm
                    {
                        PersonLastName = cl.CellLogOfficer.PersonNavigation.PersonLastName,
                        OfficerBadgeNumber = cl.CellLogOfficer.OfficerBadgeNumber
                    },
                }).ToList();
                _objFacility.ListViewerDetails.AddRange(headcountRange);

                _objFacility.GetCount.HeadCount =
                   headcountRange.Count();
            }
        }

        private void GetUpdateBy()
        {
            // To get List of UpdatedBy Ids
            List<int> updateByIds = _objFacility.ListViewerDetails.Where(c => c.UpdatedById > 0)
                .Select(i => i.UpdatedById).Distinct().ToList();
            //To get Officer Details List
            List<PersonnelVm> arrestOfficer = _personService.GetPersonNameList(updateByIds.ToList());

            _objFacility.ListViewerDetails.Where(c => c.ViewerType == ViewerType.CELL ||
                            c.ViewerType == ViewerType.HEADCOUNT && c.UpdatedById > 0).ToList().ForEach(note =>
                            {
                                note.UpdatedBy = arrestOfficer.Where(f => f.PersonnelId == note.UpdatedById).Select(s =>
                                    new PersonnelVm
                                    {
                                        PersonLastName = s.PersonLastName,
                                        OfficerBadgeNumber = s.OfficerBadgeNumber
                                    }).SingleOrDefault();
                            });
        }

        #endregion

        #region Location and General note

        private void GetFloorNote(ViewerParameter filters, IQueryable<FloorNotes> notes)
        {
            if (filters.LocationId > 0)
            {
                notes = notes.Where(ff => ff.FloorNoteLocationId == filters.LocationId);
            }
            if (!filters.All)
            {
                notes = notes.Where(df =>
                    df.FloorNoteDate >= filters.FromDate && df.FloorNoteDate <= filters.ToDate);
            }
            notes = notes.Where(d => filters.DeleteFlag || !d.FloorDeleteFlag.HasValue || d.FloorDeleteFlag == FALSE);

            GetLocationNote(filters, notes);
            GetGeneralNote(filters, notes);

        }

        //Location
        private void GetLocationNote(ViewerParameter filters, IQueryable<FloorNotes> locationNotes)
        {
            locationNotes = locationNotes.Where(l => l.FloorNoteLocationId.HasValue);
            if (filters.FacilityId > 0)
            {
                locationNotes = locationNotes.Where(ff =>
                    ff.FloorNoteLocationNavigation.FacilityId == filters.FacilityId ||
                    !ff.FloorNoteLocationNavigation.FacilityId.HasValue);
            }
            if (!string.IsNullOrWhiteSpace(filters.LocationNoteType))
            {
                locationNotes = locationNotes.Where(ff => ff.FloorNoteType == filters.LocationNoteType);
            }

            if (!filters.FilterSetting.LocationNote)
            {
                //count
                _objFacility.GetCount.LocationNoteCount = locationNotes.Count();
            }
            else
            {
                //record
                List<ViewerDetails> locationNotesRange = locationNotes
                    .Select(f => new ViewerDetails
                    {
                        Id = f.FloorNoteId,
                        ViewerType = ViewerType.LOCATIONNOTE,
                        Date = f.FloorNoteDate,
                        UpdateDate = f.UpdateDate.Value,
                        //narrative
                        Location = f.FloorNoteLocation,
                        NoteType = f.FloorNoteType,
                        Narrative = f.FloorNoteNarrative,
                        //narrative
                        //no inmate
                        DeleteFlag = f.FloorDeleteFlag == 1,
                        //officer
                        OfficerId = f.FloorNoteOfficerId,
                        Officer = new PersonnelVm
                        {
                            PersonLastName = f.FloorNoteOfficer.PersonNavigation.PersonLastName,
                            OfficerBadgeNumber = f.FloorNoteOfficer.OfficerBadgeNumber
                        }
                        //officer                   
                    }).ToList();
                _objFacility.ListViewerDetails.AddRange(locationNotesRange);

                _objFacility.GetCount.LocationNoteCount = locationNotesRange.Count();
            }

        }

        //General note
        private void GetGeneralNote(ViewerParameter filters, IQueryable<FloorNotes> generalNotes)
        {
            generalNotes = generalNotes.Where(l => !l.FloorNoteLocationId.HasValue);
            if (!string.IsNullOrWhiteSpace(filters.GeneralNoteType))
            {
                generalNotes = generalNotes.Where(ff => ff.FloorNoteType == filters.GeneralNoteType);
            }

            if (!filters.FilterSetting.GeneralNote)
            {
                //count
                _objFacility.GetCount.GeneralNoteCount = generalNotes.Count();
            }
            else
            {
                //record
                List<ViewerDetails> generalNotesRange = generalNotes
                     .Select(f => new ViewerDetails
                     {
                         Id = f.FloorNoteId,
                         ViewerType = ViewerType.GENERALNOTE,
                         Date = f.FloorNoteDate,
                         UpdateDate = f.UpdateDate.Value,
                         Location = f.FloorNoteLocation,
                        //narrative
                        NoteType = f.FloorNoteType,
                         Narrative = f.FloorNoteNarrative,
                        //narrative
                        //no-inmate
                        DeleteFlag = f.FloorDeleteFlag == 1,
                        //officer
                        OfficerId = f.FloorNoteOfficerId,
                         Officer = new PersonnelVm
                         {
                             PersonLastName = f.FloorNoteOfficer.PersonNavigation.PersonLastName,
                             OfficerBadgeNumber = f.FloorNoteOfficer.OfficerBadgeNumber
                         }
                        //officer
                    }).ToList();
                _objFacility.ListViewerDetails.AddRange(generalNotesRange);

                _objFacility.GetCount.GeneralNoteCount = generalNotesRange.Count();
            }

        }

        #endregion

        //Inmate Note
        private void GetInmateNote(ViewerParameter filters, IQueryable<FloorNoteXref> floorNoteXref)
        {
            if (filters.LocationId > 0)
            {
                floorNoteXref =
                    floorNoteXref.Where(ff => ff.FloorNote.FloorNoteLocationId == filters.LocationId);
            }
            if (!string.IsNullOrWhiteSpace(filters.InmateNoteType))
            {
                floorNoteXref = floorNoteXref.Where(ff => ff.FloorNote.FloorNoteType == filters.InmateNoteType);
            }
            if (!filters.All)
            {
                floorNoteXref = floorNoteXref.Where(df => df.FloorNote.FloorNoteDate >= filters.FromDate
                                                          && df.FloorNote.FloorNoteDate <= filters.ToDate);
            }

            floorNoteXref = floorNoteXref.Where(d => filters.DeleteFlag ||
                !d.FloorNote.FloorDeleteFlag.HasValue || d.FloorNote.FloorDeleteFlag == FALSE);

            //distinct row - Get
            List<ViewerDetails> floorNoteXrefRecords = floorNoteXref
                .Select(f => new ViewerDetails
                {
                    Id = f.FloorNoteId,
                    ViewerType = ViewerType.INMATENOTE,
                    Date = f.FloorNote.FloorNoteDate,
                    UpdateDate = f.UpdateDate,
                    Location = f.Inmate.InmateCurrentTrack,
                    //narrative
                    FloorLocation = f.FloorNote.FloorNoteLocation,
                    NoteType = f.FloorNote.FloorNoteType,
                    Narrative = f.FloorNote.FloorNoteNarrative,
                    LocationId = f.FloorNote.FloorNoteLocationId ?? 0,
                    //narrative
                    Inmate = new PersonDetailVM
                    {
                        FirstName = f.Inmate.Person.PersonFirstName,
                        MiddleName = f.Inmate.Person.PersonMiddleName,
                        LastName = f.Inmate.Person.PersonLastName
                    },
                    InmateId = f.InmateId,
                    DeleteFlag = f.FloorNote.FloorDeleteFlag == 1,
                    //officer
                    OfficerId = f.FloorNote.FloorNoteOfficerId,
                    Officer = new PersonnelVm
                    {
                        PersonLastName = f.FloorNote.FloorNoteOfficer.PersonNavigation.PersonLastName,
                        OfficerBadgeNumber = f.FloorNote.FloorNoteOfficer.OfficerBadgeNumber
                    },
                    //officer
                    PersonId = f.Inmate.PersonId,
                    InmateNumber = f.Inmate.InmateNumber
                }).Distinct().ToList();

            //count
            _objFacility.GetCount.InmateNoteCount = floorNoteXrefRecords.Count();

            if (!filters.FilterSetting.InmateNote) return;

            //record
            _objFacility.ListViewerDetails.AddRange(floorNoteXrefRecords);

        }

        //Safety Check
        private void GetSafetyCheck(ViewerParameter filters, IQueryable<SafetyCheck> safetyCheck)
        {

            if (filters.SafetyCkkHousingUnitListId > 0)
            {
                safetyCheck =
                    safetyCheck.Where(sc => sc.HousingUnitListId == filters.SafetyCkkHousingUnitListId);
            }

            if (!string.IsNullOrWhiteSpace(filters.SafetyCheckLocation))
            {
                safetyCheck = safetyCheck.Where(sc => sc.Location == filters.SafetyCheckLocation);
            }

            if (!filters.All)
            {
                safetyCheck = safetyCheck.Where(df =>
                    df.CreateDate >= filters.FromDate && df.CreateDate <= filters.ToDate);
            }

            safetyCheck = safetyCheck.Where(d => filters.DeleteFlag || !d.DeleteFlag.HasValue || d.DeleteFlag == FALSE);

            if (!filters.FilterSetting.SafetyCheck)
            {
                //count
                _objFacility.GetCount.SafetyCheckCount = safetyCheck.Count();
            }
            else
            {
                //record
                List<ViewerDetails> safetyCheckrange = safetyCheck
                     .Select(sf => new ViewerDetails
                     {
                         Id = sf.SafetyCheckId,
                         ViewerType = ViewerType.SAFETYCHECK,
                         Date = sf.CreateDate,
                         //narrative
                         Narrative = sf.SafetyCheckNote,
                         LocationId = sf.LocationId ?? 0,
                         Location = sf.Location,
                         SafetyCheckOccured = sf.SafetyCheckOccured,
                         HousingUnits = new HousingDetail
                         {
                             HousingUnitListId = sf.HousingUnitListId,
                             HousingUnitLocation = sf.HousingUnitLocation,
                             HousingUnitNumber = sf.HousingUnitNumber
                         },
                         //narrative
                         //no-inmate
                         DeleteFlag = sf.DeleteFlag == 1,
                         //officer
                         OfficerId = sf.CreateBy,
                         Officer = new PersonnelVm
                         {
                             PersonLastName = sf.CreateByNavigation.PersonNavigation.PersonLastName,
                             OfficerBadgeNumber = sf.CreateByNavigation.OfficerBadgeNumber
                         },
                         //officer
                     }).ToList();
                _objFacility.ListViewerDetails.AddRange(safetyCheckrange);

                _objFacility.GetCount.SafetyCheckCount = safetyCheckrange.Count();
            }


        }

        #endregion

        #region AttendanceAoHistory

        private void GetAttendanceDetails(ViewerParameter filters)
        {
            IQueryable<AttendanceAoHistory> attendanceHistory = _context.AttendanceAoHistory;

            if (!attendanceHistory.Any())
            {
                return;
            }

            if (filters.FacilityId > 0)
            {
                attendanceHistory =
                    attendanceHistory.Where(ah => ah.AttendanceAoLastHousingFacilityId == filters.FacilityId);
            }

            if (_attendanceId > 0)
            {
                attendanceHistory = attendanceHistory.Where(ah => ah.AttendanceAoId == filters.AttendanceId);
            }

            if (attendanceHistory.Any())
            {
                SetHousing(filters, attendanceHistory);
                SetLocation(filters, attendanceHistory);
                OfficerLog(filters, attendanceHistory);
            }

            if (!filters.All)
            {
                attendanceHistory = attendanceHistory.Where(ci =>
                    ci.AttendanceAoStatusDate >= filters.FromDate && ci.AttendanceAoStatusDate <= filters.ToDate);
            }

            if (filters.OfficerId > 0)
            {
                attendanceHistory = attendanceHistory.Where(ah => ah.AttendanceAoStatusOfficerId == filters.OfficerId);
            }

            if (attendanceHistory.Any())
            {
                SetStatus(filters, attendanceHistory);
            }

            if (!string.IsNullOrWhiteSpace(filters.Keyword))
            {
                attendanceHistory = attendanceHistory.Where(ks =>
                    ks.AttendanceAoStatusDate.Value.ToString(CultureInfo.InvariantCulture).Contains(filters.Keyword));
            }

            if (attendanceHistory.Any())
            {
                GetClockIn(filters, attendanceHistory);
                GetClockOut(filters, attendanceHistory);
            }
        }

        private void SetHousing(ViewerParameter filters, IQueryable<AttendanceAoHistory> housing)
        {
            housing = housing.Where(sh => !string.IsNullOrWhiteSpace(sh.AttendanceAoLastHousingLocation) &&
                                          !string.IsNullOrWhiteSpace(sh.AttendanceAoLastHousingNumber));
            if (!filters.All)
            {
                housing = housing.Where(sh =>
                    sh.AttendanceAoLastHousingDate >= filters.FromDate &&
                    sh.AttendanceAoLastHousingDate <= filters.ToDate);
            }
            if (filters.HousingUnitListId > 0)
            {
                housing = housing.Where(sh =>
                    sh.HousingUnitListId == filters.HousingUnitListId);
            }
            if (filters.Keyword != null)
            {
                housing = housing.Where(sh => sh.AttendanceAoLastHousingLocation.Trim().Contains(filters.Keyword) ||
                                              sh.AttendanceAoLastHousingNumber.Trim().Contains(filters.Keyword) ||
                                              sh.AttendanceAoLastHousingNote.Trim().Contains(filters.Keyword));
            }
            if (filters.OfficerId > 0)
            {
                housing = housing.Where(sh => sh.AttendanceAoLastHousingOfficerId == filters.OfficerId);
            }

            if (!filters.FilterSetting.SetHousing)
            {
                //count
                _objFacility.GetCount.SetHousingCount = housing.Count();
            }
            else
            {
                //record
                List<ViewerDetails> housingRange = housing.Select(sh =>
                      new ViewerDetails
                      {
                          Id = sh.AttendanceAoHistoryId,
                          ViewerType = ViewerType.SETHOUSING,
                          Date = sh.AttendanceAoLastHousingDate,
                          //narrative
                          NoteType = sh.AttendanceAoLastHousingNote,
                          Location = sh.AttendanceAoLastHousingLocation,
                          Number = sh.AttendanceAoLastHousingNumber,
                          //narrative
                          //officer
                          OfficerId = sh.AttendanceAoLastHousingOfficerId ?? 0,
                          Officer = new PersonnelVm
                          {
                              PersonLastName = sh.AttendanceAoLastHousingOfficer.PersonNavigation.PersonLastName,
                              OfficerBadgeNumber = sh.AttendanceAoLastHousingOfficer.OfficerBadgeNumber
                          },
                          //officer
                      }).ToList();
                _objFacility.ListViewerDetails.AddRange(housingRange);

                _objFacility.GetCount.SetHousingCount = housingRange.Count();
            }
        }

        private void SetLocation(ViewerParameter filters, IQueryable<AttendanceAoHistory> location)
        {
            location = location.Where(sl => sl.AttendanceAoHistoryLastLocTrackId.HasValue);
            if (!filters.All)
            {
                location = location.Where(sl =>
                    sl.AttendanceAoLastLocDate >= filters.FromDate && sl.AttendanceAoLastLocDate <= filters.ToDate);
            }
            if (filters.LocationId > 0)
            {
                location = location.Where(sl => sl.AttendanceAoHistoryLastLocTrackId == filters.LocationId);
            }
            if (filters.Keyword != null)
            {
                location = location.Where(sl =>
                    sl.AttendanceAoLastLocTrack.Trim().Contains(filters.Keyword) ||
                    sl.AttendanceAoLastLocDesc.Trim().Contains(filters.Keyword));
            }
            if (filters.OfficerId > 0)
            {
                location = location.Where(sl => sl.AttendanceAoLastLocOfficerId == filters.OfficerId);
            }

            if (!filters.FilterSetting.SetLocation)
            {
                //count
                _objFacility.GetCount.SetLocationCount = location.Count();
            }
            else
            {
                //record
                List<ViewerDetails> locationRange = location.Select(sl =>
                      new ViewerDetails
                      {
                          Id = sl.AttendanceAoHistoryId,
                          ViewerType = ViewerType.SETLOCATION,
                          Date = sl.AttendanceAoLastLocDate,
                          //narrative
                          LocDesc = sl.AttendanceAoLastLocDesc,
                          LocTrack = sl.AttendanceAoLastLocTrack,
                          //narrative
                          //officer
                          OfficerId = sl.AttendanceAoLastLocOfficerId ?? 0,
                          Officer = new PersonnelVm
                          {
                              PersonLastName = sl.AttendanceAoLastLocOfficer.PersonNavigation.PersonLastName,
                              OfficerBadgeNumber = sl.AttendanceAoLastLocOfficer.OfficerBadgeNumber
                          },
                          //officer
                      }).ToList();
                _objFacility.ListViewerDetails.AddRange(locationRange);

                _objFacility.GetCount.SetLocationCount = locationRange.Count();
            }
        }

        private void OfficerLog(ViewerParameter filters, IQueryable<AttendanceAoHistory> log)
        {
            log = log.Where(ol => ol.AttendanceAoOfficerLogOfficerId.HasValue);
            if (!filters.All)
            {
                log = log.Where(ol =>
                    ol.AttendanceAoOfficerLogDate >= filters.FromDate &&
                    ol.AttendanceAoOfficerLogDate <= filters.ToDate);
            }
            if (filters.Keyword != null)
            {
                log = log.Where(ol => ol.AttendanceAoOfficerLog.Trim().Contains(filters.Keyword));
            }
            if (filters.OfficerId > 0)
            {
                log = log.Where(ol => ol.AttendanceAoOfficerLogOfficerId == filters.OfficerId);
            }

            if (!filters.FilterSetting.OfficerLog)
            {
                //count
                _objFacility.GetCount.OfficerLogCount = log.Count();
            }
            else
            {
                //record
                List<ViewerDetails> logRange = log.Select(ol =>
                      new ViewerDetails
                      {
                          Id = ol.AttendanceAoHistoryId,
                          ViewerType = ViewerType.OFFICERLOG,
                          Date = ol.AttendanceAoOfficerLogDate,
                          //narrative
                          Narrative = ol.AttendanceAoOfficerLog,
                          //narrative
                          //officer
                          OfficerId = ol.AttendanceAoOfficerLogOfficerId ?? 0,
                          Officer = new PersonnelVm
                          {
                              PersonLastName = ol.AttendanceAoOfficerLogOfficer.PersonNavigation.PersonLastName,
                              OfficerBadgeNumber = ol.AttendanceAoOfficerLogOfficer.OfficerBadgeNumber
                          },
                          //officer
                      }).ToList();
                _objFacility.ListViewerDetails.AddRange(logRange);

                _objFacility.GetCount.OfficerLogCount = logRange.Count();
            }

        }

        private void SetStatus(ViewerParameter filters, IQueryable<AttendanceAoHistory> status)
        {
            status = status.Where(ss =>
                ss.AttendanceAoStatus > 0 && !ss.AttendanceAoStatusNote.Equals(ViewerType.CLOCKIN) &&
                !ss.AttendanceAoStatusNote.Equals(ViewerType.CLOCKOUT));
            if (filters.Keyword != null)
            {
                //To-do lookup description check
                status = status.Where(ss => ss.AttendanceAoLastLocTrack.Trim().Contains(filters.Keyword)
                                            || ss.AttendanceAoStatusNote.Trim().Contains(filters.Keyword) ||
                                            ss.AttendanceAoStatusNote.Trim().Contains(filters.Keyword));
            }

            if (!filters.FilterSetting.SetStatus)
            {
                //count
                _objFacility.GetCount.SetStatusCount = status.Count();
            }
            else
            {
                List<ViewerDetails> statusRange = status.Select(ss =>
                      new ViewerDetails
                      {
                          Id = ss.AttendanceAoHistoryId,
                          ViewerType = ViewerType.SETSTATUS,
                          Date = ss.AttendanceAoStatusDate,
                          //narrative
                          StatusNote = ss.AttendanceAoStatusNote,
                          LocTrack = ss.AttendanceAoLastLocTrack,
                          LocDesc = _lookUp.SingleOrDefault(lkp =>
                              lkp.LookupType == "ATTENDSTAT" && (int?)(lkp.LookupIndex) ==
                              ss.AttendanceAoStatus.GetValueOrDefault()).LookupDescription,
                          //narrative
                          //officer
                          OfficerId = ss.AttendanceAoStatusOfficerId ?? 0,
                          Officer = new PersonnelVm
                          {
                              PersonLastName = ss.AttendanceAoStatusOfficer.PersonNavigation.PersonLastName,
                              OfficerBadgeNumber = ss.AttendanceAoStatusOfficer.OfficerBadgeNumber
                          },
                          //officer
                      }).ToList();
                //record
                _objFacility.ListViewerDetails.AddRange(statusRange);

                _objFacility.GetCount.SetStatusCount = statusRange.Count();
            }

        }

        private void GetClockIn(ViewerParameter filters, IQueryable<AttendanceAoHistory> clockIn)
        {
            clockIn = clockIn.Where(ci =>
                ci.AttendanceAoStatus == 1 && ci.AttendanceAoStatusNote.Equals(ViewerType.CLOCKIN));

            if (!filters.FilterSetting.ClockIn)
            {
                //count
                _objFacility.GetCount.ClockInCount = clockIn.Count();
            }
            else
            {
                //record
                List<ViewerDetails> clockInrange = clockIn
                     .Select(ci => new ViewerDetails
                     {
                         Id = ci.AttendanceAoHistoryId,
                         ViewerType = ViewerType.CLOCKIN,
                         Date = ci.AttendanceAoStatusDate,
                         //narrative
                         NoteType = ci.AttendanceAo.ClockInNote,
                         //narrative
                         //officer
                         OfficerId = ci.AttendanceAoStatusOfficerId ?? 0,
                         Officer = new PersonnelVm
                         {
                             PersonLastName = ci.AttendanceAoStatusOfficer.PersonNavigation.PersonLastName,
                             OfficerBadgeNumber = ci.AttendanceAoStatusOfficer.OfficerBadgeNumber
                         },
                         //officer
                     }).ToList();
                _objFacility.ListViewerDetails.AddRange(clockInrange);

                _objFacility.GetCount.ClockInCount =
                   clockInrange.Count();
            }
        }

        private void GetClockOut(ViewerParameter filters, IQueryable<AttendanceAoHistory> clockOut)
        {
            clockOut = clockOut.Where(co =>
                co.AttendanceAoStatus == 2 && co.AttendanceAoStatusNote.Equals(ViewerType.CLOCKOUT));

            if (!filters.FilterSetting.ClockOut)
            {
                //count
                _objFacility.GetCount.ClockOutCount = clockOut.Count();
            }
            else
            {
                //record
                List<ViewerDetails> clockOutRange = clockOut
                     .Select(co => new ViewerDetails
                     {
                         Id = co.AttendanceAoHistoryId,
                         ViewerType = ViewerType.CLOCKOUT,
                         Date = co.AttendanceAoStatusDate,
                         //narrative
                         NoteType = co.AttendanceAo.ClockOutNote,
                         //narrative
                         //officer
                         OfficerId = co.AttendanceAoStatusOfficerId ?? 0,
                         Officer = new PersonnelVm
                         {
                             PersonLastName = co.AttendanceAoStatusOfficer.PersonNavigation.PersonLastName,
                             OfficerBadgeNumber = co.AttendanceAoStatusOfficer.OfficerBadgeNumber
                         },
                         //officer
                     }).ToList();
                _objFacility.ListViewerDetails.AddRange(clockOutRange);

                _objFacility.GetCount.ClockOutCount =
                    clockOutRange.Count();
            }
        }

        #endregion

        #region Get and Set Checkbox Checks
		public async Task<ViewerFilter> GetUserSettingDetails()
        {
            string classifyDefault = DefaultCellViewerSettings();

            Claim claim = new Claim(CustomConstants.PERSONNELID, _personnelId.ToString());

            AppUser appUser = _userManager.GetUsersForClaimAsync(claim).Result.FirstOrDefault();

            IList<Claim> claims = _userManager.GetClaimsAsync(appUser).Result;

            Claim classifyClaim = claims.FirstOrDefault(f => f.Type == "Cell_Viewer_Settings");

            if (classifyClaim == null)
            {
                await _userManager.AddClaimAsync(appUser, new Claim("Cell_Viewer_Settings", classifyDefault));
            }

            IList<Claim> newClaims = _userManager.GetClaimsAsync(appUser).Result;

            string classifyClaimLog = newClaims.FirstOrDefault(f => f.Type == "Cell_Viewer_Settings")?.Value;

            ViewerFilter logParameters = JsonConvert.DeserializeObject<ViewerFilter>(classifyClaimLog);

            return logParameters;
        }

        public async Task<IdentityResult> UpdateUserSettings(ViewerFilter objFilter)
        {
            string viewerDefault = DefaultCellViewerSettings();
            string userId = _jwtDbContext.UserClaims.Where(w => w.ClaimType == "personnel_id"
                                                               && w.ClaimValue == _personnelId.ToString())
                .Select(s => s.UserId).FirstOrDefault();

            AppUser appUser = _userManager.FindByIdAsync(userId).Result;

            IList<Claim> claims = await _userManager.GetClaimsAsync(appUser);
            Claim classifyClaim = claims.FirstOrDefault(f => f.Type == "Cell_Viewer_Settings");
            IdentityResult result = classifyClaim == null
                ? await _userManager.AddClaimAsync(appUser, new Claim("Cell_Viewer_Settings", viewerDefault))
                : await _userManager.ReplaceClaimAsync(appUser, classifyClaim,
                     new Claim("Cell_Viewer_Settings", JsonConvert.SerializeObject(objFilter)));
            return result;
        }
        private static string DefaultCellViewerSettings()
        {
            ViewerFilter lstClaimClassifySettings = new ViewerFilter
            {
                ClockIn = false,
                ClockOut = false,
                SetHousing = false,
                SetLocation = false,
                SetStatus = false,
                OfficerLog = false,
                CellLog = false,
                HeadCount = false,
                SafetyCheck = false,
                HousingIn = false,
                HousingOut = false,
                TrackingIn = false,
                TrackingOut = false,
                InmateNote = false,
                LocationNote = false,
                GeneralNote = false,
            };
            string claimClassify = JsonConvert.SerializeObject(lstClaimClassifySettings);
            return claimClassify;
        }

        #endregion

        public async Task<int> OperationDelete(DeleteParams deleteParams)
        {
            //To-Do
            //Needs to add permission 
            switch (deleteParams.Type)
            {
                case ViewerType.CELL:
                    CellLog cellLogDelete = _context.CellLog.SingleOrDefault(a =>
                        a.CellLogId == deleteParams.Id);
                    if (cellLogDelete == null)
                    {
                        return 0;
                    }
                    cellLogDelete.CellLogDeleteFlag = deleteParams.DeleteFlag;
                    cellLogDelete.DeletedBy = _personnelId;
                    cellLogDelete.DeletedDate = DateTime.Now;
                    _context.Update(cellLogDelete);
                    break;
                case ViewerType.LOCATIONNOTE:
                case ViewerType.GENERALNOTE:
                case ViewerType.INMATENOTE:
                    FloorNotes floorNoteDelete =
                        _context.FloorNotes.SingleOrDefault(a =>
                            a.FloorNoteId == deleteParams.Id);
                    if (floorNoteDelete == null)
                    {
                        return 0;
                    }
                    floorNoteDelete.FloorDeleteFlag = deleteParams.DeleteFlag;
                    floorNoteDelete.DeletedBy = _personnelId;
                    floorNoteDelete.DeletedDate = DateTime.Now;
                    _context.Update(floorNoteDelete);

                    break;
                default:
                    return 0;
            }
            return await _context.SaveChangesAsync();
        }
    }
}