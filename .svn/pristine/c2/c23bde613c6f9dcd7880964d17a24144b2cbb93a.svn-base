using System.Collections.Generic;
using System.Linq;
using GenerateTables.Models;
using ServerAPI.ViewModels;
using ServerAPI.Utilities;
using System;
using System.Globalization;
using System.Threading.Tasks;

namespace ServerAPI.Services
{
    public class ClassCountService : IClassCountService
    {
        private readonly AAtims _context;
        private readonly ICommonService _commonService;
        private IQueryable<Inmate> _lstInmate;
        private readonly HousingDetails _details = new HousingDetails();
        private ClassCountInputs _countInputs;

        public ClassCountService(AAtims context, ICommonService commonService)
        {
            _context = context;
            _commonService = commonService;
        }

        //Page loading at first time
        public ClassCountHousing GetHousing(int facilityId)
        {
            ClassCountHousing countHousing = new ClassCountHousing
            {
                //For Classification Selection       
                Classify = _commonService.GetLookupKeyValuePairs(LookupCategory.CLASREAS),

                //For Association Selection
                Association = _context.Lookup
                    .Where(l => l.LookupType == LookupConstants.CLASSGROUP && l.LookupInactive == 0)
                    .OrderByDescending(l => l.LookupOrder)
                    .ThenBy(l => l.LookupDescription).Select(l =>
                        new KeyValuePair<int, string>(l.LookupIndex, l.LookupDescription)).ToList(),

                //For Flags Selection
                Flags = _context.Lookup
                    .Where(l => (l.LookupType == LookupConstants.PERSONCAUTION ||
                                 l.LookupType == LookupConstants.TRANSCAUTION ||
                                 l.LookupType == LookupConstants.DIET) && l.LookupInactive == 0)
                                 .OrderByDescending(l => l.LookupOrder)
                                 .ThenBy(l => l.LookupDescription)
                                 .Select(l =>
                        new LookupVm
                        {
                            LookupIndex = l.LookupIndex,
                            LookupDescription = l.LookupDescription,
                            LookupType = l.LookupType
                        }).ToList(),


                //For Gender selection
                Gender = _commonService.GetLookupKeyValuePairs(LookupConstants.SEX)
            };

            ClassCountInputs inputs = new ClassCountInputs
            {
                Flag = HousingConstants.NUMBER,
                FacilityId = facilityId,
                PageLoadFlag = true,
                CountFlag = true
            };

            countHousing.HousingDetailsList = GetHousingCountDetails(inputs);

            return countHousing;
        }



        //Refreshing time
        public HousingDetails GetHousingCountDetails(ClassCountInputs countInputs)
        {
            _lstInmate = _context.Inmate.Where(i => i.InmateActive == 1);


            //filtering with facility
            if (countInputs.FacilityId != 0)
            {
                _lstInmate = _lstInmate.Where(i => i.FacilityId == countInputs.FacilityId);
            }
            //filtering with classification
            if (countInputs.Classification != null)
            {
                _lstInmate = _lstInmate.Where(i =>
                    i.InmateClassification.InmateClassificationReason == countInputs.Classification);
            }
            //filtering with association
            if (countInputs.AssociationId > 0)
            {
                List<PersonClassification> lstPerson = _context.PersonClassification
                    .Where(c => c.PersonClassificationTypeId == countInputs.AssociationId).ToList();

                _lstInmate = _lstInmate.Where(i => lstPerson.Select(l => l.PersonId).Contains(i.Person.PersonId));
            }
            //filtering with gender
            if (countInputs.Gender != 0)
            {
                _lstInmate = _lstInmate.Where(i => i.Person.PersonSexLast == countInputs.Gender);
            }
            //fitering with flag
            if (countInputs.FlagType != null && countInputs.AlertFlag != 0)
            {
                List<PersonFlag> lstPersonFlag = _context.PersonFlag.Where(p =>
                    countInputs.FlagType == HousingConstants.PERSON
                        ? p.PersonFlagIndex == countInputs.AlertFlag
                        : countInputs.FlagType == HousingConstants.INMATE
                            ? p.InmateFlagIndex == countInputs.AlertFlag
                            : p.DietFlagIndex == countInputs.AlertFlag).ToList();

                _lstInmate = _lstInmate.Where(i => lstPersonFlag.Select(l => l.PersonId).Contains(i.Person.PersonId));
            }
            _countInputs = countInputs;

            if (countInputs.CountFlag)
            {
                HousingCount();
                if (_details.ParentHousingCount.Count > 0 || _details.ParentClassDayCount.Count > 0)
                {
                    HousingDetails();
                }
            }
            else
            {
                if (countInputs.CountRefreshFlag)
                {
                    HousingCount();
                }
                HousingDetails();
            }
            return _details;
        }
        //For housing count details
        private void HousingCount()
        {
            List<HousingCount> lstHousing = _lstInmate.Select(i => new HousingCount
            {
                //this append is necessary because housing condition is based on housing unit location
                // and housing unit number not from housing unit id and housing unit list id.
                Housing = i.HousingUnit == null
                    ? HousingConstants.NOHOUSING
                    : _countInputs.Flag == HousingConstants.NUMBER
                        ? i.Facility.FacilityAbbr + ' ' + i.HousingUnit.HousingUnitLocation + ' ' +
                          i.HousingUnit.HousingUnitNumber
                        : _countInputs.Flag == HousingConstants.BUILDING
                            ? i.Facility.FacilityAbbr + ' ' + i.HousingUnit.HousingUnitLocation
                            : i.Facility.FacilityAbbr,
                Classification = i.InmateClassificationId == null
                    ? HousingConstants.NOCLASSIFY
                    : i.InmateClassification.InmateClassificationReason
            }).ToList();

            List<HousingCount> lstHousingClassDays = _lstInmate.Select(i => new HousingCount
            {
                //this append is necessary because housing condition is based on housing unit location
                // and housing unit number not from housing unit id and housing unit list id.
                Housing = i.HousingUnit == null
                   ? HousingConstants.NOHOUSING
                   : _countInputs.Flag == HousingConstants.NUMBER
                       ? i.Facility.FacilityAbbr + ' ' + i.HousingUnit.HousingUnitLocation + ' ' +
                         i.HousingUnit.HousingUnitNumber
                       : _countInputs.Flag == HousingConstants.BUILDING
                           ? i.Facility.FacilityAbbr + ' ' + i.HousingUnit.HousingUnitLocation
                           : i.Facility.FacilityAbbr,

                Days = i.LastClassReviewDate == null ? HousingConstants.NOTSET :
                Math.Round((DateTime.Now - i.LastClassReviewDate).Value.TotalDays).ToString(CultureInfo.InvariantCulture),

                //To get values for input days.
                DaysNumber = i.LastClassReviewDate == null ? 0 :
                Math.Round((DateTime.Now - i.LastClassReviewDate).Value.TotalDays),

            }).ToList();
            //parent count list 
            _details.ParentHousingCount = lstHousing.GroupBy(i => i.Housing).Select(i => new HousingCount
            {
                Count = i.Count(),
                Housing = i.Key
            }).OrderBy(i => i.Housing).ToList();

            //child count list 
            _details.ChildHousingCount = lstHousing.GroupBy(i => new { i.Housing, i.Classification }).Where(i =>
                      _details.ParentHousingCount.Select(l => l.Housing).Contains(i.Key.Housing))
                .Select(i => new HousingCount
                {
                    Classification = i.Key.Classification,
                    Housing = i.Key.Housing,
                    Count = i.Count()
                }).ToList();


            // Parent Classday Count
             //To get housingClassDaysList based on input days
            if (_countInputs.ClassifyDays == null)
            {
                // Parent Classday Count
                _details.ParentClassDayCount = lstHousingClassDays.GroupBy(i => i.Days).Select(i => new HousingCount
                {
                    Count = i.Count(),
                    Days = i.Key,
                    DayCount = i.Key != HousingConstants.NOTSET ? Convert.ToInt32(i.Key) : 0
                }).OrderByDescending(i => i.DayCount).ToList();

            }
            else
            {
                _details.ParentClassDayCount = lstHousingClassDays.Where(w => w.DaysNumber >= _countInputs.ClassifyDays).GroupBy(i => i.Days).Select(i => new HousingCount
                {
                    Count = i.Count(),
                    Days = i.Key,
                    DayCount = i.Key != HousingConstants.NOTSET ? Convert.ToInt32(i.Key) : 0
                }).OrderByDescending(i => i.DayCount).ToList();

            }


            //Child Classday Count

            _details.ChildClassDayCount = lstHousingClassDays.GroupBy(i => new { i.Housing, i.Days }).Where(i =>
                      _details.ParentClassDayCount.Select(l => l.Days).Contains(i.Key.Days))
                .Select(i => new HousingCount
                {
                    Days = i.Key.Days,
                    Housing = i.Key.Housing,
                    Count = i.Count()
                }).ToList();

            //if page loading at first time 
            if (!_countInputs.PageLoadFlag) return;
            if (_details.ParentHousingCount.Count > 0)
            {
                _countInputs.Housing = _details.ParentHousingCount[0].Housing;
                _countInputs.DetailsFlag = HousingConstants.PARENT;
            }

            if (_details.ParentClassDayCount.Count == 0) return;
            _countInputs.Days = _details.ParentClassDayCount[0].Days;
            _countInputs.DetailsFlag = HousingConstants.PARENT;
        }

        //For housing details
        private void HousingDetails()
        {
            //person details list
            List<PersonVm> lstPersonInfo = _lstInmate.Select(i => new PersonVm
            {
                PersonId = i.PersonId,
                PersonFirstName = i.Person.PersonFirstName,
                PersonMiddleName = i.Person.PersonMiddleName,
                PersonLastName = i.Person.PersonLastName,
                PersonSuffix = i.Person.PersonSuffix,
                InmateNumber = i.InmateNumber,
                InmateId = i.InmateId,
                LastReviewDate = i.LastClassReviewDate
            }).ToList();


            List<HousingCount> housingDetails = _lstInmate.Select(i => new HousingCount
            {
                //this append is necessary because housing condition is based on housing unit location
                // and housing unit number not from housing unit id and housing unit list id.
                Housing = i.HousingUnit == null
                    ? HousingConstants.NOHOUSING
                    : _countInputs.Flag == HousingConstants.NUMBER
                        ? i.Facility.FacilityAbbr + ' ' + i.HousingUnit.HousingUnitLocation + ' ' +
                          i.HousingUnit.HousingUnitNumber
                        : _countInputs.Flag == HousingConstants.BUILDING
                            ? i.Facility.FacilityAbbr + ' ' + i.HousingUnit.HousingUnitLocation
                            : i.Facility.FacilityAbbr,
                Classification = i.InmateClassificationId == null
                    ? HousingConstants.NOCLASSIFY
                    : i.InmateClassification.InmateClassificationReason,
                ClassType = i.InmateClassification.InmateClassificationType,
                ClassDate = i.InmateClassification.InmateDateAssigned,
                InmateClassificationId = i.InmateClassificationId ?? 0,
                PersonDetails = lstPersonInfo.Single(inm => inm.InmateId == i.InmateId),
                Narrative = i.InmateClassification.InmateOverrideNarrative,

                Days = i.LastClassReviewDate != null ?
                    Math.Round((DateTime.Now - i.LastClassReviewDate).Value.TotalDays).ToString(CultureInfo.InvariantCulture) :
                    HousingConstants.NOTSET,
                DayCount = i.LastClassReviewDate != null ? Math.Round((DateTime.Now - i.LastClassReviewDate).Value.TotalDays) : 0,
                Scheduledays = _context.HousingUnitScheduleClassify.Where(w => w.FacilityId == i.HousingUnit.FacilityId
                && w.HousingUnitLocation == i.HousingUnit.HousingUnitLocation && w.HousingUnitNumber == i.HousingUnit.HousingUnitNumber)
                .Select(s => s.Days).FirstOrDefault()
            }).ToList();

            //housing details
            _details.HousingDetailsList = housingDetails.Where(i =>
                _countInputs.DetailsFlag == HousingConstants.PARENT
                    ? i.Housing == _countInputs.Housing
                    : i.Classification == _countInputs.ClassifyReason && i.Housing == _countInputs.Housing).ToList();

            _details.HousingDayDetailsList = housingDetails.Where(i =>
               _countInputs.DetailsFlag == HousingConstants.PARENT
                   ? i.Days == _countInputs.Days
                   : i.Housing == _countInputs.Housing && i.Days == _countInputs.Days).ToList();
        }

        //Insert floor note details
        public async Task<int> InsertFloorNote(FloorNotesVm value)
        {
            DateTime time = Convert.ToDateTime(value.FloorNoteTime);
            if (value.FloorNoteDate != null)
            {
                DateTime date = value.FloorNoteDate.Value;
                DateTime floorDate = new DateTime(date.Year, date.Month, date.Day, time.Hour, time.Minute, time.Second,
                    time.Millisecond);
                string floorTime = time.ToString(DateConstants.TIME);

                //Insert entry in Floor Note table
                FloorNotes floorNotes = new FloorNotes
                {
                    FloorNoteOfficerId = value.FloorNoteOfficerId,
                    FloorNoteNarrative = value.FloorNoteNarrative,
                    FloorNoteDate = floorDate,
                    FloorNoteTime = floorTime,
                    FloorNoteLocation = value.FloorNoteLocation,
                    FloorNoteWatchFlag = value.FloorNoteWatchFlag,
                    FloorNoteJuvenileFlag = 0,
                    FloorNoteLocationId = value.FloorNoteLocationId
                };

                _context.Add(floorNotes);
                // _context.SaveChangesAsync();

                //Insert entry in Floor Note Xref table
                FloorNoteXref floorNotesXref = new FloorNoteXref
                {
                    FloorNoteId = floorNotes.FloorNoteId,
                    InmateId = value.InmateId
                };

                _context.Add(floorNotesXref);
            }
            return await _context.SaveChangesAsync();
        }

    }
}