﻿using GenerateTables.Models;
using ServerAPI.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using ServerAPI.Utilities;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace ServerAPI.Services
{
    public class ProgramRequestService : IProgramRequestService
    {
        private readonly AAtims _context;
        private readonly IPhotosService _photoService;
        private readonly IInmateService _inmateService;
        private readonly int _personnelId;
        private readonly IPersonService _personService;
        private readonly IFacilityHousingService _facilityHousingService;
        private string filtertype;
        private string gender;
        private int inmateClassificationId;
        private int personSexId;
        private readonly IInterfaceEngineService _interfaceEngine;


        public ProgramRequestService(AAtims context, IHttpContextAccessor httpContextAccessor,
            IPersonService personService, IInmateService inmateService,
            IFacilityHousingService facilityHousingService, IPhotosService photosService,
            IInterfaceEngineService interfaceEngine)
        {
            _context = context;
            _personnelId = Convert.ToInt32(httpContextAccessor.HttpContext.User.FindFirst("personnelId")?.Value);
            _photoService = photosService;
            _personService = personService;
            _inmateService = inmateService;
            _facilityHousingService = facilityHousingService;
            _interfaceEngine = interfaceEngine;
        }

        // load the inmate & program request grid list
        public List<ProgramEligibility> GetProgramRequestEligibility(int facilityId, int inmateId, int programId)
        {
            // This program table need to change based on programClass
            IQueryable<ProgramRequest> lstProgramReq = _context.ProgramRequest.Where(pro =>
                (inmateId == 0 || pro.InmateId == inmateId) && (programId == 0 || pro.ProgramClassId == programId)
                && pro.Inmate.InmateActive == 1
                //&& (facilityId == 0 || pro.Program.ProgramCategory.FacilityId == facilityId
                //|| pro.Program.ProgramCategory.FacilityId == 0)
                );

            int[] lstDenyIds = lstProgramReq.Where(w => w.DeniedFlag == 1 && w.DeleteFlag == 0)
                .Select(s => s.ProgramRequestId).ToArray();

            List<int> lstProgramIds = lstProgramReq.Where(w => w.DeleteFlag == 0)
           .Select(s => s.ProgramClassId).ToList();

            List<QueueEligibilityPerson> getAppealReq = _context.ProgramRequestAppeal.
                Where(w => lstDenyIds.Contains(w.ProgramRequestId)
                && (!w.DeniedFlag.HasValue || w.DeniedFlag == 0))
                .Select(p => new QueueEligibilityPerson
                {
                    ProgramRequestId = p.ProgramRequestId,
                    PersonnelId = p.AppealBy,
                    Reason = p.AppealReason,
                    Note = p.AppealNote,
                    Date = p.AppealDate
                }).ToList();

            List<ProgramEligibility> lstProgramreqEligibilities = lstProgramReq.Select(p => new ProgramEligibility
            {
                RequestId = p.ProgramRequestId,
                ProgramClassId = p.ProgramClassId,
                InmateId = p.InmateId,
                PriorityLevel = p.PriorityLevel,
                Note = p.RequestNote,
                RequestDate = p.CreateDate,
                DeleteFlag = p.DeleteFlag == 1,
                ClassifyRouteFlag = p.ClassifyRouteFlag == 1,
                DeniedFlag = p.DeniedFlag == 1,
                //AppealFlag = p.AppealFlag == 1,

                // This program table need to change based on programClass
                //ProgramCategory = p.Program.ProgramCategory.ProgramCategory1,
                //ClassOrServiceName = p.Program.ClassOrServiceName,
                AssignedDetails = new QueueEligibilityPerson
                {
                    PersonnelId = p.AssignedBy,
                    Note = p.AssignedNote,
                    Date = p.AssignedDate
                },
                ClassifyRouteDetails = new QueueEligibilityPerson
                {
                    PersonnelId = p.ClassifyRouteBy,
                    Reason = p.ClassifyRouteReason,
                    Note = p.ClassifyRouteNote,
                    Date = p.ClassifyRouteDate
                },
                DeniedDetails = new QueueEligibilityPerson
                {
                    PersonnelId = p.DeniedBy,
                    Reason = p.DeniedReason,
                    Note = p.DeniedNote,
                    Date = p.DeniedDate
                },
                AppointmentProgramAssignId = p.AppointmentProgramAssignId ?? 0,
                UpdateDetails = new QueueEligibilityPerson
                {
                    UpdateDate = p.UpdateDate,
                    PersonnelId = p.UpdateBy
                },
                CreateDate = p.CreateDate,
                PhotoFilePath = _photoService.GetPhotoByIdentifier(p.Inmate.Person.Identifiers
                    .LastOrDefault(s => s.IdentifierType == "1" && s.DeleteFlag == 0))
            }).ToList();
            //Person Details List
            List<PersonVm> lstPersonDetails =
                _inmateService.GetInmateDetails(lstProgramreqEligibilities.Select(i => i.InmateId).ToList());
            //List of Housing Details for inmates 
            List<HousingDetail> lstHousingDetail = _facilityHousingService.GetHousingList(lstPersonDetails
                .Where(i => i.HousingUnitId.HasValue).Select(i => i.HousingUnitId.Value).ToList());

            //List of Schedule Details for Program 

            //List of all Officer Ids
            List<int> personnelId = lstProgramreqEligibilities.Select(i => new[]
            {
                i.AssignedDetails.PersonnelId, i.ClassifyRouteDetails.PersonnelId,
                i.DeniedDetails.PersonnelId, i.UpdateDetails.PersonnelId
                //i.AppealDetails.PersonnelId
            }).SelectMany(i => i).Where(i => i.HasValue).Select(i => i.Value).ToList();
            //List of Officer Details
            List<PersonnelVm> lstPersonDet = _personService.GetPersonNameList(personnelId);

            IQueryable<AppointmentProgramAssign> lstApp = _context.AppointmentProgramAssign;
            List<ScheduleVm> schList = _context.ScheduleProgram.Where(a =>
              !a.DeleteFlag &&
              (programId == 0 || lstProgramIds.Contains(a.ProgramClassId ?? 0))).Select(sch => new ScheduleVm
              {
                  StartDate = sch.StartDate, //apptstartdate hrs
                  EndDate = sch.EndDate, //apptenddate hrs
                  LocationId = sch.LocationId ?? 0,
                  ScheduleId = sch.ScheduleId, // appointment id
                                               // ProgramId = sch.ProgramId, // program id
                                               //InmateId = sch.InmateId ?? 0,
                                               //ReasonId = sch.ReasonId,
                                               //TypeId = sch.TypeId,
                  Location = sch.Location.PrivilegeDescription,
                  Duration = sch.Duration,
                  DeleteReason = sch.DeleteReason,
                  //Notes = sch.Notes,
                  IsSingleOccurrence = sch.IsSingleOccurrence,
                  LocationDetail = sch.LocationDetail, // apptlocation
                  DayInterval = sch.DayInterval,
                  WeekInterval = sch.WeekInterval,
                  FrequencyType = sch.FrequencyType,
                  QuarterInterval = sch.QuarterInterval,
                  MonthOfQuarterInterval = sch.MonthOfQuarterInterval,
                  MonthOfYear = sch.MonthOfYear,
                  DayOfMonth = sch.DayOfMonth
              }).ToList();

            List<int> lstprogamids = schList.Select(s => s.ProgramId ?? 0).ToList();

            lstProgramreqEligibilities.ForEach(item =>
            {
                item.InmateDetails = lstPersonDetails.Single(inm => inm.InmateId == item.InmateId);
                item.AppealDetails = getAppealReq.LastOrDefault(w => w.ProgramRequestId == item.RequestId);
                item.ScheduleDetails = schList.Where(inm => lstprogamids.Contains(item.ProgramClassId)).ToList();
                if (item.InmateDetails.HousingUnitId.HasValue)
                {
                    item.HousingDetails =
                        lstHousingDetail.Single(inm => inm.HousingUnitId == item.InmateDetails.HousingUnitId.Value);
                }

                if (item.AssignedDetails.PersonnelId.HasValue)
                {
                    PersonnelVm personInfo =
                        lstPersonDet.Single(p => p.PersonnelId == item.AssignedDetails.PersonnelId);
                    item.AssignedDetails.PersonLastName = personInfo?.PersonLastName;
                    item.AssignedDetails.OfficerBadgeNumber = personInfo?.OfficerBadgeNumber;
                }

                if (item.ClassifyRouteDetails.PersonnelId.HasValue)
                {
                    PersonnelVm personInfo =
                        lstPersonDet.Single(p => p.PersonnelId == item.ClassifyRouteDetails.PersonnelId);
                    item.ClassifyRouteDetails.PersonLastName = personInfo?.PersonLastName;
                    item.ClassifyRouteDetails.OfficerBadgeNumber = personInfo?.OfficerBadgeNumber;
                }

                if (item.DeniedDetails.PersonnelId.HasValue)
                {
                    PersonnelVm personInfo =
                        lstPersonDet.Single(p => p.PersonnelId == item.DeniedDetails.PersonnelId);
                    item.DeniedDetails.PersonLastName = personInfo?.PersonLastName;
                    item.DeniedDetails.OfficerBadgeNumber = personInfo?.OfficerBadgeNumber;
                }
                if (item.UpdateDetails.PersonnelId.HasValue)
                {
                    PersonnelVm personInfo =
                        lstPersonDet.Single(p => p.PersonnelId == item.UpdateDetails.PersonnelId);
                    item.UpdateDetails.PersonLastName = personInfo?.PersonLastName;
                    item.UpdateDetails.OfficerBadgeNumber = personInfo?.OfficerBadgeNumber;
                }
                if (item.AppointmentProgramAssignId > 0)
                {
                    AppointmentProgramAssign appointmentinfo = lstApp?.SingleOrDefault
                        (x => x.AppointmentProgramAssignId == item.AppointmentProgramAssignId);
                    item.ProgramComplete = appointmentinfo?.ProgramComplete;
                    item.ProgramNotComplete = appointmentinfo?.ProgramNotComplete;
                    item.ProgramUnassignReason = appointmentinfo?.ProgramUnassignReason;
                }
            });

            return lstProgramreqEligibilities;
        }

        // load the inmate & program request mutiple drop down list
        public List<ProgramEligibility> GetProgramClassNameList(int facilityId, int inmateId)
        {
            if (inmateId > 0)
            {
                inmateClassificationId =
                   _context.Inmate.Single(w => w.InmateId == inmateId).InmateClassificationId ?? 0;
                personSexId = _context.Inmate.Where(w => w.InmateId == inmateId)
                   .Select(s => s.Person.PersonSexLast ?? 0).SingleOrDefault();
            }

            if (inmateClassificationId > 0)
            {
                //get classification
                filtertype = _context.InmateClassification
                    .Single(w => w.InmateId == inmateId && w.InmateClassificationId == inmateClassificationId)
                    .InmateClassificationReason;
            }

            if (personSexId > 0)
            {
                //get gender
                gender = _context.Lookup
                    .First(w => w.LookupIndex == personSexId && w.LookupType == LookupConstants.SEX)
                    .LookupDescription;
            }

            List<ProgramEligibility> lsProgramEligibility = _context.ProgramClass
                .Select(r => new ProgramEligibility
                {
                    ProgramClassId = r.ProgramClassId,
                    ProgramClassName = r.CRN,
                    ProgramCourseName = r.ProgramCourseIdNavigation.CourseName,
                    ProgramCategory = r.ProgramCourseIdNavigation.ProgramCategory,
                    IsChecked = false
                }).ToList();

            return lsProgramEligibility;
        }

        // load the inmate & program request drop down list
        public List<ProgramEligibility> GetProgramClassList(int facilityId, int inmateId)
        {
            if (inmateId > 0)
            {
                inmateClassificationId =
                    _context.Inmate.Single(w => w.InmateId == inmateId).InmateClassificationId ?? 0;
                personSexId = _context.Inmate.Where(w => w.InmateId == inmateId)
                    .Select(s => s.Person.PersonSexLast ?? 0).SingleOrDefault();
            }

            if (inmateClassificationId > 0)
            {
                //get classification
                filtertype = _context.InmateClassification
                    .Single(w => w.InmateId == inmateId && w.InmateClassificationId == inmateClassificationId)
                    .InmateClassificationReason;
            }

            if (personSexId > 0)
            {
                //get gender
                gender = _context.Lookup
                    .First(w => w.LookupIndex == personSexId && w.LookupType == LookupConstants.SEX)
                    .LookupDescription;
            }

            List<ProgramEligibility> lsKeyValuePairs = _context.Program
                .Where(p => (facilityId == 0 || p.ProgramCategory.FacilityId == facilityId) &&
                    p.DeleteFlag == 0 && p.ProgramCategory.DeleteFlag == 0 &&
                    (string.IsNullOrEmpty(filtertype) || p.ClassOrServiceClassFilter == filtertype ||
                        p.ClassOrServiceClassFilter == "" || p.ClassOrServiceClassFilter == null) &&
                    (string.IsNullOrEmpty(gender) || p.ClassOrServiceGenderFilter == gender ||
                        p.ClassOrServiceGenderFilter == "" || p.ClassOrServiceGenderFilter == null))
                .Select(r => new ProgramEligibility
                {
                    ProgramClassId = r.ProgramId,
                    ClassOrServiceNumber = r.ClassOrServiceNumber,
                    ClassOrServiceName = r.ClassOrServiceName,
                    ProgramCategory = r.ProgramCategory.ProgramCategory1
                }).ToList();

            return lsKeyValuePairs;
        }

        //save the multiple request
        public async Task<int> SaveProgramRequestDetails(ProgramRequestInputVm values)
        {
            List<ProgramRequest> programRequestadd = values.ListProgramEligibility
                //.Where(w => w.IsChecked)
                .Select(s => new ProgramRequest
                {
                    CreateBy = _personnelId,
                    CreateDate = DateTime.Now,
                    InmateId = values.InmateId,

                    RequestNote = values.Notes,
                    DeleteFlag = 0,
                    ProgramClassId = s.ProgramClassId // This program table need to change based on programClass
                }).ToList();

            _context.ProgramRequest.AddRange(programRequestadd);
            _context.SaveChanges();
            programRequestadd.ForEach(pr => _interfaceEngine.Export(new ExportRequestVm {
                EventName = EventNameConstants.PROGRAMREQUEST,
                PersonnelId = _personnelId,
                Param1 = pr.InmateId.ToString(),
                Param2 = pr.ProgramRequestId.ToString()
            }));
            return await _context.SaveChangesAsync();
        }

        // check the validation for save request
        public int ProgramRequestValid(int programId, int inmateId)
        {
            // This program table need to change based on programClass
            int lProgramRequest = _context.ProgramRequest.Count(s =>
                s.InmateId == inmateId && s.ProgramClassId == programId && !s.AppointmentProgramAssignId.HasValue
                && s.DeleteFlag == 0 && !s.DeniedFlag.HasValue);
            //request program
            if (lProgramRequest > 0)
            {
                lProgramRequest = programId;
            }
            return lProgramRequest;
        }

        // inmate schedule save & unassign
        public async Task<int> InsertInmateSchedule(ProgramEligibility values)
        {
            int appointmentId = _context.AppointmentProgramAssign
                .Single(ap => ap.AppointmentProgramAssignId == values.AppointmentProgramAssignId)
                .AppointmentId ?? 0;

            AppointmentProgramAssign appointmentProgramAssign = _context.AppointmentProgramAssign.Single(p =>
                p.AppointmentProgramAssignId == values.AppointmentProgramAssignId &&
                p.AppointmentId == appointmentId);

            appointmentProgramAssign.InmateNote = values.InmateNote;
            appointmentProgramAssign.ProgramPass = values.ProgramPass;
            appointmentProgramAssign.ProgramNotPass = values.ProgramNotPass;
            appointmentProgramAssign.ProgramGrade = values.ProgramGrade;
            appointmentProgramAssign.UpdateDate = DateTime.Now;
            appointmentProgramAssign.UpdateBy = _personnelId;

            string programAttendCertificate = _context.ProgramAttendCertificate.Single(s =>
                s.ScheduleId == appointmentId && s.InmateId == values.InmateId).CertificateName;

            if (!values.IsCheckCertificate)
            {
                if (!string.IsNullOrEmpty(programAttendCertificate))
                {
                    ProgramAttendCertificate programAttendCertificate2 = _context.ProgramAttendCertificate.Single(s =>
                        s.ScheduleId == appointmentId &&
                        s.InmateId == values.InmateId);

                    programAttendCertificate2.DeleteFlag = 1;
                    programAttendCertificate2.DeleteBy = _personnelId;
                    programAttendCertificate2.DeleteDate = DateTime.Now;
                }
            }

            return await _context.SaveChangesAsync();
        }

        public async Task<int> DeleteProgramRequest(ProgramCatogoryVm Inputs)
        {
            if (Inputs.RequestIds.Count > 0)
            {
                Inputs.RequestIds.ForEach(reqId =>
                {
                    ProgramRequest programRequestDetails = _context.ProgramRequest.SingleOrDefault(p =>
                                p.ProgramRequestId == reqId);
                    if (programRequestDetails == null) return;
                    programRequestDetails.DeleteFlag = Inputs.DeleteFlag ? 1 : 0;
                    programRequestDetails.UpdateBy = _personnelId;
                    programRequestDetails.UpdateDate = DateTime.Now;
                });
            }
            return await _context.SaveChangesAsync();
        }

        public async Task<int> SaveAssignRequest(List<ProgramEligibility> Inputs)
        {
            Inputs.ForEach(req =>
            {
                List<AppointmentProgramAssign> appointmentProgramAssigns = req
                .ScheduleDetails.Select(a => new AppointmentProgramAssign
                {
                    CreateBy = _personnelId,
                    AppointmentId = a.ScheduleId,
                    CreateDate = DateTime.Now,
                    DeleteFlag = 0,
                    InmateId = req.InmateId,
                    InmateNote = req.InmateNote,
                    UpdateDate = DateTime.Now
                }).ToList();
                _context.AppointmentProgramAssign.AddRange(appointmentProgramAssigns);
                _context.SaveChangesAsync();
                ProgramRequest programRequestDetails = _context.ProgramRequest.Single(p =>
                        p.ProgramRequestId == req.RequestId);
                programRequestDetails.AppointmentProgramAssignId = appointmentProgramAssigns.First()
                .AppointmentProgramAssignId;
                programRequestDetails.AssignedBy = _personnelId;
                programRequestDetails.AssignedDate = DateTime.Now;
                programRequestDetails.AssignedNote = req.InmateNote;
                programRequestDetails.UpdateBy = _personnelId;
                programRequestDetails.UpdateDate = DateTime.Now;
                programRequestDetails.DeniedFlag = 0;
                programRequestDetails.ClassifyRouteFlag = 0;
            });
            return await _context.SaveChangesAsync();
        }

        #region Appeal

        //Get All Appeal Details
        public List<ProgramEligibility> GetProgramRequestAppeal(int facilityId)
        {
            List<ProgramEligibility> lstPrgReqApplea = _context.ProgramRequestAppeal.Where(pr =>
                        pr.ProgramRequestAppealNavigation.Inmate.InmateActive == 1
                )
                .Select(p => new ProgramEligibility
                {
                    RequestId = p.ProgramRequestId,
                    ProgramClassId = p.ProgramRequestAppealNavigation.ProgramClassId,
                    InmateId = p.ProgramRequestAppealNavigation.InmateId,
                    RequestDate = p.CreateDate,
                    DeleteFlag = p.DeleteFlag == 1,
                    ClassifyRouteFlag = p.ClassifyRouteFlag == 1,
                    DeniedFlag = p.DeniedFlag == 1,
                    AppealFlag = p.AppealFlag == 1,
                    SendNoteToHousing = p.SendNoteToHousing,
                    DeniedDetails = new QueueEligibilityPerson
                    {
                        PersonnelId = p.DeniedBy,
                        Reason = p.DeniedReason,
                        Note = p.DeniedNote,
                        Date = p.DeniedDate
                    },
                    AppealDetails = new QueueEligibilityPerson
                    {
                        PersonnelId = p.AppealBy,
                        Reason = p.AppealReason,
                        Note = p.AppealNote,
                        Date = p.AppealDate
                    },
                }).ToList();
            //Person Details List
            List<PersonVm> lstPersonDetails =
                _inmateService.GetInmateDetails(lstPrgReqApplea.Select(i => i.InmateId).ToList());
            //List of Housing Details for inmates 
            List<HousingDetail> lstHousingDetail = _facilityHousingService.GetHousingList(lstPersonDetails
                .Where(i => i.HousingUnitId.HasValue).Select(i => i.HousingUnitId.Value).ToList());
            //List of all Officer Ids
            List<int> personnelId = lstPrgReqApplea.Select(i => new[] {i.AppealDetails.PersonnelId})
                .SelectMany(i => i).Where(i => i.HasValue).Select(i => i.Value).ToList();
            //List of Officer Details
            List<PersonnelVm> lstPersonDet = _personService.GetPersonNameList(personnelId);
            lstPrgReqApplea.ForEach(item =>
            {
                item.InmateDetails = lstPersonDetails.Single(inm => inm.InmateId == item.InmateId);
                if (item.InmateDetails.HousingUnitId.HasValue)
                {
                    item.HousingDetails =
                        lstHousingDetail.Single(inm => inm.HousingUnitId == item.InmateDetails.HousingUnitId.Value);
                }

                if (!item.AppealDetails.PersonnelId.HasValue) return;
                PersonnelVm personInfo =
                    lstPersonDet.Single(p => p.PersonnelId == item.AppealDetails.PersonnelId);
                item.AppealDetails.PersonLastName = personInfo?.PersonLastName;
                item.AppealDetails.OfficerBadgeNumber = personInfo?.OfficerBadgeNumber;
            });
            return lstPrgReqApplea;
        }
        //Save
        public async Task<int> SaveAppealProgramRequest(ProgramCatogoryVm Inputs)
        {
            Inputs.RequestIds.ForEach(reqId =>
            {
                ProgramRequestAppeal appeal = new ProgramRequestAppeal
                {
                    AppealBy = _personnelId,
                    AppealDate = DateTime.Today,
                    CreateDate = DateTime.Today,
                    CreateBy = _personnelId,
                    AppealFlag = 1,
                    AppealReason = Inputs.AppealReason,
                    AppealNote = Inputs.AppealNote,
                    ProgramRequestId = reqId,
                    DeniedDate = Inputs.DeniedDate
                };
                _context.ProgramRequestAppeal.Add(appeal);
            });
            return await _context.SaveChangesAsync();
        }

        public async Task<int> DeleteDenied(int Inputs, bool DeleteFlag)
        {
            if (Inputs == 0) return -1;
            ProgramRequest programRequestDetails = _context.ProgramRequest.SingleOrDefault(p =>
                p.ProgramRequestId == Inputs);
            if (programRequestDetails != null)
            {
                programRequestDetails.DeleteFlag = DeleteFlag ? 1 : 0;
                programRequestDetails.UpdateBy = _personnelId;
                programRequestDetails.UpdateDate = DateTime.Now;
            }
            List<ProgramRequestAppeal> appeal = _context.ProgramRequestAppeal.Where(p =>
                p.ProgramRequestId == Inputs).ToList();
            appeal.ForEach(f =>
            {
                f.DeleteFlag = DeleteFlag ? 1 : 0;
                f.DeleteBy = _personnelId;
                f.DeleteDate = DateTime.Now;
            });
            return await _context.SaveChangesAsync();
        }

        public async Task<int> SaveAppealAssignRequest(ProgramEligibility Inputs)
        {
            List<AppointmentProgramAssign> appointmentProgramAssigns = Inputs
                .ScheduleDetails.Select(a => new AppointmentProgramAssign
                {
                    CreateBy = _personnelId,
                    AppointmentId = a.ScheduleId,
                    CreateDate = DateTime.Now,
                    DeleteFlag = 0,
                    InmateId = Inputs.InmateId,
                    InmateNote = Inputs.InmateNote,
                    UpdateDate = DateTime.Now
                }).ToList();
            _context.AppointmentProgramAssign.AddRange(appointmentProgramAssigns);
            await _context.SaveChangesAsync();

            ProgramRequest programRequestDetails = _context.ProgramRequest.Single(p =>
                    p.ProgramRequestId == Inputs.RequestId);
            programRequestDetails.AppointmentProgramAssignId = appointmentProgramAssigns.First()
            .AppointmentProgramAssignId;
            programRequestDetails.AssignedBy = _personnelId;
            programRequestDetails.AssignedDate = DateTime.Now;
            programRequestDetails.AssignedNote = Inputs.InmateNote;
            programRequestDetails.UpdateBy = _personnelId;
            programRequestDetails.UpdateDate = DateTime.Now;
            programRequestDetails.DeniedFlag = 0;
            programRequestDetails.ClassifyRouteFlag = 0;

            ProgramRequestAppeal appealAssignDetails = _context.ProgramRequestAppeal.Last(p =>
                   p.ProgramRequestId == Inputs.RequestId);
            appealAssignDetails.AppointmentProgramAssignId = appointmentProgramAssigns.First()
            .AppointmentProgramAssignId;
            appealAssignDetails.AssignedBy = _personnelId;
            appealAssignDetails.AssignedDate = DateTime.Now;
            appealAssignDetails.AssignedNote = Inputs.InmateNote;
            appealAssignDetails.UpdateBy = _personnelId;
            appealAssignDetails.UpdateDate = DateTime.Now;
            appealAssignDetails.DeniedFlag = 0;
            appealAssignDetails.ClassifyRouteFlag = 0;
            return await _context.SaveChangesAsync();
        }

        // load the program appointment for program request
        public List<ProgramEligibility> ListAppointment(int inmateId, int assignId)
        {
            List<ScheduleVm> schList = _context.ScheduleInmate.Where(a =>
                    !a.DeleteFlag && (inmateId == 0 || a.InmateId == inmateId))
                .Select(sch => new ScheduleVm
                {
                    StartDate = sch.StartDate,
                    EndDate = sch.EndDate,
                    LocationId = sch.LocationId ?? 0,
                    ScheduleId = sch.ScheduleId,
                    InmateId = sch.InmateId ?? 0,
                    Duration = sch.Duration,
                    IsSingleOccurrence = sch.IsSingleOccurrence,
                    LocationDetail = sch.LocationDetail,
                    DayInterval = sch.DayInterval,
                    WeekInterval = sch.WeekInterval,
                    FrequencyType = sch.FrequencyType,
                    QuarterInterval = sch.QuarterInterval,
                    MonthOfQuarterInterval = sch.MonthOfQuarterInterval,
                    MonthOfYear = sch.MonthOfYear,
                    DayOfMonth = sch.DayOfMonth
                }).ToList();

            List<int> lstSchInmateIds = schList.Select(s => s.InmateId ?? 0).ToList();

            IQueryable<AppointmentProgramAssign> lstAppointmentProgramAssigns = _context.AppointmentProgramAssign
                .Where(a => lstSchInmateIds.Contains(a.InmateId) && a.AppointmentProgramAssignId == assignId &&
                    a.DeleteFlag == 0);

            List<int> lstAppInmateIds = lstAppointmentProgramAssigns.Select(inm => new[]
            {
                inm.InmateId, inm.AppointmentId
            }).SelectMany(i => i).Where(i => i.HasValue).Select(i => i.Value).ToList();

            //attendtotal && not attendtotal
            IQueryable<ProgramAttendInmate> lAttendInmates = _context.ProgramAttendInmate.Where(p =>
                lstAppInmateIds.Contains(p.InmateId) && (p.AttendFlag == 1 || p.NotAttendFlag == 1) &&
                lstAppInmateIds.Contains(p.ProgramAttend.ScheduleId)
                && p.ProgramAttend.CompleteFlag == 1);

            List<ProgramEligibilityClassVm> lsPrograms = _context.ProgramRequest.Where(s => lstAppInmateIds.Contains(s.InmateId))
                .Select(r => new ProgramEligibilityClassVm
                {
                    ProgramId = r.ProgramClassId,
                    InmateId = r.InmateId,
                }).ToList();

            List<ProgramEligibility> listProgramEligibilities = _context.AppointmentProgramAssign
                .Where(s => s.InmateId == inmateId && s.DeleteFlag == 0)
                .Select(sch => new ProgramEligibility
                {
                    CreateDate = sch.CreateDate, // Assign
                    DeleteDate = sch.DeleteDate, // Unassign
                    ProgramComplete = sch.ProgramComplete,
                    ProgramNotComplete = sch.ProgramNotComplete,
                    ProgramUnassignReason = sch.ProgramUnassignReason,
                    AttendCount = lAttendInmates.Count(s => s.AttendFlag == 1), // attend cnt
                    NotAttendCount = lAttendInmates.Count(s => s.NotAttendFlag == 1), // not attend cnt
                    ProgramGrade = sch.ProgramGrade,
                    ProgramPass = sch.ProgramPass, // its split into client side
                    ProgramNotPass = sch.ProgramNotPass, // its split into client side
                    Createby = sch.CreateBy,
                    Deleteby = sch.DeleteBy,
                    ProgramUnassignNote = sch.ProgramUnassignNote
                }).ToList();

            listProgramEligibilities.ForEach(item =>
            {
                item.SchedulesDetails.AddRange(schList.Where(i => i.InmateId == item.InmateId));
                item.ProgramEligibilityClass.AddRange(lsPrograms.Where(i => i.InmateId == item.InmateId));
            });

            return listProgramEligibilities;
        }

        #endregion
        public List<ProgramEligibility> GetRequestByCourse(int programCourseId)
        {
            // This program table need to change based on programClass
            List<int> lstProgramClass = _context.ProgramClass.Where(pro =>
              pro.ProgramCourseId == programCourseId).Select(a => a.ProgramClassId).ToList();

            List<ProgramEligibility> lstClassProgramrequest = _context.ProgramRequest.
            Where(w => lstProgramClass.Any(a => a == w.ProgramClassId && w.DeniedFlag != 1 && w.DeleteFlag != 1))
            .Select(p => new ProgramEligibility
            {
                ProgramRequestId = p.ProgramRequestId,
                ProgramClassId = p.ProgramClassId,
                ProgramCourseId=p.ProgramClass.ProgramCourseId,
                InmateId = p.InmateId,
                Note = p.RequestNote,
                CreateDate = p.CreateDate,
                ApproveFlag = p.ApprovedFlag,
                ClassifyRouteFlag = p.ClassifyRouteFlag == 1,

                ClassifyRouteDetails = new QueueEligibilityPerson
                {
                    PersonnelId = p.ClassifyRouteBy,
                    Reason = p.ClassifyRouteReason,
                    Note = p.ClassifyRouteNote,
                    Date = p.ClassifyRouteDate
                }
            }).ToList();
            int[] assignReqIds = _context.ProgramClassAssign.Select(s => s.ProgramRequestId).ToArray();

            lstClassProgramrequest = lstClassProgramrequest.Where(w => assignReqIds
                .All(a => a != w.ProgramRequestId)).ToList();

            List<Lookup> lstLookups = _context.Lookup.Where(look => look.LookupType == LookupConstants.SEX && look.LookupInactive == 0).ToList();
            List<int> inmateIds = lstClassProgramrequest.Select(i => i.InmateId).ToList();

            List<InmateDetailsList> inmateDetails = _context.Inmate.Where(i => inmateIds.Contains(i.InmateId)).Select(i =>
                           new InmateDetailsList
                           {
                               FacilityId = i.FacilityId,
                               FacilityAbbr = i.Facility.FacilityAbbr,
                               InmateNumber = i.InmateNumber,
                               InmateId = i.InmateId,
                               PersonFirstName = i.Person.PersonFirstName,
                               PersonLastName = i.Person.PersonLastName,
                               PersonMiddleName = i.Person.PersonMiddleName,
                               HousingUnitId = i.HousingUnitId ?? 0,
                               PersonDob = i.Person.PersonDob,
                               HousingLocation = i.HousingUnit.HousingUnitLocation,
                               HousingNumber = i.HousingUnit.HousingUnitNumber,
                               HousingUnitBedLocation = i.HousingUnit.HousingUnitBedLocation,
                               HousingUnitBedNumber = i.HousingUnit.HousingUnitBedNumber,
                               HousingUnitListId = i.HousingUnit.HousingUnitId > 0 ? i.HousingUnit.HousingUnitListId : 0,
                               PersonId = i.PersonId,
                               PersonSexLast = i.Person.PersonSexLast,
                               InmateActive = i.InmateActive,
                               ClassificationReason = i.InmateClassification.InmateClassificationReason,
                               TrackLocation = i.InmateCurrentTrackNavigation.PrivilegeDescription
                           }).ToList();

            lstClassProgramrequest.ForEach(item =>
                  {
                      item.InmateInfo = inmateDetails.Single(inm => inm.InmateId == item.InmateId);
                      if (item.InmateInfo.PersonSexLast > 0)
                      {
                          item.Gender = lstLookups.Single(l =>
                              l.LookupIndex == item.InmateInfo.PersonSexLast)?.LookupDescription;
                      }

                      if(item.ProgramCourseId==programCourseId)
                      {
                          item.RequestCount= GetRequestCount(programCourseId);
                      }

                  });
            return lstClassProgramrequest;
        }

        public List<ProgramEligibility> GetRequestByInmate(int inmateId)
        {
            List<ProgramClassAssign> lstAssignReq = _context.ProgramClassAssign.
            Where(p => p.CourseComplete).
                         Select(s => new ProgramClassAssign
                         {
                             ProgramClassAssignId = s.ProgramClassAssignId,
                             CourseComplete = s.CourseComplete,
                             AssignedBy = s.AssignedBy,
                             AssignedDate = s.AssignedDate,
                             ProgramRequestId = s.ProgramRequestId,
                         }).ToList();

            List<int> requestIds = lstAssignReq.Select(a => a.ProgramRequestId).ToList();

            // This program table need to change based on programClass
            List<ProgramEligibility> lstClassProgramrequest = _context.ProgramRequest
                .Where(w => w.InmateId == inmateId && !requestIds.Contains(w.ProgramRequestId))
                .Select(p => new ProgramEligibility
                {
                    ProgramRequestId = p.ProgramRequestId,
                    ProgramClassId = p.ProgramClassId,
                    InmateId = p.InmateId,
                    Note = p.RequestNote,
                    CreateDate = p.CreateDate,
                    ApproveFlag = p.ApprovedFlag,
                    DeniedFlag = p.DeniedFlag == 1,
                    DeleteFlag = p.DeleteFlag == 1,
                    ClassifyRouteFlag = p.ClassifyRouteFlag == 1,
                    ProgramCourseName = p.ProgramClass.ProgramCourseIdNavigation.CourseName,
                    ProgramClassName = p.ProgramClass.CRN,

                    ClassifyRouteDetails = new QueueEligibilityPerson
                    {
                        PersonnelId = p.ClassifyRouteBy,
                        Reason = p.ClassifyRouteReason,
                        Note = p.ClassifyRouteNote,
                        Date = p.ClassifyRouteDate
                    },
                    DeniedDetails = new QueueEligibilityPerson
                    {
                        PersonnelId = p.DeniedBy,
                        Reason = p.DeniedReason,
                        Note = p.DeniedNote,
                        Date = p.DeniedDate
                    },
                    ApproveDetails = new QueueEligibilityPerson
                    {
                        PersonnelId = p.ApprovedBy,
                        Date = p.ApprovedDate,
                    }
                }).ToList();

            List<Lookup> lstLookups = _context.Lookup
                .Where(look => look.LookupType == LookupConstants.SEX && look.LookupInactive == 0).ToList();
            List<int> inmateIds = lstClassProgramrequest.Select(i => i.InmateId).ToList();

            List<InmateDetailsList> inmateDetails = _context.Inmate.Where(i => inmateIds.Contains(i.InmateId)).Select(i =>
                           new InmateDetailsList
                           {
                               FacilityId = i.FacilityId,
                               FacilityAbbr = i.Facility.FacilityAbbr,
                               InmateNumber = i.InmateNumber,
                               InmateId = i.InmateId,
                               PersonFirstName = i.Person.PersonFirstName,
                               PersonLastName = i.Person.PersonLastName,
                               PersonMiddleName = i.Person.PersonMiddleName,
                               HousingUnitId = i.HousingUnitId ?? 0,
                               PersonDob = i.Person.PersonDob,
                               HousingLocation = i.HousingUnit.HousingUnitLocation,
                               HousingNumber = i.HousingUnit.HousingUnitNumber,
                               HousingUnitBedLocation = i.HousingUnit.HousingUnitBedLocation,
                               HousingUnitBedNumber = i.HousingUnit.HousingUnitBedNumber,
                               HousingUnitListId = i.HousingUnit.HousingUnitId > 0 ? i.HousingUnit.HousingUnitListId : 0,
                               PersonId = i.PersonId,
                               PersonSexLast = i.Person.PersonSexLast,
                               InmateActive = i.InmateActive,
                               ClassificationReason = i.InmateClassification.InmateClassificationReason,
                               TrackLocation = i.InmateCurrentTrackNavigation.PrivilegeDescription
                           }).ToList();

            lstClassProgramrequest.ForEach(item =>
                  {
                      item.InmateInfo = inmateDetails.Single(inm => inm.InmateId == item.InmateId);
                      if (item.InmateInfo.PersonSexLast > 0)
                          item.Gender = lstLookups.Single(l =>
                          l.LookupIndex == item.InmateInfo.PersonSexLast)?.LookupDescription;
                      if (lstAssignReq.Count > 0)
                      {
                          item.AssignedDate = lstAssignReq.SingleOrDefault(a => a.ProgramRequestId == item.ProgramRequestId)?.AssignedDate;
                      }
                  });

            return lstClassProgramrequest;
        }

        public async Task<int> SaveDenyandSentProgramRequest(ProgramCatogoryVm Inputs)
        {
            if (Inputs.RequestIds.Count > 0)
            {
                Inputs.RequestIds.ForEach(reqId =>
                {
                    ProgramRequest programRequestDetails = _context.ProgramRequest.SingleOrDefault(p =>
                                p.ProgramRequestId == reqId);
                    if (programRequestDetails == null) return;
                    if (Inputs.DeniedFlag)
                    {
                        programRequestDetails.DeniedFlag = Inputs.DeniedFlag ? 1 : 0;
                        programRequestDetails.DeniedBy = _personnelId;
                        programRequestDetails.DeniedDate = DateTime.Now;
                        programRequestDetails.DeniedNote = Inputs.DeniedNote;
                        programRequestDetails.DeniedReason = Inputs.DenyReason;
                    }
                    else
                    {
                        programRequestDetails.ClassifyRouteFlag = Inputs.ClassifyRouteFlag ? 1 : 0;
                        programRequestDetails.ClassifyRouteBy = _personnelId;
                        programRequestDetails.ClassifyRouteDate = DateTime.Now;
                        programRequestDetails.ClassifyRouteNote = Inputs.SentNote;
                        programRequestDetails.ClassifyRouteReason = Inputs.SentReason;
                    }
                });
            }
            return await _context.SaveChangesAsync();
        }

        public async Task<int> SaveApproveRequest(ProgramCatogoryVm Inputs)
        {
            if (Inputs.RequestIds.Count > 0)
            {
                Inputs.RequestIds.ForEach(reqId =>
                {
                    ProgramRequest programRequestDetails = _context.ProgramRequest.SingleOrDefault(p =>
                                p.ProgramRequestId == reqId);
                    if (programRequestDetails == null) return;
                    if (!Inputs.ApproveFlag) return;
                    programRequestDetails.ApprovedFlag = Inputs.ApproveFlag;
                    programRequestDetails.ApprovedBy = _personnelId;
                    programRequestDetails.ApprovedDate = DateTime.Now;

                });
            }
            return await _context.SaveChangesAsync();
        }

        public List<ProgramRequestCountVm> GetRequestCount(int programCourseId)
        {
            List<ProgramRequestCountVm> lstProgramCourse = _context.ProgramCourse.Where(pro =>
                programCourseId == 0 || pro.ProgramCourseId == programCourseId).Select(a => new ProgramRequestCountVm
            {
                ProgramCourseId = a.ProgramCourseId,
                CourseName = a.CourseName,
                CourseNumber = a.CourseNumber,
                ProgramCategory = a.ProgramCategory
            }).ToList();

            List<ProgramClass> lstProgramClass = _context.ProgramClass.Where(w => lstProgramCourse.Any(a =>
                    a.ProgramCourseId == w.ProgramCourseId
                    && !w.DeleteFlag && !w.InactiveFlag))
                .Select(p => new ProgramClass
                {
                    ProgramClassId = p.ProgramClassId,
                    CourseCapacity = p.CourseCapacity,
                    ProgramCourseId = p.ProgramCourseId,

                }).ToList();

            List<ProgramEligibility> lstactiveProgramrequest = _context.ProgramRequest.Where(w =>
                    lstProgramClass.Any(a => a.ProgramClassId == w.ProgramClassId && w.ClassifyRouteFlag != 1 &&
                        w.DeniedFlag != 1 && w.DeleteFlag != 1) &&
                    lstProgramCourse.Any(a => a.ProgramCourseId == w.ProgramClass.ProgramCourseId))
                .Select(p => new ProgramEligibility
                {
                    ProgramRequestId = p.ProgramRequestId,
                    ApproveFlag = p.ApprovedFlag,
                    ProgramCourseId = p.ProgramClass.ProgramCourseId,
                }).ToList();
            List<ProgramRequestCountVm> lstEnrollrequest = _context.ProgramClassAssign.Where(w =>
                    lstactiveProgramrequest.Any(a => a.ProgramRequestId == w.ProgramRequestId && a.ApproveFlag
                    ))
                .Select(i =>
                    new ProgramRequestCountVm
                    {
                        ProgramRequestId = i.ProgramRequestId,
                        InmateId = i.InmateId,
                        ProgramCourseId = i.ProgramRequest.ProgramClass.ProgramCourseId,

                    }).GroupBy(g => new
                {
                    g.ProgramRequestId,
                    g.InmateId
                }).Select(s => new ProgramRequestCountVm
                {
                    CurrentEnrolledCount = s.Count()
                }).ToList();
            int[] assignReqIds = _context.ProgramClassAssign.Select(s => s.ProgramRequestId).ToArray();

            lstProgramCourse.ForEach(item =>
            {
                item.ActiveClass = lstProgramClass.Count(p => p.ProgramCourseId == item.ProgramCourseId);
                item.ApproveRequestCount = lstactiveProgramrequest.Where(w =>
                    assignReqIds.All(a => a != w.ProgramRequestId) && item.ProgramCourseId == w.ProgramCourseId &&
                    w.ApproveFlag).ToList().Count;

                item.ActiveRequestCount = lstactiveProgramrequest.Where(w =>
                        assignReqIds.All(a => a != w.ProgramRequestId) && item.ProgramCourseId == w.ProgramCourseId)
                    .ToList().Count;

                item.TotalCapacityCount = lstProgramClass.Where(p => p.ProgramCourseId == item.ProgramCourseId)
                    .Sum(e => e.CourseCapacity);

                item.CurrentEnrolledCount = lstEnrollrequest.Count(w => w.ProgramCourseId == item.ProgramCourseId);
                item.AvailableCapacityCount = item.TotalCapacityCount - item.CurrentEnrolledCount;
            });
            return lstProgramCourse;
        }
    }
}
