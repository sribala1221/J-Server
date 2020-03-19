using GenerateTables.Models;
using Microsoft.AspNetCore.Http;
using ServerAPI.Utilities;
using ServerAPI.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ServerAPI.Services
{
    public class FacilityPrivilegeService : IFacilityPrivilegeService
    {

        private readonly AAtims _context;
        private readonly int _personnelId;
        private readonly IPersonService _personService;

        public FacilityPrivilegeService(AAtims context, IHttpContextAccessor httpContextAccessor, IPersonService personService)
        {
            _context = context;
            _personnelId =
                Convert.ToInt32(httpContextAccessor.HttpContext.User.FindFirst(AuthDetailConstants.PERSONNELID)?.Value);
            _personService = personService;
        }

        public List<FacilityPrivilegeVm> GetPrivilegeByOfficer(FacilityPrivilegeInput input)
        {

            IQueryable<InmatePrivilegeXref> list = _context.InmatePrivilegeXref.Where(f =>
                f.Privilege.RemoveFromPrivilegeFlag == 0 && f.Privilege.InactiveFlag == 0);

            list = list.Where(f =>
                !f.PrivilegeExpires.HasValue ||
                (f.PrivilegeExpires.HasValue &&
                 f.PrivilegeExpires.Value >= DateTime.Now)); //filter active(not expired) 

            if (input.FacilityId > 0)
            {
                list = list.Where(f => f.Inmate.FacilityId == input.FacilityId);
            }

            if (input.OfficerId > 0)
            {
                list = list.Where(f => f.PrivilegeOfficerId == input.OfficerId);
            }

            //if (input.FromDate !=null) - needs clarification
            //{
            //    list = list.Where(f => ((f.PrivilegeDate >= input.FromDate || f.PrivilegeDate <= input.ToDate) && f.PrivilegeDate != null) ||
            //      ((f.PrivilegeExpires >= input.FromDate || f.PrivilegeExpires >= input.ToDate) && f.PrivilegeExpires != null));
            //}

            list = !input.ActiveToday ? list.Where(f => f.PrivilegeDate >= input.FromDate && f.PrivilegeDate <= input.ToDate.Date.AddDays(1).AddTicks(-1)) : list.Where(f => !f.PrivilegeRemoveDatetime.HasValue);

            List<FacilityPrivilegeVm> privilege = list.Select(s => new FacilityPrivilegeVm
            {
                //privilege inmate xref
                InmatePrivilegeXrefId = s.InmatePrivilegeXrefId,
                PrivilegeDate = s.PrivilegeDate, //Elapsed
                PrivilegeExpires = s.PrivilegeExpires, // status
                PrivilegeRemoveDateTime = s.PrivilegeRemoveDatetime,
                PrivilegeReviewFlag = s.ReviewFlag,
                PrivilegeReviewInterval = s.ReviewInterval,
                PrivilegeNextReview = s.ReviewNext,
                PrivilegeNote = s.PrivilegeNote,
                PrivilegeRemoveNote = s.PrivilegeRemoveNote,
                PrivilegeRemoveOfficerId = s.PrivilegeRemoveOfficerId,
                Incident = new FacilityPrivilegeIncidentLinkVm
                {
                    PrivilegeDiscLinkId = s.PrivilegeDiscLinkId.Value
                },
                //privilege
                PrivilegeId = s.Privilege.PrivilegeId,
                PrivilegeType = s.Privilege.PrivilegeType,
                PrivilegeDescription = s.Privilege.PrivilegeDescription,
                //Inmate
                InmateId = s.InmateId,
                PersonId = s.Inmate.Person.PersonId,
                LastName = s.Inmate.Person.PersonLastName,
                FirstName = s.Inmate.Person.PersonFirstName,
                Number = s.Inmate.InmateNumber
            }).ToList();

            //To get incident ID
            List<int> discLinkIdLst = privilege.Where(ds => ds.Incident.PrivilegeDiscLinkId > 0)
                .Select(ds => ds.Incident.PrivilegeDiscLinkId ?? 0).Distinct().ToList();

            // To get Incident         
            List<FacilityPrivilegeIncidentLinkVm> incidentValue = _context.DisciplinaryIncident.Where(di =>
                discLinkIdLst.Contains(di.DisciplinaryIncidentId)).Select(di =>
                new FacilityPrivilegeIncidentLinkVm
                {
                    PrivilegeDiscLinkId = di.DisciplinaryIncidentId,
                    IncidentNumber = di.DisciplinaryNumber,
                    IncidentTypeId = di.DisciplinaryType,
                    IncidentDate = di.DisciplinaryIncidentDate,
                    ShortSnopsis = di.DisciplinarySynopsis,
                    ViolationNote = di.DisciplinaryInmate.Select(s => s.DisciplinaryViolationDescription)
                        .FirstOrDefault(),
                    SanctionNote = di.DisciplinaryInmate.Select(s => s.DisciplinarySanction).FirstOrDefault()
                }).ToList();

            List<int> lstPersonnelId = privilege.Select(s => s.PrivilegeRemoveOfficerId ?? 0).ToList();
            List<PersonnelVm> lstPersonDetail = _personService.GetPersonNameList(lstPersonnelId);

            //Assigning to privilege object
            privilege.Where(f => f.Incident.PrivilegeDiscLinkId > 0  || f.PrivilegeRemoveOfficerId>0 ).ToList().ForEach(i =>
            {
                i.Incident =
                    incidentValue.SingleOrDefault(f => f.PrivilegeDiscLinkId == i.Incident.PrivilegeDiscLinkId);
                if (i.Incident != null)
                    i.Incident.IncidentType = _context.Lookup.FirstOrDefault(f =>
                        f.LookupType == LookupConstants.DISCTYPE &&
                        (int?)(f.LookupIndex) == i.Incident.IncidentTypeId)?.LookupDescription;
                i.PersonnelDetails = lstPersonDetail.SingleOrDefault(s => s.PersonnelId == i.PrivilegeRemoveOfficerId);
            });

            return privilege;
        }

        public List<InmatePrivilegeReviewHistoryVm> GetReviewHistory(int inmatePrivilegeXrefId)
        {
            List<InmatePrivilegeReviewHistoryVm> inmatePrivilegeReview = _context.InmatePrivilegeReviewHistory
                .Where(i => i.InmatePrivilegeXrefId == inmatePrivilegeXrefId).Select(i =>
                    new InmatePrivilegeReviewHistoryVm
                    {
                        ScheduledReview = i.ScheduledReview,
                        ReviewActual = i.ReviewActual,
                        ReviewBy = i.ReviewBy,
                        ReviewNote = i.ReviewNote,
                        ReviewNext = i.ReviewNext
                    }).ToList();

            List<int> listPersonnelId = inmatePrivilegeReview.Select(s => s.ReviewBy).ToList();
            List<PersonnelVm> listPerson = _personService.GetPersonNameList(listPersonnelId);

            inmatePrivilegeReview.ForEach(e =>
                {
                    e.PersonLastName = listPerson.SingleOrDefault(s => s.PersonnelId == e.ReviewBy)?.PersonLastName;
                    e.OfficerBadgeNumber = listPerson.SingleOrDefault(s => s.PersonnelId == e.ReviewBy)?.PersonLastName;
                });

            return inmatePrivilegeReview; // load inmate privilege review history

        }

        public async Task<int> InsertReviewHistory(InmatePrivilegeReviewHistoryVm privilegeReview)
        {
            //get existing inmate privilege to update
            InmatePrivilegeXref dbPrivilegeXref = _context.InmatePrivilegeXref
                .Single(p => p.InmatePrivilegeXrefId == privilegeReview.InmatePrivilegeXrefId);

            InmatePrivilegeReviewHistory inmatePrivilegeReviewHistory = new InmatePrivilegeReviewHistory
            {
                //Add review history details
                InmatePrivilegeXrefId = privilegeReview.InmatePrivilegeXrefId,
                ScheduledReview = privilegeReview.ScheduledReview,
                ReviewNote = privilegeReview.ReviewNote,
                ReviewBy = _personnelId,
                ReviewActual = DateTime.Now

            };

            if (privilegeReview.IsReauthorizeOrUnassign)
            {

                if (privilegeReview.ReviewReauthorize)
                {
                    //Update inmate privilege review details
                    dbPrivilegeXref.ReviewInterval = privilegeReview.ReviewInterval;
                    dbPrivilegeXref.ReviewNext = privilegeReview.ReviewNext;

                    //Add review history reauthorize
                    inmatePrivilegeReviewHistory.ReviewReauthorize = privilegeReview.ReviewReauthorize;
                    inmatePrivilegeReviewHistory.ReviewInterval = privilegeReview.ReviewInterval;
                    inmatePrivilegeReviewHistory.ReviewNext = privilegeReview.ReviewNext;
                }
                else
                {
                    //Update inmate privilege remove details
                    dbPrivilegeXref.PrivilegeRemoveDatetime = DateTime.Now;
                    dbPrivilegeXref.PrivilegeRemoveNote = privilegeReview.RemovalNote;
                    dbPrivilegeXref.PrivilegeRemoveOfficerId = _personnelId;

                    //Add review history un assign
                    inmatePrivilegeReviewHistory.ReviewReauthorize = privilegeReview.ReviewReauthorize;
                }
            }

            _context.InmatePrivilegeXref.Update(dbPrivilegeXref); // Update inmate privilege Xref 
            _context.InmatePrivilegeReviewHistory.Add(inmatePrivilegeReviewHistory); // add inmate privilege review

            return await _context.SaveChangesAsync();

        }

        public List<PrivilegeDetailsVm> GetPrivilegeList(int facilityId) => _context.Privileges
            .Where(p => p.InactiveFlag == 0
                        && (p.FacilityId == facilityId || p.FacilityId == 0 || !p.FacilityId.HasValue))
            .Select(p => new PrivilegeDetailsVm
            {
                PrivilegeId = p.PrivilegeId,
                PrivilegeDescription = p.PrivilegeDescription,
                TrackingAllowRefusal = p.TrackingAllowRefusal
            }).ToList();

        //To get Location details based on facility
        public List<KeyValuePair<int, string>> GetLocationList(int facilityId) =>
            (from p in _context.Privileges
                where p.InactiveFlag == 0 && p.RemoveFromTrackingFlag == 0
                                          && (p.FacilityId == facilityId || p.FacilityId == 0 || !p.FacilityId.HasValue)
                orderby p.PrivilegeDescription
                select new KeyValuePair<int, string>(p.PrivilegeId, p.PrivilegeDescription)).ToList();


        // To Get Tracking Location DropDown List
        public IQueryable<PrivilegeDetailsVm> GetTrackingLocationList(int facilityId) =>
            _context.Privileges.Where(p => p.InactiveFlag == 0 && p.RemoveFromTrackingFlag == 0
                                                               && (p.FacilityId == facilityId || p.FacilityId == 0 || !p.FacilityId.HasValue))
                .Select(p => new PrivilegeDetailsVm
                {
                    PrivilegeId = p.PrivilegeId,
                    PrivilegeDescription = p.PrivilegeDescription,
                    TrackingAllowRefusal = p.TrackingAllowRefusal,
                    TrackEnrouteFlag = p.TrackEnrouteFlag,
                    ApptAlertEndDate = p.AppointmentAlertApptEndDate == 1,
                }).OrderBy(p => p.PrivilegeDescription);

    }
}
