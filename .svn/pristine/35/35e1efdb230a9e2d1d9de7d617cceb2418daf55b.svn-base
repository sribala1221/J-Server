using GenerateTables.Models;
using Microsoft.AspNetCore.Http;
using ServerAPI.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
namespace ServerAPI.Services
{
    // ReSharper disable once UnusedMember.Global
    public class AltSentInOutService : IAltSentInOutService
    {
        private readonly AAtims _context;
        private readonly int _personnelId;

        public AltSentInOutService(AAtims context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _personnelId = Convert.ToInt32(httpContextAccessor.HttpContext.User.FindFirst("personnelId")?.Value);
        }

        public List<CourtCommitHistoryVm> GetInterviewOrScheduleBook(CourtCommitHistorySearchVm searchValue)
        {
            List<CourtCommitHistoryVm> courtCommitHistory = _context.InmatePrebook.Where(w =>
                    (searchValue.FacilityId <= 0 || w.FacilityId == searchValue.FacilityId) &&
                    (!w.DeleteFlag.HasValue || w.DeleteFlag == 0) &&
                    (!w.IncarcerationId.HasValue || w.IncarcerationId == 0) &&
                    (!w.TempHoldId.HasValue || w.TempHoldId == 0) &&
                    (!w.MedPrescreenStartFlag.HasValue || w.MedPrescreenStartFlag == 0) &&
                    (AltSentInOutType.Interview == searchValue.AltSentInOutType ||
                     w.SchedBookDate.Value.Date == searchValue.ScheduleDate.Value.Date) &&
                    (AltSentInOutType.SchedBook == searchValue.AltSentInOutType ||
                     w.SchedInterviewDate.Value.Date == searchValue.InterviewFromDate)
                    && (!searchValue.ActiveInmate || w.Person.Inmate.Any(i => i.InmateActive == 1))
                    && (string.IsNullOrEmpty(searchValue.CaseNumber) || w.CaseNumber.Contains(searchValue.CaseNumber))
                    && (string.IsNullOrEmpty(searchValue.LastName) ||
                        ((w.PersonId == 0 || w.Person.PersonLastName.Contains(searchValue.LastName))
                         && (w.PersonId > 0 || w.PersonLastName.Contains(searchValue.LastName))
                        ))
                    && (string.IsNullOrEmpty(searchValue.FirstName) ||
                        ((w.PersonId == 0 || w.Person.PersonFirstName.Contains(searchValue.FirstName))
                         && (w.PersonId > 0 || w.PersonFirstName.Contains(searchValue.FirstName))
                        ))
                    && (string.IsNullOrEmpty(searchValue.Dob.ToString()) ||
                        ((w.PersonId == 0 || w.Person.PersonDob == searchValue.Dob)
                         && (w.PersonId > 0 || w.PersonDob == searchValue.Dob)
                        ))
                )
                .Select(s => new CourtCommitHistoryVm
                {
                    InmatePrebookId = s.InmatePrebookId,
                    InmateId = s.Person.Inmate.Any() ? s.Person.Inmate.FirstOrDefault().InmateId : 0,
                    InmateActive = s.Person.Inmate.FirstOrDefault().InmateActive == 1,
                    PersonId = s.PersonId ?? 0,
                    Dob = !s.PersonId.HasValue ? s.PersonDob : s.Person.PersonDob,
                    SchedBookDate = s.SchedBookDate,
                    SchedInterviewDate = s.SchedInterviewDate,
                    PrebookDate = s.PrebookDate,
                    PreBookNotes = s.PrebookNotes,
                    InterviewCompleteFlag = s.InterviewCompleteFlag == 1,
                    InmateName = new PersonInfo
                    {
                        PersonLastName = !s.PersonId.HasValue ? s.PersonLastName : s.Person.PersonLastName,
                        PersonFirstName = !s.PersonId.HasValue ? s.PersonFirstName : s.Person.PersonFirstName,
                        PersonMiddleName = !s.PersonId.HasValue ? s.PersonMiddleName : s.Person.PersonMiddleName,
                        InmateNumber = !s.PersonId.HasValue
                            ? string.Empty
                            : s.Person.Inmate.FirstOrDefault().InmateNumber
                    },
                    Officer = new PersonnelVm
                    {
                        OfficerBadgeNumber = s.ArrestingOfficer.OfficerBadgeNum,
                        PersonLastName = s.ArrestingOfficer.PersonNavigation.PersonLastName,
                        PersonFirstName = s.ArrestingOfficer.PersonNavigation.PersonFirstName,
                    }
                }).ToList();

            return courtCommitHistory;
        }

        // ReSharper disable once IdentifierTypo
        public CourtCommitHistoryVm GetInterviewOrScheduleBookCompleteDetails(int inmatePrebookId)
        {
            CourtCommitHistoryVm courtCommitHistoryVm = _context.InmatePrebook
                .Where(ip => ip.InmatePrebookId == inmatePrebookId)
                .Select(s => new CourtCommitHistoryVm
                {
                    InmatePrebookId = s.InmatePrebookId,
                    InmateId = s.Person.Inmate.FirstOrDefault().InmateId,
                    SchedBookDate = s.SchedBookDate,
                    SchedInterviewDate = s.SchedInterviewDate,
                    PrebookDate = s.PrebookDate,
                    PreBookNotes = s.PrebookNotes,
                    InterviewCompleteFlag = s.InterviewCompleteFlag == 1
                }).SingleOrDefault();

            return courtCommitHistoryVm;
        }

        public async Task<int> UpdateInterviewOrScheduleBookCompleteDetails(CourtCommitHistoryVm courtCommitHistoryVm)
        {
            // ReSharper disable once IdentifierTypo
            InmatePrebook inmatePrebook =
                _context.InmatePrebook.Single(ip => ip.InmatePrebookId == courtCommitHistoryVm.InmatePrebookId);
            inmatePrebook.PrebookNotes = courtCommitHistoryVm.PreBookNotes;
            if (courtCommitHistoryVm.InterviewCompleteFlag)
            {
                inmatePrebook.InterviewCompleteFlag = 1;
                inmatePrebook.InterviewCompleteBy = _personnelId;
                inmatePrebook.InterviewCompleteDate = DateTime.Now;
            }
            else
            {
                inmatePrebook.InterviewCompleteFlag = null;
                inmatePrebook.InterviewCompleteBy = null;
                inmatePrebook.InterviewCompleteDate = null;
            }

            return await _context.SaveChangesAsync();
        }

        // ReSharper disable once IdentifierTypo
        public CourtCommitHistoryVm GetScheduleAlternativeSentence(int inmatePrebookId)
        {
            CourtCommitHistoryVm courtCommitHistoryVm = _context.InmatePrebook
                .Where(ip => ip.InmatePrebookId == inmatePrebookId)
                .Select(s => new CourtCommitHistoryVm
                {
                    InmatePrebookId = s.InmatePrebookId,
                    InmateId = s.Person.Inmate.Any() ? s.Person.Inmate.FirstOrDefault().InmateId : 0,
                    InmateActive = s.Person.Inmate.FirstOrDefault().InmateActive == 1,
                    PersonId = s.PersonId ?? 0,
                    Dob = !s.PersonId.HasValue ? s.PersonDob : s.Person.PersonDob,
                    SchedBookDate = s.SchedBookDate,
                    SchedInterviewDate = s.SchedInterviewDate,
                    PrebookDate = s.PrebookDate,
                    PreBookNotes = s.PrebookNotes,
                    InterviewCompleteFlag = s.InterviewCompleteFlag == 1,
                    InmateName = new PersonInfo
                    {
                        PersonLastName = !s.PersonId.HasValue ? s.PersonLastName : s.Person.PersonLastName,
                        PersonFirstName = !s.PersonId.HasValue ? s.PersonFirstName : s.Person.PersonFirstName,
                        PersonMiddleName = !s.PersonId.HasValue ? s.PersonMiddleName : s.Person.PersonMiddleName,
                        InmateNumber = !s.PersonId.HasValue
                            ? string.Empty
                            : s.Person.Inmate.FirstOrDefault().InmateNumber
                    },
                    Officer = new PersonnelVm
                    {
                        OfficerBadgeNumber = s.SchedInterviewByNavigation.OfficerBadgeNum,
                        PersonLastName = s.SchedInterviewByNavigation.PersonNavigation.PersonLastName,
                        PersonFirstName = s.SchedInterviewByNavigation.PersonNavigation.PersonFirstName,
                    },
                    CourtCommitDenyReason = s.CourtCommitDenyReason,
                    CourtCommitStatus = s.CourtCommitStatus
                }).SingleOrDefault();

            if (courtCommitHistoryVm == null) return new CourtCommitHistoryVm();
            courtCommitHistoryVm.EnrollmentFeesVms = GetEnrollmentFees(inmatePrebookId);

            return courtCommitHistoryVm;
        }

        // ReSharper disable once IdentifierTypo
        private List<EnrollmentFeesVm> GetEnrollmentFees(int inmatePrebookId)
        {
            //To get collect details
            List<EnrollmentFeesVm> enrollmentFeesVms = _context.AltSentCollect
                .Where(a => a.InmatePrebookId == inmatePrebookId)
                .Select(a => new EnrollmentFeesVm
                {
                    Id = a.AltSentCollectId,
                    // below property value will be used in client side ie.hard coded value
                    // after creating client page we need to remove below property
                    //Transaction = a.VoidAltSentCollectId > 0 && a.AltSentCollectId > (a.VoidAltSentCollectId ?? 0)
                    //    ? "COLLECT - VOID"
                    //    : "COLLECT",
                    Receipt = a.ReceiptNumber,
                    TransactionDate = a.TransactionDate,
                    Amount = a.TransactionAmount,
                    Description = a.TransactionDescription,
                    ReceiveFrom = a.TransactionReceiveFrom,
                    VoidId = a.VoidAltSentCollectId ?? 0,
                    VoidNote = a.VoidNote,
                    Officer = new PersonnelVm
                    {
                        OfficerBadgeNumber = a.TransactionOfficer.OfficerBadgeNum,
                        PersonLastName = a.TransactionOfficer.PersonNavigation.PersonLastName,
                        PersonFirstName = a.TransactionOfficer.PersonNavigation.PersonFirstName,
                    }
                }).ToList();

            //To get fee details
            enrollmentFeesVms.AddRange(_context.AltSentChargeFee
                .Where(a => a.InmatePrebookId == inmatePrebookId)
                .Select(a => new EnrollmentFeesVm
                {
                    Id = a.AltSentChargeFeeId,
                    // below property value will be used in client side ie.hard coded value
                    // after creating client page we need to remove below property
                    //Transaction = a.VoidAltSentChargeFeeId > 0 && a.AltSentChargeFeeId > (a.VoidAltSentChargeFeeId ?? 0)
                    //    ? "FEE - VOID"
                    //    : "FEE",
                    Receipt = a.ReceiptNumber,
                    TransactionDate = a.CreatedDate,
                    Amount = a.FeeAmount,
                    Description = a.FeeNote,
                    VoidId = a.VoidAltSentChargeFeeId ?? 0,
                    VoidNote = a.VoidNote,
                    Officer = new PersonnelVm
                    {
                        OfficerBadgeNumber = a.CreatedByNavigation.OfficerBadgeNum,
                        PersonLastName = a.CreatedByNavigation.PersonNavigation.PersonLastName,
                        PersonFirstName = a.CreatedByNavigation.PersonNavigation.PersonFirstName,
                    }
                }).ToList());
            enrollmentFeesVms = enrollmentFeesVms.OrderBy(a => a.TransactionDate).ToList();

            decimal? balance = 0;
            enrollmentFeesVms.ForEach(item =>
            {
                item.Balance = item.Amount + balance;
                balance += item.Amount;
            });

            enrollmentFeesVms = enrollmentFeesVms.OrderByDescending(a => a.TransactionDate).ToList();

            return enrollmentFeesVms;
        }

        public async Task<int> UpdateScheduleAlternativeSentence(CourtCommitHistoryVm courtCommitHistoryVm)
        {
            // ReSharper disable once IdentifierTypo
            InmatePrebook inmatePrebook =
                _context.InmatePrebook.Single(ip => ip.InmatePrebookId == courtCommitHistoryVm.InmatePrebookId);
            inmatePrebook.FacilityId = courtCommitHistoryVm.FacilityId;
            inmatePrebook.CourtCommitStatus = !string.IsNullOrEmpty(courtCommitHistoryVm.CourtCommitStatus)
                ? courtCommitHistoryVm.CourtCommitStatus
                : null;
            inmatePrebook.CourtCommitDenyReason = !string.IsNullOrEmpty(courtCommitHistoryVm.CourtCommitDenyReason)
                ? courtCommitHistoryVm.CourtCommitDenyReason
                : null;
            inmatePrebook.SchedInterviewDate = courtCommitHistoryVm.SchedInterviewDate;
            inmatePrebook.SchedInterviewBy = null;
            if (courtCommitHistoryVm.Officer != null)
            {
                if (courtCommitHistoryVm.Officer.PersonnelId > 0)
                {
                    inmatePrebook.SchedInterviewBy = courtCommitHistoryVm.Officer.PersonnelId;
                }
            }

            inmatePrebook.SchedBookDate = courtCommitHistoryVm.SchedBookDate;
            inmatePrebook.SchedBookDateBy = _personnelId;
            inmatePrebook.PrebookNotes = courtCommitHistoryVm.PreBookNotes;
            return await _context.SaveChangesAsync();
        }
    }
}