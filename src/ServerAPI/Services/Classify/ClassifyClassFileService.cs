﻿using GenerateTables.Models;
using Microsoft.AspNetCore.Http;
using ServerAPI.Utilities;
using ServerAPI.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ServerAPI.Services
{
    public class ClassifyClassFileService : IClassifyClassFileService
    {

        #region Properties

        private readonly AAtims _context;
        private readonly IPersonService _personService;
        private readonly IClassifyViewerService _classifyViewerService;
        private readonly int _personnelId;
        private readonly IAppletsSavedService _appletsSavedService;

        private ClassFileOutputs _classFileOutput;
        private ClassFileInmateDetail _inmate;
        private List<Lookup> _lookUp;

        #endregion

        public ClassifyClassFileService(AAtims context, IHttpContextAccessor httpContextAccessor,
            IClassifyViewerService classifyViewerService, IPersonService iPersonService, IAppletsSavedService appletsSavedService)
        {
            _context = context;
            _classifyViewerService = classifyViewerService;
            _personnelId = Convert.ToInt32(httpContextAccessor.HttpContext.User.FindFirst("personnelId")?.Value);
            _personService = iPersonService;
            _appletsSavedService = appletsSavedService;
        }

        #region Functions

        /// <summary>
        /// 
        /// </summary>
        /// <param name="classFileInputs"></param>
        /// <returns></returns>
        public async Task<ClassFileOutputs> GetClassFile(ClassFileInputs classFileInputs) //Page-load and Refresh
        {
            ClassFileOutputs classFileOutputs = new ClassFileOutputs();

            if (!classFileInputs.Refresh)
            {
                classFileOutputs.GetClassifySettings = await _classifyViewerService.GetClassifySettings();
                classFileInputs.LogParameters = classFileOutputs.GetClassifySettings;
            }

            if (classFileInputs.InmateId == 0)
            {
                return classFileOutputs;
            }

            //Arrange
            GetInmate(classFileInputs.InmateId);
            GetLookUpList();

            //Act
            if (!classFileInputs.Refresh)
            {
                classFileOutputs.InmateReviewDetails =
                    _inmate.InmateClassificationId > 0 ? GetDates() : new ClassFileReview();
            }
            GetClassFileGridValues(classFileInputs);
            GetOfficer();
            GetNarrative(classFileInputs);

            //Assign
            classFileOutputs.GridValues = _classFileOutput.GridValues;
            classFileOutputs.GridCounts = _classFileOutput.GridCounts;
            classFileOutputs.SiteOption = SiteOptions();

            return classFileOutputs;
        }

        #region Support Data's

        //To get inmate details depend on FacilityId
        private void GetInmate(int inmateId) =>
            _inmate = _context.Inmate.Where(i => i.InmateId == inmateId)
                .Select(a => new ClassFileInmateDetail
                {
                    InmateId = a.InmateId,
                    PersonId = a.PersonId,
                    FacilityId = a.FacilityId,
                    HousingUnitId = a.HousingUnitId ?? 0,

                    InmateNumber = a.InmateNumber,
                    InmateActive = a.InmateActive == 1,

                    LastReviewBy = a.LastClassReviewBy ?? 0,
                    LastReviewDate = a.LastClassReviewDate.GetValueOrDefault(),

                    InmateClassificationId = a.InmateClassificationId ?? 0,
                    InmateClassificationReason = a.InmateClassificationId.HasValue
                        ? a.InmateClassification.InmateClassificationReason
                        : string.Empty,

                    HousingUnit = new HousingDetail
                    {
                        HousingUnitId = a.HousingUnitId ?? 0,
                        FacilityId = a.HousingUnitId.HasValue ? a.HousingUnit.FacilityId : 0,
                        HousingUnitListId = a.HousingUnitId.HasValue ? a.HousingUnit.HousingUnitListId : 0,
                        HousingUnitNumber = a.HousingUnitId.HasValue ? a.HousingUnit.HousingUnitNumber : string.Empty,
                        HousingUnitLocation =
                            a.HousingUnitId.HasValue ? a.HousingUnit.HousingUnitLocation : string.Empty,
                        HousingUnitBedNumber =
                            a.HousingUnitId.HasValue ? a.HousingUnit.HousingUnitBedNumber : string.Empty,
                        HousingUnitBedLocation =
                            a.HousingUnitId.HasValue ? a.HousingUnit.HousingUnitBedLocation : string.Empty

                    },
                    Person = new PersonDetail
                    {
                        PersonId = a.PersonId,
                        PersonFirstName = a.Person.PersonFirstName,
                        PersonMiddleName = a.Person.PersonMiddleName,
                        PersonLastName = a.Person.PersonLastName
                    }
                }).SingleOrDefault();

        private void GetLookUpList() =>
            _lookUp = _context.Lookup.Where(x =>
                x.LookupType == LookupConstants.CLASSLINKTYPE ||
                x.LookupType == LookupConstants.ARRTYPE ||
                x.LookupType == LookupConstants.DISCINTYPE ||
                x.LookupType == LookupConstants.DISCTYPE ||
                x.LookupType == LookupConstants.PERSONCAUTION ||
                x.LookupType == LookupConstants.TRANSCAUTION ||
                x.LookupType == LookupConstants.DIET ||
                x.LookupType == LookupConstants.CLASREAS).Select(s => new Lookup
                {
                    LookupType = s.LookupType ?? string.Empty,
                    LookupDescription = s.LookupDescription ?? string.Empty,
                    LookupCategory = s.LookupCategory ?? string.Empty,
                    LookupIndex = s.LookupIndex
                }).ToList();

        private ClassFileReview GetDates() => new ClassFileReview
        {
            LastReviewDate = _inmate.LastReviewDate.Date,
            LookupCategory = _lookUp.FirstOrDefault(lookupValues =>
                lookupValues.LookupType == LookupConstants.CLASREAS &&
                lookupValues.LookupDescription == _inmate.InmateClassificationReason)?.LookupCategory,
            Name = _context.Personnel.Where(personnelFilter => personnelFilter.PersonnelId == _inmate.LastReviewBy)
                    .Select(personnelValues => new PersonDetailVM
                    {
                        LastName = personnelValues.PersonNavigation.PersonLastName,
                        FirstName = personnelValues.PersonNavigation.PersonFirstName,
                        MiddleName = personnelValues.PersonNavigation.PersonMiddleName,
                        Number = personnelValues.OfficerBadgeNum
                    }).SingleOrDefault()
        };

        private void GetOfficer()
        {
            // To get List of Officer Ids
            List<int> officerIds = _classFileOutput.GridValues.Where(o => o.OfficerId > 0).Select(i => i.OfficerId)
                .Distinct().ToList();
            //To get Officer Details List
            List<PersonnelVm> officer = _personService.GetPersonNameList(officerIds.ToList());

            _classFileOutput.GridValues.Where(o => o.OfficerId > 0).ToList().ForEach(item =>
            {
                //To get Officer Details for all classify
                item.OfficerDetails = officer
                    .Where(arr => arr.PersonnelId == item.OfficerId).Select(p => new PersonnelVm
                    {
                        PersonLastName = p.PersonLastName,
                        PersonFirstName = p.PersonFirstName,
                        OfficerBadgeNumber = p.OfficerBadgeNumber
                    }).SingleOrDefault();
            });
        }

        private void GetNarrative(ClassFileInputs classFileInputs)
        {
            #region narrative's

            if (classFileInputs.LogParameters.Flag)
            {
                _classFileOutput.GridValues.Where(c => c.ClassType == ClassTypeConstants.FLAG).ToList().ForEach(
                    record => record.ClassNarrative = _lookUp.SingleOrDefault(l =>
                        l.LookupType == (record.PersonFlagIndex > 0 ? LookupConstants.PERSONCAUTION
                            : record.InmateFlagIndex > 0 ? LookupConstants.TRANSCAUTION
                                : record.DietFlagIndex > 0 ? LookupConstants.DIET : string.Empty)
                        && l.LookupIndex == (record.PersonFlagIndex > 0
                            ? record.PersonFlagIndex : record.InmateFlagIndex > 0
                                ? record.InmateFlagIndex : record.DietFlagIndex > 0 ? record.DietFlagIndex : 0)
                    )?.LookupDescription);
            }

            if (classFileInputs.LogParameters.Incident)
            {
                _classFileOutput.GridValues.Where(c => c.ClassType == ClassTypeConstants.INCIDENT).ToList().ForEach(
                    record =>
                    {
                        record.IncidentNarrative.IncidentlookupDescription = _lookUp.SingleOrDefault(l =>
                                l.LookupType == LookupConstants.DISCINTYPE
                                && l.LookupIndex == record.IncidentNarrative.DisciplinaryInmateType)
                            ?.LookupDescription;
                        record.IncidentNarrative.InmatelookupDescription = _lookUp.SingleOrDefault(l =>
                                l.LookupType == LookupConstants.DISCTYPE
                                && l.LookupIndex == record.IncidentNarrative.DisciplinaryType)
                            ?.LookupDescription;
                    });
            }

            if (!classFileInputs.LogParameters.KeepSep) return;
            if (!_classFileOutput.GridValues.Where(c => c.ClassType == ClassTypeConstants.KEEPSEPINMATE)
                .Select(s => s.KeepSeparateNarrative.KeepSepInmate2Id).Any())
            {
                return;
            }

            List<ClassFileInmateDetail> keepSepInmate = _context.Inmate.Where(w =>
                 _classFileOutput.GridValues.Where(c => c.ClassType == ClassTypeConstants.KEEPSEPINMATE)
                     .Select(s => s.KeepSeparateNarrative.KeepSepInmate2Id).Contains(w.InmateId))
                .Select(a => new ClassFileInmateDetail
                {
                    InmateId = a.InmateId,
                    InmateActive = a.InmateActive == 1,
                    InmateNumber = a.InmateNumber,
                    HousingUnitId = a.HousingUnitId ?? 0,
                    HousingUnit = new HousingDetail
                    {
                        HousingUnitId = a.HousingUnitId ?? 0,
                        FacilityId = a.HousingUnitId.HasValue ? a.HousingUnit.FacilityId : 0,
                        HousingUnitListId = a.HousingUnitId.HasValue ? a.HousingUnit.HousingUnitListId : 0,
                        HousingUnitNumber = a.HousingUnitId.HasValue ? a.HousingUnit.HousingUnitNumber : string.Empty,
                        HousingUnitLocation = a.HousingUnitId.HasValue ? a.HousingUnit.HousingUnitLocation : string.Empty,
                        HousingUnitBedNumber =
                        a.HousingUnitId.HasValue ? a.HousingUnit.HousingUnitBedNumber : string.Empty,
                        HousingUnitBedLocation =
                        a.HousingUnitId.HasValue ? a.HousingUnit.HousingUnitBedLocation : string.Empty
                    },
                    Person = new PersonDetail
                    {
                        PersonId = a.PersonId,
                        PersonFirstName = a.Person.PersonFirstName,
                        PersonMiddleName = a.Person.PersonMiddleName,
                        PersonLastName = a.Person.PersonLastName
                    }
                }).ToList();

            if (keepSepInmate.Count > 0)
            {
                _classFileOutput.GridValues.Where(c => c.ClassType == ClassTypeConstants.KEEPSEPINMATE).ToList()
                    .ForEach(record =>
                    {
                        record.KeepSeparateNarrative.InmateActive = keepSepInmate.Single(w =>
                            w.InmateId == record.KeepSeparateNarrative.KeepSepInmate2Id).InmateActive;
                        record.HousingDetails = keepSepInmate.Single(w =>
                            w.InmateId == record.KeepSeparateNarrative.KeepSepInmate2Id).HousingUnit;

                        record.PersonDetails = new PersonInfo
                        {
                            InmateNumber = keepSepInmate
                                .Single(w => w.InmateId == record.KeepSeparateNarrative.KeepSepInmate2Id).InmateNumber,
                            PersonFirstName = keepSepInmate
                                .Single(w => w.InmateId == record.KeepSeparateNarrative.KeepSepInmate2Id).Person
                                .PersonFirstName,
                            PersonLastName = keepSepInmate
                                .Single(w => w.InmateId == record.KeepSeparateNarrative.KeepSepInmate2Id).Person
                                .PersonLastName,
                            PersonMiddleName = keepSepInmate
                                .Single(w => w.InmateId == record.KeepSeparateNarrative.KeepSepInmate2Id).Person
                                .PersonMiddleName
                        };
                    });
            }

            #endregion
        }

        public bool PendingCheck(int inmateId) => _context.InmateClassification.Any(ic =>
            ic.InmateId == inmateId &&
            (ic.InmateClassificationType == ClassTypeConstants.INITIAL ||
             ic.InmateClassificationType == ClassTypeConstants.RECLASSIFICATION) &&
            ic.InmateClassificationReason.Contains("PENDING"));

        private List<SiteOptionProp> SiteOptions() => _context.SiteOptions.Where(sp =>
            (sp.SiteOptionsVariable == SiteOptionsConstants.ALLOWCLASSIFYEDIT ||
            sp.SiteOptionsVariable == SiteOptionsConstants.ALLOWNOTEEDIT) && sp.SiteOptionsStatus == "1")
                .Select(s => new SiteOptionProp
                {
                    SiteOptionVariable = s.SiteOptionsVariable,
                    SiteOptionValue = s.SiteOptionsValue
                }).ToList();

        #endregion

        //All grids process start
        private void GetClassFileGridValues(ClassFileInputs classFileInputs)
        {
            _classFileOutput = new ClassFileOutputs
            {
                GridCounts = new ClassifyCount(),
                GridValues = new List<ClassLog>()
            };
            //Classify
            InitialClassifyGrid(classFileInputs);
            ReviewClassNoteGrid(classFileInputs);
            FormGrid(classFileInputs);
            AttachGrid(classFileInputs);
            LinkGrid(classFileInputs);
            //General
            IntakeReleaseGrid(classFileInputs);
            HousingGrid(classFileInputs);
            NoteGrid(classFileInputs);
            IncidentGrid(classFileInputs);
            //Alert
            Message(classFileInputs);
            Flag(classFileInputs);
            Associate(classFileInputs);
            KeepSep(classFileInputs);
            Privileges(classFileInputs);
        }

        #region Classify

        private void InitialClassifyGrid(ClassFileInputs classFileInputs)
        {
            List<InmateClassification> inmateClassification =
                _context.InmateClassification.Where(ic => ic.InmateId == classFileInputs.InmateId).ToList();

            List<FormRecord> formRecord = _context.FormRecord.Where(f => f.InmateId == classFileInputs.InmateId).ToList(); 
            
            if (!inmateClassification.Any())
            {
                return;
            }

            //Count
            _classFileOutput.GridCounts.Initial =
                inmateClassification.Count(c => c.InmateClassificationType == ClassTypeConstants.INITIAL);
            _classFileOutput.GridCounts.ReClassify = inmateClassification.Count(c =>
                c.InmateClassificationType == ClassTypeConstants.RECLASSIFICATION);
            //Record
            _classFileOutput.GridValues.AddRange(inmateClassification.Where(ic =>
                    classFileInputs.LogParameters.Initial &&
                    ic.InmateClassificationType == ClassTypeConstants.INITIAL ||
                    classFileInputs.LogParameters.ReClassify &&
                    ic.InmateClassificationType == ClassTypeConstants.RECLASSIFICATION)
                .Select(ic => new ClassLog
                {
                    Id = ic.InmateClassificationId,
                    InmateId = ic.InmateId,
                    ClassType = ic.InmateClassificationType,
                    ClassDate = ic.InmateDateAssigned,
                    ClassNarrative = ic.InmateOverrideNarrative,
                    Reason = ic.InmateClassificationReason,
                    OfficerId = ic.ClassificationOfficerId,
                    PersonId = _inmate.PersonId,
                    ValidationFlag = formRecord.First(f => f.InmateClassificationId == ic.InmateClassificationId).ValidationFlag,
                })
                .ToList());
        }

        private void ReviewClassNoteGrid(ClassFileInputs classFileInputs)
        {
            IQueryable<InmateClassificationNarrative> inmateClassifyNarrative =
                _context.InmateClassificationNarrative.Where(icn => icn.InmateId == classFileInputs.InmateId);

            if (!inmateClassifyNarrative.Any())
            {
                return;
            }

            //filter
            inmateClassifyNarrative = inmateClassifyNarrative.Where(c =>
                c.ReviewFlag == 1 || !c.ReviewFlag.HasValue || c.ReviewFlag == 0);
            List<InmateClassificationNarrative> inmateClassifyNarrativeLst =
                inmateClassifyNarrative
                    .Where(d => classFileInputs.DeleteFlag || !d.DeleteFlag.HasValue || d.DeleteFlag == 0).ToList();
            //count
            _classFileOutput.GridCounts.Review =
                inmateClassifyNarrativeLst.Count(c => c.ReviewFlag == 1);
            _classFileOutput.GridCounts.ClassNote =
                inmateClassifyNarrativeLst.Count(c => !c.ReviewFlag.HasValue || c.ReviewFlag == 0);
            //record
            _classFileOutput.GridValues.AddRange(inmateClassifyNarrativeLst
                .Where(icn =>
                    classFileInputs.LogParameters.Review && icn.ReviewFlag == 1 ||
                    classFileInputs.LogParameters.ClassNote && (!icn.ReviewFlag.HasValue || icn.ReviewFlag == 0))
                .Select(icn => new ClassLog
                {
                    Id = icn.InmateClassificationNarrativeId,
                    InmateId = icn.InmateId,
                    ClassType = icn.ReviewFlag == 1 ? ClassTypeConstants.REVIEW : ClassTypeConstants.CLASSNOTE,
                    ClassDate = icn.CreateDate,
                    ClassNarrative = icn.Narrative,
                    OfficerId = icn.CreatedBy,
                    ReviewFlag = icn.ReviewFlag ?? 0,
                    DeleteFlag = icn.DeleteFlag == 1,
                    ReviewNote = icn.NoteType
                }).ToList());
        }

        private void FormGrid(ClassFileInputs classFileInputs)
        {
            IQueryable<FormRecord> formRecord =
                _context.FormRecord.Where(fr => fr.ClassificationFormInmateId == classFileInputs.InmateId);

            if (!formRecord.Any())
            {
                return;
            }

            //filter
            List<FormRecord> formRecordLst =
                formRecord.Where(c => classFileInputs.DeleteFlag || c.DeleteFlag == 0).ToList();
            //count
            _classFileOutput.GridCounts.Form = formRecordLst.Count;
            //record
            _classFileOutput.GridValues.AddRange(formRecordLst.Where(fr => classFileInputs.LogParameters.Form)
                .Select(f => new ClassLog
                {
                    Id = f.FormRecordId,
                    InmateId = f.ClassificationFormInmateId ?? 0,
                    ClassType = ClassTypeConstants.FORM,
                    ClassDate = f.CreateDate,
                    ClassNarrative = f.FormNotes,
                    OfficerId = f.CreateBy,
                    DeleteFlag = f.DeleteFlag == 1,
                    PersonId = _inmate.PersonId,
                }).ToList());
        }

        private void AttachGrid(ClassFileInputs classFileInputs)
        {
            IQueryable<AppletsSaved> appletsSaved = _context.AppletsSaved.Where(a =>
                a.InmateId == classFileInputs.InmateId && !a.ArrestId.HasValue);

            if (!appletsSaved.Any())
            {
                return;
            }
            //filter
            List<AppletsSaved> appletsSavedLst =
                appletsSaved.Where(c => classFileInputs.DeleteFlag || c.AppletsDeleteFlag == 0).ToList();
            //count
            _classFileOutput.GridCounts.Attach = appletsSavedLst.Count;
            //record
            _classFileOutput.GridValues.AddRange(appletsSavedLst.Where(fr => classFileInputs.LogParameters.Attach)
                .Select(a => new ClassLog
                {
                    Id = a.AppletsSavedId,
                    InmateId = a.InmateId ?? 0,
                    ClassType = ClassTypeConstants.ATTACH,
                    ClassDate = a.CreateDate,
                    Type = a.AppletsSavedType,
                    Note = a.AppletsSavedTitle,
                    PathName = a.AppletsSavedIsExternal ? _appletsSavedService.GetExternalPath() + a.AppletsSavedPath : a.AppletsSavedPath,
                    OfficerId = a.CreatedBy,
                    DeleteFlag = a.AppletsDeleteFlag == 1
                }).ToList());
        }

        private void LinkGrid(ClassFileInputs classFileInputs)
        {
            IQueryable<InmateClassificationLinkXref> inmateClassifyLinkRef =
                _context.InmateClassificationLinkXref.Where(icl => icl.InmateId == classFileInputs.InmateId
                     && !icl.InmateClassificationLink.DeleteFlag.HasValue);

            if (!inmateClassifyLinkRef.Any())
            {
                return;
            }

            //filter
            inmateClassifyLinkRef = inmateClassifyLinkRef.Where(
                    c => classFileInputs.DeleteFlag || !c.DeleteFlag.HasValue || c.DeleteFlag == 0);

            if (!classFileInputs.LogParameters.Link)
            {
                //count
                _classFileOutput.GridCounts.Link = inmateClassifyLinkRef.Count();
            }
            else
            {
                //record
                _classFileOutput.GridValues.AddRange(inmateClassifyLinkRef
                    .Where(fr => classFileInputs.LogParameters.Link)
                    .Select(l => new ClassLog
                    {
                        Id = l.InmateClassificationLink.InmateClassificationLinkId,
                        InmateId = l.InmateId,
                        ClassType = ClassTypeConstants.LINK,
                        ClassDate = l.InmateClassificationLink.CreateDate,
                        Type = l.InmateClassificationLink.LinkType.HasValue
                            ? _lookUp.Where(lu =>
                                    lu.LookupIndex == (l.InmateClassificationLink.LinkType ?? 0) &&
                                    lu.LookupType == LookupConstants.CLASSLINKTYPE).Select(lu => lu.LookupDescription)
                                .SingleOrDefault()
                            : string.Empty,
                        Note = l.InmateClassificationLink.LinkNote,
                        OfficerId = l.InmateClassificationLink.CreateBy,
                        DeleteFlag = l.DeleteFlag == 1,
                        LinkType=l.InmateClassificationLink.LinkType
                    }).ToList());

                _classFileOutput.GridCounts.Link =
                    _classFileOutput.GridValues.Count(c => c.ClassType == ClassTypeConstants.LINK);
            }

        }

        #endregion

        #region General

        private void IntakeReleaseGrid(ClassFileInputs classFileInputs)
        {
            IQueryable<Incarceration> incarceration =
                _context.Incarceration.Where(ic => ic.InmateId == classFileInputs.InmateId);

            if (!incarceration.Any())
            {
                return;
            }

            //filter
            List<Incarceration> incarcerationLst =
                incarceration.Where(c => c.DateIn.HasValue || c.ReleaseOut.HasValue).ToList();
            //count
            _classFileOutput.GridCounts.Intake = incarcerationLst.Count(c => c.DateIn.HasValue);
            _classFileOutput.GridCounts.Release = incarcerationLst.Count(c => c.ReleaseOut.HasValue);
            //record
            _classFileOutput.GridValues.AddRange(incarcerationLst
                .Where(d => classFileInputs.LogParameters.Intake && d.DateIn.HasValue)
                .Select(c => new ClassLog
                {
                    Id = c.IncarcerationId,
                    IncarcerationId = c.IncarcerationId,
                    InmateId = c.InmateId ?? 0,
                    ClassType = ClassTypeConstants.INTAKE,
                    ClassDate = c.DateIn,
                    ReleaseOut = c.ReleaseOut,
                    OfficerId = c.InOfficerId
                }).ToList());

            _classFileOutput.GridValues.AddRange(incarcerationLst
                .Where(d => classFileInputs.LogParameters.Release && d.ReleaseOut.HasValue)
                .Select(c => new ClassLog
                {
                    Id = c.IncarcerationId,
                    IncarcerationId = c.IncarcerationId,
                    InmateId = c.InmateId ?? 0,
                    ClassType = ClassTypeConstants.RELEASE,
                    ClassDate = c.ReleaseOut,
                    OfficerId = c.InOfficerId
                }).ToList());

            if (classFileInputs.LogParameters.Intake || classFileInputs.LogParameters.Release)
            {
                GetArrestDetails();
            }
        }

        private void GetArrestDetails()
        {
            #region ArrestDetail's

            // To get List of Officer Ids
            List<int> incarcerationIds = _classFileOutput.GridValues.Select(i => i.IncarcerationId).ToList();

            ////To get IncarcerationArrestXref Details
            List<ArrestIntake> lstIncarcerationArrestXref = _context.IncarcerationArrestXref
                .Where(i => incarcerationIds.Contains(i.IncarcerationId.Value))
                .Select(i => new ArrestIntake
                {
                    IncarcerationId = i.IncarcerationId.Value,
                    BookingNumber = i.Arrest.ArrestBookingNo,
                    ArrestType = i.Arrest.ArrestType
                }).ToList();

            _classFileOutput.GridValues
                .Where(c => c.ClassType == ClassTypeConstants.INTAKE || c.ClassType == ClassTypeConstants.RELEASE)
                .ToList().ForEach(item =>
                {
                    item.ArrestDetails = lstIncarcerationArrestXref
                        .Where(arr => arr.IncarcerationId == item.IncarcerationId).Select(p => new ArrestIntake
                        {
                            Description = _lookUp.SingleOrDefault(l =>
                                l.LookupIndex == Convert.ToInt32(p.ArrestType) &&
                                l.LookupType == LookupConstants.ARRTYPE)?.LookupDescription,
                            BookingNumber = p.BookingNumber,
                            Count = lstIncarcerationArrestXref.Count(a => a.IncarcerationId == item.IncarcerationId)
                        }).FirstOrDefault();
                });

            #endregion
        }

        private void HousingGrid(ClassFileInputs classFileInputs)
        {
            IQueryable<HousingUnitMoveHistory> housingUnitMoveHistory =
                _context.HousingUnitMoveHistory.Where(hmv =>
                    hmv.HousingUnitToId.HasValue && hmv.InmateId == classFileInputs.InmateId);

            if (!housingUnitMoveHistory.Any())
            {
                return;
            }

            if (!classFileInputs.LogParameters.Housing)
            {
                //count
                _classFileOutput.GridCounts.Housing = housingUnitMoveHistory.Count();
            }
            else
            {
                //record
                _classFileOutput.GridValues.AddRange(housingUnitMoveHistory
                    .Select(hmh => new ClassLog
                    {
                        Id = hmh.HousingUnitMoveHistoryId,
                        InmateId = hmh.InmateId,
                        ClassType = ClassTypeConstants.HOUSING,
                        ClassDate = hmh.MoveDate,
                        HousingDetails = new HousingDetail
                        {
                            HousingUnitNumber = hmh.HousingUnitTo.HousingUnitNumber,
                            HousingUnitLocation = hmh.HousingUnitTo.HousingUnitLocation,
                            FacilityAbbr = hmh.HousingUnitTo.FacilityId > 0
                                ? hmh.HousingUnitTo.Facility.FacilityAbbr
                                : string.Empty
                        },
                        Reason = hmh.MoveReason,
                        OfficerId = hmh.MoveOfficerId,
                    }).ToList());
                _classFileOutput.GridCounts.Housing =
                    _classFileOutput.GridValues.Count(c => c.ClassType == ClassTypeConstants.HOUSING);
            }
        }

        private void NoteGrid(ClassFileInputs classFileInputs)
        {
            IQueryable<FloorNoteXref> floorNoteXref =
                _context.FloorNoteXref.Where(fnr => fnr.InmateId == classFileInputs.InmateId);

            if (!floorNoteXref.Any())
            {
                return;
            }

            //filter
            floorNoteXref = floorNoteXref.Where(c =>
                classFileInputs.DeleteFlag || !c.FloorNote.FloorDeleteFlag.HasValue || c.FloorNote.FloorDeleteFlag == 0);

            if (!classFileInputs.LogParameters.Note)
            {
                //count
                _classFileOutput.GridCounts.Note = floorNoteXref.Count();
            }
            else
            {
                //record
                _classFileOutput.GridValues.AddRange(floorNoteXref
                    .Select(fnr => new ClassLog
                    {
                        Id = fnr.FloorNoteId,
                        InmateId = fnr.InmateId,
                        ClassType = ClassTypeConstants.NOTE,
                        ClassDate = fnr.FloorNote.FloorNoteDate,
                        Note = fnr.FloorNote.FloorNoteNarrative,
                        Type = fnr.FloorNote.FloorNoteType,
                        OfficerId = fnr.FloorNote.FloorNoteOfficerId,
                        DeleteFlag = fnr.FloorNote.FloorDeleteFlag == 1
                    }).ToList());
                _classFileOutput.GridCounts.Note =
                    _classFileOutput.GridValues.Count(c => c.ClassType == ClassTypeConstants.NOTE);
            }
        }

        private void IncidentGrid(ClassFileInputs classFileInputs)
        {
            IQueryable<DisciplinaryInmate> inmateDisciplinary =
                _context.DisciplinaryInmate.Where(id => id.InmateId == classFileInputs.InmateId);

            if (!inmateDisciplinary.Any())
            {
                return;
            }

            //filter
            inmateDisciplinary =
                inmateDisciplinary.Where(c => classFileInputs.DeleteFlag || !c.DeleteFlag.HasValue || c.DeleteFlag == 0);

            if (!classFileInputs.LogParameters.Incident)
            {
                //count
                _classFileOutput.GridCounts.Incident = inmateDisciplinary.Count();
            }
            else
            {
                //record
                _classFileOutput.GridValues.AddRange(inmateDisciplinary
                    .Select(id => new ClassLog
                    {
                        Id = id.DisciplinaryIncident.DisciplinaryIncidentId,
                        InmateId = id.InmateId.Value,
                        ClassType = ClassTypeConstants.INCIDENT,
                        ClassDate = id.DisciplinaryIncident.DisciplinaryIncidentDate,
                        IncidentNarrative = new IncidentNarrative
                        {
                            DisciplinaryNumber = id.DisciplinaryIncident.DisciplinaryNumber,
                            DisciplinarySynopsis = id.DisciplinaryIncident.DisciplinarySynopsis,
                            DisciplinaryInmateType = id.DisciplinaryInmateType ?? 0,
                            DisciplinaryType = id.DisciplinaryIncident.DisciplinaryType ?? 0
                        },

                        OfficerId = id.DisciplinaryIncident.DisciplinaryOfficerId,
                        DeleteFlag = id.DeleteFlag == 1
                    }).ToList());
                _classFileOutput.GridCounts.Incident =
                    _classFileOutput.GridValues.Count(c => c.ClassType == ClassTypeConstants.INCIDENT);
            }
        }

        #endregion

        #region Alert

        private void Message(ClassFileInputs classFileInputs)
        {
            IQueryable<PersonAlert> personAlert = _context.PersonAlert.Where(pa => pa.PersonId == _inmate.PersonId);

            if (!personAlert.Any())
            {
                return;
            }

            //filter
            List<PersonAlert> personAlertList =
                personAlert.Where(c => classFileInputs.DeleteFlag || c.ActiveAlertFlag == 1).ToList();
            //count
            _classFileOutput.GridCounts.Message = personAlertList.Count;
            //records
            _classFileOutput.GridValues.AddRange(personAlertList.Where(w => classFileInputs.LogParameters.Message)
                .Select(alert => new ClassLog
                {
                    Id = alert.PersonAlertId,
                    InmateId = _inmate.InmateId,
                    PersonId = alert.PersonId,
                    ClassDate = alert.CreateDate,
                    OfficerId = alert.CreatedBy,
                    ClassType = ClassTypeConstants.MESSAGE,
                    DeleteFlag = alert.ActiveAlertFlag.GetValueOrDefault() == 0,
                    ClassNarrative = alert.Alert
                }).ToList());
        }

        private void Flag(ClassFileInputs classFileInputs)
        {
            IQueryable<PersonFlag> personFlag = _context.PersonFlag.Where(pa =>
                pa.PersonId == _inmate.PersonId && (!pa.MedicalFlagIndex.HasValue || pa.MedicalFlagIndex == 0) &&
                (!pa.ProgramCaseFlagIndex.HasValue || pa.ProgramCaseFlagIndex == 0));

            if (!personFlag.Any())
            {
                return;
            }

            //filter
            List<PersonFlag> personFlagLst = personFlag.Where(c => c.DeleteFlag == 0).ToList();
            //count
            _classFileOutput.GridCounts.Flag = personFlagLst.Count;
            //records
            _classFileOutput.GridValues.AddRange(personFlagLst.Where(pf => classFileInputs.LogParameters.Flag)
                .Select(flg => new ClassLog
                {
                    Id = flg.PersonFlagId,
                    ClassType = ClassTypeConstants.FLAG,
                    InmateId = _inmate.InmateId,
                    PersonId = flg.PersonId,
                    ClassDate = flg.CreateDate,
                    OfficerId = flg.CreateBy,
                    PersonFlagIndex = flg.PersonFlagIndex ?? 0,
                    InmateFlagIndex = flg.InmateFlagIndex ?? 0,
                    DietFlagIndex = flg.DietFlagIndex ?? 0,
                    DeleteFlag = flg.DeleteFlag == 1
                }).ToList());
        }

        private void Associate(ClassFileInputs classFileInputs)
        {
            List<Lookup> lookupslist = _context.Lookup.Where(w => w.LookupType == LookupConstants.CLASSGROUP).ToList();
            List<Lookup> lookupSubsetlist = _context.Lookup.Where(w => w.LookupType == LookupConstants.CLASSGROUPSUB).ToList();
            List<int> lookupDescLst = lookupslist.Where(x => x.LookupFlag9 == 1 || x.LookupFlag10 == 1)
              .Select(x => Convert.ToInt32(x.LookupIndex)).ToList();
            IQueryable<PersonClassification> personClassification =
                _context.PersonClassification.Where(pc => pc.PersonId == _inmate.PersonId &&
                lookupDescLst.Contains(pc.PersonClassificationTypeId ?? 0));

            if (!personClassification.Any())
            {
                return;
            }

            //filter
            List<PersonClassification> personClassificationLst = personClassification
                .Where(c => classFileInputs.DeleteFlag || c.InactiveFlag == 0).ToList();
            //count
            _classFileOutput.GridCounts.Assoc = personClassificationLst.Count;
            //records
            _classFileOutput.GridValues.AddRange(personClassificationLst
                .Where(pf => classFileInputs.LogParameters.Assoc)
                .Select(person => new ClassLog
                {
                    Id = person.PersonClassificationId,
                    InmateId = _inmate.InmateId,
                    PersonId = person.PersonId,
                    ClassDate = person.PersonClassificationDateFrom,
                    OfficerId = person.CreatedByPersonnelId,
                    ClassType = ClassTypeConstants.ASSOC,
                    DeleteFlag = person.InactiveFlag == 1,
                    AssocNarrative = new AssocNarrative
                    {
                        PersonClassificationType = lookupslist
                            .SingleOrDefault(f => f.LookupIndex == person.PersonClassificationTypeId)
                            ?.LookupDescription,
                        PersonClassificationSubset = lookupSubsetlist
                            .SingleOrDefault(f => f.LookupIndex == person.PersonClassificationSubsetId)
                            ?.LookupDescription,
                        PersonClassificationNotes = person.PersonClassificationNotes,
                        PersonClassificationStatus = person.PersonClassificationStatus,
                        PersonClassificationDateThru = person.PersonClassificationDateFrom,
                        PersonClassificationTypeId = person.PersonClassificationTypeId,
                        PersonClassificationSubsetId = person.PersonClassificationSubsetId,
                    }
                }).ToList());
        }

        #region Keep Seperate

        private void KeepSep(ClassFileInputs classFileInputs)
        {
            IQueryable<KeepSeparate> keepSep = _context.KeepSeparate.Where(ks1 =>
                ks1.KeepSeparateInmate1Id == classFileInputs.InmateId ||
                ks1.KeepSeparateInmate2Id == classFileInputs.InmateId);
            IQueryable<KeepSeparate> keepSep1 =
                keepSep.Where(ks1 => ks1.KeepSeparateInmate1Id == classFileInputs.InmateId);
            IQueryable<KeepSeparate> keepSep2 =
                keepSep.Where(ks2 => ks2.KeepSeparateInmate2Id == classFileInputs.InmateId);
            IQueryable<KeepSepAssocInmate> keepSepAssoc =
                _context.KeepSepAssocInmate.Where(ksa => ksa.KeepSepInmate2Id == classFileInputs.InmateId);
            IQueryable<KeepSepSubsetInmate> keepSepSubset =
                _context.KeepSepSubsetInmate.Where(kss => kss.KeepSepInmate2Id == classFileInputs.InmateId);

            if (!keepSep.Any() && !keepSepAssoc.Any() && !keepSepSubset.Any())
            {
                return;
            }

            //count
            if (keepSep1.Any())
            {
                List<KeepSeparate> keepSep1Lst = keepSep1.Where(c => c.InactiveFlag == 0).ToList();
                _classFileOutput.GridCounts.KeepSep = keepSep1Lst.Count;
                //record
                if (classFileInputs.LogParameters.KeepSep)
                {
                    KeepSep1(keepSep1Lst);
                }
            }
            if (keepSep2.Any())
            {
                List<KeepSeparate> keepSep2Lst = keepSep2.Where(c => c.InactiveFlag == 0).ToList();
                _classFileOutput.GridCounts.KeepSep += keepSep2Lst.Count;

                if (classFileInputs.LogParameters.KeepSep)
                {
                    KeepSep2(keepSep2Lst);
                }
            }
            if (keepSepAssoc.Any())
            {
                List<KeepSepAssocInmate> keepSepAssocLst = keepSepAssoc.Where(c => c.DeleteFlag == 0).ToList();
                _classFileOutput.GridCounts.KeepSep += keepSepAssocLst.Count;
                if (classFileInputs.LogParameters.KeepSep)
                {
                    KeepAssociate(keepSepAssocLst);
                }
            }

            if (!keepSepSubset.Any()) return;
            List<KeepSepSubsetInmate> keepSepSubsetLst = keepSepSubset.Where(c => c.DeleteFlag == 0).ToList();
            _classFileOutput.GridCounts.KeepSep += keepSepSubsetLst.Count;
            if (classFileInputs.LogParameters.KeepSep)
            {
                KeepSubSet(keepSepSubsetLst);
            }
        }

        private void KeepSubSet(List<KeepSepSubsetInmate> keepSepSubset)
        {
            List<Lookup> lookupslist = _context.Lookup.Where(w => w.LookupType == LookupConstants.CLASSGROUP).ToList();
            List<Lookup> lookupSubsetlist = _context.Lookup.Where(w => w.LookupType == LookupConstants.CLASSGROUPSUB).ToList();
            _classFileOutput.GridValues.AddRange(keepSepSubset.Select(kss => new ClassLog
            {
                Id = kss.KeepSepSubsetInmateId,
                InmateId = kss.KeepSepInmate2Id,
                ClassType = ClassTypeConstants.KEEPSEPSUBSET,
                ClassDate = kss.CreateDate,
                KeepSeparateNarrative = new KeepSeparateNarrative
                {
                    KeepSepAssoc = lookupslist.Single(k => k.LookupIndex == kss.KeepSepAssoc1Id).LookupDescription,
                    KeepSepAssocSubset = lookupSubsetlist.Single(k => k.LookupIndex == kss.KeepSepAssoc1SubsetId).LookupDescription,
                    KeepSepAssoc1Id = kss.KeepSepAssoc1Id,
                    KeepSepAssoc1SubsetId = kss.KeepSepAssoc1SubsetId,
                    KeepSepType = kss.KeepSeparateType,
                    KeepSepReason = kss.KeepSepReason,
                    KeepSepNote = kss.KeepSeparateNote
                },
                OfficerId = kss.KeepSepOfficerId,
                DeleteFlag = kss.DeleteFlag == 1

            }).ToList());
        }

        private void KeepAssociate(List<KeepSepAssocInmate> keepSepAssoc)
        {
            List<Lookup> lookupslist = _context.Lookup.Where(w => w.LookupType == LookupConstants.CLASSGROUP).ToList();
            _classFileOutput.GridValues.AddRange(keepSepAssoc.Select(ksa => new ClassLog
            {
                Id = ksa.KeepSepAssocInmateId,
                InmateId = ksa.KeepSepInmate2Id,
                ClassType = ClassTypeConstants.KEEPSEPASSOC,
                ClassDate = ksa.CreateDate,

                KeepSeparateNarrative = new KeepSeparateNarrative
                {
                    KeepSepAssoc = lookupslist.Single(k => k.LookupIndex == ksa.KeepSepAssoc1Id).LookupDescription,
                    KeepSepAssoc1Id = ksa.KeepSepAssoc1Id,
                    KeepSepType = ksa.KeepSeparateType,
                    KeepSepReason = ksa.KeepSepReason,
                    KeepSepNote = ksa.KeepSeparateNote
                },
                OfficerId = ksa.KeepSepOfficerId,
                DeleteFlag = ksa.DeleteFlag == 1
            }).ToList());
        }
        
        private void KeepSep2(List<KeepSeparate> keepSep2) =>
            _classFileOutput.GridValues.AddRange(keepSep2.Select(ks2 => new ClassLog
            {
                Id = ks2.KeepSeparateId,
                InmateId = ks2.KeepSeparateInmate2Id,
                ClassType = ClassTypeConstants.KEEPSEPINMATE,
                ClassDate = ks2.KeepSeparateDate,
                KeepSeparateNarrative = new KeepSeparateNarrative
                {
                    KeepSepInmate2Id = ks2.KeepSeparateInmate1Id,
                    KeepSepType = ks2.KeepSeparateType,
                    KeepSepReason = ks2.KeepSeparateReason,
                    KeepSepNote = ks2.KeepSeparateNote
                },
                OfficerId = ks2.KeepSeparateOfficerId,
                DeleteFlag = ks2.InactiveFlag == 1

            }).ToList());

        private void KeepSep1(List<KeepSeparate> keepSep1) =>
            _classFileOutput.GridValues.AddRange(keepSep1.Select(ks1 => new ClassLog
            {
                Id = ks1.KeepSeparateId,
                InmateId = ks1.KeepSeparateInmate1Id,
                ClassType = ClassTypeConstants.KEEPSEPINMATE,
                ClassDate = ks1.KeepSeparateDate,
                KeepSeparateNarrative = new KeepSeparateNarrative
                {
                    KeepSepInmate2Id = ks1.KeepSeparateInmate2Id,
                    KeepSepType = ks1.KeepSeparateType,
                    KeepSepReason = ks1.KeepSeparateReason,
                    KeepSepNote = ks1.KeepSeparateNote
                },
                OfficerId = ks1.KeepSeparateOfficerId,
                DeleteFlag = ks1.InactiveFlag == 1

            }).ToList());

        #endregion

        private void Privileges(ClassFileInputs classFileInputs)
        {
            IQueryable<InmatePrivilegeXref> inmatePrivilegeXref =
                _context.InmatePrivilegeXref.Where(pref => pref.InmateId == classFileInputs.InmateId);

            if (!inmatePrivilegeXref.Any())
            {
                return;
            }

            //filter
            inmatePrivilegeXref =
                inmatePrivilegeXref.Where(c => classFileInputs.DeleteFlag || !c.PrivilegeRemoveDatetime.HasValue);

            if (!classFileInputs.LogParameters.Privileges)
            {
                //count
                _classFileOutput.GridCounts.Privileges = inmatePrivilegeXref.Count();
            }
            else
            {
                //records
                _classFileOutput.GridValues.AddRange(inmatePrivilegeXref
                    .Select(xref => new ClassLog
                    {
                        Id = xref.InmatePrivilegeXrefId,
                        InmateId = xref.InmateId,
                        ClassDate = xref.CreateDate,
                        ClassType = ClassTypeConstants.PRIVILEGES,
                        OfficerId = xref.PrivilegeOfficerId,
                        PrivilegesNarrative = new PrivilegesNarrative
                        {
                            PrivilegeRemoveOfficerId = xref.PrivilegeRemoveOfficerId,
                            PrivilegeDate = xref.PrivilegeDate,
                            PrivilegeNote = xref.PrivilegeNote,
                            PrivilegeExpires = xref.PrivilegeExpires,
                            PrivilegeId = xref.PrivilegeId,
                            PrivilegeDescription = xref.Privilege.PrivilegeDescription
                        },
                        DeleteFlag = xref.PrivilegeRemoveDatetime.HasValue
                    }).ToList());

                _classFileOutput.GridCounts.Privileges =
                    _classFileOutput.GridValues.Count(c => c.ClassType == ClassTypeConstants.PRIVILEGES);
            }
        }

        #endregion

        public async Task<int> InmateDeleteUndo(DeleteParams deleteParams)
        {
            //To-Do
            //Needs to add permission 
            switch (deleteParams.Type)
            {
                case ClassTypeConstants.ATTACH:
                    AppletsSaved appletSaved = _context.AppletsSaved.SingleOrDefault(a =>
                        a.InmateId == deleteParams.InmateId && a.AppletsSavedId == deleteParams.Id);
                    if (appletSaved == null)
                    {
                        return 0;
                    }
                    appletSaved.AppletsDeleteFlag = deleteParams.DeleteFlag;
                    appletSaved.DeletedBy = deleteParams.DeleteFlag == 1 ? _personnelId : (int?)null;
                    appletSaved.DeleteDate = deleteParams.DeleteFlag == 1 ? DateTime.Now : (DateTime?)null;
                    _context.Update(appletSaved);
                    break;
                case ClassTypeConstants.REVIEW:
                    InmateClassificationNarrative inmateClassificationNarrative =
                        _context.InmateClassificationNarrative.SingleOrDefault(a =>
                            a.InmateId == deleteParams.InmateId &&
                            a.InmateClassificationNarrativeId == deleteParams.Id);
                    if (inmateClassificationNarrative == null)
                    {
                        return 0;
                    }
                    inmateClassificationNarrative.DeleteFlag = deleteParams.DeleteFlag;
                    inmateClassificationNarrative.DeletedBy =
                        deleteParams.DeleteFlag == 1 ? _personnelId : (int?)null;
                    inmateClassificationNarrative.DeleteDate =
                        deleteParams.DeleteFlag == 1 ? DateTime.Now : (DateTime?)null;
                    _context.Update(inmateClassificationNarrative);

                    break;
                case ClassTypeConstants.NOTE:
                    FloorNotes floorNotes = _context.FloorNotes.SingleOrDefault(a => a.FloorNoteId == deleteParams.Id);
                    if (floorNotes == null)
                    {
                        return 0;
                    }
                    floorNotes.FloorDeleteFlag = deleteParams.DeleteFlag;
                    floorNotes.DeletedBy = deleteParams.DeleteFlag == 1 ? _personnelId : (int?)null;
                    floorNotes.DeletedDate = deleteParams.DeleteFlag == 1 ? DateTime.Now : (DateTime?)null;
                    _context.Update(floorNotes);
                    break;
                case ClassTypeConstants.LINK:
                    InmateClassificationLinkXref inmateClassificationLink =
                        _context.InmateClassificationLinkXref.SingleOrDefault(a =>
                            a.InmateId == deleteParams.InmateId && a.InmateClassificationLinkId == deleteParams.Id);
                    if (inmateClassificationLink == null)
                    {
                        return 0;
                    }
                    inmateClassificationLink.DeleteFlag = deleteParams.DeleteFlag;
                    inmateClassificationLink.DeleteBy = deleteParams.DeleteFlag == 1 ? _personnelId : (int?)null;
                    inmateClassificationLink.DeleteDate =
                        deleteParams.DeleteFlag == 1 ? DateTime.Now : (DateTime?)null;
                    _context.Update(inmateClassificationLink);
                    break;
                case ClassTypeConstants.FORM:
                    FormRecord formRecord = _context.FormRecord.SingleOrDefault(a => a.FormRecordId == deleteParams.Id);
                    if (formRecord == null)
                    {
                        return 0;
                    }
                    formRecord.DeleteFlag = deleteParams.DeleteFlag;
                    formRecord.DeleteBy = deleteParams.DeleteFlag == 1 ? _personnelId : (int?)null;
                    formRecord.DeleteDate = deleteParams.DeleteFlag == 1 ? DateTime.Now : (DateTime?)null;
                    _context.Update(formRecord);
                    break;
            }
            return await _context.SaveChangesAsync();
        }

        #endregion

    }
}