using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ServerAPI.ViewModels;
using GenerateTables.Models;
using ServerAPI.Utilities;
using Microsoft.AspNetCore.Http;

namespace ServerAPI.Services
{
    public class InmatePhoneService : IInmatePhoneService
    {
        private readonly AAtims _context;
        private readonly ICommonService _commonservice;
        private readonly IHttpContextAccessor _iHttpContextAccessor;

        readonly InmatePhoneHistoryVm PhoneDetails = new InmatePhoneHistoryVm();

        public InmatePhoneService(AAtims context, ICommonService commonservice, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _commonservice = commonservice;
            _iHttpContextAccessor = httpContextAccessor;
        }

        #region CallLog
        //Load Call Log History
        public InmatePhoneHistoryVm GetCallLogHistroy(int inmateId)
        {


            PhoneDetails.LstCallLogDetails = (from pcl in _context.PhoneCallLog
                where pcl.InmateId == inmateId
                select new PhoneDetailsVm
                {
                    PhoneCallLogId = pcl.PhoneCallLogId,
                    InmateId = pcl.InmateId,
                    CallLogDate = pcl.CallLogDate,
                    CallLogType = pcl.CallLogType,
                    CreateDate = pcl.CreateDate,
                    UpdateDate = pcl.UpdateDate,
                    DeleteDate = pcl.DeleteDate,
                    CreateBy = pcl.CreateBy,
                    UpdateBy = pcl.UpdateBy,
                    DeleteBy = pcl.DeleteBy,
                    DeleteFlag = pcl.DeleteFlag
                }).ToList();

            if (PhoneDetails.LstCallLogDetails != null)
            {

                List<int> personIds =
                    PhoneDetails.LstCallLogDetails.Select(i => new[] {i.CreateBy, i.DeleteBy, i.UpdateBy})
                        .SelectMany(i => i)
                        .Where(i => i.HasValue)
                        .Select(i => i.Value)
                        .ToList();

                List<PhoneDetailsVm> lstPersonDet = (from per in _context.Personnel
                    where
                    personIds.Contains(per.PersonnelId)
                    select new PhoneDetailsVm
                    {
                        Personneld = per.PersonnelId,
                        PersonLastName = per.PersonNavigation.PersonLastName,
                        OfficerBadgeNumber = per.OfficerBadgeNum
                    }).ToList();

                PhoneDetails.LstCallLogDetails.ForEach(item =>
                {
                    PhoneDetailsVm personnelDetails;
                    if (item.CreateBy > 0)
                    {
                        personnelDetails = lstPersonDet.Single(p => p.Personneld == item.CreateBy);
                        item.CreateByPersonName = personnelDetails.PersonLastName;
                        item.CreateByOfficerBadgeNumber = personnelDetails.OfficerBadgeNumber;
                    }


                    if (item.UpdateBy > 0)
                    {
                        personnelDetails = lstPersonDet.Single(p => p.Personneld == item.CreateBy);
                        item.UpdateByPersonName = personnelDetails.PersonLastName;
                        item.UpdateByOfficerBadgeNumber = personnelDetails.OfficerBadgeNumber;
                    }


                    if (item.DeleteBy > 0)
                    {
                        personnelDetails = lstPersonDet.Single(p => p.Personneld == item.CreateBy);
                        item.DeleteByPersonName = personnelDetails.PersonLastName;
                        item.DeleteByOfficerBadgeNumber = personnelDetails.OfficerBadgeNumber;
                    }

                });
            }

            PhoneDetails.LookUp = 
                _commonservice.GetLookupList(LookupConstants.CALLLOGTYPE);

            return PhoneDetails;
        }

        //Insert Update Call Log Details
        public async Task<int> InsertUpdateCallLog(PhoneDetailsVm objCallDetails)
        {
            int personnelId = Convert.ToInt32(_iHttpContextAccessor.HttpContext.User.FindFirst("personnelId")?.Value);

            if (PhoneLogStatus.Insert == objCallDetails.PhoneLogStatus)
            {
                PhoneCallLog obInsertCallLog = new PhoneCallLog
                {
                    InmateId = objCallDetails.InmateId,
                    CreateDate = DateTime.Now,
                    CreateBy = personnelId,
                    PhoneNumber = objCallDetails.PhoneNumber,
                    ContactName = objCallDetails.ContactName,
                    Duration = objCallDetails.Duration,
                    Note = objCallDetails.Note,
                    CallLogType = objCallDetails.CallLogType,
                    CallLogDate = objCallDetails.CallLogDate
                };
                _context.Add(obInsertCallLog);
            }
            else
            {
                PhoneCallLog objPhoneLog =
                    _context.PhoneCallLog.Single(p => p.PhoneCallLogId == objCallDetails.PhoneCallLogId);
                if (PhoneLogStatus.Update == objCallDetails.PhoneLogStatus)
                {
                    objPhoneLog.CallLogDate = objCallDetails.CallLogDate;
                    objPhoneLog.UpdateDate = DateTime.Now;
                    objPhoneLog.UpdateBy = personnelId;
                    objPhoneLog.PhoneNumber = objCallDetails.PhoneNumber;
                    objPhoneLog.CallLogType = objCallDetails.CallLogType;
                    objPhoneLog.ContactName = objCallDetails.ContactName;
                    objPhoneLog.Duration = objCallDetails.Duration;
                    objPhoneLog.Note = objCallDetails.Note;
                }
                else if (PhoneLogStatus.Delete == objCallDetails.PhoneLogStatus)
                {
                    objPhoneLog.DeleteFlag = 1;
                    objPhoneLog.DeleteDate = DateTime.Now;
                    objPhoneLog.DeleteBy = personnelId;
                }
                else
                {
                    objPhoneLog.DeleteFlag = 0;
                    objPhoneLog.DeleteDate = null;
                    objPhoneLog.DeleteBy = null;
                }
            }


            return await _context.SaveChangesAsync();
        }

        #endregion

        #region PhonePin

        //LoadPinHistory

        public InmatePhoneHistoryVm GetPinHistroy(int inmateId)
        {
            PhoneDetails.LstPinHistory =
                (from i in _context.PhonePinHistory
                 where i.InmateId == inmateId
                 orderby i.CreateDate descending
                 select new PhoneDetailsVm
                 {
                     PhonePinHistoryId = i.PhonePinHistoryId,
                     InmateId = i.InmateId,
                     CreateDate = i.CreateDate,
                     Pin = i.Pin,
                     Note = i.Note,
                     CreateBy = i.CreateBy
                 }).ToList();


            if (PhoneDetails.LstPinHistory != null)
            {
                PhoneDetailsVm personnelDetails;
                List<int> personIds =
                    PhoneDetails.LstPinHistory.Select(i => new[] { i.CreateBy })
                        .SelectMany(i => i)
                        .ToList();

                List<PhoneDetailsVm> lstPersonDet =
                (from per in _context.Personnel
                 where
                 personIds.Contains(per.PersonnelId)
                 select new PhoneDetailsVm
                 {
                     Personneld = per.PersonnelId,
                     PersonLastName = per.PersonNavigation.PersonLastName,
                     OfficerBadgeNumber = per.OfficerBadgeNum
                 }).ToList();
                PhoneDetails.LstPinHistory.ForEach(item =>
                {
                    if (item.CreateBy > 0)
                    {
                        personnelDetails =
                            lstPersonDet.Single(p => p.Personneld == item.CreateBy);
                        item.CreateByPersonName = personnelDetails.PersonLastName;
                        item.CreateByOfficerBadgeNumber = personnelDetails.OfficerBadgeNumber;
                    }
                });
            }

            InmatePhoneHistoryVm phonepin =
                _context.Inmate.Where(i => i.InmateId == inmateId)
                        .Select(i => new InmatePhoneHistoryVm
                        {
                            PersonSsn = i.Person.PersonSsn,
                            CurrentPinId = i.PhonePin,
                            PersonSeal = i.PersonId
                        }).Single();
            if (phonepin.CurrentPinId != null || phonepin.PersonSsn != null)
            {
                PhoneDetails.CurrentPinId = phonepin.CurrentPinId;
                PhoneDetails.PersonSsn = phonepin.PersonSsn;
                PhoneDetails.PersonSeal =
                    _context.PersonSeal.SingleOrDefault(i => i.PersonId == phonepin.PersonSeal)?.PersonId;
            }


            return PhoneDetails;
        }

        public async Task<int> InsertDeletePhonePin(PhoneDetailsVm objPhonePin)
        {
            int personnelId = Convert.ToInt32(_iHttpContextAccessor.HttpContext.User.FindFirst("personnelId")?.Value);
            Inmate obInmate =
                _context.Inmate.Single(i => i.InmateId == objPhonePin.InmateId);
            obInmate.PhonePin = objPhonePin.DeleteFlag == 1 ? string.Empty : objPhonePin.Pin;

            PhonePinHistory objInsertPinHistory = new PhonePinHistory
            {
                InmateId = objPhonePin.InmateId,
                CreateDate = DateTime.Now,
                CreateBy = personnelId, 
                Pin = objPhonePin.Pin,
                Note = objPhonePin.DeleteFlag == 1 ? "DELETED" : objPhonePin.Note,
            };
            _context.Add(objInsertPinHistory);

            return await _context.SaveChangesAsync();
        }
        #endregion
    }
}
