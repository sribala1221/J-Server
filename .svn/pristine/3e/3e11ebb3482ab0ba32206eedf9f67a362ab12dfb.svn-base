using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GenerateTables.Models;
using ServerAPI.ViewModels;
using Microsoft.AspNetCore.Http;

namespace ServerAPI.Services
{
    public class MiscAttendeeService : IMiscAttendeeService
    {
        private readonly AAtims _context;
        private readonly int _personnelId;

        public MiscAttendeeService(AAtims context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _personnelId = Convert.ToInt32(httpContextAccessor.HttpContext.User.FindFirst("personnelId")?.Value);
        }

        public List<AttendenceVm> GetInmateAttendeeList(int inmateId)
        {
            DateTime attendanceDate = DateTime.Now;
            List<AttendenceVm> lstInmateAttendee =
                (from a in _context.ArrestSentenceAttendance
                 where inmateId != 0 ? a.InmateId == inmateId :
                     a.ArrestSentenceAttendanceDay.AttendanceDate == attendanceDate.Date
                 select new AttendenceVm
                 {
                     AttendanceId = a.ArrestSentenceAttendanceId,
                     AppliedBy = a.ArrestAppliedBy,
                     AttendanceDayId = a.ArrestSentenceAttendanceDayId,
                     InmateId = inmateId,
                     InmateNumber = a.Inmate.InmateNumber,
                     PersonLastName = a.Inmate.Person.PersonLastName,
                     PersonMiddleName = a.Inmate.Person.PersonMiddleName,
                     PersonFirstName = a.Inmate.Person.PersonFirstName,
                     HousingUnitLocation = a.Inmate.HousingUnit.HousingUnitLocation,
                     HousingUnitNumber = a.Inmate.HousingUnit.HousingUnitNumber,
                     HousingUnitBedLocation = a.Inmate.HousingUnit.HousingUnitBedLocation,
                     HousingUnitBedNumber = a.Inmate.HousingUnit.HousingUnitBedNumber,
                     PersonId = a.Inmate.PersonId,
                     ProgramFlag = a.ProgramFlag == 1,
                     WorkCrewFlag = a.WorkCrewFlag == 1,
                     AttendFlag = a.AttendFlag == 1,
                     AttendCredit = a.AttendCredit,
                     AttendNote = a.AttendNote,
                     NoDayDayFlag = a.NoDayDayFlag == 1,
                     ArrestId = a.ArrestId,
                     DeleteFlag = a.DeleteFlag,
                     AttendanceDate = a.ArrestSentenceAttendanceDay.AttendanceDate,
                     CreateDate = a.CreateDate,
                     CreateBy = a.CreateBy,
                     DeleteBy = a.DeleteBy,
                     DeleteDate = a.DeleteDate,
                     AppliedDate = a.ArrestAppliedDate,
                     CourtCase = a.Arrest.ArrestingAgency.AgencyAbbreviation,
                     ReCalcComplete = a.ReCalcComplete ==1
                 }).OrderBy(o => o.DeleteFlag).ThenBy(o => o.ArrestId).ThenBy(o => o.NoDayDayFlag).ToList();

            List<int> personIds =
                   lstInmateAttendee.Select(i => new[]
                       {
                            i.CreateBy,
                            i.AppliedBy,
                            i.DeleteBy
                       })
                       .SelectMany(i => i)
                       .Where(i => i.HasValue)
                       .Select(i => i.Value)   
                       .ToList();        

            List<AttendenceVm> lstPersonDet = (from per in _context.Personnel
                                               where
                                               personIds.Contains(per.PersonnelId)
                                               select new AttendenceVm
                                               {
                                                   Personneld = per.PersonnelId,
                                                   PersonLastName = per.PersonNavigation.PersonLastName,
                                                   OfficerBadgeNumber = per.OfficerBadgeNum
                                               }).ToList();

            List<AttendeeFlagVm> obj = AttendeeFlagCheck(inmateId);

            lstInmateAttendee.ForEach(item =>
            {
                AttendenceVm personnelDetails;
                if (item.CreateBy > 0)
                {
                    item.CreatedDetails = new PersonnelVm();
                    personnelDetails = lstPersonDet.Single(p => p.Personneld == item.CreateBy);
                    item.CreatedDetails.PersonLastName = personnelDetails.PersonLastName;
                    item.CreatedDetails.OfficerBadgeNumber = personnelDetails.OfficerBadgeNumber;
                }

                if (item.AppliedBy > 0)
                {
                    item.AppliedDetails = new PersonnelVm();
                    personnelDetails = lstPersonDet.Single(p => p.Personneld == item.AppliedBy);
                    item.AppliedDetails.PersonLastName = personnelDetails.PersonLastName;
                    item.AppliedDetails.OfficerBadgeNumber = personnelDetails.OfficerBadgeNumber;
                }
                if (item.DeleteBy > 0)
                {
                    item.UndoDetails = new PersonnelVm();
                    personnelDetails = lstPersonDet.Single(p => p.Personneld == item.DeleteBy);
                    item.UndoDetails.PersonLastName = personnelDetails.PersonLastName;
                    item.UndoDetails.OfficerBadgeNumber = personnelDetails.OfficerBadgeNumber;
                }
             
                item.InmateFlag = obj.Any(w => w.AttedanceDate == item.AttendanceDate);
              
            });

            return lstInmateAttendee;
        }


        private List<AttendeeFlagVm> AttendeeFlagCheck(int inmateId)
        {
            List<int> inmateIds = _context.Incarceration.Where(w => !w.ReleaseOut.HasValue && w.Inmate.InmateActive == 1)
                .Select(s => s.Inmate.InmateId).ToList();

            return _context.ArrestSentenceAttendance.Where(w => inmateIds.Contains(w.InmateId) &&
             w.DeleteFlag == 0 && !w.ArrestId.HasValue && (!w.NoDayDayFlag.HasValue || w.NoDayDayFlag == 0))
             .Select(s => new AttendeeFlagVm
             {
                 InmateId = s.InmateId,
                 Count = s.Arrest.ArrestSentenceDayForDayAllowed > 0 &&
                         !s.Inmate.Incarceration.Any(i=>i.IncarcerationArrestXref.Any(f=>f.ReleaseDate.HasValue))
                         && (!s.Arrest.ArrestSentenceNoDayForDay.HasValue ||
                             s.Arrest.ArrestSentenceNoDayForDay == 0) && (s.Arrest.ArrestSentenceMethod.ArrestSentenceDdFixed > 0 ||
                                                                          s.Arrest.ArrestSentenceMethod.ArrestSentenceDdFactor > 0 
                                                                          || !string.IsNullOrEmpty(s.Arrest.ArrestSentenceMethod.ArrestSentenceDdSql)) ? 1 : 0,
                 AttedanceDate = s.ArrestSentenceAttendanceDay.AttendanceDate
             }).Where(w => w.Count == 0 && w.InmateId==inmateId).ToList();
        }
               
        public async Task<bool> InsertAttendee(AttendenceVm obAttendance)
        {
            int dayId = _context.ArrestSentenceAttendanceDay.Where(a => a.AttendanceDate
                                == obAttendance.AttendanceDate).Select(a => a.ArrestSentenceAttendanceDayId)
                                .SingleOrDefault();

            if (dayId == 0 && obAttendance.ManualFlag == false)
            {
                return false;
            }

            if (dayId == 0 && obAttendance.ManualFlag)
            {

                ArrestSentenceAttendanceDay dbAttandaceday = new ArrestSentenceAttendanceDay
                {
                    AttendanceDate = obAttendance.AttendanceDate,
                    CreateBy = _personnelId,
                    CreateDate = DateTime.Now
                };
                _context.ArrestSentenceAttendanceDay.Add(dbAttandaceday);

                dayId = dbAttandaceday.ArrestSentenceAttendanceDayId;
            }

            ArrestSentenceAttendance dbAttendeeSentance = new ArrestSentenceAttendance
            {
                ArrestSentenceAttendanceDayId = dayId,
                InmateId = obAttendance.InmateId ?? 0,
                CreateBy = _personnelId,
                CreateDate = DateTime.Now,
                ProgramFlag = obAttendance.ProgramFlag ? 1 : 0,
                WorkCrewFlag = obAttendance.WorkCrewFlag ? 1 : 0,
                AttendFlag = obAttendance.AttendFlag ? 1 : 0,
                AttendCredit = obAttendance.AttendCredit,
                AttendNote = obAttendance.AttendNote,
                NoDayDayFlag = obAttendance.NoDayDayFlag ? 1 : 0,

            };

            _context.ArrestSentenceAttendance.Add(dbAttendeeSentance);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<int> UpdateAttendee(AttendenceVm obAttendance)
        {
            ArrestSentenceAttendance dbUpdateAttendance = _context.ArrestSentenceAttendance.SingleOrDefault(asa =>
                asa.ArrestSentenceAttendanceId == obAttendance.AttendanceId);

            if (dbUpdateAttendance != null)
            {

                dbUpdateAttendance.ProgramFlag = obAttendance.ProgramFlag ? 1 : 0;
                dbUpdateAttendance.WorkCrewFlag = obAttendance.WorkCrewFlag ? 1 : 0;
                dbUpdateAttendance.AttendFlag = obAttendance.AttendFlag ? 1 : 0;
                dbUpdateAttendance.NoDayDayFlag = obAttendance.NoDayDayFlag ? 1 : 0;
                dbUpdateAttendance.AttendCredit = obAttendance.AttendCredit;
                dbUpdateAttendance.AttendNote = obAttendance.AttendNote;
                dbUpdateAttendance.UpdateBy = _personnelId;
                dbUpdateAttendance.UpdateDate = DateTime.Now;
            }

            return await _context.SaveChangesAsync();
        }

        public async Task<int> DeleteUndoAttendee(int attendanceId)
        {
            ArrestSentenceAttendance dbUpdateAttendance = _context.ArrestSentenceAttendance.Single(asa =>
                asa.ArrestSentenceAttendanceId == attendanceId);
            if (dbUpdateAttendance.DeleteFlag == 1)
            {
                dbUpdateAttendance.DeleteFlag = 0;
                dbUpdateAttendance.DeleteDate = null;
                dbUpdateAttendance.DeleteBy = _personnelId;
            }
            else
            {
                dbUpdateAttendance.DeleteFlag = 1;
                dbUpdateAttendance.DeleteDate = DateTime.Now;
                dbUpdateAttendance.DeleteBy = _personnelId;
            }

            return await _context.SaveChangesAsync();
        }
    }
}
