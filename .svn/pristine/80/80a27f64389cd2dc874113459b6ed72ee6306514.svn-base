using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ServerAPI.ViewModels;
using GenerateTables.Models;
using Microsoft.AspNetCore.Http;
using ServerAPI.Utilities;

namespace ServerAPI.Services
{
    public class InmateCellService : IInmateCellService
    {
        private IQueryable<Inmate> _inmateList;
        private readonly AAtims _context;
        private readonly int _personnelId;
        private readonly IHousingService _housingService;

        private List<int> _housingUnitListIds;
        private IEnumerable<HousingUnit> _listHousingUnit;
        private readonly IPhotosService _photos;

        public InmateCellService(AAtims context, IHttpContextAccessor httpContextAccessor, IHousingService housingService, IPhotosService photosService)
        {
            _context = context;
            _personnelId = Convert.ToInt32(httpContextAccessor.HttpContext.User.FindFirst(AuthDetailConstants.PERSONNELID)?.Value);
            _housingService = housingService;
            _photos = photosService;
        }

        public InmateCellVm GetInmateCell(CellInmateInputs input)
        {
            InmateCellVm inmateCellOutputs = new InmateCellVm();

            _housingUnitListIds = new List<int>();

            if (input.HousingUnitGroupId > 0)
            {
                _housingUnitListIds = _context.HousingGroupAssign.Where(h => h.HousingGroupId == input.HousingUnitGroupId
                && h.HousingUnitListId.HasValue && h.DeleteFlag == 0)
                    .Select(s => s.HousingUnitListId ?? 0).ToList();
            }
            else
            {
                _housingUnitListIds.Add(input.HousingUnitListId);
            }

            //To get inmate details
            _inmateList = _context.Inmate
                .Where(w => w.InmateActive == 1 && w.FacilityId == input.FacilityId
                            && w.HousingUnitId>0 &&
                            _housingUnitListIds.Contains(w.HousingUnit.HousingUnitListId));

            //To get HousingUnit details
            _listHousingUnit = _context.HousingUnit
                .Where(w => w.FacilityId == input.FacilityId
                            && _housingUnitListIds.Contains(w.HousingUnitListId)
                            && (!w.HousingUnitInactive.HasValue || w.HousingUnitInactive==0));

            //Inmate Housing Capacity
            inmateCellOutputs.HousingCapacity = new HousingCapacityVm
            {
                //capacity - Actual-Outof service
                Actual = _listHousingUnit.Sum(c => c.HousingUnitActualCapacity ?? 0),
                OutofService = _listHousingUnit.Sum(c => c.HousingUnitOutOfService ?? 0),
                //Current - Asigned - Out
                Assigned = _inmateList.Count(),
                Out = _inmateList.Count(c => c.InmateCurrentTrackId.HasValue)
            };

            //Inmate Housing Header
            inmateCellOutputs.HousingHeaderDetails = GetHousingHeaderDetails();

            //Inmate Housing Visitation
            inmateCellOutputs.HousingVisitationDetails = GetHousingVisitationDetails(_housingUnitListIds);

            //Inmate Housing Stats
            inmateCellOutputs.HousingStatsDetails = _housingService.LoadHousingStatsCount(_inmateList);

            if (input.TabId == 0)
            {
                //Inmate List
                inmateCellOutputs.InmateDetailsList = GetInmateList();
            }
            else
            {
                //Inmate History List 
                inmateCellOutputs.HousingInmateHistoryList = GetInmateCellHistory(input);
            }

            return inmateCellOutputs;
        }

        #region Inmate Cell

        private List<InmateSearchVm> GetInmateList()
        {
            List<InmateSearchVm> inmateList = _inmateList
                  .Select(s => new InmateSearchVm
                  {
                      InmateId = s.InmateId,
                      InmateNumber = s.InmateNumber,
                      LocationId = s.InmateCurrentTrackId,
                      Location = s.InmateCurrentTrack,
                      HousingDetail = new HousingDetail
                      {
                          FacilityAbbr = s.Facility.FacilityAbbr,
                          HousingUnitId = s.HousingUnit.HousingUnitId,
                          HousingUnitListId = s.HousingUnit.HousingUnitListId,
                          HousingUnitLocation = s.HousingUnit.HousingUnitLocation,
                          HousingUnitNumber = s.HousingUnit.HousingUnitNumber,
                          HousingUnitBedNumber = s.HousingUnit.HousingUnitBedNumber,
                          HousingUnitBedLocation = s.HousingUnit.HousingUnitBedLocation
                      },
                      PersonDetail = new PersonInfoVm
                      {
                          PersonId = s.PersonId,
                          PersonLastName = s.Person.PersonLastName,
                          PersonMiddleName = s.Person.PersonMiddleName,
                          PersonFirstName = s.Person.PersonFirstName
                      },
                      PhotoFilePath = _photos.GetPhotoByCollectionIdentifier(s.Person.Identifiers),
                      Classify = s.InmateClassificationId.HasValue ? s.InmateClassification.InmateClassificationReason : "",
                      Sentenced = s.Incarceration.OrderByDescending(o => o.IncarcerationId).Where(w => w.InmateId == s.InmateId)
                          .Select(se => se.OverallFinalReleaseDate).FirstOrDefault()
                  }).ToList();          

            return inmateList;
        }

        private HousingHeaderVm GetHousingHeaderDetails() => _listHousingUnit
            .Select(s => new HousingHeaderVm
            {
                Status = s.Facility?.DeleteFlag ?? 0,                
                Gender = _context.Lookup
                .Where(w => w.LookupIndex == s.HousingUnitSex
                 && w.LookupType == LookupConstants.SEX)
                .Select(se => se.LookupDescription).FirstOrDefault(),
                Floor = s.HousingUnitFloor,
                Offsite = s.HousingUnitOffsite,
                Medical = s.HousingUnitMedical,
                Mental = s.HousingUnitMental,
                Visitation = s.HousingUnitVisitAllow,
                Commission = s.HousingUnitCommAllow
            }).FirstOrDefault();

        private List<HousingVisitationVm> GetHousingVisitationDetails(List<int> housingUnitListId) =>
            _context.HousingUnitVisitation
             .Where(w => !w.DeleteFlag.HasValue && housingUnitListId.Contains(w.HousingUnitListId.Value))
            .Select(s => new HousingVisitationVm
            {
                VisitDay = s.VisitationDay,
                VisitFrom = s.VisitationFrom,
                VisitTo = s.VisitationTo
            }).ToList();

        #endregion

        #region Inmate Cell History
        public List<HousingInmateHistory> GetInmateCellHistory(CellInmateInputs value)
        {
            value.ThruDate = value.ThruDate?.Date.AddDays(1).AddTicks(-1);
            IQueryable<HousingUnitMoveHistory> housingUnitMoveHistory = _context.HousingUnitMoveHistory
                .Where(w => (w.HousingUnitToId.HasValue)
                            && (w.MoveDate <= (value.ThruDate ?? DateTime.Now))
                            && ((w.MoveDateThru ?? DateTime.Now) >= (value.FromDate ?? DateTime.Now)));

            if (value.FacilityId > 0)
            {
                housingUnitMoveHistory = housingUnitMoveHistory
                    .Where(w => w.HousingUnitTo.FacilityId == value.FacilityId);
            }
            if (value.HousingUnitListId > 0 || value.HousingUnitGroupId > 0)
            {
                housingUnitMoveHistory = housingUnitMoveHistory
                    .Where(w => _housingUnitListIds.Contains(w.HousingUnitTo.HousingUnitListId));
            }

            return housingUnitMoveHistory
                .OrderByDescending(o => o.MoveDate).Select(s =>
                  new HousingInmateHistory
                  {
                      InmateId = s.Inmate.InmateId,
                      PersonDetail = new PersonInfoVm
                      {
                          PersonLastName = s.Inmate.Person.PersonLastName,
                          PersonFirstName = s.Inmate.Person.PersonFirstName,
                          PersonMiddleName = s.Inmate.Person.PersonMiddleName,
                          PersonId = s.Inmate.PersonId
                      },
                      InmateNumber = s.Inmate.InmateNumber,
                      FacilityAbbr = s.HousingUnitTo.Facility.FacilityAbbr,
                      HousingUnitLocation = s.HousingUnitTo.HousingUnitLocation,
                      HousingUnitNumber = s.HousingUnitTo.HousingUnitNumber,
                      HousingUnitBedNumber = s.HousingUnitTo.HousingUnitBedNumber,
                      HousingUnitBedLocation = s.HousingUnitTo.HousingUnitBedLocation,
                      Location = s.Inmate.InmateTrak.OrderByDescending(o => o.InmateTrakId)
                      .Select(se => se.InmateTrakNote).FirstOrDefault(),
                      FromDate = s.MoveDate,
                      ThruDate = s.MoveDateThru,
                      Active = s.Inmate.InmateActive == 1,
                      PhotoFilePath = _photos.GetPhotoByCollectionIdentifier(s.Inmate.Person.Identifiers),
                  }).ToList();
        }
        #endregion

        #region Cell Log

        //Initialize-view cell log
        public CellLogVm GetCellLog(int cellLodId, int facilityId)
        {
            CellLogVm values = new CellLogVm();

            // Filters DD's
            // Housing Details
            List<HousingUnit> housingUnit =
                _context.HousingUnit.Where(hu => hu.FacilityId == facilityId &&
                (!hu.HousingUnitInactive.HasValue || hu.HousingUnitInactive == 0)).ToList();

            values.HousingDetail = housingUnit
                .GroupBy(g => new { g.HousingUnitListId, g.HousingUnitLocation, g.HousingUnitNumber })
                .Select(s => new HousingDetail
                {
                    HousingUnitListId = s.Key.HousingUnitListId,
                    HousingUnitLocation = s.Key.HousingUnitLocation,
                    HousingUnitNumber = s.Key.HousingUnitNumber
                }).Distinct().ToList();

            values.HousingBedNumberList = housingUnit
                .Select(s =>
                new KeyValuePair<int, string>
                (s.HousingUnitListId, s.HousingUnitBedNumber)).Distinct().ToList();

            values.NoteList = _context.Lookup.Where(f => f.LookupType == LookupConstants.NOTETYPECELL && f.LookupInactive == 0)
                .OrderByDescending(l => l.LookupOrder)
                .ThenBy(l => l.LookupDescription)
                .Select(s => s.LookupDescription).ToList();


            if (cellLodId > 0)
            {
                values.CellLogDetail = _context.CellLog.Where(f => f.CellLogId == cellLodId)
                .Select(s => new InmateCellLogDetailsVm
                {
                    HousingUnitListId = s.HousingUnitListId ?? 0,
                    HousingBedNumber = s.HousingUnitBedNumber,
                    LogDate = s.CellLogDate,
                    LogTime = s.CellLogTime,
                    Count = s.CellLogCount ?? 0,
                    NoteType = s.CellLogNoteType,
                    Note = s.CellLogComments,
                    CreatedBy = new PersonnelVm
                    {
                        PersonFirstName = s.CreateByNavigation.PersonNavigation.PersonFirstName,
                        PersonLastName = s.CreateByNavigation.PersonNavigation.PersonLastName,
                        OfficerBadgeNumber = s.CreateByNavigation.OfficerBadgeNum
                    },
                    CreatedDate = s.CreateDate,
                    UpdatedBy = new PersonnelVm
                    {
                        PersonFirstName = s.UpdateByNavigation.PersonNavigation.PersonFirstName,
                        PersonLastName = s.UpdateByNavigation.PersonNavigation.PersonLastName,
                        OfficerBadgeNumber = s.UpdateByNavigation.OfficerBadgeNum
                    },
                    UpdatedDate = s.UpdateDate
                }).SingleOrDefault();
            }

            return values;
        }

        //Add-Edit Inmate Cell Log
        public async Task<int> InmateCellLog(InmateCellLogDetailsVm values)
        {
            //Get Housing Locationa and Number using Housing ListId
            KeyValuePair<string, string> housingUnitValue = _context.HousingUnitList.Where(w => w.HousingUnitListId == values.HousingUnitListId)
                .Select(s => new KeyValuePair<string, string>(s.HousingUnitLocation, s.HousingUnitNumber)).SingleOrDefault();

            switch (values.Mode)
            {
                case InputMode.Add:
                    CellLog addCellLog = new CellLog
                    {
                        FacilityId = values.FacilityId,
                        HousingUnitLocation = housingUnitValue.Key,
                        HousingUnitNumber = housingUnitValue.Value,
                        HousingUnitListId = values.HousingUnitListId,
                        HousingUnitBedNumber = values.HousingBedNumber,
                        CellLogDate = values.LogDate,
                        CellLogTime = values.LogTime,
                        CellLogCount = values.Count,
                        CellLogNoteType = values.NoteType,
                        CellLogComments = values.Note,
                        CellLogOfficerId = _personnelId,
                        CreateBy = _personnelId,
                        CreateDate = DateTime.Now
                    };

                    _context.CellLog.Add(addCellLog);
                    break;
                case InputMode.Edit:
                    CellLog editCellLog = _context.CellLog.SingleOrDefault(s => s.CellLogId == values.CellLogId);
                    if (editCellLog == null)
                    {
                        return 0;
                    }
                    editCellLog.HousingUnitLocation = housingUnitValue.Key;
                    editCellLog.HousingUnitNumber = housingUnitValue.Value;
                    editCellLog.HousingUnitListId = values.HousingUnitListId;
                    editCellLog.HousingUnitBedNumber = values.HousingBedNumber;
                    editCellLog.CellLogDate = values.LogDate;
                    editCellLog.CellLogTime = values.LogTime;
                    editCellLog.CellLogCount = values.Count;
                    editCellLog.CellLogNoteType = values.NoteType;
                    editCellLog.CellLogComments = values.Note;
                    editCellLog.UpdateBy = _personnelId;
                    editCellLog.UpdateDate = DateTime.Now;

                    _context.CellLog.Update(editCellLog);
                    break;
            }
            return await _context.SaveChangesAsync();

        }

        #endregion

    }
}
