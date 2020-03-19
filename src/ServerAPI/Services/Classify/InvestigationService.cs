using System;
using System.Collections.Generic;
using System.Linq;
using GenerateTables.Models;
using ServerAPI.ViewModels;
using ServerAPI.Utilities;
using System.IO;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace ServerAPI.Services
{
    public class InvestigationService : IInvestigationService
    {
        private readonly AAtims _context;
        private readonly int _personnelId;
        private readonly ICommonService _commonService;
        private readonly IPersonService _iPersonService;
        private readonly IBookingService _iBookingService;

        public InvestigationService(AAtims context, IHttpContextAccessor httpContextAccessor,
            ICommonService commonService, IPersonService iPersonService, IBookingService iBookingService)
        {
            _context = context;
            _personnelId =
                Convert.ToInt32(httpContextAccessor.HttpContext.User.FindFirst(AuthDetailConstants.PERSONNELID)?.Value);
            _commonService = commonService;
            _iPersonService = iPersonService;
            _iBookingService = iBookingService;
        }

        public InvestigationDataVm GetInvestigations(InvestigationInputs inputs)
        {
            InvestigationDataVm invest = new InvestigationDataVm
            {
                Investigation = _context.Investigation
                    .Where(inv =>
                    inputs.InvestigationId > 0 ? inv.InvestigationId == inputs.InvestigationId :
                        inputs.Range == InvstRange.active
                            ? (inputs.ToDate >= inv.StartDate &&
                               (!inv.CompleteDate.HasValue || inputs.FromDate <= inv.CompleteDate))
                            : inputs.Range == InvstRange.start
                                ? (inputs.FromDate <= inv.StartDate &&
                                   inv.StartDate <= inputs.ToDate)
                                : inputs.Range == InvstRange.complete
                                    ? (inputs.FromDate <= inv.CompleteDate &&
                                       inv.CompleteDate <= inputs.ToDate)
                                    : inputs.Range == InvstRange.delete
                                        ? (inputs.FromDate <= inv.DeleteDate &&
                                           inv.DeleteDate <= inputs.ToDate)
                                        : (inputs.FromDate <= inv.ReceiveDate &&
                                           inv.ReceiveDate <= inputs.ToDate)).Select(inv =>
                        new InvestigationVm
                        {
                            InvestigationId = inv.InvestigationId,
                            InvestigationName = inv.InvestigationName,
                            InvestigationNumber = inv.InvestigationNumber,
                            InvestigationSummary = inv.InvestigationSummary,
                            CompleteFlag = inv.CompleteFlag,
                            StartDate = inv.StartDate,
                            InvestigationType = inv.InvestigationType,
                            InvestigationStatus = inv.InvestigationStatus,
                            CompleteDisposition = inv.CompleteDisposition,
                            DeleteFlag = inv.DeleteFlag,
                            OfficerId = inv.OfficerId,
                            ManagerFlag = inv.OfficerId == _personnelId,
                            SourceReferral = inv.SourceReferral,
                            SourceDepartment = inv.SourceDepartment,
                            ReceiveDate = inv.ReceiveDate,
                            CompleteDate = inv.CompleteDate
                        }).ToList()
            };


            int[] investigationIds = invest.Investigation.Select(inv => inv.InvestigationId ?? 0).ToArray();

            IQueryable<InvestigationPersonnel> lstInvestigationPersonnel = _context.InvestigationPersonnel.Where(inv =>
                investigationIds.Contains(inv.InvestigationId) && !inv.DeleteFlag);


            List<int> officerIds = invest.Investigation.Select(i => i.OfficerId).ToList();
            List<PersonnelVm> officer = _iPersonService.GetPersonNameList(officerIds.ToList());

            invest.Investigation.ForEach(inv =>
            {
                inv.Officer = officer.Single(per => per.PersonnelId == inv.OfficerId);
                inv.PersonnelList = lstInvestigationPersonnel
                    .Where(invPer => invPer.InvestigationId == inv.InvestigationId).Select(invPer =>
                        new KeyValuePair<int, string>(invPer.PersonnelId,
                            invPer.ContributerFlag ? "Contributer" : invPer.ViewerFlag ? "Viewer" : "Named Only"))
                    .ToList();
            });

            if(inputs.InvestigationId == 0)
            {
                invest.Lookups = _commonService.GetLookups(new[] { "INVESTIGATIONTYPE", "INVESTIGATIONSTATUS", "INVESTIGATIONCOMPLETEDISPO", "INVESTIGATIONDELETEREAS",
                            "INVESTIGATIONFLAGS", "INVESTIGATIONNOTETYPE", "INVESTIGATIONATTACHTYPE", "DISCTYPE", "GRIEVTYPE",
                            "INVESTIGATIONATTACHTYPE", "CLASSLINKTYPE", "DEPARTMENT" });

                invest.Personnels = _commonService.GetPersonnelDetails();
            }

            return invest;
        }

        public InvestigationVm InsertUpdateInvestigation(InvestigationVm iInvestigation)
        {
            Investigation investigation = new Investigation();
            if (iInvestigation.InvestigationId is null)
            {
                investigation.InvestigationNumber =
                    _commonService.GetGlobalNumber((int) AtimsGlobalNumber.InvestigationNumber);
                investigation.InvestigationName = iInvestigation.InvestigationName;
                investigation.OfficerId = iInvestigation.OfficerId;
                investigation.InvestigationStatus = iInvestigation.InvestigationStatus;
                investigation.InvestigationType = iInvestigation.InvestigationType;
                investigation.StartDate = iInvestigation.StartDate;
                investigation.InvestigationSummary = iInvestigation.InvestigationSummary;
                investigation.CreateBy = _personnelId;
                investigation.CreateDate = DateTime.Now;
                investigation.SourceReferral = iInvestigation.SourceReferral;
                investigation.SourceDepartment = iInvestigation.SourceDepartment;
                investigation.ReceiveDate = iInvestigation.ReceiveDate;
                _context.Investigation.Add(investigation);
            }
            else
            {
                investigation =
                    _context.Investigation.Single(inv =>
                        inv.InvestigationId == iInvestigation.InvestigationId);
                investigation.StartDate = iInvestigation.StartDate;
                investigation.InvestigationName = iInvestigation.InvestigationName;
                investigation.OfficerId = iInvestigation.OfficerId;
                investigation.InvestigationStatus = iInvestigation.InvestigationStatus;
                investigation.InvestigationType = iInvestigation.InvestigationType;
                investigation.InvestigationSummary = iInvestigation.InvestigationSummary;
                investigation.SourceReferral = iInvestigation.SourceReferral;
                investigation.SourceDepartment = iInvestigation.SourceDepartment;
                investigation.ReceiveDate = iInvestigation.ReceiveDate;
                investigation.UpdateBy = _personnelId;
                investigation.UpdateDate = DateTime.Now;
            }

            _context.SaveChanges();
            InvestigationHistory investigationHistory = new InvestigationHistory
            {
                InvestigationId = investigation.InvestigationId,
                InvestigationHistoryList = iInvestigation.InvestigationHistory,
                PersonnelId = _personnelId,
                CreateDate = DateTime.Now
            };

            _context.InvestigationHistory.Add(investigationHistory);
            _context.SaveChanges();

            InvestigationDataVm invest = GetInvestigations(new InvestigationInputs { InvestigationId = investigation.InvestigationId });
            return invest.Investigation[0];
        }

        public int InsertUpdateInvestigationFlags(InvestigationFlag iInvestigation)
        {
            InvestigationFlags investigation = new InvestigationFlags();
            if (_context.InvestigationFlags.Count(ff => ff.InvestigationId == iInvestigation.InvestigationId &&
                                                        ff.InvestigationFlagIndex ==
                                                        iInvestigation.InvestigationFlagIndex &&
                                                        (!iInvestigation.InvestigationFlagsId.HasValue ||
                                                         ff.InvestigationFlagsId !=
                                                         iInvestigation.InvestigationFlagsId) &&
                                                        !ff.DeleteFlag) > 0)
            {
                return default;
            }

            if (iInvestigation.InvestigationFlagsId is null)
            {
                investigation.InvestigationFlagIndex = iInvestigation.InvestigationFlagIndex;
                investigation.InvestigationId = iInvestigation.InvestigationId;
                investigation.FlagNote = iInvestigation.FlagNotes;
                investigation.CreateBy = _personnelId;
                investigation.CreateDate = DateTime.Now;
                _context.InvestigationFlags.Add(investigation);
            }
            else
            {
                investigation =
                    _context.InvestigationFlags.Single(inv =>
                        inv.InvestigationFlagsId == iInvestigation.InvestigationFlagsId);
                investigation.InvestigationFlagIndex = iInvestigation.InvestigationFlagIndex;
                investigation.InvestigationId = iInvestigation.InvestigationId;
                investigation.FlagNote = iInvestigation.FlagNotes;
                if (iInvestigation.DeleteFlag)
                {
                    investigation.DeleteFlag = iInvestigation.DeleteFlag;
                    investigation.DeleteBy = _personnelId;
                    investigation.DeleteDate = DateTime.Now;
                }
                else
                {
                    investigation.DeleteFlag = iInvestigation.DeleteFlag;
                    investigation.DeleteBy = default;
                    investigation.DeleteDate = default;
                }

                investigation.UpdateBy = _personnelId;
                investigation.UpdateDate = DateTime.Now;
            }

            return _context.SaveChanges();
        }

        public int InsertUpdateInvestigationPersonnel(InvestigationPersonnelVm iInvestigation)
        {
            if (_context.InvestigationPersonnel.Count(ff => ff.InvestigationId == iInvestigation.InvestigationId &&
                                                        ff.PersonnelId ==
                                                        iInvestigation.PersonnelId &&
                                                        (!iInvestigation.InvestigationPersonnelId.HasValue ||
                                                         ff.InvestigationPersonnelId !=
                                                         iInvestigation.InvestigationPersonnelId) &&
                                                        !ff.DeleteFlag) > 0)
            {
                return default;
            }

            InvestigationPersonnel investigation = new InvestigationPersonnel();
            if (iInvestigation.InvestigationPersonnelId is null)
            {
                investigation.PersonnelId = iInvestigation.PersonnelId;
                investigation.InvestigationId = iInvestigation.InvestigationId;
                investigation.PersonnelNote = iInvestigation.PersonnelNote;
                investigation.ContributerFlag = iInvestigation.ContributerFlag;
                investigation.ViewerFlag = iInvestigation.ViewerFlag;
                investigation.NamedOnlyFlag = iInvestigation.NamedOnlyFlag;
                investigation.CreateBy = _personnelId;
                investigation.CreateDate = DateTime.Now;
                _context.InvestigationPersonnel.Add(investigation);
            }
            else
            {
                investigation =
                    _context.InvestigationPersonnel.Single(inv =>
                        inv.InvestigationPersonnelId == iInvestigation.InvestigationPersonnelId);
                investigation.PersonnelId = iInvestigation.PersonnelId;
                investigation.InvestigationId = iInvestigation.InvestigationId;
                investigation.PersonnelNote = iInvestigation.PersonnelNote;
                investigation.ContributerFlag = iInvestigation.ContributerFlag;
                investigation.ViewerFlag = iInvestigation.ViewerFlag;
                investigation.NamedOnlyFlag = iInvestigation.NamedOnlyFlag;
                investigation.UpdateBy = _personnelId;
                investigation.UpdateDate = DateTime.Now;

                if (iInvestigation.DeleteFlag)
                {
                    investigation.DeleteFlag = iInvestigation.DeleteFlag;
                    investigation.DeleteBy = _personnelId;
                    investigation.DeleteDate = DateTime.Now;
                }
                else
                {
                    investigation.DeleteFlag = iInvestigation.DeleteFlag;
                    investigation.DeleteBy = default;
                    investigation.DeleteDate = default;
                }
            }



            return _context.SaveChanges();
        }

        public int InsertUpdateInvestigationNotes(InvestigationNotesVm iInvestigation)
        {
            InvestigationNotes investigation = new InvestigationNotes();
            if (iInvestigation.InvestigationNotesId is null)
            {
                investigation.InvestigationId = iInvestigation.InvestigationId;
                investigation.InvestigationNote = iInvestigation.InvestigationNotes;
                investigation.InvestigationNoteTypeId = iInvestigation.InvestigationFlagId;
                investigation.InmateId = iInvestigation.InmateId;
                investigation.CreateBy = _personnelId;
                investigation.CreateDate = DateTime.Now;
                _context.InvestigationNotes.Add(investigation);
            }
            else
            {
                investigation =
                    _context.InvestigationNotes.Single(inv =>
                        inv.InvestigationNotesId == iInvestigation.InvestigationNotesId);
                investigation.InvestigationId = iInvestigation.InvestigationId;
                investigation.InvestigationNote = iInvestigation.InvestigationNotes;
                investigation.InvestigationNoteTypeId = iInvestigation.InvestigationFlagId;
                investigation.InmateId = iInvestigation.InmateId;
                investigation.UpdateBy = _personnelId;
                investigation.UpdateDate = DateTime.Now;

                if (iInvestigation.DeleteFlag)
                {
                    investigation.DeleteFlag = iInvestigation.DeleteFlag;
                    investigation.DeleteBy = _personnelId;
                    investigation.DeleteDate = DateTime.Now;
                }
                else
                {
                    investigation.DeleteFlag = iInvestigation.DeleteFlag;
                    investigation.DeleteBy = default;
                    investigation.DeleteDate = default;
                }
            }

            return _context.SaveChanges();
        }

        public int InsertUpdateInvestigationIncident(InvestigationIncident iInvestigation)
        {
            if (_context.InvestigationToIncident.Count(ff => ff.InvestigationId == iInvestigation.InvestigationId &&
                                                             ff.DisciplinaryIncidentId ==
                                                             iInvestigation.DisciplinaryIncidentId &&
                                                             (!iInvestigation.InvestigationToIncidentId.HasValue ||
                                                              ff.InvestigationToIncidentId !=
                                                              iInvestigation.InvestigationToIncidentId) &&
                                                             !ff.DeleteFlag) > 0)
            {
                return default;
            }

            InvestigationToIncident investigation = new InvestigationToIncident();
            if (iInvestigation.InvestigationToIncidentId is null)
            {
                investigation.InvestigationId = iInvestigation.InvestigationId;
                investigation.DisciplinaryIncidentId = iInvestigation.DisciplinaryIncidentId;
                investigation.IncidentReferenceNote = iInvestigation.IncidentReferenceNote;
                investigation.CreateBy = _personnelId;
                investigation.CreateDate = DateTime.Now;
                _context.InvestigationToIncident.Add(investigation);
            }
            else
            {
                investigation =
                    _context.InvestigationToIncident.Single(inv =>
                        inv.InvestigationToIncidentId == iInvestigation.InvestigationToIncidentId);
                investigation.InvestigationId = iInvestigation.InvestigationId;
                investigation.DisciplinaryIncidentId = iInvestigation.DisciplinaryIncidentId;
                investigation.IncidentReferenceNote = iInvestigation.IncidentReferenceNote;
                investigation.UpdateBy = _personnelId;
                investigation.UpdateDate = DateTime.Now;

                if (iInvestigation.DeleteFlag)
                {
                    investigation.DeleteFlag = iInvestigation.DeleteFlag;
                    investigation.DeleteBy = _personnelId;
                    investigation.DeleteDate = DateTime.Now;
                }
                else
                {
                    investigation.DeleteFlag = iInvestigation.DeleteFlag;
                    investigation.DeleteBy = default;
                    investigation.DeleteDate = default;
                }
            }

            return _context.SaveChanges();
        }

        public int InsertUpdateInvestigationLink(InvestigationLinkVm iInvestigation)
        {
            InvestigationLink investigation = new InvestigationLink();
            if (iInvestigation.InvestigationLinkId is null)
            {
                investigation.InvestigationId = iInvestigation.InvestigationId;
                investigation.LinkNote = iInvestigation.LinkNotes;
                investigation.LinkType = iInvestigation.LinkTypeId;
                investigation.CreateBy = _personnelId;
                investigation.CreateDate = DateTime.Now;
                _context.InvestigationLink.Add(investigation);
                _context.SaveChanges();

                if(iInvestigation.InmateIds.Length > 0)
                {
                    foreach (int ch in iInvestigation.InmateIds)
                    {
                        InvestigationLinkXref linkXref = new InvestigationLinkXref
                        {
                            InmateId = ch,
                            InvestigationLinkId = investigation.InvestigationLinkId,
                            CreateBy = _personnelId,
                            CreateDate = DateTime.Now
                        };
                        _context.InvestigationLinkXref.Add(linkXref);
                    }
                }
            }
            else
            {
                investigation =
                    _context.InvestigationLink.Single(inv =>
                        inv.InvestigationLinkId == iInvestigation.InvestigationLinkId);
                investigation.InvestigationId = iInvestigation.InvestigationId;
                investigation.LinkNote = iInvestigation.LinkNotes;
                investigation.LinkType = iInvestigation.LinkTypeId;
                investigation.UpdateBy = _personnelId;
                investigation.UpdateDate = DateTime.Now;

                if (iInvestigation.InmateIds.Length > 0)
                {
                    List<InvestigationLinkXref> lstLinkXrefDeleted = _context.InvestigationLinkXref.Where(xref =>
                            xref.InvestigationLinkId == iInvestigation.InvestigationLinkId &&
                            !xref.DeleteFlag &&
                            !iInvestigation.InmateIds.Contains(xref.InmateId)).ToList();

                    lstLinkXrefDeleted.ForEach(inm =>
                    {
                        inm.DeleteFlag = true;
                        inm.DeleteDate = DateTime.Now;
                        inm.DeleteBy = _personnelId;
                    });

                    foreach (int ch in iInvestigation.InmateIds)
                    {
                        InvestigationLinkXref lstLinkXref = _context.InvestigationLinkXref.SingleOrDefault(xref =>
                            xref.InvestigationLinkId == iInvestigation.InvestigationLinkId &&
                            !xref.DeleteFlag &&
                            xref.InmateId == ch);

                        if(lstLinkXref is null)
                        {
                            InvestigationLinkXref linkXref = new InvestigationLinkXref
                            {
                                InmateId = ch,
                                InvestigationLinkId = investigation.InvestigationLinkId,
                                CreateBy = _personnelId,
                                CreateDate = DateTime.Now
                            };
                            _context.InvestigationLinkXref.Add(linkXref);
                        }
                    }
                }


                if (iInvestigation.DeleteFlag)
                {
                    investigation.DeleteFlag = iInvestigation.DeleteFlag;
                    investigation.DeleteBy = _personnelId;
                    investigation.DeleteDate = DateTime.Now;
                }
                else
                {
                    investigation.DeleteFlag = iInvestigation.DeleteFlag;
                    investigation.DeleteBy = default;
                    investigation.DeleteDate = default;
                }
            }

            return _context.SaveChanges();
        }

        public List<KeyValuePair<int, string>> GetInvestigationIncidents()
        {
            return _context.DisciplinaryIncident
                .Where(dis => dis.DisciplinaryActive == 1).Select(dis =>
                    new KeyValuePair<int, string>(dis.DisciplinaryIncidentId, dis.DisciplinaryNumber.Trim())).ToList();
        }

        public List<KeyValuePair<int, string>> GetInvestigationGrievance()
        {
            return _context.Grievance
                .Where(dis => dis.DeleteFlag != 1).Select(dis =>
                    new KeyValuePair<int, string>(dis.GrievanceId, dis.GrievanceNumber.Trim())).ToList();
        }

        public int InsertUpdateInvestigationGrievance(InvestigationIncident iInvestigation)
        {
            if (_context.InvestigationToGrievance.Count(ff => ff.InvestigationId == iInvestigation.InvestigationId &&
                                                              ff.GrievanceId ==
                                                              iInvestigation.DisciplinaryIncidentId &&
                                                              (!iInvestigation.InvestigationToIncidentId.HasValue ||
                                                               ff.InvestigationToGrievanceId !=
                                                               iInvestigation.InvestigationToIncidentId) &&
                                                              !ff.DeleteFlag) > 0)
            {
                return default;
            }

            InvestigationToGrievance investigation = new InvestigationToGrievance();
            if (iInvestigation.InvestigationToIncidentId is null)
            {
                investigation.InvestigationId = iInvestigation.InvestigationId;
                investigation.GrievanceId = iInvestigation.DisciplinaryIncidentId;
                investigation.GrievanceReferenceNote = iInvestigation.IncidentReferenceNote;
                investigation.CreateBy = _personnelId;
                investigation.CreateDate = DateTime.Now;
                _context.InvestigationToGrievance.Add(investigation);
            }
            else
            {
                investigation =
                    _context.InvestigationToGrievance.Single(inv =>
                        inv.InvestigationToGrievanceId == iInvestigation.InvestigationToIncidentId);
                investigation.InvestigationId = iInvestigation.InvestigationId;
                investigation.GrievanceId = iInvestigation.DisciplinaryIncidentId;
                investigation.GrievanceReferenceNote = iInvestigation.IncidentReferenceNote;
                investigation.UpdateBy = _personnelId;
                investigation.UpdateDate = DateTime.Now;

                if (iInvestigation.DeleteFlag)
                {
                    investigation.DeleteFlag = iInvestigation.DeleteFlag;
                    investigation.DeleteBy = _personnelId;
                    investigation.DeleteDate = DateTime.Now;
                }
                else
                {
                    investigation.DeleteFlag = iInvestigation.DeleteFlag;
                    investigation.DeleteBy = default;
                    investigation.DeleteDate = default;
                }
            }

            return _context.SaveChanges();
        }

        public InvestigationAllDetails GetInvestigationAllDetails(int investigationId)
        {
            InvestigationAllDetails invDetails = new InvestigationAllDetails
            {
                InvestigationFlag = _context.InvestigationFlags.Where(inv =>
                    inv.InvestigationId == investigationId).Select(inv => new InvestigationFlag
                {
                    InvestigationFlagIndex = inv.InvestigationFlagIndex,
                    InvestigationId = inv.InvestigationId,
                    FlagNotes = inv.FlagNote,
                    DeleteFlag = inv.DeleteFlag,
                    InvestigationFlagsId = inv.InvestigationFlagsId
                }).ToList(),
                InvestigationPersonnel = _context.InvestigationPersonnel.Where(inv =>
                    inv.InvestigationId == investigationId).Select(inv => new InvestigationPersonnelVm
                {
                    InvestigationPersonnelId = inv.InvestigationPersonnelId,
                    DeleteFlag = inv.DeleteFlag,
                    PersonnelId = inv.PersonnelId,
                    InvestigationId = inv.InvestigationId,
                    PersonnelNote = inv.PersonnelNote,
                    ContributerFlag = inv.ContributerFlag,
                    ViewerFlag = inv.ViewerFlag,
                    NamedOnlyFlag = inv.NamedOnlyFlag
                }).ToList()
            };


            List<int> officerIds = invDetails.InvestigationPersonnel.Select(i => i.PersonnelId).ToList();
            List<PersonnelVm> officer = _iPersonService.GetPersonNameList(officerIds.ToList());

            invDetails.InvestigationPersonnel.ForEach(inv =>
            {
                inv.PersonnelOfficer = officer.Single(per => per.PersonnelId == inv.PersonnelId);
            });
            
            invDetails.InvestigationNotes = _context.InvestigationNotes.Where(inv =>
                inv.InvestigationId == investigationId).Select(inv => new InvestigationNotesVm
            {
                InvestigationNotesId = inv.InvestigationNotesId,
                DeleteFlag = inv.DeleteFlag,
                InvestigationId = inv.InvestigationId,
                InmateId = inv.InmateId,
                InvestigationNotes = inv.InvestigationNote,
                InvestigationFlagId = inv.InvestigationNoteTypeId
            }).ToList();

            invDetails.InvestigationAttachments = _context.AppletsSaved
                .Where(ii => ii.InvestigationId == investigationId).Select(y => new PrebookAttachment
                {
                    AttachmentId = y.AppletsSavedId,
                    AttachmentDate = y.CreateDate,
                    AttachmentDeleted = y.AppletsDeleteFlag == 1,
                    AttachmentType = y.AppletsSavedType,
                    AttachmentTitle = y.AppletsSavedTitle,
                    AttachmentDescription = y.AppletsSavedDescription,
                    AttachmentKeyword1 = y.AppletsSavedKeyword1,
                    AttachmentKeyword2 = y.AppletsSavedKeyword2,
                    AttachmentKeyword3 = y.AppletsSavedKeyword3,
                    AttachmentKeyword4 = y.AppletsSavedKeyword4,
                    AttachmentKeyword5 = y.AppletsSavedKeyword5,
                    InmatePrebookId = y.InmatePrebookId,
                    AltSentRequestId = y.AltSentRequestId,
                    InmateId = y.InmateId,
                    DisciplinaryIncidentId = y.DisciplinaryIncidentId,
                    MedicalInmateId = y.MedicalInmateId,
                    ArrestId = y.ArrestId,
                    IncarcerationId = y.IncarcerationId,
                    ProgramCaseInmateId = y.ProgramCaseInmateId,
                    GrievanceId = y.GrievanceId,
                    RegistrantRecordId = y.RegistrantRecordId,
                    FacilityId = y.FacilityId,
                    HousingUnitLocation = y.HousingUnitLocation,
                    HousingUnitNumber = y.HousingUnitNumber,
                    AltSentId = y.AltSentId,
                    ExternalInmateId = y.ExternalInmateId,
                    AttachmentFile = Path.GetFileName(y.AppletsSavedPath),
                    CreatedBy = new PersonnelVm
                    {
                        PersonLastName = y.CreatedByNavigation.PersonNavigation.PersonLastName,
                        PersonFirstName = y.CreatedByNavigation.PersonNavigation.PersonFirstName,
                        PersonMiddleName = y.CreatedByNavigation.PersonNavigation.PersonMiddleName
                    },
                    UpdateDate = y.UpdateDate,
                    UpdatedBy = new PersonnelVm
                    {
                        PersonLastName = y.UpdatedByNavigation.PersonNavigation.PersonLastName,
                        PersonFirstName = y.UpdatedByNavigation.PersonNavigation.PersonFirstName,
                        PersonMiddleName = y.UpdatedByNavigation.PersonNavigation.PersonMiddleName
                    }
                }).ToList();

            invDetails.InvestigationIncident = _context.InvestigationToIncident
                .Where(ii => ii.InvestigationId == investigationId).Select(y => new InvestigationIncident
                {
                    InvestigationToIncidentId = y.InvestigationToIncidentId,
                    DisciplinaryIncidentId = y.DisciplinaryIncidentId,
                    InvestigationId = y.InvestigationId,
                    IncidentReferenceNote = y.IncidentReferenceNote,
                    DeleteFlag = y.DeleteFlag,
                    IncidentNumber = y.DisciplinaryIncident.DisciplinaryNumber,
                    IncidentType = y.DisciplinaryIncident.DisciplinaryType
                }).ToList();

            invDetails.InvestigationGrievance = _context.InvestigationToGrievance
                .Where(ii => ii.InvestigationId == investigationId).Select(y => new InvestigationIncident
                {
                    InvestigationToIncidentId = y.InvestigationToGrievanceId,
                    DisciplinaryIncidentId = y.GrievanceId,
                    InvestigationId = y.InvestigationId,
                    IncidentReferenceNote = y.GrievanceReferenceNote,
                    DeleteFlag = y.DeleteFlag,
                    IncidentNumber = y.Grievance.GrievanceNumber,
                    IncidentType = y.Grievance.GrievanceType
                }).ToList();

            invDetails.InvestigationLink = _context.InvestigationLink.Where(iXref => iXref.InvestigationId == investigationId).Select(ii =>
            new InvestigationLinkVm
            {
                InvestigationId = ii.InvestigationId,
                InvestigationLinkId = ii.InvestigationLinkId,
                CreateDate = ii.CreateDate,
                LinkTypeId = ii.LinkType ?? 0,
                InmateIds = ii.InvestigationLinkXref.Where(xr => !xr.DeleteFlag).Select(iXre => iXre.InmateId).ToArray(),
                LinkNotes = ii.LinkNote,
                DeleteFlag = ii.DeleteFlag
            }).ToList();

            invDetails.InvestigationForms = GetInvestigationForms(investigationId);

            List<int> inmateIds = invDetails.InvestigationLink.SelectMany(ii => ii.InmateIds).ToList();

            inmateIds.AddRange(invDetails.InvestigationNotes.Where(ii => ii.InmateId.HasValue).Select(ii => ii.InmateId ?? 0).ToList());

            List<InmateHousing> lstPersonDetails = _iBookingService.GetInmateDetails(inmateIds);

            invDetails.InvestigationLink.ForEach(inv => {
                inv.InmateDetails = lstPersonDetails.Where(per => inv.InmateIds.Contains(per.InmateId)).ToList();
            });

            invDetails.InvestigationNotes.ForEach(inv => {
                inv.InmateDetail = inv.InmateId.HasValue ? lstPersonDetails.Single(per => inv.InmateId == per.InmateId) : default;
            });


            return invDetails;
        }

        public List<IncarcerationForms> GetInvestigationForms(int investigationId)
        {
            List<IncarcerationForms> forms = _context.FormRecord.Where(w => w.InvestigationId == investigationId)
                .Select(s => new IncarcerationForms
                {
                    FormRecordId = s.FormRecordId,
                    DisplayName = s.FormTemplates.DisplayName,
                    FormNotes = s.FormNotes,
                    DeleteFlag = s.DeleteFlag,
                    XmlData = s.XmlData,
                    FormCategoryFolderName = s.FormTemplates.FormCategory.FormCategoryFolderName,
                    HtmlFileName = s.FormTemplates.HtmlFileName,
                    CreateDate = s.CreateDate
                }).OrderByDescending(f => f.CreateDate).ToList();
            return forms;
        }

        public int DeleteInvestigationAttachment(int attachmentId)
        {
            AppletsSaved appletsSaved = _context.AppletsSaved.Single(app => app.AppletsSavedId == attachmentId);
            if(appletsSaved.AppletsDeleteFlag == 1)
            {
                appletsSaved.DeleteDate = null;
                appletsSaved.DeletedBy = null;
            }
            else
            {
                appletsSaved.DeleteDate = DateTime.Now;
                appletsSaved.DeletedBy = _personnelId;
            }
            appletsSaved.AppletsDeleteFlag = appletsSaved.AppletsDeleteFlag == 1 ? 0 : 1;
            return _context.SaveChanges();
        }

        public int DeleteInvestigationForms(int formId)
        {
            FormRecord formRecord = _context.FormRecord.Single(app => app.FormRecordId == formId);
            if (formRecord.DeleteFlag == 1)
            {
                formRecord.DeleteDate = null;
                formRecord.DeleteBy = null;
            }
            else
            {
                formRecord.DeleteDate = DateTime.Now;
                formRecord.DeleteBy = _personnelId;
            }
            formRecord.DeleteFlag = formRecord.DeleteFlag == 1 ? 0 : 1;
            return _context.SaveChanges();
        }

        public int UpdateInvestigationComplete(InvestigationVm investigation)
        {
            Investigation inv = _context.Investigation.Single(ii => ii.InvestigationId == investigation.InvestigationId);
            inv.CompleteDisposition = default;
            inv.CompleteFlag = default;
            inv.CompleteDate = default;
            inv.CompleteBy = default;

            if (!investigation.CompleteFlag)
            {
                inv.CompleteDisposition = investigation.CompleteDisposition;
                inv.CompleteFlag = !investigation.CompleteFlag;
                inv.CompleteDate = DateTime.Now;
                inv.CompleteBy = _personnelId;
                inv.CompleteReason = investigation.CompleteReason;
            }

            InvestigationHistory investigationHistory = new InvestigationHistory
            {
                InvestigationId = inv.InvestigationId,
                InvestigationHistoryList = investigation.InvestigationHistory,
                PersonnelId = _personnelId,
                CreateDate = DateTime.Now
            };

            _context.InvestigationHistory.Add(investigationHistory);
            return _context.SaveChanges();
        }

        public int DeleteInvestigation(InvestigationVm investigation)
        {
            Investigation inv = _context.Investigation.Single(app => app.InvestigationId == investigation.InvestigationId);
            if (inv.DeleteFlag)
            {
                inv.DeleteDate = null;
                inv.DeleteBy = null;
            }
            else
            {
                inv.DeleteDate = DateTime.Now;
                inv.DeleteBy = _personnelId;
            }
            inv.DeleteFlag = !inv.DeleteFlag;
            InvestigationHistory investigationHistory = new InvestigationHistory
            {
                InvestigationId = inv.InvestigationId,
                InvestigationHistoryList = investigation.InvestigationHistory,
                PersonnelId = _personnelId,
                CreateDate = DateTime.Now
            };

            _context.InvestigationHistory.Add(investigationHistory);
            return _context.SaveChanges();
        }

        public List<HistoryVm> GetInvestigationHistoryDetails(int investigationId)
        {
            List<HistoryVm> lstInvestigation = _context.InvestigationHistory.Where(w => w.InvestigationId == investigationId)
                .OrderByDescending(o => o.CreateDate)
                .Select(s => new HistoryVm
                {
                    HistoryId = s.InvestigationHistoryId,
                    CreateDate = s.CreateDate,
                    PersonId = s.Personnel.PersonId,
                    OfficerBadgeNumber = s.Personnel.OfficerBadgeNum,
                    HistoryList = s.InvestigationHistoryList
                }).ToList();

            if (lstInvestigation.Count <= 0) return lstInvestigation;

            int[] personIds = lstInvestigation.Select(s => s.PersonId).ToArray();

            List<Person> lstPersonDetails = _context.Person.Where(w => personIds.Contains(w.PersonId)).ToList();

            lstInvestigation.ForEach(item =>
            {
                item.PersonLastName =
                    lstPersonDetails.SingleOrDefault(s => s.PersonId == item.PersonId)?.PersonLastName;

                if (item.HistoryList == null) return;
                Dictionary<string, string> personHistoryList =
                    JsonConvert.DeserializeObject<Dictionary<string, string>>(item.HistoryList);
                item.Header =
                    personHistoryList.Select(ph => new PersonHeader { Header = ph.Key, Detail = ph.Value }).ToList();
            });

            return lstInvestigation;
        }
    }
}
