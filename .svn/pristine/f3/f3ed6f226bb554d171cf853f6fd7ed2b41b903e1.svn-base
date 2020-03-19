using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using ServerAPI.ViewModels;
using GenerateTables.Models;
using ServerAPI.Utilities;

namespace ServerAPI.Services
{
    public class BatchHousingService : IBatchHousingService
    {
        private readonly AAtims _context;
        private readonly IHousingConflictService _housingConflictService;
        private readonly int _personnelId;
        private List<Privileges> _lstPrivilege;
        private List<ActiveInmates> _lstInmate;
        private readonly IFacilityHousingService _facilityHousingService;

        public BatchHousingService(AAtims context, IHousingConflictService housingConflictService,
            IHttpContextAccessor httpContextAccessor, IFacilityHousingService facilityHousingService)
        {
            _context = context;
            _housingConflictService = housingConflictService;
            _personnelId = Convert.ToInt32(httpContextAccessor.HttpContext.User
                .FindFirst(AuthDetailConstants.PERSONNELID)?.Value);
            _facilityHousingService = facilityHousingService;
        }

        public BatchHousingVm GetBatchHousingDetails(bool externalOnly, int facilityId)
        {
            LoadInmateDetails(externalOnly, facilityId);

            BatchHousingVm batchHousingVm = new BatchHousingVm
            {
                LstHousing = _facilityHousingService.GetHousing(facilityId),

                LstCheckedOutLocation = GetCheckedOutLocationDetails(false),

                LstTransferHousing = GetCheckedOutLocationDetails(true),

                LstFacilityHousing = !externalOnly ? GetFacilityHousingDetails(facilityId) : null
            };

            return batchHousingVm;
        }

        private void LoadInmateDetails(bool externalOnly, int facilityId)
        {
            //Privilege details
            _lstPrivilege = _context.Privileges.Where(w => w.InactiveFlag == 0 && !externalOnly &&
                w.FacilityId == facilityId || !w.FacilityId.HasValue)
                .Select(s => new Privileges
                {
                    PrivilegeId = s.PrivilegeId,
                    TransferFlag = s.TransferFlag
                }).ToList();

            //Active Inmate Details
            _lstInmate = _context.Inmate.Where(w => w.InmateActive == 1)
                .Select(s => new ActiveInmates
                {
                    FacilityId = s.FacilityId,
                    Facility = s.Facility.FacilityAbbr,
                    InmateCurrentTrackId = s.InmateCurrentTrackId,
                    InmateCurrentTrack = s.InmateCurrentTrack,
                    HousingUnitListId = s.HousingUnit.HousingUnitListId,
                    HousingUnitLocation = s.HousingUnit.HousingUnitLocation,
                    HousingUnitNumber = s.HousingUnit.HousingUnitNumber
                }).ToList();
        }

        private List<CheckedOutLocation> GetCheckedOutLocationDetails(bool transfer)
        {
            //Privilege Ids list
            List<int> lstPrivilegeId = transfer
                ? _lstPrivilege.Where(w => w.TransferFlag == 1).Select(s => s.PrivilegeId).ToList()
                : _lstPrivilege.Where(w => w.TransferFlag is null).Select(s => s.PrivilegeId).ToList();

            //To get Checkedout Locations Count
            List<CheckedOutLocation> lstCheckedOutLocation = _lstInmate.Where(w => w.InmateCurrentTrackId.HasValue
                    && lstPrivilegeId.Contains(w.InmateCurrentTrackId ?? 0))
                .GroupBy(g => new { g.InmateCurrentTrackId, g.InmateCurrentTrack })
                .Select(s => new CheckedOutLocation
                {
                    LocationId = s.Key.InmateCurrentTrackId ?? 0,
                    Location = s.Key.InmateCurrentTrack,
                    CheckedOutCount = s.Count()
                }).OrderBy(o => o.Location).ToList();

            return lstCheckedOutLocation;
        }

        private List<FacilityHousing> GetFacilityHousingDetails(int facilityId) =>
            //To get Checked out and Total inmate count based on Housing
            _lstInmate.Where(w => w.FacilityId == facilityId)
                .GroupBy(g => new
                {
                    g.HousingUnitListId,
                    g.HousingUnitLocation,
                    g.HousingUnitNumber
                })
                .OrderBy(o => o.Key.HousingUnitLocation).ThenBy(t => t.Key.HousingUnitNumber)
                .Select(s => new FacilityHousing
                {
                    HousingUnitListId = s.Key.HousingUnitListId,
                    HousingUnitLocation = s.Key.HousingUnitLocation,
                    HousingUnitNumber = s.Key.HousingUnitNumber,
                    Facility = s.Select(x => x.Facility).FirstOrDefault(),
                    InmateCount = s.Count(),
                    CheckedOutCount = s.Count(x => x.InmateCurrentTrackId.HasValue)
                }).ToList();

        public List<InmateVm> GetBatchHousingInmateDetails(int facilityId, int? locationId, int? housingUnitListId) =>
            //To get Inmate details based on Location or Housing
            _context.Inmate.Where(w => w.InmateActive == 1 && (!locationId.HasValue ||
                 w.InmateCurrentTrackId == locationId)
                && (!housingUnitListId.HasValue || w.HousingUnit.HousingUnitListId == housingUnitListId)
                && (locationId.HasValue || housingUnitListId.HasValue || w.HousingUnit == null || w.HousingUnit.HousingUnitId == 0))
                .Select(s => new InmateVm
                {
                    InmateId = s.InmateId,
                    InmateNumber = s.InmateNumber,
                    InmateCurrentTrackId = s.InmateCurrentTrackId,
                    InmateCurrentTrack = s.InmateCurrentTrack,
                    Person = new PersonVm
                    {
                        PersonLastName = s.Person.PersonLastName,
                        PersonFirstName = s.Person.PersonFirstName,
                        PersonMiddleName = s.Person.PersonMiddleName,
                        PersonSuffix = s.Person.PersonSuffix
                    },
                    HousingUnit = new HousingDetail
                    {
                        FacilityAbbr = s.Facility.FacilityAbbr,
                        HousingUnitId = s.HousingUnitId ?? 0,
                        HousingUnitLocation = s.HousingUnit != null ? s.HousingUnit.HousingUnitLocation : default,
                        HousingUnitNumber = s.HousingUnit != null ? s.HousingUnit.HousingUnitNumber : default,
                        HousingUnitBedLocation = s.HousingUnit != null ? s.HousingUnit.HousingUnitBedLocation : default,
                        HousingUnitBedNumber = s.HousingUnit != null ? s.HousingUnit.HousingUnitBedNumber : default
                    },
                    InmateClassification = new InmateClassificationVm
                    {
                        InmateClassificationId = s.InmateClassificationId ?? 0
                    }
                }).ToList();

        public List<HousingCapacityVm> GetHousingBedDetails(int housingUnitListId)
        {
            //To get Housing Bed Number details
            List<HousingCapacityVm> lstHousingBedDetail = _context.HousingUnit.Where(w =>
                    w.HousingUnitListId == housingUnitListId && !string.IsNullOrEmpty(w.HousingUnitBedNumber))
                     .GroupBy(g => new { g.HousingUnitBedNumber, g.HousingUnitBedLocation })
                .Select(s => new HousingCapacityVm
                {
                    HousingId = s.Select(se => se.HousingUnitId).FirstOrDefault(),
                    HousingBedNumber = s.Key.HousingUnitBedNumber,
                    HousingBedLocation = s.Key.HousingUnitBedLocation,
                    Actual = s.Sum(c => c.HousingUnitActualCapacity ?? 0),
                    OutofService = s.Sum(c => c.HousingUnitOutOfService ?? 0)
                }).OrderBy(o => o.HousingBedNumber).ThenBy(t => t.HousingBedLocation).ToList();

            int[] housingUnitIds = lstHousingBedDetail.Select(s => s.HousingId).ToArray();

            int[] lstInmate = _context.Inmate
                .Where(w => w.InmateActive == 1 && housingUnitIds.Contains(w.HousingUnitId ?? 0))
                .Select(s => s.HousingUnitId ?? 0).ToArray();

            lstHousingBedDetail.ForEach(item => item.Assigned = lstInmate.Count(w => w == item.HousingId));

            return lstHousingBedDetail;
        }

        public List<HousingConflictVm> GetHousingConflict(List<HousingInputVm> objHousingInput)
        {
            List<HousingConflictVm> lstHousingConflict = new List<HousingConflictVm>();
            objHousingInput?.ForEach(item =>
            {
                item.ViewerFlag = true;
                List<HousingConflictVm> lstHousingConflict1 = _housingConflictService.GetInmateHousingConflictVm(item);

                lstHousingConflict1.ForEach(f =>
                {
                    f.AssignHousing = new HousingDetail
                    {
                        FacilityId = item.FacilityId,
                        HousingUnitListId = item.HousingUnitListId,
                        HousingUnitLocation = item.HousingLocation,
                        HousingUnitNumber = item.HousingNumber,
                        HousingUnitBedNumber = item.HousingBedNumber,
                        HousingUnitBedLocation = item.HousingBedLocation
                    };

                });

                lstHousingConflict.AddRange(lstHousingConflict1);
            });
            // For total separation conflict check when doing batch housing
            lstHousingConflict.ToList().ForEach(x =>
            {
                if (x.ConflictType == ConflictTypeConstants.TOTALSEPBATCHHOUSINGCONFLICT || x.ConflictType == ConflictTypeConstants.TOTALSEPINMATECONFLICT)
                {
                    HousingInputVm inmateDetail = objHousingInput.First(w => w.InmateId == x.InmateId);
                    List<int> coInmateDetail = objHousingInput.Where(w => w.HousingUnitListId == inmateDetail.HousingUnitListId && w.HousingBedNumber == inmateDetail.HousingBedNumber && w.InmateId != x.InmateId).Select(s => s.InmateId).ToList();
                    if (coInmateDetail.Count > 0)
                    {
                        coInmateDetail.ForEach(item =>
                        {
                            PersonInfoVm inmate = _context.Inmate.Where(w => w.InmateActive == 1 & w.InmateId == item)
                                                .Select(inm => new PersonInfoVm
                                                {
                                                    InmateId = inm.InmateId,
                                                    InmateNumber = inm.InmateNumber,
                                                    PersonLastName = inm.Person.PersonLastName,
                                                    PersonFirstName = inm.Person.PersonFirstName,
                                                    PersonMiddleName = inm.Person.PersonMiddleName
                                                }).SingleOrDefault();
                            if (inmate != null && (lstHousingConflict.Count(c => c.InmateId == item && c.ConflictType == ConflictTypeConstants.TOTALSEPBACTHHOUSINGCONFLICTLIST) == 0))
                            {
                                lstHousingConflict.Add(new HousingConflictVm
                                {
                                    ConflictType = ConflictTypeConstants.TOTALSEPBACTHHOUSINGCONFLICTLIST,
                                    InmateId = item,
                                    InmateNumber = inmate.InmateNumber,
                                    PersonLastName = inmate.PersonLastName,
                                    PersonMiddleName = inmate.PersonMiddleName,
                                    PersonFirstName = inmate.PersonFirstName
                                });
                            }
                        });
                    }
                    else
                    {
                        if (x.ConflictType == ConflictTypeConstants.TOTALSEPBATCHHOUSINGCONFLICT)
                        {
                            lstHousingConflict.Remove(x);
                        }
                    }
                }
            });
            return lstHousingConflict;
        }

        public async Task<int> CreateHousingUnitMoveHistory(List<HousingMoveParameter> objHousingMoveParameter)
        {
            if (objHousingMoveParameter.Count > 0)
            {
                objHousingMoveParameter.ForEach(item =>
                {
                    //Update Housing Unit Id in Inmate table 
                    Inmate inmate = _context.Inmate.Single(w => w.InmateId == item.InmateId);

                    inmate.FacilityId = item.FacilityId;
                    inmate.HousingUnitId = item.HousingUnitToId;

                    //Update HousingUnitMoveHistory Move Thru details
                    HousingUnitMoveHistory updateHousingUnitMoveHis = _context.HousingUnitMoveHistory.FirstOrDefault(
                        w => w.InmateId == item.InmateId && !w.MoveDateThru.HasValue);

                    if (updateHousingUnitMoveHis != null)
                    {
                        updateHousingUnitMoveHis.MoveDateThru = DateTime.Now;
                        updateHousingUnitMoveHis.MoveThruBy = _personnelId;
                    }

                    //Insert HousingUnitMoveHistory
                    HousingUnitMoveHistory insertHousingUnitMoveHis = new HousingUnitMoveHistory
                    {
                        InmateClassificationId = item.InmateClassificationId,
                        InmateId = item.InmateId,
                        HousingUnitFromId = item.HousingUnitFromId == 0 ? default : item.HousingUnitFromId,
                        HousingUnitToId = item.HousingUnitToId == 0 ? default : item.HousingUnitToId,
                        MoveOfficerId = _personnelId,
                        MoveReason = item.MoveReason,
                        MoveDate = DateTime.Now
                    };
                    _context.HousingUnitMoveHistory.Add(insertHousingUnitMoveHis);

                    if (item.CheckIn)
                    {
                        //Update Inmate Track table
                        InmateTrak inmateTrak = _context.InmateTrak.FirstOrDefault(w =>
                            w.InmateId == item.InmateId && !w.InmateTrakDateIn.HasValue);

                        if (inmateTrak != null)
                        {
                            inmateTrak.InmateTrakDateIn = DateTime.Now;
                            //inmateTrak.InmateTrakTimeIn = DateTime.Now.ToString("HH:mm:ss");
                            inmateTrak.InPersonnelId = _personnelId;
                        }

                        //Update InmateCurrentTrack details in Inmate table
                        inmate.InmateCurrentTrack = default;
                        inmate.InmateCurrentTrackId = null;
                    }
                });
            }

            return await _context.SaveChangesAsync();
        }
    }
}
