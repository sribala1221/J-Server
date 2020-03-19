﻿using GenerateTables.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using ServerAPI.Interfaces;
using ServerAPI.Policies;
using ServerAPI.Utilities;
using ServerAPI.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
namespace ServerAPI.Services
{
    public class RegisterService : IRegisterService
    {
        private readonly AAtims _context;
        private readonly int _personnelId;
        private readonly ICommonService _commonService;
        private readonly IPersonCharService _personCharService;
        private readonly IPhotosService _photosService;
        private readonly IWizardService _wizardService;
        private readonly IPersonIdentityService _personIdentityService;
        private readonly IKeepSepAlertService _keepSepAlertService;
        private readonly IUserPermissionPolicy _iUserPermissionPolicy;
        private readonly List<TrackingConflictVm> _trackingConflictDetails = new List<TrackingConflictVm>();
        public RegisterService(AAtims context, IHttpContextAccessor httpContextAccessor, ICommonService commonService,
            IPersonCharService personCharService, IPhotosService photosService, IWizardService wizardService,
            IPersonIdentityService personIdentityService, IKeepSepAlertService keepSepAlertService, IUserPermissionPolicy iUserPermissionPolicy)
        {
            _context = context;
            _personnelId = Convert.ToInt32(httpContextAccessor.HttpContext.User.FindFirst("personnelId")?.Value);
            _commonService = commonService;
            _personCharService = personCharService;
            _photosService = photosService;
            _wizardService = wizardService;
            _personIdentityService = personIdentityService;
            _keepSepAlertService = keepSepAlertService;
            _iUserPermissionPolicy = iUserPermissionPolicy;
        }

        #region Step 1 - Identify Visitor

        public AoWizardFacilityVm GetVisitRegistrationWizard()
        {
            //Here 13 refers to VISITOR REGISTRATION wizard
            AoWizardVm wizardDetails = _wizardService.GetWizardSteps(13)[0];
            return wizardDetails.WizardFacilities.FirstOrDefault();
        }

        public List<VisitDetails> GetVisitDetails(VisitParam objVisitParam)
        {
            List<VisitDetails> lstRegistrationDetails = _context.VisitToVisitor.AsNoTracking().Where(w =>
                     (!objVisitParam.PersonId.HasValue || w.PersonId == objVisitParam.PersonId)
                     &&(!objVisitParam.FacilityId.HasValue || w.Visit.RegFacilityId == objVisitParam.FacilityId)
                     && !w.Visit.DeleteFlag && !w.Visit.VisitSecondaryFlag.HasValue
                     && (w.Visit.EndDate == null || w.Visit.EndDate.HasValue) && !w.PrimaryVisitorId.HasValue
                     && (((objVisitParam.ProfVisitHistory || !objVisitParam.PendingVisit) || w.Visit.VisitDenyFlag == 0 && w.Visit.StartDate.Date == DateTime.Now.Date && !w.Visit.CompleteVisitFlag)
                     && ((objVisitParam.ProfVisitHistory || objVisitParam.PendingVisit) || !w.Visit.CompleteRegistration && w.Visit.VisitDenyFlag == 0)))
                .Select(s => new VisitDetails
                {
                    VisitToVisitorId = s.VisitToVisitorId,
                    ScheduleId = s.ScheduleId,
                    InmateId = s.Visit.InmateId??0,
                    PrimaryVisitor = new PersonInfoVm
                    {
                        PersonId = s.PersonId,
                        PersonLastName = s.Visitor.PersonLastName,
                        PersonFirstName = s.Visitor.PersonFirstName,
                        PersonDob = s.Visitor.PersonDob
                    },
                    InmateInfo = new PersonInfoVm
                    {
                        InmateId = s.Visit.InmateId,
                        InmateNumber = s.Visit.Inmate.InmateNumber,
                        PersonLastName = s.Visit.Inmate.Person.PersonLastName,
                        PersonFirstName = s.Visit.Inmate.Person.PersonFirstName,
                        Facility = s.Visit.Inmate.Facility.FacilityAbbr
                    },
                    Location = s.Visit.Location.PrivilegeDescription,
                    CompleteVisitReason = s.Visit.CompleteVisitReason,
                    VisitDenyReason = s.Visit.VisitDenyReason,
                    VisitDenyNote = s.Visit.VisitDenyNote,
                    ScheduleDateTime = s.Visit.StartDate,
                    CountAsVisit = s.Visit.CountAsVisit,
                    AdultVisitorsCount = s.Visit.VisitAdditionAdultCount,
                    ChildVisitorsCount = s.Visit.VisitAdditionChildCount,
                    CompleteVisitFlag = s.Visit.CompleteVisitFlag,
                    VisitDenyFlag = s.Visit.VisitDenyFlag == 1,
                    CreateDate = s.Visit.CreateDate,
                    CompleteRegistration = s.Visit.CompleteRegistration,
                    VisitorType = s.Visit.VisitorType ?? 0,
                    VisitorBefore = s.Visitor.VisitorBefore,
                    FrontDeskFlag = s.Visit.FrontDeskFlag,
                    RelationshipId = s.Visitor.VisitorToInmate.FirstOrDefault(w => w.InmateId == s.Visit.InmateId).VisitorRelationship ?? 0,
                    ProfVisitorType=s.Visitor.VisitorType,
                    Notes = s.Visit.Notes
                }).OrderBy(o => o.CreateDate).ToList();

            List<LookupVm> lstLookup = _commonService.GetLookups(new[]
                {LookupConstants.RELATIONS, LookupConstants.VISTYPE});

            List<int> lstScheduleId = lstRegistrationDetails.Select(s => s.ScheduleId).ToList();

            List<AoWizardProgressVm> lstVisitRegistrationWizardProgress = GetVisitRegistrationWizardProgress(lstScheduleId);

            lstRegistrationDetails.ForEach(item =>
            {
                if (item.VisitorType > 0)
                {
                        item.ProfessionalRelation = lstLookup.SingleOrDefault(l =>
                            l.LookupIndex == item.ProfVisitorType
                            && l.LookupType == LookupConstants.VISTYPE)?.LookupDescription;
                } else {
                    //Personal Relationship
                    item.Relationship = lstLookup.SingleOrDefault(l =>
                            l.LookupIndex == item.RelationshipId && l.LookupType == LookupConstants.RELATIONS)
                        ?.LookupDescription;
                }

                //Wizard details
                item.RegistrationProgress =
                    lstVisitRegistrationWizardProgress.FirstOrDefault(w => w.ScheduleId == item.ScheduleId);

            });
            return lstRegistrationDetails;
        }

        public List<AoWizardProgressVm> GetVisitRegistrationWizardProgress(List<int> lstScheduleId) =>
            _context.AoWizardProgressSchedule
                .Where(a => lstScheduleId.Contains(a.ScheduleId))
                .Select(wp => new AoWizardProgressVm
                {
                    WizardProgressId = wp.AoWizardProgressId,
                    WizardId = wp.AoWizardId,
                    ScheduleId = wp.ScheduleId,
                    WizardStepProgress = wp.AoWizardStepProgress
                        .Select(wsp => new AoWizardStepProgressVm
                        {
                            AoWizardFacilityStepId = wsp.AoWizardFacilityStepId,
                            WizardStepProgressId = wsp.AoWizardStepProgressId,
                            WizardProgressId = wsp.AoWizardProgressId,
                            ComponentId = wsp.AoComponentId,
                            Component = new AoComponentVm
                            {
                                AppAoFunctionalityId = wsp.AoComponent.AppAofunctionalityId,
                                CanChangeVisibility = wsp.AoComponent.CanChangeVisibility,
                                ComponentId = wsp.AoComponent.AoComponentId,
                                ComponentName = wsp.AoComponent.ComponentName,
                                CustomFieldAllowed = wsp.AoComponent.CustomFieldAllowed,
                                CustomFieldKeyName = wsp.AoComponent.CustomFieldKeyName,
                                CustomFieldTableName = wsp.AoComponent.CustomFieldTableName,
                                DisplayName = wsp.AoComponent.DisplayName,
                                HasConfigurableFields = wsp.AoComponent.HasConfigurableFields,
                                IsLastScreen = wsp.AoComponent.IsLastScreen
                            },
                            StepComplete = wsp.StepComplete,
                            StepCompleteBy = new PersonnelVm
                            {
                                PersonnelId = wsp.StepCompleteBy.PersonnelId,
                                OfficerBadgeNumber = wsp.StepCompleteBy.OfficerBadgeNum,
                                PersonLastName = wsp.StepCompleteBy.PersonNavigation.PersonLastName,
                                PersonFirstName = wsp.StepCompleteBy.PersonNavigation.PersonFirstName,
                                PersonMiddleName = wsp.StepCompleteBy.PersonNavigation.PersonMiddleName
                            },
                            StepCompleteDate = wsp.StepCompleteDate,
                            StepCompleteNote = wsp.StepCompleteNote
                        }).ToList()
                }).ToList();

        public IdentifyVisitorVm GetIdentifiedVisitorDetails(VisitParam objVisitParam)
        {
            IdentifyVisitorVm identifyVisitorVm = new IdentifyVisitorVm
            {
                IsVisitDeny = GetVisitDenyFlag(),
                PersonBasicDetails = GetPersonBasicDetails(objVisitParam.PersonId ?? 0),

                //Visit Child Detail
                VisitChildDetail = GetChildVisitors(objVisitParam.ScheduleId ?? 0, objVisitParam.PersonId ?? 0),

                VisitAdultDetail = GetAdultVisitorDetails(objVisitParam.ScheduleId ?? 0),

                VisitorIdDetails = GetVisitorIdDetails(objVisitParam.PersonId ?? 0),
            };

            if (objVisitParam.ScheduleId.HasValue)
            {
                identifyVisitorVm.VisitorFlagDetails = _context.Visitor.Where(w => w.PersonId == objVisitParam.PersonId)
                      .Select(s => new VisitorFlagDetails
                      {
                          VisitorNotAllowedFlag = s.VisitorNotAllowedFlag,
                          VisitorNotAllowedReason = s.VisitorNotAllowedReason,
                          VisitorNotAllowedExpire = s.VisitorNotAllowedExpire,
                          VisitorNotAllowedNote = s.VisitorNotAllowedNote,
                          PersonOfInterestFlag = s.PersonOfInterestFlag,
                          PersonOfInterestReason = s.PersonOfInterestReason,
                          PersonOfInterestExpire = s.PersonOfInterestExpire,
                          PersonOfInterestNote = s.PersonOfInterestNote
                      }).SingleOrDefault();
            }

            return identifyVisitorVm;
        }

      

        private PersonIdentity GetPersonBasicDetails(int personId)
        {
            //Person Identity
            PersonIdentity personIdentity = _context.Person.Where(w => w.PersonId == personId)
                        .Select(s => new PersonIdentity
                        {
                            PersonId = s.PersonId,
                            PersonLastName = s.PersonLastName,
                            PersonFirstName = s.PersonFirstName,
                            PersonDob = s.PersonDob,
                            PersonCellPhone = s.PersonCellPhone,
                            PersonPhone = s.PersonPhone,
                            PersonEmail = s.PersonEmail,
                            PersonDlNumber = s.PersonDlNumber,
                            PersonOtherIdNumber = s.PersonOtherIdNumber,
                            PersonOtherIdType = s.PersonOtherIdType,
                            PersonOtherIdState = s.PersonOtherIdState
                        }).Single();

            //Person Residential Address
            personIdentity.ResAddress = _context.Address.Where(w => w.PersonId == personId
                    && w.AddressType == PersonConstants.RESIDENCE)
                .OrderByDescending(o => o.UpdateDate)
                .Select(pa => new PersonAddressVm
                {
                    Number = pa.AddressNumber,
                    Direction = pa.AddressDirection,
                    DirectionSuffix = pa.AddressDirectionSuffix,
                    Street = pa.AddressStreet,
                    Suffix = pa.AddressSuffix,
                    UnitType = pa.AddressUnitType,
                    UnitNo = pa.AddressUnitNumber,
                    Line2 = pa.AddressLine2,
                    City = pa.AddressCity,
                    State = pa.AddressState,
                    Zip = pa.AddressZip
                }).FirstOrDefault();

            //Person Characteristics
            personIdentity.PersonChar = _personCharService.GetCharacteristics(personId);

            //Person last uploaded Front View Photo
            personIdentity.PhotoFilePath = _photosService.GetPhotoByPersonId(personId);

            return personIdentity;
        }
        private VisitDetails GetVisitorIdDetails(int personId)
        {
            //Personal and Professional Id Details
            VisitDetails visitorIdDetails = _context.VisitToVisitor.Where(w =>
                    w.PersonId == personId)
                .OrderByDescending(o => o.ScheduleId)
                .Select(s => new VisitDetails
                {
                    VisitorIdType = s.VisitorIdType,
                    VisitorIdNumber = s.VisitorIdNumber,
                    VisitorIdState = s.VisitorIdState,
                    VisitorNotes = s.Visitor.VisitorNotes,
                    FrontDeskFlag = s.Visit.FrontDeskFlag,
                    VisitorBefore = s.Visitor.VisitorBefore,
                    VisitorType = s.Visit.VisitorType ?? 0,
                    ProfessionalType=s.Visitor.VisitorType
                }).FirstOrDefault();

            if (visitorIdDetails == null) return null;

            //Here 1 refers to Professional Visitor Type
            if (visitorIdDetails.VisitorType > 0)
            {
                VisitorProfessional visitorProfessional = _context.VisitorProfessional
                .SingleOrDefault(w => w.PersonId == personId);
                visitorIdDetails.ProfVisitorIdExpDate = visitorProfessional?.ProfVisitorIdExpdate;
            }

            return visitorIdDetails;
        }

        private VisitDetails GetAdultVisitorIdDetails(int personId, int scheduleId)
        {
            //Personal and Professional Id Details
            VisitDetails visitorIdDetails = _context.VisitToVisitor.Where(w =>
                    w.PersonId == personId && w.ScheduleId == scheduleId)
                .Select(s => new VisitDetails
                {
                    VisitorIdType = s.VisitorIdType,
                    VisitorIdNumber = s.VisitorIdNumber,
                    VisitorIdState = s.VisitorIdState,
                    VisitorNotes = s.Visitor.VisitorNotes,
                    FrontDeskFlag = s.Visit.FrontDeskFlag,
                    VisitorBefore = s.Visitor.VisitorBefore,
                    VisitorType = s.Visit.VisitorType ?? 0,
                    ScheduleId = s.ScheduleId,
                    VisitToVisitorId = s.VisitToVisitorId
                }).SingleOrDefault();

            if (visitorIdDetails == null) return null;

            //Here 1 refers to Professional Visitor Type
            //if (visitorIdDetails.VisitorType > 0)
            //{
            VisitorProfessional visitorProfessional = _context.VisitorProfessional
            .SingleOrDefault(w => w.PersonId == personId);

            visitorIdDetails.ProfessionalType = visitorProfessional?.VisitorType;
            visitorIdDetails.ProfVisitorIdExpDate = visitorProfessional?.ProfVisitorIdExpdate;
            //}
            return visitorIdDetails;
        }


        private VisitDenyDetails GetVisitDenyDetails(int scheduleId) => _context.Visit
                .Where(w => w.ScheduleId == scheduleId)
                .Select(s => new VisitDenyDetails
                {
                    VisitDenyFlag = s.VisitDenyFlag == 1,
                    VisitDenyReason = s.VisitDenyReason,
                    VisitDenyNote = s.VisitDenyNote
                }).SingleOrDefault();

        public List<ProfessionalSearchDetails> GetProfessionalDetails(ProfessionalSearchDetails objParam)
        {
            //Professional Credential Search
            List<VisitorProfessional> lstVisitorProfessional = _context.VisitorProfessional.Where(w =>
                       (string.IsNullOrEmpty(objParam.PersonLastName) || w.PersonLastName.Contains(objParam.PersonLastName)) &&
                       (string.IsNullOrEmpty(objParam.PersonFirstName) || w.PersonFirstName.Contains(objParam.PersonFirstName)) &&
                       (!objParam.ProfessionalType.HasValue || w.VisitorType == objParam.ProfessionalType))
                       .Select(s => new VisitorProfessional
                       {
                           PersonId = s.PersonId,
                           PersonLastName=s.PersonLastName,
                           PersonFirstName=s.PersonFirstName,
                           VisitorType=s.VisitorType
                       }).ToList();

            List<LookupVm> lstLookup = _commonService.GetLookups(new[]{LookupConstants.VISTYPE}); 

            List<ProfessionalSearchDetails> lstProfessionalDetails = _context.VisitToVisitor
                .Where(w => lstVisitorProfessional.Select(s=>s.PersonId).Contains(w.PersonId) &&
                    (string.IsNullOrEmpty(objParam.VisitorIdType) || w.VisitorIdType == objParam.VisitorIdType) &&
                    (string.IsNullOrEmpty(objParam.VisitorIdNumber) || w.VisitorIdNumber == objParam.VisitorIdNumber) &&
                    (string.IsNullOrEmpty(objParam.VisitorIdState) || w.VisitorIdState == objParam.VisitorIdState))
                .Select(s => new ProfessionalSearchDetails
                {
                    PersonId = s.PersonId,
                    VisitorIdType = s.VisitorIdType,
                    VisitorIdNumber = s.VisitorIdNumber,
                    VisitorIdState = s.VisitorIdState
                }).ToList();

            lstProfessionalDetails.ForEach(item =>
            {
                VisitorProfessional profDetail = lstVisitorProfessional.SingleOrDefault(w =>w.PersonId == item.PersonId);
                if (profDetail == null) return;
                item.PersonLastName = profDetail.PersonLastName;
                item.PersonFirstName = profDetail.PersonFirstName;
                item.Description = lstLookup.SingleOrDefault(l => l.LookupIndex == profDetail.VisitorType
                                                        && l.LookupType == LookupConstants.VISTYPE)?.LookupDescription; 
            });

            return lstProfessionalDetails;
        }
        public async Task<int> InsertUpdateVisitorPersonDetails(PersonIdentity visitor)
        {
            VisitorPersonal dbVisitor = _context.VisitorPersonal.SingleOrDefault(p => p.PersonId == visitor.PersonId);
            bool isFromInsert = false;
            if (dbVisitor == null)
            {
                isFromInsert = true;
                dbVisitor = new VisitorPersonal
                {
                    PersonContactId = visitor.PersonContactId,
                    PersonContactRelationship = visitor.PersonContactRelationship,
                    PersonSiteId = visitor.PersonSiteId,
                    PersonSiteBnum = visitor.PersonSiteBnum,
                    CreateBy = _personnelId,
                    FknFirstName = visitor.PersonFirstName,
                    FknLastName = visitor.PersonLastName,
                    FknMiddleName = visitor.PersonMiddleName,
                    FknSuffixName = visitor.PersonSuffix
                };
            }
            else
            {
                dbVisitor.PersonDlNoExpiration = visitor.PersonDlNoExpiration;
                dbVisitor.UpdateBy = _personnelId;
                dbVisitor.UpdateDate = DateTime.Now;
            }
            dbVisitor.PersonFirstName = visitor.PersonFirstName;
            dbVisitor.PersonMiddleName = visitor.PersonMiddleName;
            dbVisitor.PersonLastName = visitor.PersonLastName;
            dbVisitor.PersonPhone = visitor.PersonPhone;
            dbVisitor.PersonBusinessPhone = visitor.PersonBusinessPhone;
            dbVisitor.PersonBusinessFax = visitor.PersonBusinessFax;
            dbVisitor.PersonDob = visitor.PersonDob;
            dbVisitor.PersonPlaceOfBirth = visitor.PersonPlaceOfBirth;
            dbVisitor.PersonDlNumber = visitor.PersonDlNumber;
            dbVisitor.PersonDlState = visitor.PersonDlState;
            dbVisitor.PersonDlClass = visitor.PersonDlClass;
            dbVisitor.PersonDlExpiration = visitor.PersonDlExpiration;
            dbVisitor.PersonOtherIdType = visitor.PersonOtherIdType;
            dbVisitor.PersonOtherIdNumber = visitor.PersonOtherIdNumber;
            dbVisitor.PersonOtherIdState = visitor.PersonOtherIdState;
            dbVisitor.PersonOtherIdDescription = visitor.PersonOtherIdDescription;
            dbVisitor.PersonOtherIdExpiration = visitor.PersonOtherIdExpiration;
            dbVisitor.PersonSsn = visitor.PersonSsn;
            dbVisitor.PersonSuffix = visitor.PersonSuffix;
            dbVisitor.PersonFbiNo = visitor.PersonFbiNo;
            dbVisitor.PersonDeceased = visitor.PersonDeceased;
            dbVisitor.PersonDeceasedDate = visitor.PersonDeceasedDate;
            dbVisitor.PersonMissing = visitor.PersonMissing;
            dbVisitor.PersonMissingDate = visitor.PersonMissingDate;
            dbVisitor.PersonPlaceOfBirthList = visitor.PersonPlaceOfBirthList;
            dbVisitor.PersonCii = visitor.PersonCii;
            dbVisitor.PersonDoc = visitor.PersonDoc;
            dbVisitor.PersonPhone2 = visitor.PersonPhone2;
            dbVisitor.PersonCellPhone = visitor.PersonCellPhone;
            dbVisitor.PersonEmail = visitor.PersonEmail;
            dbVisitor.PersonMaidenName = visitor.PersonMaidenName;
            dbVisitor.IllegalAlienFlag = visitor.PersonIllegalAlien;
            dbVisitor.UsCitizenFlag = visitor.PersonUsCitizen;
            dbVisitor.PersonAlienNo = visitor.PersonAlienNo;
            dbVisitor.AfisNumber = visitor.AfisNumber;
            if (dbVisitor.PersonId <= 0)
            {
                _context.Visitor.Add(dbVisitor);
            }
            _personIdentityService.LoadInsertPersonHistory(dbVisitor.PersonId, visitor.PersonHistoryList);
            _personIdentityService.InsertUpdateCustomField(dbVisitor.PersonId, visitor.customFields);
            await _context.SaveChangesAsync();
            if (visitor.PersonUsCitizen && isFromInsert)
            {
                PersonCitizenshipVm citizenship = new PersonCitizenshipVm
                {
                    PersonId = dbVisitor.PersonId,
                    CitizenshipCountry = PersonConstants.UNITEDSTATES,
                    IsFromUsCitizen = true
                };
                await _personIdentityService.InsertUpdatePersonCitizenship(citizenship);
            }
            return dbVisitor.PersonId;
        }
        public async Task<KeyValuePair<int, int>> InsertIdentifiedVisitorDetails(IdentifyVisitorVm objParam)
        {
            //FrontDeskFlag
            Visit visit = new Visit
            {
                CreateBy = _personnelId,
                CreateDate = DateTime.Now,
                StartDate = DateTime.Now,
                FrontDeskFlag = objParam.VisitorIdDetails.FrontDeskFlag,
                RegFacilityId = objParam.VisitorIdDetails.FacilityId,
                VisitorType = objParam.VisitorIdDetails.VisitorType,
                VisitAdditionAdultCount = objParam.VisitorIdDetails.AdultVisitorsCount,
                VisitAdditionChildCount = objParam.VisitorIdDetails.ChildVisitorsCount
            };
            _context.Visit.Add(visit);
            _context.SaveChanges();

            //Visitor Id details
            VisitToVisitor visitToVisitor = new VisitToVisitor
            {
                ScheduleId = visit.ScheduleId,
                PersonId = objParam.PersonBasicDetails.PersonId,
                VisitorIdType = objParam.VisitorIdDetails.VisitorIdType,
                VisitorIdNumber = objParam.VisitorIdDetails.VisitorIdNumber,
                VisitorIdState = objParam.VisitorIdDetails.VisitorIdState
            };
            _context.VisitToVisitor.Add(visitToVisitor);

            await InsertVisitorDetails(objParam);

            if (visit.VisitAdditionChildCount > 0)
            {
                await InsertChildVisitors(objParam, visitToVisitor);
            }
            //Need to Test
            if (visit.VisitAdditionAdultCount > 0)
            {
                await InsertAdultVisitor(objParam.VisitAdultDetail, visitToVisitor, visit);
            }

            return new KeyValuePair<int, int>(visitToVisitor.VisitToVisitorId, visitToVisitor.ScheduleId);
        }
        private async Task InsertChildVisitors(IdentifyVisitorVm childList, VisitToVisitor visitToVisitor)
        {
            childList.VisitChildDetail.ForEach(item =>
            {
                VisitToChild visitToChild = new VisitToChild
                {
                    VisitChildName = item.VisitChildName,
                    VisitChildDob = item.VisitChildDob,
                    VisitChildNote = item.VisitChildNote,
                    VisitChildVisitId = visitToVisitor.VisitToVisitorId,
                    PersonId = visitToVisitor.PersonId,
                    VisitScheduleId = visitToVisitor.ScheduleId,
                    VisitChildFlag = childList.VisitorIdDetails.VisitChildFlag //item.VisitChildFlag
                };
                _context.VisitToChild.Add(visitToChild);
            });

            await _context.SaveChangesAsync();
        }

        public List<VisitToChild> GetChildVisitors(int scheduleId, int personId)
        {
            List<VisitToChild> visitToChild = new List<VisitToChild>();
            if (scheduleId > 0)
            {
                visitToChild = _context.VisitToChild.Where(s => s.VisitScheduleId == scheduleId).ToList();
            }
            else
            {
                VisitToChild childDetails= _context.VisitToChild.OrderByDescending(o => o.VisitScheduleId)
                    .Where(s => s.PersonId == personId).FirstOrDefault();

                //int? visiScheduleId = _context.VisitToChild.OrderByDescending(o => o.VisitScheduleId)
                //    .Where(s => s.PersonId == personId && s.VisitChildFlag).Select(s => s.VisitScheduleId).FirstOrDefault();
                if (childDetails!=null && childDetails.VisitChildFlag)
                {
                    visitToChild = _context.VisitToChild.Where(s =>
                        s.PersonId == personId && s.VisitChildFlag && s.VisitScheduleId == childDetails.VisitScheduleId).ToList();
                }
            }

            return visitToChild;
        }

        private async Task InsertVisitorDetails(IdentifyVisitorVm objParam)
        {
            Person personToVisit = _context.Person.AsNoTracking().Single(p => p.PersonId == objParam.PersonBasicDetails.PersonId);
            if (objParam.VisitorIdDetails.VisitorType == 1)
            {
                VisitorProfessional visitorProfessional = new VisitorProfessional();
                foreach (PropertyInfo property in typeof(Person).GetProperties())
                {
                    property.SetValue(visitorProfessional, property.GetValue(personToVisit, null), null);
                }
                visitorProfessional.ProfVisitorIdExpdate = objParam.VisitorIdDetails.ProfVisitorIdExpDate;
                visitorProfessional.VisitorType = objParam.VisitorIdDetails.ProfessionalType;
                visitorProfessional.VisitorBefore = objParam.VisitorIdDetails.VisitorBefore;
                visitorProfessional.VisitorNotes = objParam.VisitorIdDetails.VisitorNotes;

                visitorProfessional.PersonOfInterestFlag = objParam.VisitorFlagDetails.PersonOfInterestFlag;
                visitorProfessional.PersonOfInterestReason = objParam.VisitorFlagDetails.PersonOfInterestReason;
                visitorProfessional.PersonOfInterestExpire = objParam.VisitorFlagDetails.PersonOfInterestExpire;
                visitorProfessional.PersonOfInterestNote = objParam.VisitorFlagDetails.PersonOfInterestNote;

                visitorProfessional.VisitorNotAllowedFlag = objParam.VisitorFlagDetails.VisitorNotAllowedFlag;
                visitorProfessional.VisitorNotAllowedReason = objParam.VisitorFlagDetails.VisitorNotAllowedReason;
                visitorProfessional.VisitorNotAllowedExpire = objParam.VisitorFlagDetails.VisitorNotAllowedExpire;
                visitorProfessional.VisitorNotAllowedNote = objParam.VisitorFlagDetails.VisitorNotAllowedNote;

                _context.Person.Update(visitorProfessional);
                _context.Entry(visitorProfessional).Property("Discriminator").CurrentValue = "VisitorProfessional";
                await _context.SaveChangesAsync();
            }
            else
            {
                VisitorPersonal visitorPersonal = new VisitorPersonal();
                foreach (PropertyInfo property in  typeof(Person).GetProperties())
                {
                    property.SetValue(visitorPersonal, property.GetValue(personToVisit, null), null);
                }
                visitorPersonal.PersonOfInterestFlag = objParam.VisitorFlagDetails.PersonOfInterestFlag;
                visitorPersonal.PersonOfInterestReason = objParam.VisitorFlagDetails.PersonOfInterestReason;
                visitorPersonal.PersonOfInterestExpire = objParam.VisitorFlagDetails.PersonOfInterestExpire;
                visitorPersonal.PersonOfInterestNote = objParam.VisitorFlagDetails.PersonOfInterestNote;
                visitorPersonal.VisitorType = objParam.VisitorIdDetails.ProfessionalType;
                visitorPersonal.VisitorNotAllowedFlag = objParam.VisitorFlagDetails.VisitorNotAllowedFlag;
                visitorPersonal.VisitorNotAllowedReason = objParam.VisitorFlagDetails.VisitorNotAllowedReason;
                visitorPersonal.VisitorNotAllowedExpire = objParam.VisitorFlagDetails.VisitorNotAllowedExpire;
                visitorPersonal.VisitorNotAllowedNote = objParam.VisitorFlagDetails.VisitorNotAllowedNote;

                visitorPersonal.VisitorNotes = objParam.VisitorIdDetails.VisitorNotes;
                visitorPersonal.VisitorBefore = objParam.VisitorIdDetails.VisitorBefore;

                _context.Person.Update(visitorPersonal);
                _context.Entry(visitorPersonal).Property("Discriminator").CurrentValue = "VisitorPersonal";
                await _context.SaveChangesAsync();
            }
        }
        public async Task<int> UpdateIdentifiedVisitorDetails(IdentifyVisitorVm objParam)
        {
            Visit visit = _context.Visit.SingleOrDefault(w => w.ScheduleId == objParam.VisitorIdDetails.ScheduleId);
            if (visit != null)
            {
                //Updating FrontDeskFlag
                visit.FrontDeskFlag = objParam.VisitorIdDetails.FrontDeskFlag;
                visit.VisitorType = objParam.VisitorIdDetails.VisitorType;
                //Updating Additional Visitors count
                visit.VisitAdditionAdultCount = objParam.VisitorIdDetails.AdultVisitorsCount;
                visit.VisitAdditionChildCount = objParam.VisitorIdDetails.ChildVisitorsCount;
            }

            _context.SaveChanges();

            //Updating Visitor Id details
            VisitToVisitor visitToVisitor =
                _context.VisitToVisitor.Single(w => w.VisitToVisitorId == objParam.VisitorIdDetails.VisitToVisitorId);
            visitToVisitor.PersonId = objParam.PersonBasicDetails.PersonId;
            visitToVisitor.VisitorIdType = objParam.VisitorIdDetails.VisitorIdType;
            visitToVisitor.VisitorIdNumber = objParam.VisitorIdDetails.VisitorIdNumber;
            visitToVisitor.VisitorIdState = objParam.VisitorIdDetails.VisitorIdState;

            await InsertVisitorDetails(objParam);

            if (visit?.VisitAdditionChildCount > 0)
            {
                await UpdateChildVisitors(objParam);
            }
            if (visit?.VisitAdditionAdultCount > 0)
            {
                await UpdateAdultVisitorDetails(objParam, visit);
            }
            return await _context.SaveChangesAsync();
        }
        public async Task<int> UpdateVisitDenyDetails(VisitDetails objParam)
        {
            Visit visit = _context.Visit.Single(w => w.ScheduleId == objParam.ScheduleId);

            if (objParam.VisitDenyFlag)
            {
                visit.VisitDenyFlag = objParam.VisitDenyFlag ? 1 : 0;
                visit.VisitDenyReason = objParam.VisitDenyReason;
                visit.VisitDenyNote = objParam.VisitDenyNote;
                visit.CountAsVisit = 0;
                visit.CompleteVisitFlag = true;
                visit.CompleteVisitReason = _commonService.GetLookups(new[] { LookupConstants.VISITCOMPLETE })
                    .SingleOrDefault(w => w.LookupFlag10 == 1)?.LookupDescription;
            }

            return await _context.SaveChangesAsync();
        }
        public async Task<int> DenyVisitor(VisitDetails objParam)
        {
            Visit visit = _context.Visit.Single(w => w.ScheduleId == objParam.ScheduleId);
            if (objParam.VisitDenyFlag)
            {
                visit.VisitDenyFlag = objParam.VisitDenyFlag ? 1 : 0;
                visit.VisitDenyReason = objParam.VisitDenyReason;
                visit.VisitDenyNote = objParam.VisitDenyNote;
                visit.CountAsVisit = 0;
                visit.CompleteVisitFlag = true;
                visit.CompleteVisitReason = _commonService.GetLookups(new[] { LookupConstants.VISITCOMPLETE })
                    .SingleOrDefault(w => w.LookupFlag10 == 1)?.LookupDescription;
            }
            VisitToVisitor visitor = _context.VisitToVisitor.Single(w => w.ScheduleId == objParam.ScheduleId);
            List<VisitToVisitor> visitToVisitors = _context.VisitToVisitor
                .Where(s => s.PrimaryVisitorId == visitor.VisitToVisitorId).ToList();
            int[] scheduleId = visitToVisitors.Select(s => s.ScheduleId).ToArray();

            List<Visit> lstVisit = _context.Visit.Where(s => scheduleId.Contains(s.ScheduleId)).ToList();

            visitToVisitors.ForEach(item =>
            {
                Visit visits = lstVisit.Single(w => w.ScheduleId == item.ScheduleId);
                if (objParam.VisitDenyFlag)
                {
                    visits.VisitDenyFlag = objParam.VisitDenyFlag ? 1 : 0;
                    visits.VisitDenyReason = objParam.VisitDenyReason;
                    visits.VisitDenyNote = objParam.VisitDenyNote;
                    visits.CountAsVisit = 0;
                    visits.CompleteVisitFlag = true;
                    visits.CompleteVisitReason = _commonService.GetLookups(new[] { LookupConstants.VISITCOMPLETE })
                        .SingleOrDefault(w => w.LookupFlag10 == 1)?.LookupDescription;
                }
            });

            return await _context.SaveChangesAsync();
        }

        private async Task UpdateChildVisitors(IdentifyVisitorVm objChildList)
        {
            objChildList.VisitChildDetail.ForEach(item =>
            {
                VisitToChild visitToChild = _context.VisitToChild.Single(w => w.VisitChildId == item.VisitChildId);

                visitToChild.VisitChildName = item.VisitChildName;
                visitToChild.VisitChildDob = item.VisitChildDob;
                visitToChild.VisitChildNote = item.VisitChildNote;
                visitToChild.VisitChildFlag = objChildList.VisitorIdDetails.VisitChildFlag;
            });

            await _context.SaveChangesAsync();
        }

        private async Task<int> InsertAdultVisitor(List<VisitToAdultDetail> adultDetail, VisitToVisitor visitToVisit, Visit primaryVisit)
        {
            adultDetail.ForEach(item =>
            {
                Visit visit = new Visit
                {
                    CreateBy = _personnelId,
                    CreateDate = DateTime.Now,
                    StartDate = DateTime.Now,
                    RegFacilityId = primaryVisit.RegFacilityId
                };
                _context.Visit.Add(visit);
                _context.SaveChanges();

                //Visitor Id details
                VisitToVisitor visitToVisitor = new VisitToVisitor
                {
                    ScheduleId = visit.ScheduleId,
                    PersonId = item.PersonBasicDetails.PersonId,
                    VisitorIdType = item.VisitorIdDetails.VisitorIdType,
                    VisitorIdNumber = item.VisitorIdDetails.VisitorIdNumber,
                    VisitorIdState = item.VisitorIdDetails.VisitorIdState,
                    PrimaryVisitorId = visitToVisit.VisitToVisitorId
                };

                _context.VisitToVisitor.Add(visitToVisitor);
                _context.SaveChanges();

                Task.Run(() => InsertAdultVisitorDetails(item, primaryVisit));
            });
            return await _context.SaveChangesAsync();
        }
        private async Task<int> InsertAdultVisitorDetails(VisitToAdultDetail objParam, Visit visit)
        {
            Person personToVisit = _context.Person.AsNoTracking().Single(p => p.PersonId == objParam.PersonBasicDetails.PersonId);
            if (visit.VisitorType == 1)
            {
                VisitorProfessional visitorProfessional = new VisitorProfessional();
                foreach (PropertyInfo property in typeof(Person).GetProperties())
                {
                    property.SetValue(visitorProfessional, property.GetValue(personToVisit, null), null);
                }
                visitorProfessional.ProfVisitorIdExpdate = objParam.VisitorIdDetails.ProfVisitorIdExpDate;
                visitorProfessional.VisitorType = objParam.VisitorIdDetails.ProfessionalType;
                visitorProfessional.VisitorNotes = objParam.VisitorIdDetails.VisitorNotes;
                visitorProfessional.PersonOfInterestFlag = objParam.VisitorFlagDetails.PersonOfInterestFlag;
                visitorProfessional.PersonOfInterestReason = objParam.VisitorFlagDetails.PersonOfInterestReason;
                visitorProfessional.PersonOfInterestExpire = objParam.VisitorFlagDetails.PersonOfInterestExpire;
                visitorProfessional.PersonOfInterestNote = objParam.VisitorFlagDetails.PersonOfInterestNote;

                visitorProfessional.VisitorNotAllowedFlag = objParam.VisitorFlagDetails.VisitorNotAllowedFlag;
                visitorProfessional.VisitorNotAllowedReason = objParam.VisitorFlagDetails.VisitorNotAllowedReason;
                visitorProfessional.VisitorNotAllowedExpire = objParam.VisitorFlagDetails.VisitorNotAllowedExpire;
                visitorProfessional.VisitorNotAllowedNote = objParam.VisitorFlagDetails.VisitorNotAllowedNote;

                _context.Person.Update(visitorProfessional);
                _context.Entry(visitorProfessional).Property("Discriminator").CurrentValue = "VisitorProfessional";
                await _context.SaveChangesAsync();
            }
            else
            {
                VisitorPersonal visitorPersonal = new VisitorPersonal();
                foreach (PropertyInfo property in typeof(Person).GetProperties())
                {
                    property.SetValue(visitorPersonal, property.GetValue(personToVisit, null), null);
                }
                visitorPersonal.PersonOfInterestFlag = objParam.VisitorFlagDetails.PersonOfInterestFlag;
                visitorPersonal.PersonOfInterestReason = objParam.VisitorFlagDetails.PersonOfInterestReason;
                visitorPersonal.PersonOfInterestExpire = objParam.VisitorFlagDetails.PersonOfInterestExpire;
                visitorPersonal.PersonOfInterestNote = objParam.VisitorFlagDetails.PersonOfInterestNote;

                visitorPersonal.VisitorNotAllowedFlag = objParam.VisitorFlagDetails.VisitorNotAllowedFlag;
                visitorPersonal.VisitorNotAllowedReason = objParam.VisitorFlagDetails.VisitorNotAllowedReason;
                visitorPersonal.VisitorNotAllowedExpire = objParam.VisitorFlagDetails.VisitorNotAllowedExpire;
                visitorPersonal.VisitorNotAllowedNote = objParam.VisitorFlagDetails.VisitorNotAllowedNote;

                visitorPersonal.VisitorNotes = objParam.VisitorIdDetails.VisitorNotes;

                _context.Person.Update(visitorPersonal);
                _context.Entry(visitorPersonal).Property("Discriminator").CurrentValue = "VisitorPersonal";
                await _context.SaveChangesAsync();
            }
            return await _context.SaveChangesAsync();
        }

        public List<VisitorIdentityAndRelationship> GetInmateRelationShipDetails(int scheduleId, VisitorIdentityAndRelationship relationShip)
        {

            List<VisitorIdentityAndRelationship> adultVisitorVm = new List<VisitorIdentityAndRelationship>();
            if (scheduleId <= 0) return adultVisitorVm;
            int visitToVisitorId = _context.VisitToVisitor.Single(s => s.ScheduleId == scheduleId).VisitToVisitorId;

            List<VisitorIdentityAndRelationship> lstVisitorDetails = _context.VisitToVisitor
                .Where(w => w.PrimaryVisitorId == visitToVisitorId).Select(s => new VisitorIdentityAndRelationship
                {
                    PersonId = s.PersonId,
                    InmateId = relationShip.InmateId,
                    VisitorBefore = relationShip.VisitorBefore,
                    VisitorType = relationShip.VisitorType,
                    PrimaryVisitorId = s.PrimaryVisitorId ?? null,
                    VisitToVisitorId = s.VisitToVisitorId,
                    ScheduleId = s.ScheduleId
                }).OrderBy(o => o.VisitToVisitorId).ToList();


            // if(lstVisitorDetails.Where(a=>a.PrimaryVisitorId!=null))
            List<int> lstPersonId = lstVisitorDetails.Select(s => s.PersonId).ToList();
            //To get Relationship details for the visitors
            lstVisitorDetails = GetVisitorRelationship(false, lstPersonId, lstVisitorDetails);
            lstVisitorDetails.ForEach(item =>
            {
                item.LstVisitorBasedInmates = GetVisitorBasedInmates(item.PersonId);
                item.VisitorToInmateDetails = item.LstVisitorBasedInmates.Count > 0 ?
                item.LstVisitorBasedInmates.SingleOrDefault(a => a.InmateId == item.InmateId) : new VisitorBasedInmates();
            });


            return lstVisitorDetails;
        }

        public List<VisitToAdultDetail> GetAdultVisitorDetails(int scheduleId)
        {
            List<VisitToAdultDetail> adultVisitorVm = new List<VisitToAdultDetail>();
            if (scheduleId <= 0) return adultVisitorVm;
            int visitToVisitorId = _context.VisitToVisitor.Single(s => s.ScheduleId == scheduleId).VisitToVisitorId;

            List<VisitToVisitor> visitToVisitor = _context.VisitToVisitor.Where(w => w.PrimaryVisitorId == visitToVisitorId).ToList();

            visitToVisitor.ForEach(item =>
            {
                VisitToAdultDetail visitToAdult = new VisitToAdultDetail
                {
                    PersonBasicDetails = GetPersonBasicDetails(item.PersonId),
                    VisitorIdDetails = GetAdultVisitorIdDetails(item.PersonId, item.ScheduleId)
                };
                if (item.ScheduleId > 0)
                {
                    visitToAdult.VisitorFlagDetails = _context.Visitor.Where(w => w.PersonId == item.PersonId)
                        .Select(s => new VisitorFlagDetails
                        {
                            VisitorNotAllowedFlag = s.VisitorNotAllowedFlag,
                            VisitorNotAllowedReason = s.VisitorNotAllowedReason,
                            VisitorNotAllowedExpire = s.VisitorNotAllowedExpire,
                            VisitorNotAllowedNote = s.VisitorNotAllowedNote,
                            PersonOfInterestFlag = s.PersonOfInterestFlag,
                            PersonOfInterestReason = s.PersonOfInterestReason,
                            PersonOfInterestExpire = s.PersonOfInterestExpire,
                            PersonOfInterestNote = s.PersonOfInterestNote
                        }).SingleOrDefault();
                }
                adultVisitorVm.Add(visitToAdult);
            });

            return adultVisitorVm;
        }

        public async Task<int> UpdateAdultVisitorDetails(IdentifyVisitorVm objParam, Visit visits)
        {
            objParam.VisitAdultDetail.ForEach(item =>
            {
                VisitToVisitor visitor =
                    _context.VisitToVisitor.Single(w => w.VisitToVisitorId == item.VisitorIdDetails.VisitToVisitorId);
                visitor.PersonId = item.PersonBasicDetails.PersonId;
                visitor.VisitorIdType = item.VisitorIdDetails.VisitorIdType;
                visitor.VisitorIdNumber = item.VisitorIdDetails.VisitorIdNumber;
                visitor.VisitorIdState = item.VisitorIdDetails.VisitorIdState;
                visitor.PrimaryVisitorId = objParam.VisitorIdDetails.VisitToVisitorId;
                _context.SaveChanges();

                Task.Run(() => InsertAdultVisitorDetails(item, visits));
                // InsertAdultVisitorDetails(item, visit);
            });
            return await _context.SaveChangesAsync();
        }

        #endregion

        #region  Step 2 - Select Inmate

        public SavedVisitorDetailsVm GetSavedVisitorDetails(int scheduleId) =>
            new SavedVisitorDetailsVm
            {
                LstVisitorDetails = GetSavedVisitorAndRelationshipDetails(scheduleId, false),

                //Relationship details
                LstRelationship = _commonService.GetLookups(new[] { LookupConstants.RELATIONS }),

                //Flag Reason
                LstFlagReason =
                    _commonService.GetLookupKeyValuePairs(LookupConstants.VISPERSINTREAS),

                //Deny Reason
                LstDenyReason =
                    _commonService.GetLookupKeyValuePairs(LookupConstants.VISITDENYREAS),

                LstVisitChild = GetChildVisitors(scheduleId, 0),

                IsVisitDeny = GetVisitDenyFlag()
            };

        private List<VisitorIdentityAndRelationship> GetSavedVisitorAndRelationshipDetails(int scheduleId, bool flag)
        {
            //To get Primary  Visitor details
            //  IList<VisitorIdentityAndRelationship> visitToVisitor = _ToList();
            List<VisitorIdentityAndRelationship> lstVisitorDetails = new List<VisitorIdentityAndRelationship>();
            VisitorIdentityAndRelationship relationShip = _context.VisitToVisitor.Where(w => w.ScheduleId == scheduleId).Select(s => new VisitorIdentityAndRelationship
            {
                PersonId = s.PersonId,
                InmateId = s.Visit.InmateId,

                VisitorBefore = s.Visitor.VisitorBefore,
                VisitorType = s.Visit.VisitorType ?? 0,
                PrimaryVisitorId = s.PrimaryVisitorId ?? null,
                VisitToVisitorId = s.VisitToVisitorId,
                ScheduleId = s.ScheduleId
            }).OrderBy(o => o.VisitToVisitorId).SingleOrDefault();

            if (relationShip != null)
            {
                lstVisitorDetails.Add(relationShip);
                //To get  Adult Visitor details
                IQueryable<VisitorIdentityAndRelationship> lstVisitorDetails1 = _context.VisitToVisitor
                  .Where(w => w.PrimaryVisitorId == relationShip.VisitToVisitorId).Select(s => new VisitorIdentityAndRelationship
                  {
                      PersonId = s.PersonId,
                      InmateId = relationShip.InmateId,

                      VisitorBefore = relationShip.VisitorBefore,
                      VisitorType = relationShip.VisitorType,
                      PrimaryVisitorId = s.PrimaryVisitorId ?? null,
                      VisitToVisitorId = s.VisitToVisitorId,
                      ScheduleId = s.ScheduleId
                  }).OrderBy(o => o.VisitToVisitorId);

                lstVisitorDetails.AddRange(lstVisitorDetails1);

                // if(lstVisitorDetails.Where(a=>a.PrimaryVisitorId!=null))

                List<int> lstPersonId = lstVisitorDetails.Select(s => s.PersonId).ToList();

                //To get Relationship details for the visitors
                lstVisitorDetails = GetVisitorRelationship(flag, lstPersonId, lstVisitorDetails);

                if (!flag)
                {
                    lstVisitorDetails.ForEach(item =>
                    {
                        item.LstVisitorBasedInmates = GetVisitorBasedInmates(item.PersonId);
                        item.VisitorToInmateDetails = item.LstVisitorBasedInmates.Count > 0 ?
                        item.LstVisitorBasedInmates.SingleOrDefault(a => a.InmateId == item.InmateId) : new VisitorBasedInmates();

                    });
                }

            }



            return lstVisitorDetails;
        }

        private List<VisitorBasedInmates> GetVisitorBasedInmates(int visitorId)
        {
            //Get Visitor based Inmate details with relationship
            List<VisitorBasedInmates> visitorBasedInmates = _context.VisitorToInmate.Where(v => v.VisitorId == visitorId)
                .Select(s => new VisitorBasedInmates
                {
                    InmateId = s.InmateId,
                    InmateNumber = s.Inmate.InmateNumber,
                    PersonFirstName = s.Inmate.Person.PersonFirstName,
                    PersonLastName = s.Inmate.Person.PersonLastName,
                    VisitorRelationshipId = s.VisitorRelationship,
                    VisitorNote = s.VisitorNote,
                    VisitorNotAllowedExpire = s.VisitorNotAllowedExpire,
                    VisitorNotAllowedFlag = s.VisitorNotAllowedFlag,
                    VisitorNotAllowedNote = s.VisitorNotAllowedNote,
                    VisitorNotAllowedReason = s.VisitorNotAllowedReason,
                    VisitorToInmateId = s.VisitorToInmateId,
                    VisitorId = s.VisitorId,
                }).ToList();

            List<LookupVm> lstLookup = _commonService.GetLookups(new[] { LookupConstants.RELATIONS });

            visitorBasedInmates.ForEach(item =>
            {
                item.VisitorRelationship = lstLookup.SingleOrDefault(l => l.LookupIndex == item.VisitorRelationshipId)
                    ?.LookupDescription;
            });

            return visitorBasedInmates;
        }

        public SelectInmateVm GetSelectedInmateVisitDetails(int inmateId, List<int> lstVisitorId, int scheduleId)
        {
            SelectInmateVm selectInmateVm = new SelectInmateVm
            {
                //Inmate Visitation Information
                InmateVisitationInfo = GetInmateVisitationInfo(inmateId),

                //Visitor To Inmate Assignment details
                LstVisitorToInmateDetail = _context.VisitorToInmate
                    .Where(w => w.InmateId == inmateId && lstVisitorId.Contains(w.VisitorId ?? 0))
                    .Select(s => new VisitorToInmateDetails
                    {
                        VisitorToInmateId = s.VisitorToInmateId,
                        VisitorId = s.VisitorId,
                        InmateId = s.InmateId,
                        VisitorNotAllowedExpire = s.VisitorNotAllowedExpire,
                        VisitorNotAllowedFlag = s.VisitorNotAllowedFlag,
                        VisitorNotAllowedNote = s.VisitorNotAllowedNote,
                        VisitorNotAllowedReason = s.VisitorNotAllowedReason,
                        VisitorNote = s.VisitorNote,
                        VisitorRelationshipId = s.VisitorRelationship
                    }).ToList(),

                //Deny Details
                VisitDenyDetails = GetVisitDenyDetails(scheduleId)
            };

            return selectInmateVm;
        }

        private InmateVisitationInfo GetInmateVisitationInfo(int inmateId)
        {
            InmateVisitationInfo inmateVisitationInfo = _context.Inmate.Where(i => i.InmateId == inmateId)
                .Select(s => new InmateVisitationInfo
                {
                    HousingDetail = new HousingDetail
                    {
                        FacilityAbbr = s.Facility.FacilityAbbr,
                        HousingUnitListId = s.HousingUnit.HousingUnitListId,
                        HousingUnitLocation = s.HousingUnit.HousingUnitLocation,
                        HousingUnitNumber = s.HousingUnit.HousingUnitNumber,
                        HousingUnitBedLocation = s.HousingUnit.HousingUnitBedLocation,
                        HousingUnitBedNumber = s.HousingUnit.HousingUnitBedNumber
                    },
                    InmateClassificationReason = s.InmateClassification.InmateClassificationReason
                }).Single();


            if (inmateVisitationInfo.InmateClassificationReason == null ||
                inmateVisitationInfo.HousingDetail.HousingUnitListId == null) return inmateVisitationInfo;
            HousingUnitVisitationClassRule housingUnitVisClassRule = _context.HousingUnitVisitationClassRule
                .SingleOrDefault(s => s.HousingUnitListId == inmateVisitationInfo.HousingDetail.HousingUnitListId
                    && s.ClassificationReason == inmateVisitationInfo.InmateClassificationReason);
            DateTime startDate = CountAsVisitPerWeek();
            DateTime endDate = startDate.AddDays(6).Date;
            List<Visit> visits = _context.Visit.Where(v => v.CompleteVisitFlag
                && v.InmateId == inmateId && v.StartDate.Date >= startDate && v.StartDate.Date <= endDate).ToList();

            if (housingUnitVisClassRule == null) return inmateVisitationInfo;
            inmateVisitationInfo.VisitPerWeek = housingUnitVisClassRule.VisitRulePerWeek;
            inmateVisitationInfo.TotalVisitPerWeekCount = visits.Count;

            return inmateVisitationInfo;
        }

        public async Task<int> InsertVisitorToInmate(List<VisitorToInmateDetails> lstVisitorToInmateDetail)
        {
            lstVisitorToInmateDetail.ForEach(item =>
            {
                VisitorToInmate visitorToInmate = new VisitorToInmate
                {
                    InmateId = item.InmateId,
                    VisitorId = item.VisitorId,
                    VisitorNotAllowedExpire = item.VisitorNotAllowedExpire,
                    VisitorNotAllowedFlag = item.VisitorNotAllowedFlag,
                    VisitorNotAllowedNote = item.VisitorNotAllowedNote,
                    VisitorNotAllowedReason = item.VisitorNotAllowedReason,
                    VisitorNote = item.VisitorNote,
                    VisitorRelationship = item.VisitorRelationshipId,
                    CreateDate = DateTime.Now,
                    CreateBy = _personnelId
                };
                _context.VisitorToInmate.Add(visitorToInmate);
                if (item.TrackingConflict != null)
                {
                    if (item.TrackingConflict.Count > 0 && item.IsPrimary)
                    {
                        item.TrackingConflict.ForEach(Tc =>
                        {
                            string note = $"{FloorNotesConflictConstants.TYPE} {Tc.ConflictType} " +
                                          $"{FloorNotesConflictConstants.DESCRIPTION} {Tc.ConflictDescription}";
                            InsertFloorNote(note, visitorToInmate.InmateId);
                        });
                    }
                }


            });

            return await _context.SaveChangesAsync();
        }

        public async Task<int> UpdateVisitorToInmate(List<VisitorToInmateDetails> lstVisitorToInmateDetail)
        {
            lstVisitorToInmateDetail.ForEach(item =>
            {
                //VisitorToInmate visitorToInmate =
                //    _context.VisitorToInmate.Single(w => w.VisitorToInmateId == item.VisitorToInmateId);
                //if(visitorToInmate!=null)
                int inmateId = item.InmateId;


                if (item.VisitorToInmateId > 0)
                {
                    VisitorToInmate visitorToInmate = _context.VisitorToInmate.Find(item.VisitorToInmateId);
                    visitorToInmate.InmateId = item.InmateId;
                    visitorToInmate.VisitorId = item.VisitorId;
                    visitorToInmate.VisitorNotAllowedExpire = item.VisitorNotAllowedExpire;
                    visitorToInmate.VisitorNotAllowedFlag = item.VisitorNotAllowedFlag;
                    visitorToInmate.VisitorNotAllowedNote = item.VisitorNotAllowedNote;
                    visitorToInmate.VisitorNotAllowedReason = item.VisitorNotAllowedReason;
                    visitorToInmate.VisitorNote = item.VisitorNote;
                    visitorToInmate.VisitorRelationship = item.VisitorRelationshipId;
                    visitorToInmate.UpdateDate = DateTime.Now;
                    visitorToInmate.UpdateBy = _personnelId;
                    _context.VisitorToInmate.Update(visitorToInmate);
                    _context.SaveChanges();
                }
                else
                {
                    VisitorToInmate visitorToInmate1 = new VisitorToInmate
                    {
                        InmateId = item.InmateId,
                        VisitorId = item.VisitorId,
                        VisitorNotAllowedExpire = item.VisitorNotAllowedExpire,
                        VisitorNotAllowedFlag = item.VisitorNotAllowedFlag,
                        VisitorNotAllowedNote = item.VisitorNotAllowedNote,
                        VisitorNotAllowedReason = item.VisitorNotAllowedReason,
                        VisitorNote = item.VisitorNote,
                        VisitorRelationship = item.VisitorRelationshipId,
                        CreateDate = DateTime.Now,
                        CreateBy = _personnelId
                    };
                    _context.VisitorToInmate.Add(visitorToInmate1);
                    _context.SaveChanges();
                    //visitorToInmate.Add
                }

                if (item.TrackingConflict != null)
                {
                    if (item.TrackingConflict.Count > 0 && item.IsPrimary)
                    {
                        item.TrackingConflict.ForEach(Tc =>
                        {
                            string note = $"{FloorNotesConflictConstants.TYPE} {Tc.ConflictType} " +
                                          $"{FloorNotesConflictConstants.DESCRIPTION} {Tc.ConflictDescription}";
                            InsertFloorNote(note, inmateId);
                        });
                    }
                }
            });


            return await _context.SaveChangesAsync();
        }

        public async Task<int> UpdateScheduleInmate(int scheduleId, int inmateId)
        {
            Visit visit = _context.Visit.Single(w => w.ScheduleId == scheduleId);
            visit.InmateId = inmateId;
            return await _context.SaveChangesAsync();
        }

        private void LoadRejectVisitationforInmate(int inmateId, int personId)
        {
            List<VisitorToInmate> VisitToInmate = _context.VisitorToInmate.Where(
                ipx => ipx.InmateId == inmateId
                    && ipx.VisitorId == personId
                    && (!ipx.DeleteFlag.HasValue || ipx.DeleteFlag == 0) && ipx.VisitorNotAllowedFlag == 1).ToList();
            if (VisitToInmate.Count == 0) return;
            TrackingConflictVm conflict = new TrackingConflictVm
            {
                ConflictType = ConflictTypeConstants.RejectVisitorForInmate,
                ConflictDescription = ConflictTypeConstants.RejectVisitorForInmateDescription,
            };
            _trackingConflictDetails.Add(conflict);


            // _trackingConflictDetails.AddRange(
            //                   .Select(ipx => new TrackingConflictVm
            //                   {
            //                       InmateId = ipx.Inmate.InmateId,
            //                       ConflictType = ConflictTypeConstants.RejectVisitorForInmate,
            //                       PersonId = ipx.Inmate.PersonId,
            //                       ConflictDescription = ConflictTypeConstants.RejectVisitorForInmateDescription,
            //                       InmateNumber = ipx.Inmate.InmateNumber
            //                   }).ToList());
        }

        private void LoadRevokeVisitNoteAcknowlege(int inmateId)
        {
            DateTime todayDate = DateTime.Now;

            List<InmatePrivilegeXref> inmatePrivilegeXref = _context.InmatePrivilegeXref.Where(i =>
                   i.Privilege.ShowVisitationAckNote && i.Privilege.InactiveFlag == 0
                      && i.PrivilegeDate.Value.Date <= todayDate.Date &&
                      (i.PrivilegeExpires.HasValue ? i.PrivilegeExpires.Value.Date.AddDays(1)
                          : DateTime.Now.Date.AddDays(1)) >= todayDate.Date
                    && !i.PrivilegeRemoveOfficerId.HasValue && i.InmateId == inmateId)
                     .Select(ipx => new InmatePrivilegeXref
                     {
                         PrivilegeNote = ipx.PrivilegeNote,
                         Privilege = ipx.Privilege
                     })
                    .ToList();

            if (inmatePrivilegeXref.Count == 0) return;

            InmatePrivilegeXref xref = inmatePrivilegeXref.Last();
            TrackingConflictVm conflict = new TrackingConflictVm
            {
                ConflictType = ConflictTypeConstants.RevokeVisitNoteAcknowledgement,
                ConflictDescription = ConflictTypeConstants.RevokeVisitNoteAcknowledgementDescription +
                    xref.Privilege.PrivilegeDescription + " " + xref.PrivilegeNote,
            };
            _trackingConflictDetails.Add(conflict);

        }

        private void LoadVictimContact(int inmateId, int personId)
        {
            //based on vistorinmate
            // List<Lookup> lookupLst = _context.Lookup.Where(lkp => lkp.LookupType == LookupConstants.RELATIONS).ToList();

            // List<int> lookupIds = lookupLst.Where(lkp => lkp.LookupName == LookupConstants.VICTIM).Select(lkp => lkp.LookupId).ToList();
            int[] personIds = _context.Inmate.Where(p => p.InmateId == inmateId).Select(a => a.PersonId).ToArray();
            List<Contact> VisitToInmate = _context.Contact.Where(
                                         ipx => ipx.PersonId == personId && personIds.Contains(ipx.TypePersonId)
                                         && ipx.ContactActiveFlag == "1" && ipx.VictimNotify == 1).ToList();
            if (VisitToInmate.Count <= 0) return;
            TrackingConflictVm conflict = new TrackingConflictVm
            {
                ConflictType = ConflictTypeConstants.VicitimContact,
                ConflictDescription = ConflictTypeConstants.VicitimContactDescription,
            };
            _trackingConflictDetails.Add(conflict);
        }

        private void LoadCustodyConflict(int personId)
        {
            string siteOption = _commonService.GetSiteOptionValue(string.Empty, SiteOptionsConstants.VISITORINCUSTODY);
            int days = siteOption != null ? Convert.ToInt32(siteOption) : 30;

            List<Incarceration> incarceration =
             _context.Incarceration.Where(i => i.Inmate.Person.PersonId == personId
              && i.ReleaseOut != null).OrderByDescending(o => o.IncarcerationId).ToList();
            Incarceration incarce = incarceration.FirstOrDefault(e => e.ReleaseOut >= DateTime.Now.AddDays(-days));
            if (incarce == null) return;
            TrackingConflictVm conflict = new TrackingConflictVm
            {
                ConflictType = ConflictTypeConstants.VisitorInCustody + " " + days + " days",//day refers from admin site options
                ConflictDescription = ConflictTypeConstants.VisitorInCustodyDescription,
            };
            _trackingConflictDetails.Add(conflict);
        }

        private static DateTime StartOfWeek(DateTime dt, DayOfWeek startOfWeek)
        {
            int diff = (7 + (dt.DayOfWeek - startOfWeek)) % 7;
            return dt.AddDays(-1 * diff).Date;
        }

        private DateTime CountAsVisitPerWeek()
        {
            string siteOption = _commonService.GetSiteOptionValue(string.Empty, SiteOptionsConstants.VISITSTARTOFWEEK);
            int number = siteOption != null ? Convert.ToInt32(siteOption) : 1;
            DateTime startDate = DateTime.Now;
            //TODO Really?!
            switch (number)
            {
                case 1:
                    startDate = StartOfWeek(DateTime.Now, DayOfWeek.Monday);
                    break;
                case 2:
                    startDate = StartOfWeek(DateTime.Now, DayOfWeek.Tuesday);
                    break;
                case 3:
                    startDate = StartOfWeek(DateTime.Now, DayOfWeek.Wednesday);
                    break;
                case 4:
                    startDate = StartOfWeek(DateTime.Now, DayOfWeek.Thursday);
                    break;
                case 5:
                    startDate = StartOfWeek(DateTime.Now, DayOfWeek.Friday);
                    break;
                case 6:
                    startDate = StartOfWeek(DateTime.Now, DayOfWeek.Saturday);
                    break;
                case 7:
                    startDate = StartOfWeek(DateTime.Now, DayOfWeek.Sunday);
                    break;
            }
            return startDate;
        }

        private void LoadVisitOfWeekConflict(int inmateId)
        {
            InmateVisitationInfo inmateVisitationInfo = GetInmateVisitationInfo(inmateId);
            // int dayNumber = (int)DateTime.Now.Date.DayOfWeek;
            DateTime startDate = CountAsVisitPerWeek();

            DateTime EndDate = startDate.AddDays(6).Date;
            // DateTime EndDate = StartOfWeek(startDate, DayOfWeek.Sunday);

            if (inmateVisitationInfo.VisitPerWeek == 0 || !(inmateVisitationInfo.VisitPerWeek.HasValue)) return;
            List<Visit> Visit = _context.Visit.Where(v => v.CompleteVisitFlag
                && v.InmateId == inmateId && v.StartDate.Date >= startDate && v.StartDate.Date <= EndDate).ToList();
            if (Visit.Count < inmateVisitationInfo.VisitPerWeek) return;
            TrackingConflictVm conflict = new TrackingConflictVm
            {
                ConflictType = ConflictTypeConstants.VisitsPerWeek,
                ConflictDescription = ConflictTypeConstants.VisitsPerWeekDescription,
            };
            _trackingConflictDetails.Add(conflict);
        }

        private void LoadgangKeepSeperateConflict(int inmateId, int personId)
        {
            int[] inmateIds = _context.Inmate.Where(p => p.PersonId == personId).Select(a => a.InmateId).ToArray();

            List<KeepSepInmateDetailsVm> keepsep = _keepSepAlertService.GetKeepSepInmateConflictDetails(inmateId);
            keepsep = keepsep.Where(x => inmateIds.Any(a => a == x.InmateId) && x.PeronId == personId).ToList();
            if (keepsep.Count == 0) return;
            TrackingConflictVm conflict = new TrackingConflictVm
            {
                ConflictType = ConflictTypeConstants.AssociationKeepSeparate,
                ConflictDescription = ConflictTypeConstants.AssociationKeepSeparateDescription,
            };
            _trackingConflictDetails.Add(conflict);
        }

        private void LoadVisitationRevokeConflict(int inmateId)
        {
            DateTime todayDate = DateTime.Now;

            List<InmatePrivilegeXref> inmatePrivilegeXref = _context.InmatePrivilegeXref.Where(i =>
                   i.Privilege.VisitationPrivilegeFlag == 1 && i.Privilege.InactiveFlag == 0 && i.PrivilegeDate.Value.Date <= todayDate.Date &&
                      (i.PrivilegeExpires.HasValue ? i.PrivilegeExpires.Value.Date.AddDays(1)
                          : DateTime.Now.Date.AddDays(1)) >= todayDate.Date
                    && !i.PrivilegeRemoveOfficerId.HasValue && i.InmateId == inmateId).ToList();

            if (inmatePrivilegeXref.Count == 0) return;
            TrackingConflictVm conflict = new TrackingConflictVm
            {
                ConflictType = ConflictTypeConstants.RevokedVisitation,
                ConflictDescription = ConflictTypeConstants.RevokedVisitationDescription,
            };
            _trackingConflictDetails.Add(conflict);
        }

        public List<TrackingConflictVm> GetInmateConflict(int inmateId, int personId)
        {
            LoadRejectVisitationforInmate(inmateId, personId);
            LoadVisitationRevokeConflict(inmateId);
            LoadVisitOfWeekConflict(inmateId);
            LoadVictimContact(inmateId, personId);
            LoadCustodyConflict(personId);
            LoadgangKeepSeperateConflict(inmateId, personId);
            LoadRevokeVisitNoteAcknowlege(inmateId);
            return _trackingConflictDetails;
        }

        #endregion

        #region Step 3 - Visit Scheduling
        public PrimaryVisitorVm GetSchedulePrimaryDetail(PrimaryVisitorVm objParam)
        {
            List<VisitorIdentityAndRelationship> visitorIdentityAndRelationships =
                GetSavedVisitorAndRelationshipDetails(objParam.ScheduleId, true);

            VisitSchedule visitSchedule = GetSavedScheduleDetails(objParam.ScheduleId);

            Visit visit = _context.VisitToVisitor.Where(w => w.ScheduleId == objParam.ScheduleId
                && w.PersonId == objParam.PersonId).Select(s => s.Visit).Single();


            InmateDetail inmateDetail = GetVisitInmateDetails(visit.InmateId ?? 0);

            VisitScheduledVm visitScheduleVm = new VisitScheduledVm
            {
                InmateId = visit.InmateId ?? 0,
                FrontDeskFlag = visit.FrontDeskFlag,
                VisitType = objParam.VisitType,
                VisitDate = !visit.FrontDeskFlag ? DateTime.Now : objParam.VisitDate ?? visit.StartDate
            };

            VisitScheduledDetail visitScheduledDetail = new VisitScheduledDetail();

            if (visitScheduleVm.VisitDate != null)
            {
                visitScheduledDetail = GetVisitRoomDetails(visitScheduleVm);
            }

            PrimaryVisitorVm primaryVisitorDetail = new PrimaryVisitorVm
            {
                InmateId = visit.InmateId ?? 0,
                FrontDeskFlag = visit.FrontDeskFlag,
                InmateDetail = inmateDetail,
                VisitScheduledDetail = visitScheduledDetail,
                VisitorIdentityAndRelationships = visitorIdentityAndRelationships,
                VisitSchedule = visitSchedule,
                VisitChild = GetChildVisitors(objParam.ScheduleId, 0)
            };

            return primaryVisitorDetail;
        }
        public InmateDetail GetVisitInmateDetails(int inmateId)
        {
            InmateDetail inmateDetail = _context.Inmate.Where(w => w.InmateId == inmateId)
                .Select(s => new InmateDetail
                {
                    InmateNumber = s.InmateNumber,
                    Person = new PersonVm
                    {
                        PersonId = s.PersonId,
                        PersonLastName = s.Person.PersonLastName,
                        PersonFirstName = s.Person.PersonFirstName,
                        PersonMiddleName = s.Person.PersonMiddleName
                    },
                    HousingUnit = new HousingDetail
                    {
                        FacilityAbbr = s.Facility.FacilityAbbr,
                        HousingUnitListId = s.HousingUnit.HousingUnitListId,
                        HousingUnitLocation = s.HousingUnit.HousingUnitLocation,
                        HousingUnitNumber = s.HousingUnit.HousingUnitNumber,
                        HousingUnitBedLocation = s.HousingUnit.HousingUnitBedLocation,
                        HousingUnitBedNumber = s.HousingUnit.HousingUnitBedNumber
                    },
                    InmateClassification = new InmateClassificationVm
                    {
                        ClassificationReason = s.InmateClassification.InmateClassificationReason
                    }
                }).SingleOrDefault();

            return inmateDetail;
        }

        private VisitSchedule GetSavedScheduleDetails(int scheduleId)
        {
            VisitSchedule visitSchedule = _context.VisitToVisitor.Where(w => w.ScheduleId == scheduleId)
                .Select(s => new VisitSchedule
                {
                    RoomLocationId = s.Visit.LocationId,
                    RoomLocation = s.Visit.Location.PrivilegeDescription,
                    VisitBoothId = s.Visit.VisitBooth,
                    StartDate = s.Visit.StartDate,
                    EndDate = s.Visit.EndDate,
                    ReasonId = s.Visit.ReasonId,
                    TypeId = s.Visit.TypeId,
                    VisitLocker = s.Visit.VisitorLocker,
                    Notes = s.Visit.Notes,
                    FrontDeskFlag = s.Visit.FrontDeskFlag,
                    VisitOpenScheduleFlag = s.Visit.Location.PrivilegeDescription != null && s.Visit.Location.VisitOpenScheduleFlag
                }).SingleOrDefault();

            if (visitSchedule?.VisitBoothId != null)
            {
                List<VisitScheduledBoothVm> visitScheduledBoothVm = _context.Lookup
                    .Where(w => w.LookupName == visitSchedule.RoomLocation && w.LookupInactive == 0)
                    .Select(s => new VisitScheduledBoothVm
                    {
                        Booth = s.LookupDescription,
                        BoothId = s.LookupIndex
                    }).ToList();
                visitSchedule.VisitBooth = visitScheduledBoothVm
                    .SingleOrDefault(w => Equals(w.BoothId, visitSchedule.VisitBoothId))?.Booth;
            }
            return visitSchedule;
        }

        public VisitScheduledDetail GetVisitRoomDetails(VisitScheduledVm objParam)
        {
            List<VisitScheduledVm> visitationRoomInfoFinal = new List<VisitScheduledVm>();
            HousingUnitList housingUnitList;
            HousingUnitVisitationClassRule classRule = new HousingUnitVisitationClassRule();

            InmateDetailVm inmateInfo = _context.Inmate.Where(s => s.InmateId == objParam.InmateId).Select(s =>
                new InmateDetailVm
                {
                    FacilityId = s.FacilityId,
                    HousingUnitId = s.HousingUnitId,
                    HousingUnitListId = s.HousingUnit.HousingUnitListId,
                    InmateClassificationId = s.InmateClassificationId,
                    Classification = s.InmateClassification.InmateClassificationReason
                }).Single();
            //Get only assigned location from housing.
            List<HousingUnitList> housingUnitLocationList = _context.HousingUnitList
                .Where(x => x.LocationIdList != null && x.FacilityId == inmateInfo.FacilityId)
                .Select(s => new HousingUnitList
                {
                    LocationIdList = s.LocationIdList,
                    HousingUnitListId = s.HousingUnitListId,
                    HousingUnitLocation = s.HousingUnitLocation,
                    HousingUnitNumber = s.HousingUnitNumber
                }).ToList();
            List<string> lst = housingUnitLocationList.Select(x => x.LocationIdList).ToList();
            string loc = string.Join(",", lst);

            int[] location = !string.IsNullOrEmpty(loc) ? loc.Split(",").ToArray().Select(int.Parse).ToArray() : null;

            List<VisitScheduledVm> visitationRoomInfo = _context.Privileges
                .Where(w => inmateInfo.FacilityId == w.FacilityId && w.ShowInVisitation && w.InactiveFlag == 0
                    && (!objParam.VisitType ? !w.VisitAllowProfOnlyFlag : !w.VisitAllowPersonalOnlyFlag)
                    && w.RemoveFromPrivilegeFlag == 1 && w.RemoveFromTrackingFlag == 0 && location != null &&
                    location.Contains(w.PrivilegeId))
                .Select(s => new VisitScheduledVm
                {
                    VisitOpenScheduleFlag = s.VisitOpenScheduleFlag,
                    VisitationRoom = s.PrivilegeDescription,
                    VisitDate = objParam.VisitDate,
                    PrivilegeId = s.PrivilegeId,
                    RoomCapacity = s.Capacity ?? 0,
                }).ToList();

            int[] unitListId = housingUnitLocationList.Select(x => x.HousingUnitListId).ToArray();

            //Get schedule for assigned housing.
            List<HousingUnitVisitation> housingUnitVisitation = _context.HousingUnitVisitation.Where(v =>
                    v.FacilityId == inmateInfo.FacilityId && unitListId.Contains(v.HousingUnitListId ?? 0) &&
                    (!v.DeleteFlag.HasValue || v.DeleteFlag == 0) &&
                    objParam.VisitDate.Value.Date.DayOfWeek.ToString() == v.VisitationDay)
                .Select(s => new HousingUnitVisitation
                {
                    HousingUnitListId = s.HousingUnitListId,
                    HousingUnitLocation = s.HousingUnitLocation,
                    HousingUnitNumber = s.HousingUnitNumber,
                    VisitationFrom = s.VisitationFrom,
                    VisitationTo = s.VisitationTo
                }).ToList();

            //Get Registered visit to calculate pending and progress and estimate wait.
            List<VisitToVisitor> visitCount = _context.VisitToVisitor.Where(v =>
                    v.Visit.VisitDenyFlag != 1 && !v.Visit.DeleteFlag &&
                    v.Visit.StartDate.Date == objParam.VisitDate.Value.Date &&
                    v.Visit.CompleteRegistration && !v.Visit.CompleteVisitFlag)
                .Select(s => new VisitToVisitor
                {
                    Visit = s.Visit
                }).ToList();

            if (inmateInfo.HousingUnitId != null)
            {
                housingUnitList = _context.Inmate.Where(w => w.InmateId == objParam.InmateId)
                    .Select(s => new HousingUnitList
                    {
                        HousingUnitListId = s.HousingUnit.HousingUnitList.HousingUnitListId,
                        LocationIdList = s.HousingUnit.HousingUnitList.LocationIdList
                    }).SingleOrDefault();

                if (housingUnitList?.LocationIdList != null)
                {
                    int[] locationList = housingUnitList.LocationIdList.Split(",").ToArray().Select(int.Parse).ToArray();

                    visitationRoomInfo.ForEach(i =>
                    {
                        if (!locationList.Contains(i.PrivilegeId)) return;
                        i.HousingUnitId = inmateInfo.HousingUnitId ?? 0;
                        i.HousingUnitListId = inmateInfo.HousingUnitListId ?? 0;
                    });
                }

                if (housingUnitList?.HousingUnitListId != null && inmateInfo.InmateClassificationId != null)
                {
                    classRule = _context.HousingUnitVisitationClassRule.SingleOrDefault(w =>
                        w.HousingUnitListId == housingUnitList.HousingUnitListId &&
                        w.ClassificationReason == inmateInfo.Classification);
                }

            }
            //Open Personal And prof
            visitationRoomInfo.ForEach(item =>
            {
                if (item.VisitOpenScheduleFlag)
                {
                    VisitScheduledVm housingUnitVisitation2 = new VisitScheduledVm
                    {
                        VisitationRoom = item.VisitationRoom,
                        VisitOpenScheduleFlag = item.VisitOpenScheduleFlag,
                        VisitDate = item.VisitDate,
                        FrontDeskFlag = item.FrontDeskFlag,
                        HousingUnitId = item.HousingUnitId,
                        HousingUnitListId = item.HousingUnitListId,
                        PrivilegeId = item.PrivilegeId,
                        RoomCapacity = item.RoomCapacity,
                        VisitsPending = visitCount.Count(s =>
                            !s.Visit.InmateTrackingStart.HasValue && item.PrivilegeId == s.Visit.LocationId),
                        VisitsInProgress = visitCount.Count(s =>
                            s.Visit.InmateTrackingStart.HasValue && item.PrivilegeId == s.Visit.LocationId)
                    };

                    if (objParam.VisitDate != null && (item.RoomCapacity ?? 0) - housingUnitVisitation2.VisitsPending -
                        housingUnitVisitation2.VisitsInProgress < 0 && objParam.VisitDate.Value.Date == DateTime.Now.Date)
                    {
                        List<Visit> pending = visitCount.Where(s =>
                               !s.Visit.InmateTrackingStart.HasValue && item.PrivilegeId == s.Visit.LocationId).Select(e => e.Visit).ToList();
                        List<Visit> progress = visitCount.Where(s =>
                            s.Visit.InmateTrackingStart.HasValue && item.PrivilegeId == s.Visit.LocationId).Select(e => e.Visit).ToList();
                        housingUnitVisitation2.Rules = CalculateEstimate(pending, progress);
                    }
                    visitationRoomInfoFinal.Add(housingUnitVisitation2);
                }
            });
            // Slot Personal
            if (!objParam.VisitType)
            {
                List<HousingUnitList> lstHousingUnit = new List<HousingUnitList>();

                housingUnitLocationList.ForEach(i =>
                {
                    if (i.LocationIdList != null)
                    {
                        string data = string.Join(",", i.LocationIdList);
                        List<string> hoc = data.Split(",").ToList();
                        hoc.ForEach(s =>
                        {
                            HousingUnitList housingUnit = new HousingUnitList
                            {
                                LocationIdList = s,
                                HousingUnitLocation = i.HousingUnitLocation,
                                HousingUnitNumber = i.HousingUnitNumber,
                                HousingUnitListId = i.HousingUnitListId
                            };
                            lstHousingUnit.Add(housingUnit);
                        });
                    }
                });

                visitationRoomInfo.ForEach(item =>
                {
                    if (!item.VisitOpenScheduleFlag)
                    {
                        List<HousingUnitList> lstUnit = lstHousingUnit.Where(s => s.LocationIdList == item.PrivilegeId.ToString())
                            .ToList();
                        lstUnit.ForEach(d =>
                        {
                            housingUnitVisitation.ForEach(val =>
                            {
                                if (val.HousingUnitLocation.ToUpper().Trim() == d.HousingUnitLocation.ToUpper().Trim() &&
                                    val.HousingUnitNumber.Trim() == d.HousingUnitNumber.Trim())
                                {

                                    VisitScheduledVm housingUnitVisitation1 = new VisitScheduledVm
                                    {
                                        VisitationRoom = item.VisitationRoom,
                                        VisitOpenScheduleFlag = item.VisitOpenScheduleFlag,
                                        VisitDate = item.VisitDate,
                                        FrontDeskFlag = item.FrontDeskFlag,
                                        PrivilegeId = item.PrivilegeId,
                                        RoomCapacity = item.RoomCapacity,
                                        HousingUnitLocation = d.HousingUnitLocation,
                                        HousingUnitNumber = d.HousingUnitNumber
                                    };
                                    if (objParam.VisitDate != null && val.VisitationFrom != null && val.VisitationTo != null)
                                    {
                                        DateTime? from = objParam.VisitDate.Value.Date;
                                        DateTime? to = objParam.VisitDate.Value.Date;
                                        housingUnitVisitation1.FromTime = from.Value
                                            .AddHours(val.VisitationFrom.Value.Hour)
                                            .AddMinutes(val.VisitationFrom.Value.Minute);
                                        housingUnitVisitation1.ToTime = to.Value
                                            .AddHours(val.VisitationTo.Value.Hour)
                                            .AddMinutes(val.VisitationTo.Value.Minute);
                                    }

                                    housingUnitVisitation1.HousingUnitId = item.HousingUnitListId == val.HousingUnitListId
                                        ? item.HousingUnitId
                                        : 0;
                                    housingUnitVisitation1.HousingUnitListId =
                                        item.HousingUnitListId == val.HousingUnitListId
                                            ? item.HousingUnitListId
                                            : 0;

                                    housingUnitVisitation1.VisitsPending = visitCount.Count(s =>
                                        s.Visit.EndDate != null && housingUnitVisitation1.FromTime != null &&
                                        housingUnitVisitation1.ToTime != null &&
                                        item.PrivilegeId == s.Visit.LocationId && s.Visit.StartDate >= DateTime.Now &&
                                        housingUnitVisitation1.FromTime.Value.Date == s.Visit.StartDate.Date &&
                                        housingUnitVisitation1.FromTime.Value.ToShortTimeString() ==
                                        s.Visit.StartDate.ToShortTimeString() &&
                                        housingUnitVisitation1.ToTime.Value.ToShortTimeString() ==
                                        s.Visit.EndDate.Value.ToShortTimeString());

                                    housingUnitVisitation1.VisitsInProgress = visitCount.Count(s =>
                                        s.Visit.EndDate != null && housingUnitVisitation1.ToTime != null &&
                                        housingUnitVisitation1.FromTime != null &&
                                        item.PrivilegeId == s.Visit.LocationId && s.Visit.StartDate < DateTime.Now &&
                                        housingUnitVisitation1.FromTime.Value.Date == s.Visit.StartDate.Date &&
                                        housingUnitVisitation1.FromTime.Value.ToShortTimeString() ==
                                        s.Visit.StartDate.ToShortTimeString() &&
                                        housingUnitVisitation1.ToTime.Value.ToShortTimeString() ==
                                        s.Visit.EndDate.Value.ToShortTimeString());

                                    visitationRoomInfoFinal.Add(housingUnitVisitation1);
                                }
                            });
                        });
                    }
                });
            }
            //Slot Professional 
            List<VisitScheduledVm> slotProfRoomInfoFinal = new List<VisitScheduledVm>();
            if (objParam.VisitType)
            {
                List<VisitToVisitor> schedule = _context.VisitToVisitor.Where(x => x.Visit.CompleteRegistration &&
                        x.Visit.VisitorType == 1 && !x.Visit.CompleteVisitFlag)
                    .Select(x => new VisitToVisitor
                    {
                        PersonId = x.PersonId,
                        Visit = x.Visit
                    }).ToList();

                List<Visit> profVisit = schedule.Where(v =>
                    objParam.VisitDate != null && v.Visit.VisitDenyFlag != 1 && !v.Visit.DeleteFlag &&
                    v.Visit.StartDate.Date == objParam.VisitDate.Value.Date && v.Visit.CompleteRegistration &&
                    !v.Visit.CompleteVisitFlag).Select(x => x.Visit).ToList();

                visitationRoomInfo.ForEach(item =>
                {
                    VisitScheduledVm housingUnitVisitation2 = new VisitScheduledVm
                    {
                        VisitationRoom = item.VisitationRoom,
                        VisitOpenScheduleFlag = item.VisitOpenScheduleFlag,
                        VisitDate = item.VisitDate,
                        FrontDeskFlag = item.FrontDeskFlag,
                        HousingUnitId = item.HousingUnitId,
                        HousingUnitListId = item.HousingUnitListId,
                        PrivilegeId = item.PrivilegeId,
                        RoomCapacity = item.RoomCapacity,
                        IsSlotProf = true
                    };
                    slotProfRoomInfoFinal.Add(housingUnitVisitation2);
                    profVisit.ForEach(val =>
                    {
                        VisitScheduledVm housingUnitVisitation1 = new VisitScheduledVm
                        {
                            VisitationRoom = item.VisitationRoom,
                            VisitOpenScheduleFlag = item.VisitOpenScheduleFlag,
                            VisitDate = item.VisitDate,
                            FrontDeskFlag = item.FrontDeskFlag,
                            HousingUnitId = item.HousingUnitId,
                            HousingUnitListId = item.HousingUnitListId,
                            PrivilegeId = item.PrivilegeId,
                            RoomCapacity = item.RoomCapacity,
                            FromTime = val.StartDate,
                            ToTime = val.EndDate
                        };
                        housingUnitVisitation1.VisitsPending = profVisit.Count(s =>
                            housingUnitVisitation1.FromTime != null && !s.InmateTrackingStart.HasValue &&
                            item.PrivilegeId == s.LocationId &&
                            housingUnitVisitation1.FromTime.Value.ToShortTimeString() ==
                            s.StartDate.ToShortTimeString());

                        housingUnitVisitation1.VisitsInProgress = profVisit.Count(s =>
                            s.EndDate != null && housingUnitVisitation1.ToTime != null &&
                            s.InmateTrackingStart.HasValue && item.PrivilegeId == s.LocationId &&
                            housingUnitVisitation1.ToTime.Value.ToShortTimeString() ==
                            s.EndDate.Value.ToShortTimeString());

                        if (housingUnitVisitation1.VisitsPending > 0 || housingUnitVisitation1.VisitsInProgress > 0)
                            slotProfRoomInfoFinal.Add(housingUnitVisitation1);
                    });
                });
            }
            VisitScheduledDetail visitScheduledDetail = new VisitScheduledDetail
            {
                NoRoom = !visitationRoomInfo.Any(),
                IsVisitDeny = GetVisitDenyFlag(),
                VisitRuleMaxLengthMin = classRule?.VisitRuleMaxLengthMin ?? 0,
                VisitationRoomInfo = visitationRoomInfoFinal,
                SlotProfRoomInfo = slotProfRoomInfoFinal
            };

            return visitScheduledDetail;
        }
        private List<VisitClassRule> CalculateEstimate(List<Visit> pending, List<Visit> progress)
        {
            List<VisitClassRule> visitClassRule = new List<VisitClassRule>();
            int[] lstInmate = pending.Select(x => x.InmateId ?? 0).ToArray();
            int[] lstInmates = progress.Select(x => x.InmateId ?? 0).ToArray();
            List<InmateDetailVm> inmateDetail = _context.Inmate.Where(s => lstInmate.Contains(s.InmateId) && s.HousingUnitId != null || lstInmates.Contains(s.InmateId)).Select(s =>
                new InmateDetailVm
                {
                    InmateId = s.InmateId,
                    HousingUnitListId = s.HousingUnit.HousingUnitList.HousingUnitListId,
                    InmateClassificationId = s.InmateClassificationId,
                    Classification = s.InmateClassification.InmateClassificationReason
                }).ToList();
            int[] unitId = inmateDetail.Select(x => x.HousingUnitListId ?? 0).ToArray();
            List<HousingUnitVisitationClassRule> rules = _context.HousingUnitVisitationClassRule
                .Where(x => unitId.Contains(x.HousingUnitListId ?? 0)).ToList();
            pending.ForEach(n =>
            {
                InmateDetailVm detail = inmateDetail.SingleOrDefault(s => s.InmateId == n.InmateId);
                if (detail != null)
                {
                    int classRule = rules.SingleOrDefault(w =>
                                        w.HousingUnitListId == detail.HousingUnitListId &&
                                        w.ClassificationReason == detail.Classification)?.VisitRuleMaxLengthMin ?? 0;
                    VisitClassRule rule = new VisitClassRule
                    {
                        InmateId = detail.InmateId,
                        ClassRule = classRule,
                        IsPending = true
                    };
                    visitClassRule.Add(rule);
                }
            });
            progress.ForEach(n =>
            {
                InmateDetailVm detail = inmateDetail.SingleOrDefault(s => s.InmateId == n.InmateId);
                if (detail != null)
                {
                    int classRule = rules.SingleOrDefault(w =>
                                        w.HousingUnitListId == detail.HousingUnitListId &&
                                        w.ClassificationReason == detail.Classification)?.VisitRuleMaxLengthMin ?? 0;

                    VisitClassRule rule = new VisitClassRule
                    {
                        InmateId = detail.InmateId,
                        ClassRule = classRule,
                        StartDate = n.InmateTrackingStart
                    };
                    visitClassRule.Add(rule);
                }
            });
            return visitClassRule;
        }
        public VisitScheduledBoothDetail GetVisitBoothDetails(VisitScheduledBoothVm objParam)
        {
            List<VisitScheduledBoothVm> visitScheduledBoothVm = _context.Lookup
                .Where(w => w.LookupName == objParam.RoomName && w.LookupInactive == 0)
                .Select(s => new VisitScheduledBoothVm
                {
                    Booth = s.LookupDescription,
                    BoothId = s.LookupIndex
                }).ToList();

            int[] boothId = visitScheduledBoothVm.Select(s => s.BoothId).ToArray();

            List<VisitScheduledBoothVm> boothDetail = _context.VisitToVisitor.Where(v =>
                    boothId.Contains(v.Visit.VisitBooth ?? 0) && v.Visit.VisitDenyFlag != 1 && !v.Visit.DeleteFlag &&
                    v.Visit.StartDate.Date == objParam.VisitDate.Value.Date && !v.Visit.CompleteVisitFlag &&
                    v.Visit.Location.PrivilegeDescription == objParam.RoomName)
                .Select(s => new VisitScheduledBoothVm
                {
                    FromDateTime = s.Visit.StartDate,
                    ToDateTime = s.Visit.EndDate,
                    BoothId = s.Visit.VisitBooth ?? 0,
                    InmateId = s.Visit.InmateId ?? 0,
                    InmateTrackingStart = s.Visit.InmateTrackingStart,
                    OpenScheduleFlag = s.Visit.Location.VisitOpenScheduleFlag
                }).ToList();
            List<VisitScheduledBoothVm> slotBooth =
                boothDetail.Where(s => !s.OpenScheduleFlag && s.ToDateTime > DateTime.Now).ToList();

            boothDetail = boothDetail.Where(s => s.OpenScheduleFlag).ToList();

            boothDetail.AddRange(slotBooth);
            visitScheduledBoothVm.ForEach(item =>
            {
                VisitScheduledBoothVm visitScheduledBooth = boothDetail.FirstOrDefault(f => f.BoothId == item.BoothId);
                if (visitScheduledBooth != null && visitScheduledBooth.OpenScheduleFlag)
                {
                    item.FromDateTime = visitScheduledBooth.FromDateTime;
                    item.ToDateTime = visitScheduledBooth.ToDateTime;
                    item.InmateTrackingStart = visitScheduledBooth.InmateTrackingStart;
                    item.InmateDetail = GetVisitInmateDetails(visitScheduledBooth.InmateId);
                }
                else if (visitScheduledBooth != null && !visitScheduledBooth.OpenScheduleFlag && visitScheduledBooth.ToDateTime > DateTime.Now)
                {
                    item.FromDateTime = visitScheduledBooth.FromDateTime;
                    item.ToDateTime = visitScheduledBooth.ToDateTime;
                    item.InmateDetail = GetVisitInmateDetails(visitScheduledBooth.InmateId);
                }
            });
            int facilityId = _context.Inmate.First(s => s.InmateId == objParam.InmateId).FacilityId;

            VisitScheduledBoothDetail visitScheduledBoothDetail = _context.Privileges
                .Where(w => w.PrivilegeDescription == objParam.RoomName && w.FacilityId== facilityId)
                .Select(p => new VisitScheduledBoothDetail
                {
                    VisitBoothManageFlag = p.VisitBoothManageFlag,
                    VisitBoothAssignRoomFlag = p.VisitBoothAssignRoomFlag,
                    VisitationBoothInfo = visitScheduledBoothVm.OrderBy(o => o.FromDateTime).ToList(),
                }).Single();

            return visitScheduledBoothDetail;
        }
        public async Task<int> UpdateRoomBoothDetails(VisitSchedule objParam)
        {
            objParam.RoomLocationId = _context.Privileges.FirstOrDefault(w => w.PrivilegeId == objParam.RoomLocationId)?.PrivilegeId;

            Visit visit = _context.Visit.Single(w => w.ScheduleId == objParam.ScheduleId);

            if (visit != null)
            {
                visit.LocationId = objParam.RoomLocationId;
                visit.StartDate = objParam.StartDate;
                visit.EndDate = objParam.EndDate;
                visit.ReasonId = objParam.ReasonId;
                visit.TypeId = objParam.TypeId;
                visit.VisitorLocker = objParam.VisitLocker;
                visit.Notes = objParam.Notes;
                visit.VisitBooth = objParam.VisitBoothId;
                visit.FrontDeskFlag = objParam.FrontDeskFlag;

                if(objParam.isBumpQueue){
                    visit.BumpNewVisitId = objParam.ScheduleId;
                    visit.BumpClearFlag = 1;
                    visit.BumpClearBy = _personnelId;
                    visit.BumpClearDate = DateTime.Now;                    
                }                
            }

            if (objParam.TrackingConflict.Count > 0)
            {
                objParam.TrackingConflict.ForEach(item =>
                {
                    string note = $"{FloorNotesConflictConstants.TYPE} {item.ConflictType} " +
                        $"{FloorNotesConflictConstants.DESCRIPTION} {item.ConflictDescription}";
                    if (visit != null) InsertFloorNote(note, visit.InmateId ?? 0);
                });
            }
            if (visit != null)
            {
                InsertRescheduleVisit(objParam);
            }
           
            return await _context.SaveChangesAsync();
        }
        public List<TrackingConflictVm> SchedulingConflict(VisitSchedule visitSchedule)
        {
            List<TrackingConflictVm> trackingConflict = new List<TrackingConflictVm>();
            List<ScheduleInmate> sameLocApp = new List<ScheduleInmate>();
            List<ScheduleInmate> diffLocApp = new List<ScheduleInmate>();
            List<VisitToVisitor> sameVisitLocApp = new List<VisitToVisitor>();
            List<VisitToVisitor> diffVisitLocApp = new List<VisitToVisitor>();
            List<string> locDetail = new List<string>();
            Privileges privilege = _context.Privileges.Where(w => w.PrivilegeId == visitSchedule.RoomLocationId).Select(
                s => new Privileges
                {
                    Capacity = s.Capacity ?? 0,
                    AppointmentHierarchyId = s.AppointmentHierarchyId ?? 0
                }).First();

            bool genderConflict = _context.Privileges.First(w => w.PrivilegeId == visitSchedule.RoomLocationId)
                                       .RemoveCoflictCheckGender == 1;
            bool keepSepConflict = _context.Privileges.First(w => w.PrivilegeId == visitSchedule.RoomLocationId)
                                       .RemoveCoflictCheckKeepSep == 1;
            bool housingLockConflict = _context.Privileges.First(w => w.PrivilegeId == visitSchedule.RoomLocationId)
                                       .ConflictCheckRoomPrivilege == 1;
            if (!keepSepConflict)
            {
                ICollection<int> id = new List<int> { visitSchedule.InmateId };
                List<TrackingConflictVm> trackingConflictDetail = LoadKeepSeparateConflict(id, visitSchedule.RoomLocationId ?? 0);
                string inmateNumber = string.Empty;

                if (trackingConflictDetail.Count > 0)
                {
                    trackingConflictDetail.ForEach(s =>
                    {
                        inmateNumber = string.Join(",", s.KeepSepInmateNumber);
                    });
                    TrackingConflictVm conflict = new TrackingConflictVm
                    {
                        KeepSepInmateNumber = inmateNumber,
                        ConflictType = ConflictTypeConstants.KEEPSEPARATECONFLICT,
                        ConflictDescription = ConflictTypeConstants.DIFFERENTGENDERANDLOCATION
                    };
                    trackingConflict.Add(conflict);
                }
            }
            if (!genderConflict)
            {
                int personId = _context.Inmate.First(x => x.InmateId == visitSchedule.InmateId).PersonId;
                int type = _context.PersonDescription.Where(x => x.PersonId == personId)
                    .Select(s => s.Person.PersonSexLast ?? 0).FirstOrDefault();
                if (type > 0)
                {
                    IQueryable<VisitToVisitor> visits = _context.VisitToVisitor;
                    //Time Overlap
                    List<VisitToVisitor> lstTimeOverlaps = visits.Where(v =>
                            v.Visit.StartDate.Date == visitSchedule.StartDate.Date &&
                            v.Visit.EndDate.Value.Date == visitSchedule.EndDate.Value.Date &&
                            visitSchedule.StartDate < v.Visit.EndDate && visitSchedule.EndDate >= v.Visit.EndDate ||
                            visitSchedule.StartDate <= v.Visit.StartDate && visitSchedule.EndDate > v.Visit.StartDate ||
                            visitSchedule.StartDate == v.Visit.StartDate && visitSchedule.EndDate == v.Visit.EndDate ||
                            visitSchedule.StartDate > v.Visit.StartDate && visitSchedule.EndDate < v.Visit.EndDate)
                        .Select(x => new VisitToVisitor
                        {
                            PersonId = x.PersonId,
                            ScheduleId = x.ScheduleId,
                            Visit = x.Visit
                        }).ToList();

                    lstTimeOverlaps = lstTimeOverlaps.Where(s =>
                        s.ScheduleId != visitSchedule.ScheduleId && s.Visit.InmateId != null &&
                        s.Visit.LocationId==visitSchedule.RoomLocationId &&
                        s.Visit.CompleteRegistration && !s.Visit.CompleteVisitFlag &&
                        s.Visit.VisitDenyFlag != 1 && !s.Visit.DeleteFlag).ToList();

                    int[] inmateId = lstTimeOverlaps.Select(s => s.Visit.InmateId??0).ToArray();

                     List<Inmate> gender = _context.Inmate.Where(s =>
                         inmateId.Contains(s.InmateId) && (type == (int) Gender.Male ||
                                                           type == (int) Gender.Female) && (type == (int) Gender.Male
                             ? s.Person.PersonSexLast == (int) Gender.Female
                             : s.Person.PersonSexLast == (int) Gender.Male)).ToList();
                     if (gender.Any())
                     {
                         TrackingConflictVm conflict = new TrackingConflictVm
                         {
                             ConflictType = ConflictTypeConstants.GENDERCONFLICT,
                             ConflictDescription = ConflictTypeConstants.DIFFERENTGENDERANDLOCATION
                         };
                         trackingConflict.Add(conflict);
                     }
                }
            }

            if (housingLockConflict)
            {
                Inmate inmate = _context.Inmate.Single(si => si.InmateId == visitSchedule.InmateId);
                if (inmate.HousingUnitId != null || inmate.HousingUnitId != null)
                {
                    List<TrackingConflictVm> trackingConflictDetail = VisitHousingLockDownConflict(inmate,visitSchedule);
                    if (trackingConflictDetail.Count > 0)
                    {
                        TrackingConflictVm conflict = new TrackingConflictVm
                        {
                            HousingInfo = trackingConflictDetail[0].HousingInfo,
                            ConflictType = ConflictTypeConstants.HOUSINGLOCKDOWNCONFLICT,
                            ConflictDescription = ConflictTypeConstants.HOUSINGLOCKDOWNDESCRIPTION
                        };
                        trackingConflict.Add(conflict);
                    }
                }

            }
            if (visitSchedule.VisitOpenScheduleFlag)
            {
                int visitCount = _context.VisitToVisitor.Count(v =>
                    v.Visit.InmateTrackingStart.HasValue && v.Visit.LocationId == visitSchedule.RoomLocationId &&
                    v.Visit.StartDate.Date == visitSchedule.StartDate.Date && v.Visit.VisitDenyFlag != 1 &&
                    !v.Visit.DeleteFlag &&
                    v.Visit.CompleteRegistration && !v.Visit.CompleteVisitFlag);
                if (privilege.Capacity <= visitCount)
                {
                    TrackingConflictVm conflict = new TrackingConflictVm
                    {
                        ConflictType = ConflictTypeConstants.CAPACITYCONFLICT,
                        ConflictDescription = ConflictTypeConstants.CAPACITYOVERCONFLICT
                    };
                    trackingConflict.Add(conflict);
                }

            }
            if (!visitSchedule.VisitOpenScheduleFlag)
            {
                int[] person;
                List<VisitToVisitor> schedule = _context.VisitToVisitor.Where(v =>
                        v.Visit.LocationId == visitSchedule.RoomLocationId &&
                        v.Visit.VisitDenyFlag != 1 && !v.Visit.DeleteFlag &&
                        v.Visit.CompleteRegistration && !v.Visit.CompleteVisitFlag)
                    .Select(s => new VisitToVisitor
                    {
                        PersonId = s.PersonId,
                        Visit = s.Visit
                    }).ToList();
                if (!visitSchedule.VisitType)
                {
                    schedule = schedule.Where(s => !s.Visit.VisitorType.HasValue).ToList();

                    person = schedule.Select(x => x.PersonId).ToArray();
                }
                else
                {
                    schedule = schedule.Where(s => s.Visit.VisitorType == 1).ToList();
                    person = schedule.Select(x => x.PersonId).ToArray();
                }

                int count = schedule.Count(s => s.Visit.EndDate != null && visitSchedule.EndDate != null &&
                    person.Contains(s.PersonId) && s.Visit.StartDate.Date ==
                    visitSchedule.StartDate.Date && visitSchedule.StartDate.ToShortTimeString() ==
                    s.Visit.StartDate.ToShortTimeString() && visitSchedule.EndDate.Value
                        .ToShortTimeString() == s.Visit.EndDate.Value.ToShortTimeString());
                if (privilege.Capacity <= count)
                {
                    TrackingConflictVm conflict = new TrackingConflictVm
                    {
                        ConflictType = ConflictTypeConstants.CAPACITYCONFLICT,
                        ConflictDescription = ConflictTypeConstants.CAPACITYOVERCONFLICT
                    };
                    trackingConflict.Add(conflict);
                }
            }

            //Overlap Event Conflict for appointment
            IQueryable<ScheduleInmate> appointment = _context.Schedule.OfType<ScheduleInmate>().Where(s => !(s is Visit));
            //Time Overlap
            List<ScheduleInmate> lstApp = appointment.Where(v =>
                v.ScheduleId != visitSchedule.ScheduleId && visitSchedule.EndDate != null && v.EndDate != null &&
                !v.DeleteFlag && v.InmateId == visitSchedule.InmateId &&
                v.StartDate.Date == visitSchedule.StartDate.Date &&
                v.EndDate.Value.Date == visitSchedule.EndDate.Value.Date && visitSchedule.StartDate < v.EndDate &&
                visitSchedule.EndDate >= v.EndDate ||
                visitSchedule.StartDate <= v.StartDate && visitSchedule.EndDate > v.StartDate ||
                visitSchedule.StartDate == v.StartDate && visitSchedule.EndDate == v.EndDate ||
                visitSchedule.StartDate > v.StartDate && visitSchedule.EndDate < v.EndDate).Select(n =>
                new ScheduleInmate
                {
                    LocationId = n.LocationId,
                    ScheduleId = n.ScheduleId,
                    InmateId = n.InmateId
                }).ToList();
            //Priority Overlap
            List<ScheduleInmate> priorityApp = appointment.Where(v =>
                v.ScheduleId != visitSchedule.ScheduleId && visitSchedule.EndDate != null &&
                v.EndDate != null && !v.DeleteFlag && v.InmateId == visitSchedule.InmateId &&
                v.StartDate.Date == visitSchedule.StartDate.Date &&
                v.EndDate.Value.Date == visitSchedule.EndDate.Value.Date).Select(n => new ScheduleInmate
                {
                    LocationId = n.LocationId,
                    ScheduleId = n.ScheduleId,
                    InmateId = n.InmateId
                }).ToList();

            int?[] priIds = lstApp.Select(s => s.LocationId).ToArray();
            priIds = priIds.Concat(priorityApp.Select(s => s.LocationId).ToArray()).ToArray();

            List<Privileges> pri = _context.Privileges.Where(s => priIds.Contains(s.PrivilegeId)).ToList();

            lstApp.ForEach(item =>
            {
                if (item.LocationId == visitSchedule.RoomLocationId &&
                    item.ScheduleId != visitSchedule.ScheduleId && item.InmateId == visitSchedule.InmateId)
                {
                    sameLocApp.Add(item);
                }
                else if (item.LocationId != visitSchedule.RoomLocationId &&
                    item.ScheduleId != visitSchedule.ScheduleId && item.InmateId == visitSchedule.InmateId)
                {
                    diffLocApp.Add(item);
                }
            });
            priorityApp.ForEach(item =>
            {
                Privileges privileges = pri.FirstOrDefault(s => s.PrivilegeId == item.LocationId);
                int hired = privileges?.AppointmentHierarchyId ?? 0;
                if (hired > 0 && privilege.AppointmentHierarchyId > 0 &&
                    hired < privilege.AppointmentHierarchyId && item.LocationId == visitSchedule.RoomLocationId &&
                    item.ScheduleId != visitSchedule.ScheduleId && item.InmateId == visitSchedule.InmateId ||
                    hired > 0 && privilege.AppointmentHierarchyId == 0 &&
                    item.LocationId == visitSchedule.RoomLocationId &&
                    item.ScheduleId != visitSchedule.ScheduleId && item.InmateId == visitSchedule.InmateId)
                {
                    sameLocApp.Add(item);
                }
                else if (hired > 0 && privilege.AppointmentHierarchyId > 0 &&
                    hired < privilege.AppointmentHierarchyId &&
                    item.LocationId != visitSchedule.RoomLocationId &&
                    item.ScheduleId != visitSchedule.ScheduleId && item.InmateId == visitSchedule.InmateId ||
                         hired > 0 && privilege.AppointmentHierarchyId == 0 &&
                         item.LocationId != visitSchedule.RoomLocationId &&
                         item.ScheduleId != visitSchedule.ScheduleId && item.InmateId == visitSchedule.InmateId)
                {
                    diffLocApp.Add(item);
                }
            });
            //  Overlap Event Conflict for Visit
            IQueryable<VisitToVisitor> visit = _context.VisitToVisitor;
            //Time Overlap
            List<VisitToVisitor> lstTimeOverlap = visit.Where(v =>
                    v.Visit.StartDate.Date == visitSchedule.StartDate.Date &&
                    v.Visit.InmateId == visitSchedule.InmateId &&
                    v.Visit.EndDate.Value.Date == visitSchedule.EndDate.Value.Date &&
                    visitSchedule.StartDate < v.Visit.EndDate && visitSchedule.EndDate >= v.Visit.EndDate ||
                    visitSchedule.StartDate <= v.Visit.StartDate && visitSchedule.EndDate > v.Visit.StartDate ||
                    visitSchedule.StartDate == v.Visit.StartDate && visitSchedule.EndDate == v.Visit.EndDate ||
                    visitSchedule.StartDate > v.Visit.StartDate && visitSchedule.EndDate < v.Visit.EndDate)
                .Select(x => new VisitToVisitor
                {
                    PersonId = x.PersonId,
                    ScheduleId = x.ScheduleId,
                    Visit = x.Visit
                }).ToList();
            //Priority Overlap
            List<VisitToVisitor> lstPriorityOverlap = _context.VisitToVisitor.Where(v =>
                    v.Visit.StartDate.Date == visitSchedule.StartDate.Date &&
                    v.Visit.InmateId == visitSchedule.InmateId &&
                    v.Visit.EndDate.Value.Date == visitSchedule.EndDate.Value.Date)
                .Select(x => new VisitToVisitor
                {
                    PersonId = x.PersonId,
                    ScheduleId = x.ScheduleId,
                    Visit = x.Visit
                }).ToList();
            lstTimeOverlap = lstTimeOverlap.Where(s =>
                s.ScheduleId != visitSchedule.ScheduleId && s.Visit.InmateId != null &&
                s.Visit.InmateId == visitSchedule.InmateId &&
                s.Visit.CompleteRegistration && !s.Visit.CompleteVisitFlag &&
                s.Visit.VisitDenyFlag != 1 && !s.Visit.DeleteFlag).ToList();

            lstPriorityOverlap = lstPriorityOverlap.Where(s =>
                s.ScheduleId != visitSchedule.ScheduleId && s.Visit.InmateId != null &&
                s.Visit.InmateId == visitSchedule.InmateId &&
                s.Visit.CompleteRegistration && !s.Visit.CompleteVisitFlag &&
                s.Visit.VisitDenyFlag != 1 && !s.Visit.DeleteFlag).ToList();
            int?[] priorityIds = lstPriorityOverlap.Select(s => s.Visit.LocationId).ToArray();
            List<Privileges> priority = _context.Privileges.Where(s => priorityIds.Contains(s.PrivilegeId)).ToList();
            lstTimeOverlap.ForEach(item =>
            {
                if (item.Visit.LocationId == visitSchedule.RoomLocationId)
                {
                    sameVisitLocApp.Add(item);
                }
                else if (item.Visit.LocationId != visitSchedule.RoomLocationId)
                {
                    diffVisitLocApp.Add(item);
                }
            });
            lstPriorityOverlap.ForEach(item =>
            {
                Privileges privileges = priority.FirstOrDefault(s => s.PrivilegeId == item.Visit.LocationId);
                int hired = privileges?.AppointmentHierarchyId ?? 0;
                if (hired > 0 && privilege.AppointmentHierarchyId > 0 && hired < privilege.AppointmentHierarchyId &&
                    item.Visit.LocationId == visitSchedule.RoomLocationId ||
                    hired > 0 && privilege.AppointmentHierarchyId == 0 &&
                    item.Visit.LocationId == visitSchedule.RoomLocationId)
                {
                    sameVisitLocApp.Add(item);
                }
                else if (hired > 0 && privilege.AppointmentHierarchyId > 0 &&
                    hired < privilege.AppointmentHierarchyId &&
                    item.Visit.LocationId != visitSchedule.RoomLocationId ||
                         hired > 0 && privilege.AppointmentHierarchyId == 0 &&
                         item.Visit.LocationId != visitSchedule.RoomLocationId)
                {
                    diffVisitLocApp.Add(item);
                }
            });

            //Conflict location detail
            int[] scheduleId = sameLocApp.Select(s => s.ScheduleId).ToArray();
            scheduleId = scheduleId.Concat(diffLocApp.Select(s => s.ScheduleId).ToArray()).ToArray();
            scheduleId = scheduleId.Concat(sameVisitLocApp.Select(s => s.ScheduleId).ToArray()).ToArray();
            scheduleId = scheduleId.Concat(diffVisitLocApp.Select(s => s.ScheduleId).ToArray()).ToArray();

            if (sameLocApp.Any() || diffLocApp.Any() || sameVisitLocApp.Any() || diffVisitLocApp.Any())
            {
                List<Schedule> lstVisitSchedule =
                    _context.Schedule.Where(w => scheduleId.Contains(w.ScheduleId)).Distinct().ToList();
                lstVisitSchedule.ForEach(val =>
                {
                    Schedule lstVisit = _context.Schedule.Single(s => s.ScheduleId == val.ScheduleId);
                    Privileges privileges = _context.Privileges.FirstOrDefault(s => s.PrivilegeId == lstVisit.LocationId);
                    if (lstVisit?.EndDate != null)
                    {
                        locDetail.Add(
                            $"{privileges?.PrivilegeDescription} {lstVisit.StartDate.ToShortTimeString()} - {lstVisit.EndDate.Value.ToShortTimeString()} - Priority {privileges?.AppointmentHierarchyId}");
                    }
                });
                TrackingConflictVm conflict = new TrackingConflictVm
                {
                    ConflictType = ConflictTypeConstants.OVERALAPINGEVENTCONFLICT,
                    ConflictDescription = ConflictTypeConstants.SCHEDULEEVENTCONFLICT,
                    LocationDetail = locDetail
                };
                trackingConflict.Add(conflict);
            }
            else if (!visitSchedule.Flag)
            {
                List<Schedule> lstSchedule =
                    _context.Schedule.Where(w => scheduleId.Contains(w.ScheduleId)).Distinct().ToList();

                lstSchedule.ForEach(val =>
                {
                    Schedule lstVisit = _context.Schedule.Single(s => s.ScheduleId == val.ScheduleId);
                    if (lstVisit != null)
                    {
                        lstVisit.BumpBy = _personnelId;
                        lstVisit.BumpDate = DateTime.Now;
                        lstVisit.BumpFlag = 1;
                        lstVisit.DeleteFlag = true;
                        lstVisit.DeleteBy = _personnelId;
                        lstVisit.DeleteDate = DateTime.Now;
                    }
                });
                _context.SaveChanges();
            }
            return trackingConflict;
        }
        private void InsertRescheduleVisit(VisitSchedule visitSchedule)
        {
            List<ScheduleInmate> sameLocApp = new List<ScheduleInmate>();
            List<ScheduleInmate> diffLocApp = new List<ScheduleInmate>();
            Privileges privilege = _context.Privileges.Where(w => w.PrivilegeId == visitSchedule.RoomLocationId).Select(s => new Privileges
            {
                Capacity = s.Capacity ?? 0,
                AppointmentHierarchyId = s.AppointmentHierarchyId ?? 0
            }).First();
            IQueryable<ScheduleInmate> appointment = _context.Schedule.OfType<ScheduleInmate>().Where(s => !(s is Visit));
            List<ScheduleInmate> priorityApp = appointment.Where(v =>
                v.ScheduleId != visitSchedule.ScheduleId && visitSchedule.EndDate != null &&
                v.EndDate != null && !v.DeleteFlag && v.InmateId == visitSchedule.InmateId &&
                v.StartDate.Date == visitSchedule.StartDate.Date &&
                v.EndDate.Value.Date == visitSchedule.EndDate.Value.Date).Select(n => new ScheduleInmate
                {
                    LocationId = n.LocationId,
                    ScheduleId = n.ScheduleId,
                    InmateId = n.InmateId
                }).ToList();
            int?[] priIds = priorityApp.Select(s => s.LocationId).ToArray();
            List<Privileges> pri = _context.Privileges.Where(s => priIds.Contains(s.PrivilegeId)).ToList();
            priorityApp.ForEach(item =>
            {
                Privileges privileges = pri.FirstOrDefault(s => s.PrivilegeId == item.LocationId);
                int hired = privileges?.AppointmentHierarchyId ?? 0;
                if (hired > 0 && privilege.AppointmentHierarchyId > 0 && hired >= privilege.AppointmentHierarchyId &&
                    item.LocationId == visitSchedule.RoomLocationId && item.ScheduleId != visitSchedule.ScheduleId &&
                    item.InmateId == visitSchedule.InmateId)
                {
                    sameLocApp.Add(item);
                }
                else if (hired > 0 && privilege.AppointmentHierarchyId > 0 && hired >= privilege.AppointmentHierarchyId &&
                         item.LocationId != visitSchedule.RoomLocationId &&
                         item.ScheduleId != visitSchedule.ScheduleId && item.InmateId == visitSchedule.InmateId)
                {
                    diffLocApp.Add(item);
                }
            });

            List<VisitToVisitor> scheduleVisit = _context.VisitToVisitor.Where(v =>
                    v.Visit.Location.AppointmentHierarchyId.HasValue && v.Visit.Location.AppointmentHierarchyId > 0 &&
                    privilege.AppointmentHierarchyId > 0 &&
                    v.Visit.Location.AppointmentHierarchyId >= privilege.AppointmentHierarchyId &&
                    v.Visit.StartDate.Date == visitSchedule.StartDate.Date &&
                    v.Visit.InmateId == visitSchedule.InmateId &&
                    v.Visit.EndDate.Value.Date == visitSchedule.EndDate.Value.Date)
                .Select(x => new VisitToVisitor
                {
                    PersonId = x.PersonId,
                    ScheduleId = x.ScheduleId,
                    Visit = x.Visit
                }).ToList();
            scheduleVisit = scheduleVisit.Where(s => s.ScheduleId != visitSchedule.ScheduleId &&
                s.Visit.InmateId != null && s.Visit.InmateId == visitSchedule.InmateId &&
                s.Visit.CompleteRegistration && !s.Visit.CompleteVisitFlag &&
                s.Visit.VisitDenyFlag != 1 && !s.Visit.DeleteFlag).ToList();

            List<VisitToVisitor> sameVisitLocApp =
                scheduleVisit.Where(s => s.Visit.LocationId == visitSchedule.RoomLocationId).ToList();
            List<VisitToVisitor> diffVisitLocApp =
                scheduleVisit.Where(s => s.Visit.LocationId != visitSchedule.RoomLocationId).ToList();
            int[] scheduleId = sameLocApp.Select(s => s.ScheduleId).ToArray();
            scheduleId = scheduleId.Concat(diffLocApp.Select(s => s.ScheduleId).ToArray()).ToArray();
            scheduleId = scheduleId.Concat(sameVisitLocApp.Select(s => s.ScheduleId).ToArray()).ToArray();
            scheduleId = scheduleId.Concat(diffVisitLocApp.Select(s => s.ScheduleId).ToArray()).ToArray();
            List<Schedule> lstSchedule =
                _context.Schedule.Where(w => scheduleId.Contains(w.ScheduleId)).Distinct().ToList();

            lstSchedule.ForEach(val =>
            {
                Schedule lstVisit = lstSchedule.Single(s => s.ScheduleId == val.ScheduleId);
                if (lstVisit != null)
                {
                    lstVisit.BumpBy = _personnelId;
                    lstVisit.BumpDate = DateTime.Now;
                    lstVisit.BumpFlag = 1;
                    lstVisit.DeleteFlag = true;
                    lstVisit.DeleteBy = _personnelId;
                    lstVisit.DeleteDate = DateTime.Now;
                }
            });
            _context.SaveChanges();
        }
        private void InsertFloorNote(string note, int inmateId)
        {
            FloorNotes floorNotes = new FloorNotes
            {
                FloorNoteNarrative = note,
                FloorNoteOfficerId = _personnelId,
                FloorNoteDate = DateTime.Now,
                FloorNoteType = FloorNotesConflictConstants.CONFLICTCHECK,
                FloorNoteTime = DateTime.Now.ToString("HH:mm:ss")
            };
            _context.FloorNotes.Add(floorNotes);

            FloorNoteXref floorNoteXref = new FloorNoteXref
            {
                FloorNoteId = floorNotes.FloorNoteId,
                InmateId = inmateId,
                CreateDate = DateTime.Now
            };

            _context.FloorNoteXref.Add(floorNoteXref);
        }
        private bool GetVisitDenyFlag()
        {
            List<Lookup> lookup = _context.Lookup.Where(s =>
                            s.LookupType == LookupConstants.VISITCOMPLETE && s.LookupFlag10 == 1).ToList();

            return lookup.Any();
        }
        private List<TrackingConflictVm> LoadKeepSeparateConflict(ICollection<int> inmateList, int moveLocationId)
        {
            List<TrackingConflictVm> lstKeepSeeps = new List<TrackingConflictVm>();
            List<TrackingConflictVm> trackingConflictDetail = new List<TrackingConflictVm>();
            IEnumerable<Inmate> lstInmate =
                _context.Inmate.Where(i => i.InmateActive == 1 &&
                (i.InmateCurrentTrackId == moveLocationId || inmateList.Contains(i.InmateId)))
                .Select(i => new Inmate
                {
                    InmateId = i.InmateId,
                    InmateNumber = i.InmateNumber,
                    InmateCurrentTrackId = i.InmateCurrentTrackId,
                    PersonId = i.PersonId
                }).ToList();

            //bool genderConflict = _context.Privileges.First(w => w.PrivilegeId == moveLocationId).RemoveCoflictCheckKeepSep == 1;
            //if (genderConflict) return;
            IQueryable<PersonClassification> dbPersonClassifications = _context.PersonClassification
                .Where(pc => pc.InactiveFlag == 0 && pc.PersonClassificationDateFrom < DateTime.Now
                    && (!pc.PersonClassificationDateThru.HasValue || pc.PersonClassificationDateThru >= DateTime.Now));

            //TODO: Move evaluation from server to client... why do we need SelectMany?
            List<TrackingConflictVm> lstPersonClassify =
                dbPersonClassifications.SelectMany(pc => lstInmate.Where(i => pc.PersonId == i.PersonId),
                    (pc, i) => new TrackingConflictVm
                    {
                        InmateId = i.InmateId,
                        InmateNumber = i.InmateNumber,
                        PersonId = pc.PersonId,
                        InmateCurrentTrackId = i.InmateCurrentTrackId,
                        PersonClassificationType = pc.PersonClassificationType,
                        PersonClassificationSubset = pc.PersonClassificationSubset
                    }).ToList();

            //Move Location Inmate List
            List<TrackingConflictVm> lstPerClassifyLocInmates =
                lstPersonClassify.Where(p => p.InmateCurrentTrackId == moveLocationId).ToList();

            //Selected Inmate List
            List<TrackingConflictVm> lstPerClassifySelectedInmates =
                lstPersonClassify.Where(i => inmateList.Contains(i.InmateId)).ToList();

            List<int> locationInmateList = lstInmate.Where(p => p.InmateCurrentTrackId == moveLocationId)
                .Select(p => p.InmateId).ToList();

            #region Assoc To Assoc

            //Keep Sep Details for Assoc - Assoc
            IQueryable<KeepSepAssocAssoc> tkeepSepAssocAssoc =
                _context.KeepSepAssocAssoc.Where(ksaa => ksaa.DeleteFlag == 0);

            if (lstPerClassifyLocInmates.Count > 0)
            {
                List<TrackingConflictVm> keepSepAssocAssoc =
                    (from ksaa in tkeepSepAssocAssoc
                     from lpcl in lstPerClassifyLocInmates
                     where lpcl.PersonClassificationType == ksaa.KeepSepAssoc1
                     select new TrackingConflictVm
                     {
                         PersonId = lpcl.PersonId,
                         InmateId = lpcl.InmateId,
                         InmateNumber = lpcl.InmateNumber,
                         KeepSepReason = ksaa.KeepSepReason,
                         KeepSepAssoc2 = ksaa.KeepSepAssoc2,
                         KeepSepAssoc1 = ksaa.KeepSepAssoc1
                     }).ToList();

                List<TrackingConflictVm> tKeepSepAssocAssoc = (from ksaa in tkeepSepAssocAssoc
                                                               from lpcl in lstPerClassifyLocInmates
                                                               where lpcl.PersonClassificationType == ksaa.KeepSepAssoc2
                                                               select new TrackingConflictVm
                                                               {
                                                                   PersonId = lpcl.PersonId,
                                                                   InmateId = lpcl.InmateId,
                                                                   InmateNumber = lpcl.InmateNumber,
                                                                   KeepSepReason = ksaa.KeepSepReason,
                                                                   KeepSepAssoc2 = ksaa.KeepSepAssoc2,
                                                                   KeepSepAssoc1 = ksaa.KeepSepAssoc1
                                                               }).ToList();

                lstKeepSeeps = (from ksaa in keepSepAssocAssoc
                                from lksaas in lstPerClassifySelectedInmates
                                where lksaas.PersonClassificationType == ksaa.KeepSepAssoc2
                                      && ksaa.PersonId != lksaas.PersonId
                                select new TrackingConflictVm
                                {
                                    ConflictType = KeepSepLabel.ASSOCKEEPSEPASSOC,
                                    KeepSepPersonId = ksaa.PersonId,
                                    KeepSepInmateId = ksaa.InmateId,
                                    KeepSepInmateNumber = ksaa.InmateNumber,
                                    ConflictDescription = ksaa.KeepSepReason,
                                    KeepSepAssoc1 = ksaa.KeepSepAssoc2,
                                    KeepSepAssoc2 = ksaa.KeepSepAssoc1,
                                    PersonId = lksaas.PersonId,
                                    InmateId = lksaas.InmateId,
                                    InmateNumber = lksaas.InmateNumber
                                }).ToList();

                lstKeepSeeps.AddRange(from ksaa in tKeepSepAssocAssoc
                                      from lksaas in lstPerClassifySelectedInmates
                                      where lksaas.PersonClassificationType == ksaa.KeepSepAssoc1
                                            && ksaa.PersonId != lksaas.PersonId
                                      select new TrackingConflictVm
                                      {
                                          ConflictType = KeepSepLabel.ASSOCKEEPSEPASSOC,
                                          ConflictDescription = ksaa.KeepSepReason,
                                          KeepSepAssoc1 = ksaa.KeepSepAssoc1,
                                          KeepSepAssoc2 = ksaa.KeepSepAssoc2,
                                          PersonId = lksaas.PersonId,
                                          InmateId = lksaas.InmateId,
                                          InmateNumber = lksaas.InmateNumber,
                                          KeepSepPersonId = ksaa.PersonId,
                                          KeepSepInmateId = ksaa.InmateId,
                                          KeepSepInmateNumber = ksaa.InmateNumber,
                                      });
            }

            if (lstPerClassifySelectedInmates.Count > 0)
            {
                List<TrackingConflictVm> tLstKeepSepAssocAssoc = (from ksaa in tkeepSepAssocAssoc
                                                                  from lpcl in lstPerClassifySelectedInmates
                                                                  where lpcl.PersonClassificationType == ksaa.KeepSepAssoc1
                                                                  select new TrackingConflictVm
                                                                  {
                                                                      PersonId = lpcl.PersonId,
                                                                      InmateId = lpcl.InmateId,
                                                                      InmateNumber = lpcl.InmateNumber,
                                                                      KeepSepReason = ksaa.KeepSepReason,
                                                                      KeepSepAssoc2 = ksaa.KeepSepAssoc2,
                                                                      KeepSepAssoc1 = ksaa.KeepSepAssoc1
                                                                  }).ToList();

                List<TrackingConflictVm> keepSepAssocAssocS = (from ksaa in tkeepSepAssocAssoc
                                                               from lpcl in lstPerClassifySelectedInmates
                                                               where lpcl.PersonClassificationType == ksaa.KeepSepAssoc2
                                                               select new TrackingConflictVm
                                                               {
                                                                   PersonId = lpcl.PersonId,
                                                                   InmateId = lpcl.InmateId,
                                                                   InmateNumber = lpcl.InmateNumber,
                                                                   KeepSepReason = ksaa.KeepSepReason,
                                                                   KeepSepAssoc2 = ksaa.KeepSepAssoc2,
                                                                   KeepSepAssoc1 = ksaa.KeepSepAssoc1
                                                               }).ToList();

                lstKeepSeeps.AddRange(from ksaa in tLstKeepSepAssocAssoc
                                      from lksaas in lstPerClassifySelectedInmates
                                      where lksaas.PersonClassificationType == ksaa.KeepSepAssoc2
                                            && ksaa.PersonId != lksaas.PersonId
                                      select new TrackingConflictVm
                                      {
                                          ConflictType = KeepSepLabel.ASSOCKEEPSEPASSOC,
                                          ConflictDescription = ksaa.KeepSepReason,
                                          KeepSepAssoc1 = ksaa.KeepSepAssoc1,
                                          KeepSepAssoc2 = ksaa.KeepSepAssoc2,
                                          PersonId = lksaas.PersonId,
                                          InmateId = lksaas.InmateId,
                                          InmateNumber = lksaas.InmateNumber,
                                          KeepSepPersonId = ksaa.PersonId,
                                          KeepSepInmateId = ksaa.InmateId,
                                          KeepSepInmateNumber = ksaa.InmateNumber,
                                      });

                lstKeepSeeps.AddRange(from ksaa in keepSepAssocAssocS
                                      from lksaas in lstPerClassifySelectedInmates
                                      where lksaas.PersonClassificationType == ksaa.KeepSepAssoc1
                                            && ksaa.PersonId != lksaas.PersonId
                                      select new TrackingConflictVm
                                      {
                                          ConflictType = KeepSepLabel.ASSOCKEEPSEPASSOC,
                                          ConflictDescription = ksaa.KeepSepReason,
                                          KeepSepAssoc1 = ksaa.KeepSepAssoc1,
                                          KeepSepAssoc2 = ksaa.KeepSepAssoc2,
                                          PersonId = lksaas.PersonId,
                                          InmateId = lksaas.InmateId,
                                          InmateNumber = lksaas.InmateNumber,
                                          KeepSepPersonId = ksaa.PersonId,
                                          KeepSepInmateId = ksaa.InmateId,
                                          KeepSepInmateNumber = ksaa.InmateNumber,
                                      });
            }
            #endregion

            #region Assoc To Inmate

            //Keep Sep Details for Assoc - Inmate && Inmate - Assoc
            IQueryable<KeepSepAssocInmate> tKeepSepAssocInmate =
                _context.KeepSepAssocInmate.Where(ksa =>
                    ksa.DeleteFlag == 0 && ksa.KeepSepInmate2.InmateCurrentTrackId == moveLocationId &&
                    ksa.KeepSepInmate2.InmateActive == 1);

            IQueryable<KeepSepAssocInmate> tKeepSepInmateAssoc = _context.KeepSepAssocInmate.Where(ksa =>
                ksa.DeleteFlag == 0 && inmateList.Contains(ksa.KeepSepInmate2Id));

            if (lstPerClassifySelectedInmates.Count > 0)
            {
                lstKeepSeeps.AddRange(from ksa in tKeepSepAssocInmate
                                      from lksa in lstPerClassifySelectedInmates
                                      where lksa.PersonClassificationType == ksa.KeepSepAssoc1
                                      select new TrackingConflictVm
                                      {
                                          ConflictType = KeepSepLabel.ASSOCKEEPSEPINMATE,
                                          KeepSepPersonId = ksa.KeepSepInmate2.PersonId,
                                          KeepSepInmateId = ksa.KeepSepInmate2Id,
                                          KeepSepInmateNumber = ksa.KeepSepInmate2.InmateNumber,

                                          PersonId = lksa.PersonId,
                                          InmateId = lksa.InmateId,
                                          InmateNumber = lksa.InmateNumber,

                                          KeepSepAssoc1 = ksa.KeepSepAssoc1,
                                          KeepSepType = ksa.KeepSeparateType,
                                          ConflictDescription = ksa.KeepSepReason
                                      });


                lstKeepSeeps.AddRange(from ksa in tKeepSepInmateAssoc
                                      from lksa in lstPerClassifySelectedInmates
                                      where lksa.PersonClassificationType == ksa.KeepSepAssoc1
                                      select new TrackingConflictVm
                                      {
                                          ConflictType = KeepSepLabel.ASSOCKEEPSEPINMATE,

                                          KeepSepPersonId = ksa.KeepSepInmate2.PersonId,
                                          KeepSepInmateId = ksa.KeepSepInmate2Id,
                                          KeepSepInmateNumber = ksa.KeepSepInmate2.InmateNumber,

                                          PersonId = lksa.PersonId,
                                          InmateId = lksa.InmateId,
                                          InmateNumber = lksa.InmateNumber,

                                          KeepSepAssoc1 = ksa.KeepSepAssoc1,
                                          KeepSepType = ksa.KeepSeparateType,
                                          ConflictDescription = ksa.KeepSepReason
                                      });
            }
            #endregion

            #region Assoc To Subset

            //Keep Sep Details for Assoc - Subset && Subset - Assoc
            IQueryable<KeepSepAssocSubset> tKeepSepAssocSubset =
                _context.KeepSepAssocSubset.Where(ksas => ksas.DeleteFlag == 0);

            if (lstPerClassifySelectedInmates.Count > 0)
            {
                IEnumerable<TrackingConflictVm> lstKeepSepAssocSubset = (from ksas in tKeepSepAssocSubset
                                                                         from ksaas in lstPerClassifySelectedInmates
                                                                         where ksas.KeepSepAssoc1 == ksaas.PersonClassificationType
                                                                         select new TrackingConflictVm
                                                                         {
                                                                             PersonId = ksaas.PersonId,
                                                                             InmateId = ksaas.InmateId,
                                                                             InmateNumber = ksaas.InmateNumber,
                                                                             KeepSepAssoc1 = ksas.KeepSepAssoc1,
                                                                             KeepSepAssoc2 = ksas.KeepSepAssoc2,
                                                                             KeepSepAssoc2Subset = ksas.KeepSepAssoc2Subset
                                                                         }).ToList();

                lstKeepSeeps.AddRange(from ksas in lstKeepSepAssocSubset
                                      from lksa in lstPerClassifyLocInmates
                                      where ksas.KeepSepAssoc2 == lksa.PersonClassificationType
                                            && ksas.KeepSepAssoc2Subset == lksa.PersonClassificationSubset
                                            && ksas.PersonId != lksa.PersonId
                                      select new TrackingConflictVm
                                      {
                                          ConflictType = KeepSepLabel.ASSOCKEEPSEPSUBSET,
                                          PersonId = ksas.PersonId,
                                          InmateId = ksas.InmateId,
                                          InmateNumber = ksas.InmateNumber,
                                          KeepSepPersonId = lksa.PersonId,
                                          KeepSepInmateId = lksa.InmateId,
                                          KeepSepInmateNumber = lksa.InmateNumber,
                                          KeepSepAssoc1 = ksas.KeepSepAssoc1,
                                          KeepSepAssoc2 = ksas.KeepSepAssoc2,
                                          KeepSepAssocSubset2 = ksas.KeepSepAssoc2Subset
                                      });

                lstKeepSeeps.AddRange(from ksas in lstKeepSepAssocSubset
                                      from lksa in lstPerClassifySelectedInmates
                                      where ksas.KeepSepAssoc2 == lksa.PersonClassificationType
                                            && ksas.KeepSepAssoc2Subset == lksa.PersonClassificationSubset
                                            && ksas.PersonId != lksa.PersonId
                                      select new TrackingConflictVm
                                      {
                                          ConflictType = KeepSepLabel.ASSOCKEEPSEPSUBSET,
                                          PersonId = ksas.PersonId,
                                          InmateId = ksas.InmateId,
                                          InmateNumber = ksas.InmateNumber,
                                          KeepSepPersonId = lksa.PersonId,
                                          KeepSepInmateId = lksa.InmateId,
                                          KeepSepInmateNumber = lksa.InmateNumber,
                                          KeepSepAssoc1 = ksas.KeepSepAssoc1,
                                          KeepSepAssoc2 = ksas.KeepSepAssoc2,
                                          KeepSepAssocSubset2 = ksas.KeepSepAssoc2Subset
                                      });
            }
            #endregion

            #region Inmate To Assoc
            if (tKeepSepInmateAssoc.Any())
            {
                //Keep Seperate for Inmate to Association Check MoveLocation Inmates
                if (lstPerClassifyLocInmates.Any())
                {
                    lstKeepSeeps.AddRange(from ksa in tKeepSepInmateAssoc
                                          from lksa in lstPerClassifyLocInmates
                                          where lksa.PersonClassificationType == ksa.KeepSepAssoc1
                                          select new TrackingConflictVm
                                          {
                                              ConflictType = KeepSepLabel.INMATEKEEPSEPASSOC,
                                              PersonId = ksa.KeepSepInmate2.PersonId,
                                              InmateId = ksa.KeepSepInmate2.InmateId,
                                              InmateNumber = ksa.KeepSepInmate2.InmateNumber,
                                              KeepSepPersonId = lksa.PersonId,
                                              KeepSepInmateNumber = lksa.InmateNumber,
                                              KeepSepAssoc1 = ksa.KeepSepAssoc1,
                                              KeepSepType = ksa.KeepSeparateType,
                                              ConflictDescription = ksa.KeepSepReason
                                          });
                }

                //Keep Separate for Inmate to Association Check MoveInmates
                if (lstPerClassifySelectedInmates.Any())
                {
                    lstKeepSeeps.AddRange(from ksa in tKeepSepInmateAssoc
                                          from lksa in lstPerClassifySelectedInmates
                                          where lksa.PersonClassificationType == ksa.KeepSepAssoc1
                                          select new TrackingConflictVm
                                          {
                                              ConflictType = KeepSepLabel.INMATEKEEPSEPASSOC,
                                              PersonId = ksa.KeepSepInmate2.PersonId,
                                              InmateId = ksa.KeepSepInmate2.InmateId,
                                              InmateNumber = ksa.KeepSepInmate2.InmateNumber,
                                              KeepSepPersonId = lksa.PersonId,
                                              KeepSepInmateNumber = lksa.InmateNumber,
                                              KeepSepAssoc1 = ksa.KeepSepAssoc1,
                                              KeepSepType = ksa.KeepSeparateType,
                                              ConflictDescription = ksa.KeepSepReason
                                          });
                }
            }
            #endregion

            #region Inmate To Inmate

            //Keep Sep Inmate 1 List
            IQueryable<KeepSeparate> kepSeparateDetails = from ks in _context.KeepSeparate
                                                          where ks.InactiveFlag == 0 &&
                                                              (inmateList.Contains(ks.KeepSeparateInmate1Id) || inmateList.Contains(ks.KeepSeparateInmate2Id))
                                                          select ks;

            if (kepSeparateDetails.Any())
            {
                lstKeepSeeps.AddRange(from ks in kepSeparateDetails
                                      from i in locationInmateList
                                      where ks.KeepSeparateInmate1Id == i
                                      select new TrackingConflictVm
                                      {
                                          ConflictType = KeepSepLabel.INMATEKEEPSEPINMATE,
                                          KeepSepPersonId = ks.KeepSeparateInmate1.PersonId,
                                          KeepSepInmateId = ks.KeepSeparateInmate1Id,
                                          KeepSepInmateNumber = ks.KeepSeparateInmate1.InmateNumber,

                                          PersonId = ks.KeepSeparateInmate2.PersonId,
                                          InmateId = ks.KeepSeparateInmate2Id,
                                          InmateNumber = ks.KeepSeparateInmate2.InmateNumber,
                                          KeepSepType = ks.KeepSeparateType,
                                          ConflictDescription = ks.KeepSeparateReason
                                      });

                lstKeepSeeps.AddRange(from ks in kepSeparateDetails
                                      from i in locationInmateList
                                      where ks.KeepSeparateInmate2Id == i
                                      select new TrackingConflictVm
                                      {
                                          ConflictType = KeepSepLabel.INMATEKEEPSEPINMATE,
                                          KeepSepPersonId = ks.KeepSeparateInmate2.PersonId,
                                          KeepSepInmateId = ks.KeepSeparateInmate2Id,
                                          KeepSepInmateNumber = ks.KeepSeparateInmate2.InmateNumber,

                                          PersonId = ks.KeepSeparateInmate1.PersonId,
                                          InmateId = ks.KeepSeparateInmate1Id,
                                          InmateNumber = ks.KeepSeparateInmate1.InmateNumber,
                                          KeepSepType = ks.KeepSeparateType,
                                          ConflictDescription = ks.KeepSeparateReason
                                      });


                lstKeepSeeps.AddRange(from ks in kepSeparateDetails
                                      from i in locationInmateList
                                      where ks.KeepSeparateInmate1Id == i
                                            && ks.KeepSeparateInmate2.InmateCurrentTrackId == moveLocationId
                                      select new TrackingConflictVm
                                      {
                                          ConflictType = KeepSepLabel.INMATEKEEPSEPINMATE,
                                          KeepSepPersonId = ks.KeepSeparateInmate1.PersonId,
                                          KeepSepInmateId = ks.KeepSeparateInmate1Id,
                                          KeepSepInmateNumber = ks.KeepSeparateInmate1.InmateNumber,

                                          PersonId = ks.KeepSeparateInmate2.PersonId,
                                          InmateId = ks.KeepSeparateInmate2Id,
                                          InmateNumber = ks.KeepSeparateInmate2.InmateNumber,
                                          KeepSepType = ks.KeepSeparateType,
                                          ConflictDescription = ks.KeepSeparateReason
                                      });

                lstKeepSeeps.AddRange(from ks in kepSeparateDetails
                                      from i in locationInmateList
                                      where ks.KeepSeparateInmate2Id == i
                                            && ks.KeepSeparateInmate1.InmateCurrentTrackId == moveLocationId
                                      select new TrackingConflictVm
                                      {
                                          ConflictType = KeepSepLabel.INMATEKEEPSEPINMATE,
                                          KeepSepPersonId = ks.KeepSeparateInmate2.PersonId,
                                          KeepSepInmateId = ks.KeepSeparateInmate2Id,
                                          KeepSepInmateNumber = ks.KeepSeparateInmate2.InmateNumber,

                                          PersonId = ks.KeepSeparateInmate1.PersonId,
                                          InmateId = ks.KeepSeparateInmate1Id,
                                          InmateNumber = ks.KeepSeparateInmate1.InmateNumber,
                                          KeepSepType = ks.KeepSeparateType,
                                          ConflictDescription = ks.KeepSeparateReason
                                      });

                lstKeepSeeps.AddRange(from ks in kepSeparateDetails
                                      from i in inmateList
                                      where ks.KeepSeparateInmate2Id == i
                                            && inmateList.Contains(ks.KeepSeparateInmate1Id)
                                      select new TrackingConflictVm
                                      {
                                          ConflictType = KeepSepLabel.INMATEKEEPSEPINMATE,
                                          KeepSepPersonId = ks.KeepSeparateInmate2.PersonId,
                                          KeepSepInmateId = ks.KeepSeparateInmate2Id,
                                          KeepSepInmateNumber = ks.KeepSeparateInmate2.InmateNumber,

                                          PersonId = ks.KeepSeparateInmate1.PersonId,
                                          InmateId = ks.KeepSeparateInmate1Id,
                                          InmateNumber = ks.KeepSeparateInmate1.InmateNumber,
                                          KeepSepType = ks.KeepSeparateType,
                                          ConflictDescription = ks.KeepSeparateReason
                                      });

                lstKeepSeeps.AddRange(from ks in kepSeparateDetails
                                      from i in inmateList
                                      where ks.KeepSeparateInmate1Id == i
                                            && inmateList.Contains(ks.KeepSeparateInmate2Id)
                                      select new TrackingConflictVm
                                      {
                                          ConflictType = KeepSepLabel.INMATEKEEPSEPINMATE,
                                          KeepSepPersonId = ks.KeepSeparateInmate1.PersonId,
                                          KeepSepInmateId = ks.KeepSeparateInmate1Id,
                                          KeepSepInmateNumber = ks.KeepSeparateInmate1.InmateNumber,

                                          PersonId = ks.KeepSeparateInmate2.PersonId,
                                          InmateId = ks.KeepSeparateInmate2Id,
                                          InmateNumber = ks.KeepSeparateInmate2.InmateNumber,
                                          KeepSepType = ks.KeepSeparateType,
                                          ConflictDescription = ks.KeepSeparateReason
                                      });
            }
            #endregion

            #region Inmate To Subset

            IQueryable<KeepSepSubsetInmate> tKeepSepInmateSubset =
                _context.KeepSepSubsetInmate.Where(
                    ksa => ksa.DeleteFlag == 0 && inmateList.Contains(ksa.KeepSepInmate2Id));

            if (tKeepSepInmateSubset.Any())
            {
                //Keep Sep : SUBSET KEEP SEPARATE To INMATE
                if (lstPerClassifyLocInmates.Count > 0)
                {
                    lstKeepSeeps.AddRange(from ksa in tKeepSepInmateSubset
                                          from lpcl in lstPerClassifyLocInmates
                                          where lpcl.PersonClassificationType == ksa.KeepSepAssoc1
                                                && lpcl.PersonClassificationSubset == ksa.KeepSepAssoc1Subset
                                          select new TrackingConflictVm
                                          {
                                              ConflictType = KeepSepLabel.INMATEKEEPSEPSUBSET,
                                              PersonId = ksa.KeepSepInmate2.PersonId,
                                              InmateId = ksa.KeepSepInmate2Id,
                                              InmateNumber = ksa.KeepSepInmate2.InmateNumber,

                                              KeepSepPersonId = lpcl.PersonId,
                                              KeepSepInmateId = lpcl.InmateId,
                                              KeepSepInmateNumber = lpcl.InmateNumber,
                                              KeepSepAssoc1 = ksa.KeepSepAssoc1,
                                              KeepSepAssocSubset1 = ksa.KeepSepAssoc1Subset,
                                              KeepSepType = ksa.KeepSeparateType,
                                              ConflictDescription = ksa.KeepSepReason
                                          });
                }

                if (lstPerClassifySelectedInmates.Count > 0)
                {
                    lstKeepSeeps.AddRange(from ksa in tKeepSepInmateSubset
                                          from lpcl in lstPerClassifySelectedInmates
                                          where lpcl.PersonClassificationType == ksa.KeepSepAssoc1
                                                && lpcl.PersonClassificationSubset == ksa.KeepSepAssoc1Subset
                                                && lpcl.InmateId != ksa.KeepSepInmate2Id
                                          select new TrackingConflictVm
                                          {
                                              ConflictType = KeepSepLabel.INMATEKEEPSEPSUBSET,
                                              PersonId = ksa.KeepSepInmate2.PersonId,
                                              InmateId = ksa.KeepSepInmate2Id,
                                              InmateNumber = ksa.KeepSepInmate2.InmateNumber,

                                              KeepSepPersonId = lpcl.PersonId,
                                              KeepSepInmateId = lpcl.InmateId,
                                              KeepSepInmateNumber = lpcl.InmateNumber,
                                              KeepSepAssoc1 = ksa.KeepSepAssoc1,
                                              KeepSepAssocSubset1 = ksa.KeepSepAssoc1Subset,
                                              KeepSepType = ksa.KeepSeparateType,
                                              ConflictDescription = ksa.KeepSepReason
                                          });
                }
            }
            #endregion

            #region Subset To Assoc

            List<TrackingConflictVm> selLstKeepSepAssocSubset =
                (from ksas in tKeepSepAssocSubset
                 from ksaas in lstPerClassifySelectedInmates
                 where ksas.KeepSepAssoc2 == ksaas.PersonClassificationType
                       && ksas.KeepSepAssoc2Subset == ksaas.PersonClassificationSubset
                 select new TrackingConflictVm
                 {
                     PersonId = ksaas.PersonId,
                     InmateId = ksaas.InmateId,
                     InmateNumber = ksaas.InmateNumber,
                     KeepSepReason = ksas.KeepSepReason,
                     KeepSepAssoc1 = ksas.KeepSepAssoc1,
                     KeepSepAssoc2 = ksas.KeepSepAssoc2,
                     KeepSepAssoc2Subset = ksas.KeepSepAssoc2Subset
                 }).ToList();

            if (selLstKeepSepAssocSubset.Count > 0)
            {
                lstKeepSeeps.AddRange(from ksas in selLstKeepSepAssocSubset
                                      from lpcl in lstPerClassifySelectedInmates
                                      where lpcl.PersonClassificationType == ksas.KeepSepAssoc1
                                            && ksas.PersonId != lpcl.PersonId
                                      select new TrackingConflictVm
                                      {
                                          ConflictType = KeepSepLabel.SUBSETKEEPSEPASSOC,

                                          PersonId = ksas.PersonId,
                                          InmateId = ksas.InmateId,
                                          InmateNumber = ksas.InmateNumber,

                                          KeepSepPersonId = lpcl.PersonId,
                                          KeepSepInmateId = lpcl.InmateId,
                                          KeepSepInmateNumber = lpcl.InmateNumber,

                                          KeepSepAssoc1 = ksas.KeepSepAssoc1,
                                          KeepSepAssocSubset1 = ksas.KeepSepAssoc2,
                                          KeepSepAssoc2 = ksas.KeepSepAssoc2Subset
                                      });

                lstKeepSeeps.AddRange(from ksas in selLstKeepSepAssocSubset
                                      from lpcl in lstPerClassifyLocInmates
                                      where lpcl.PersonClassificationType == ksas.KeepSepAssoc1
                                            && ksas.PersonId != lpcl.PersonId
                                      select new TrackingConflictVm
                                      {
                                          ConflictType = KeepSepLabel.SUBSETKEEPSEPASSOC,
                                          PersonId = ksas.PersonId,
                                          InmateId = ksas.InmateId,
                                          InmateNumber = ksas.InmateNumber,

                                          KeepSepPersonId = lpcl.PersonId,
                                          KeepSepInmateId = lpcl.InmateId,
                                          KeepSepInmateNumber = lpcl.InmateNumber,

                                          KeepSepAssoc1 = ksas.KeepSepAssoc1,
                                          KeepSepAssocSubset1 = ksas.KeepSepAssoc2,
                                          KeepSepAssoc2 = ksas.KeepSepAssoc2Subset
                                      });
            }
            #endregion

            #region Subset To Inmate

            IQueryable<KeepSepSubsetInmate> tKeepSepSubsetInmate = _context.KeepSepSubsetInmate.Where(ksa =>
                ksa.DeleteFlag == 0 && ksa.KeepSepInmate2.InmateActive == 1 &&
                ksa.KeepSepInmate2.InmateCurrentTrackId == moveLocationId);

            if (lstPerClassifySelectedInmates.Count > 0)
            {
                lstKeepSeeps.AddRange(from ksa in tKeepSepSubsetInmate
                                      from lksa in lstPerClassifySelectedInmates
                                      where lksa.PersonClassificationType == ksa.KeepSepAssoc1
                                            && lksa.PersonClassificationSubset == ksa.KeepSepAssoc1Subset
                                      select new TrackingConflictVm
                                      {
                                          ConflictType = KeepSepLabel.SUBSETKEEPSEPINMATE,
                                          KeepSepPersonId = ksa.KeepSepInmate2.PersonId,
                                          KeepSepInmateId = ksa.KeepSepInmate2Id,
                                          KeepSepInmateNumber = ksa.KeepSepInmate2.InmateNumber,

                                          PersonId = lksa.PersonId,
                                          InmateId = lksa.InmateId,
                                          InmateNumber = lksa.InmateNumber,

                                          KeepSepAssoc1 = ksa.KeepSepAssoc1,
                                          KeepSepType = ksa.KeepSeparateType,
                                          ConflictDescription = ksa.KeepSepReason,
                                          KeepSepAssocSubset1 = ksa.KeepSepAssoc1Subset
                                      });

                lstKeepSeeps.AddRange(from ksa in tKeepSepInmateSubset
                                      from lpcl in lstPerClassifySelectedInmates
                                      where lpcl.PersonClassificationType == ksa.KeepSepAssoc1
                                            && lpcl.PersonClassificationSubset == ksa.KeepSepAssoc1Subset
                                            && lpcl.InmateId != ksa.KeepSepInmate2Id
                                      select new TrackingConflictVm
                                      {
                                          ConflictType = KeepSepLabel.SUBSETKEEPSEPINMATE,
                                          KeepSepPersonId = ksa.KeepSepInmate2.PersonId,
                                          KeepSepInmateId = ksa.KeepSepInmate2Id,
                                          KeepSepInmateNumber = ksa.KeepSepInmate2.InmateNumber,
                                          PersonId = lpcl.PersonId,
                                          InmateId = lpcl.InmateId,
                                          InmateNumber = lpcl.InmateNumber,
                                          KeepSepAssoc1 = ksa.KeepSepAssoc1,
                                          KeepSepAssocSubset1 = ksa.KeepSepAssoc1Subset,
                                          KeepSepType = ksa.KeepSeparateType,
                                          ConflictDescription = ksa.KeepSepReason
                                      });
            }
            #endregion

            #region Subset To Subset

            if (lstPerClassifyLocInmates.Count > 0)
            {
                IQueryable<TrackingConflictVm> keepSepSubsetSubset = from ksss in _context.KeepSepSubsetSubset
                                                                     from ksaas in lstPerClassifyLocInmates
                                                                     where ksss.KeepSepAssoc1 == ksaas.PersonClassificationType
                                                                           && ksss.KeepSepAssoc1Subset == ksaas.PersonClassificationSubset
                                                                           && ksss.DeleteFlag == 0
                                                                     select new TrackingConflictVm
                                                                     {
                                                                         PersonId = ksaas.PersonId,
                                                                         InmateId = ksaas.InmateId,
                                                                         InmateNumber = ksaas.InmateNumber,
                                                                         KeepSepReason = ksss.KeepSepReason,
                                                                         KeepSepAssoc2 = ksss.KeepSepAssoc2,
                                                                         KeepSepAssoc2Subset = ksss.KeepSepAssoc2Subset,
                                                                         KeepSepAssoc1 = ksss.KeepSepAssoc1,
                                                                         KeepSepAssoc1Subset = ksss.KeepSepAssoc1Subset
                                                                     };

                IQueryable<TrackingConflictVm> tKeepSepSubsetSubset = from ksss in _context.KeepSepSubsetSubset
                                                                      from ksaas in lstPerClassifyLocInmates
                                                                      where ksss.KeepSepAssoc2 == ksaas.PersonClassificationType
                                                                            && ksss.KeepSepAssoc2Subset == ksaas.PersonClassificationSubset
                                                                            && ksss.DeleteFlag == 0
                                                                      select new TrackingConflictVm
                                                                      {
                                                                          PersonId = ksaas.PersonId,
                                                                          InmateId = ksaas.InmateId,
                                                                          InmateNumber = ksaas.InmateNumber,
                                                                          KeepSepReason = ksss.KeepSepReason,
                                                                          KeepSepAssoc2 = ksss.KeepSepAssoc2,
                                                                          KeepSepAssoc2Subset = ksss.KeepSepAssoc2Subset,
                                                                          KeepSepAssoc1 = ksss.KeepSepAssoc1,
                                                                          KeepSepAssoc1Subset = ksss.KeepSepAssoc1Subset
                                                                      };


                lstKeepSeeps.AddRange(from ksss in keepSepSubsetSubset
                                      from lksaas in lstPerClassifySelectedInmates
                                      where ksss.KeepSepAssoc2 == lksaas.PersonClassificationType
                                            && ksss.KeepSepAssoc2Subset == lksaas.PersonClassificationSubset
                                            && ksss.PersonId != lksaas.PersonId
                                      select new TrackingConflictVm
                                      {
                                          ConflictType = KeepSepLabel.SUBSETKEEPSEPSUBSET,
                                          KeepSepPersonId = ksss.PersonId,
                                          KeepSepInmateId = ksss.InmateId,
                                          KeepSepInmateNumber = ksss.InmateNumber,
                                          ConflictDescription = ksss.KeepSepReason,
                                          KeepSepAssoc1 = ksss.KeepSepAssoc2,
                                          KeepSepAssocSubset1 = ksss.KeepSepAssoc2Subset,
                                          KeepSepAssoc2 = ksss.KeepSepAssoc1,
                                          KeepSepAssocSubset2 = ksss.KeepSepAssoc1Subset,

                                          PersonId = lksaas.PersonId,
                                          InmateId = lksaas.InmateId,
                                          InmateNumber = lksaas.InmateNumber
                                      });

                lstKeepSeeps.AddRange(from ksss in tKeepSepSubsetSubset
                                      from lksaas in lstPerClassifySelectedInmates
                                      where ksss.KeepSepAssoc1 == lksaas.PersonClassificationType
                                            && ksss.KeepSepAssoc1Subset == lksaas.PersonClassificationSubset
                                            && ksss.PersonId != lksaas.PersonId
                                      select new TrackingConflictVm
                                      {
                                          ConflictType = KeepSepLabel.SUBSETKEEPSEPSUBSET,
                                          KeepSepPersonId = ksss.PersonId,
                                          KeepSepInmateId = ksss.InmateId,
                                          KeepSepInmateNumber = ksss.InmateNumber,
                                          ConflictDescription = ksss.KeepSepReason,
                                          KeepSepAssoc1 = ksss.KeepSepAssoc1,
                                          KeepSepAssocSubset1 = ksss.KeepSepAssoc1Subset,
                                          KeepSepAssoc2 = ksss.KeepSepAssoc2,
                                          KeepSepAssocSubset2 = ksss.KeepSepAssoc2Subset,

                                          PersonId = lksaas.PersonId,
                                          InmateId = lksaas.InmateId,
                                          InmateNumber = lksaas.InmateNumber
                                      });
            }

            if (lstPerClassifySelectedInmates.Count > 0)
            {
                IQueryable<TrackingConflictVm> keepSepSubsetSubsetT = from ksss in _context.KeepSepSubsetSubset
                                                                      from ksaas in lstPerClassifySelectedInmates
                                                                      where ksss.KeepSepAssoc1 == ksaas.PersonClassificationType
                                                                            && ksss.KeepSepAssoc1Subset == ksaas.PersonClassificationSubset
                                                                            && ksss.DeleteFlag == 0
                                                                      select new TrackingConflictVm
                                                                      {

                                                                          PersonId = ksaas.PersonId,
                                                                          InmateId = ksaas.InmateId,
                                                                          InmateNumber = ksaas.InmateNumber,
                                                                          KeepSepReason = ksss.KeepSepReason,
                                                                          KeepSepAssoc2 = ksss.KeepSepAssoc2,
                                                                          KeepSepAssoc2Subset = ksss.KeepSepAssoc2Subset,
                                                                          KeepSepAssoc1 = ksss.KeepSepAssoc1,
                                                                          KeepSepAssoc1Subset = ksss.KeepSepAssoc1Subset
                                                                      };

                IQueryable<TrackingConflictVm> tLstKeepSepSubsetSubset = from ksss in _context.KeepSepSubsetSubset
                                                                         from ksaas in lstPerClassifySelectedInmates
                                                                         where ksss.KeepSepAssoc2 == ksaas.PersonClassificationType
                                                                               && ksss.KeepSepAssoc2Subset == ksaas.PersonClassificationSubset
                                                                               && ksss.DeleteFlag == 0
                                                                         select new TrackingConflictVm
                                                                         {
                                                                             PersonId = ksaas.PersonId,
                                                                             InmateId = ksaas.InmateId,
                                                                             InmateNumber = ksaas.InmateNumber,
                                                                             KeepSepReason = ksss.KeepSepReason,
                                                                             KeepSepAssoc2 = ksss.KeepSepAssoc2,
                                                                             KeepSepAssoc2Subset = ksss.KeepSepAssoc2Subset,
                                                                             KeepSepAssoc1 = ksss.KeepSepAssoc1,
                                                                             KeepSepAssoc1Subset = ksss.KeepSepAssoc1Subset
                                                                         };


                lstKeepSeeps.AddRange(from ksaa in keepSepSubsetSubsetT
                                      from lksaas in lstPerClassifySelectedInmates
                                      where lksaas.PersonClassificationType == ksaa.KeepSepAssoc2
                                            && lksaas.PersonClassificationSubset == ksaa.KeepSepAssoc2Subset
                                            && ksaa.PersonId != lksaas.PersonId
                                      select new TrackingConflictVm
                                      {
                                          ConflictType = KeepSepLabel.SUBSETKEEPSEPSUBSET,
                                          KeepSepPersonId = ksaa.PersonId,
                                          KeepSepInmateId = ksaa.InmateId,
                                          KeepSepInmateNumber = ksaa.InmateNumber,
                                          ConflictDescription = ksaa.KeepSepReason,
                                          KeepSepAssoc1 = ksaa.KeepSepAssoc2,
                                          KeepSepAssocSubset1 = ksaa.KeepSepAssoc2Subset,
                                          KeepSepAssoc2 = ksaa.KeepSepAssoc1,
                                          KeepSepAssocSubset2 = ksaa.KeepSepAssoc1Subset,

                                          PersonId = lksaas.PersonId,
                                          InmateId = lksaas.InmateId,
                                          InmateNumber = lksaas.InmateNumber
                                      });

                lstKeepSeeps.AddRange(from ksaa in tLstKeepSepSubsetSubset
                                      from lksaas in lstPerClassifySelectedInmates
                                      where lksaas.PersonClassificationType == ksaa.KeepSepAssoc1
                                            && lksaas.PersonClassificationSubset == ksaa.KeepSepAssoc1Subset
                                            && ksaa.PersonId != lksaas.PersonId
                                      select new TrackingConflictVm
                                      {
                                          ConflictType = KeepSepLabel.SUBSETKEEPSEPSUBSET,
                                          KeepSepPersonId = ksaa.PersonId,
                                          KeepSepInmateId = ksaa.InmateId,
                                          KeepSepInmateNumber = ksaa.InmateNumber,
                                          ConflictDescription = ksaa.KeepSepReason,
                                          KeepSepAssoc1 = ksaa.KeepSepAssoc2,
                                          KeepSepAssocSubset1 = ksaa.KeepSepAssoc2Subset,
                                          KeepSepAssoc2 = ksaa.KeepSepAssoc1,
                                          KeepSepAssocSubset2 = ksaa.KeepSepAssoc1Subset,

                                          PersonId = lksaas.PersonId,
                                          InmateId = lksaas.InmateId,
                                          InmateNumber = lksaas.InmateNumber
                                      });
            }
            #endregion
            trackingConflictDetail.AddRange(lstKeepSeeps);
            return trackingConflictDetail;
        }
        private List<TrackingConflictVm> VisitHousingLockDownConflict(Inmate inmate, VisitSchedule visitSchedule)
        {
            List<TrackingConflictVm> conflict = new List<TrackingConflictVm>();
            List<HousingLockdown> lockDownDetails = _context.HousingLockdown.Where(w =>
                visitSchedule.StartDate < w.EndLockdown &&
                visitSchedule.EndDate >= w.EndLockdown ||
                visitSchedule.StartDate <= w.StartLockdown && visitSchedule.EndDate > w.StartLockdown ||
                visitSchedule.StartDate == w.StartLockdown && visitSchedule.EndDate == w.EndLockdown ||
                visitSchedule.StartDate > w.StartLockdown && visitSchedule.EndDate < w.EndLockdown && !w.DeleteFlag).ToList();
            IQueryable<HousingUnit> housingUnits = _context.HousingUnit;
            List<HousingGroupAssign> housingAssign = _context.HousingGroupAssign.Where(w => w.HousingUnitListId.HasValue).ToList();
            HousingDetail housingInfo = housingUnits.Where(w => w.HousingUnitId == inmate.HousingUnitId)
                .Select(s => new HousingDetail
                {
                    HousingUnitId = s.HousingUnitId,
                    FacilityId = s.FacilityId,
                    FacilityAbbr = s.Facility.FacilityAbbr,
                    HousingUnitListId = s.HousingUnitListId,
                    HousingUnitLocation = s.HousingUnitLocation,
                    HousingUnitNumber = s.HousingUnitNumber,
                    HousingUnitBedLocation = s.HousingUnitBedLocation,
                    HousingUnitBedNumber = s.HousingUnitBedNumber,
                    HousingUnitBedGroupId = s.HousingUnitListBedGroupId ?? 0
                }).Single();
            lockDownDetails.ForEach(lockDown =>
            {
                TrackingConflictVm trackingConflict = new TrackingConflictVm()
                {
                    ConflictType = ConflictTypeConstants.HOUSINGLOCKDOWNCONFLICT,
                    PersonId = inmate.PersonId,
                    InmateNumber = inmate.InmateNumber,
                    InmateId = inmate.InmateId,
                    HousingInfo = housingInfo,
                    StartLockdown = lockDown.StartLockdown,
                    EndLockdown = lockDown.EndLockdown
                };
                switch (lockDown.LockdownSource)
                {
                    case "Facility":
                        if (lockDown.SourceId == housingInfo.FacilityId)
                        {
                            trackingConflict.LockDown = LockDownType.Facility;
                            _trackingConflictDetails.Add(trackingConflict);
                            conflict.Add(trackingConflict);
                        }
                        break;
                    case "Building":
                        KeyValuePair<string, int> buildingInfo = housingUnits.Where(w =>
                             w.HousingUnitListId == lockDown.SourceId).Select(se =>
                             new KeyValuePair<string, int>(se.HousingUnitLocation, se.FacilityId)).First();
                        if (buildingInfo.Key == housingInfo.HousingUnitLocation && buildingInfo.Value == housingInfo.FacilityId)
                        {
                            trackingConflict.LockDown = LockDownType.Building;
                            _trackingConflictDetails.Add(trackingConflict);
                            conflict.Add(trackingConflict);
                        }
                        break;
                    case "Pod":
                        if (lockDown.SourceId == housingInfo.HousingUnitListId)
                        {
                            trackingConflict.LockDown = LockDownType.Pod;
                            _trackingConflictDetails.Add(trackingConflict);
                            conflict.Add(trackingConflict);
                        }
                        break;
                    case "Cell":
                        KeyValuePair<string, int> cellInfo = housingUnits.Where(si =>
                             si.HousingUnitId == lockDown.SourceId).Select(se =>
                             new KeyValuePair<string, int>(se.HousingUnitBedNumber, se.HousingUnitListId)).Single();
                        if (cellInfo.Key == housingInfo.HousingUnitBedNumber && cellInfo.Value == housingInfo.HousingUnitListId)
                        {
                            trackingConflict.LockDown = LockDownType.Cell;
                            _trackingConflictDetails.Add(trackingConflict);
                            conflict.Add(trackingConflict);
                        }
                        break;
                    case "CellGroup":
                        if (lockDown.SourceId == housingInfo.HousingUnitBedGroupId)
                        {
                            trackingConflict.LockDown = LockDownType.CellGroup;
                            _trackingConflictDetails.Add(trackingConflict);
                            conflict.Add(trackingConflict);
                        }
                        break;
                    case "HousingGroup":
                        List<int?> housingUnitListIds = housingAssign.Where(w =>
                            w.HousingGroupId == lockDown.SourceId && w.HousingUnitListId.HasValue).Select(sa => sa.HousingUnitListId).ToList();
                        if (housingUnitListIds.Contains(housingInfo.HousingUnitListId))
                        {
                            trackingConflict.LockDown = LockDownType.CellGroup;
                            _trackingConflictDetails.Add(trackingConflict);
                            conflict.Add(trackingConflict);
                        }
                        break;
                }

            });
            return conflict;
        }
        #endregion

        #region Step 4 - Confirm Visitor ID

        public ConfirmVisitorIdVm GetConfirmVisitorIdDetails(int inmateId, int scheduleId)
        {
            ConfirmVisitorIdVm confirmVisitorIdVm = new ConfirmVisitorIdVm
            {
                //Inmate Housing details
                InmateDetail = GetVisitInmateDetails(inmateId),

                //Schedule details
                VisitSchedule = _context.Visit.Where(w => w.ScheduleId == scheduleId)
                    .Select(s => new VisitSchedule
                    {
                        RoomLocation = s.LocationId > 0 ? s.Location.PrivilegeDescription : string.Empty,
                        StartDate = s.StartDate,
                        EndDate = s.EndDate,
                        ReasonId = s.ReasonId,
                        TypeId = s.TypeId,
                        VisitBoothId = s.VisitBooth,
                        VisitorPassCombineFlag = s.LocationId > 0 ? s.Location.VisitorPassCombineFlag : false,
                        ConfirmIdFlag = s.ConfirmIdFlag,
                        AckBackgroundFlag = s.AcknowledgementBackground,
                        VisitOpenScheduleFlag = s.LocationId > 0 ? s.Location.VisitOpenScheduleFlag : false
                    }).Single()
            };

            //Getting Reason, Type and Visit Booth details
            List<LookupVm> lstLookup = _commonService.GetLookups(new[] { LookupConstants.VISREAS,
                LookupConstants.VISITTYPE, LookupConstants.VISBOOTH });

            confirmVisitorIdVm.VisitSchedule.Reason = confirmVisitorIdVm.VisitSchedule.ReasonId.HasValue
                ? lstLookup.SingleOrDefault(w =>
                    w.LookupType == LookupConstants.VISREAS &&
                    w.LookupIndex == confirmVisitorIdVm.VisitSchedule.ReasonId)?.LookupDescription
                : null;

            LookupVm lookupVm = confirmVisitorIdVm.VisitSchedule.TypeId.HasValue
                ? lstLookup.SingleOrDefault(w =>
                    w.LookupType == LookupConstants.VISITTYPE &&
                    w.LookupIndex == confirmVisitorIdVm.VisitSchedule.TypeId)
                : null;

            if (lookupVm != null)
            {
                confirmVisitorIdVm.VisitSchedule.Type = lookupVm.LookupDescription;
                confirmVisitorIdVm.VisitSchedule.VisitTypeLookupFlag6 = lookupVm.LookupFlag6;
                confirmVisitorIdVm.VisitSchedule.VisitTypeLookupFlag7 = lookupVm.LookupFlag7;
                confirmVisitorIdVm.VisitSchedule.VisitTypeLookupFlag8 = lookupVm.LookupFlag8;
                confirmVisitorIdVm.VisitSchedule.VisitTypeLookupFlag9 = lookupVm.LookupFlag9;
            }

            confirmVisitorIdVm.VisitSchedule.VisitBooth = confirmVisitorIdVm.VisitSchedule.VisitBoothId.HasValue
                ? lstLookup.SingleOrDefault(w =>
                    w.LookupType == LookupConstants.VISBOOTH &&
                    w.LookupIndex == confirmVisitorIdVm.VisitSchedule.VisitBoothId)?.LookupDescription
                : null;

            //Getting Visitor Name, Identity, Address, Characteristics and Photo
            confirmVisitorIdVm.LstVisitorIdentityAndRelationship = _context.VisitToVisitor
                .Where(w => w.ScheduleId == scheduleId)
                .OrderBy(o => o.VisitToVisitorId)
                .Select(s => new VisitorIdentityAndRelationship
                {
                    PersonId = s.PersonId,
                    InmateId = s.Visit.InmateId,
                    VisitorIdType = s.VisitorIdType,
                    VisitorIdNumber = s.VisitorIdNumber,
                    VisitorIdState = s.VisitorIdState,
                    VisitorType = s.Visitor.VisitorType
                }).ToList();

            List<int> lstPersonId =
                confirmVisitorIdVm.LstVisitorIdentityAndRelationship.Select(s => s.PersonId).ToList();

            confirmVisitorIdVm.LstVisitorIdentityAndRelationship = GetVisitorRelationship(true, lstPersonId,
                confirmVisitorIdVm.LstVisitorIdentityAndRelationship);

            //Site Option for NCIC RUN
            confirmVisitorIdVm.SiteOption = _commonService.GetSiteOptionValue(SiteOptionsConstants.NCIC_RUN);

            //To get RequestActionLookupId when VisitorBackgroundFlag is true
            confirmVisitorIdVm.RequestActionLookupId = _context.RequestActionLookup.FirstOrDefault(s =>
                                                           s.VisitorBackgroundFlag)?.RequestActionLookupId ?? 0;

            //Deny Reason
            confirmVisitorIdVm.LstDenyReason =
                _commonService.GetLookupKeyValuePairs(LookupConstants.VISITDENYREAS);

            confirmVisitorIdVm.IsVisitDeny = GetVisitDenyFlag();

            return confirmVisitorIdVm;
        }

        private List<VisitorIdentityAndRelationship> GetVisitorRelationship(bool getRelationship, List<int> lstPersonId,
            List<VisitorIdentityAndRelationship> lstVisitorDetails)
        {
            List<VisitorToInmate> lstVisitorToInmate = new List<VisitorToInmate>();
            List<LookupVm> lstLookup = new List<LookupVm>();

            if (getRelationship)
            {
                lstVisitorToInmate =
                    _context.VisitorToInmate.Where(w => lstPersonId.Contains(w.VisitorId ?? 0)).ToList();

                lstLookup = _commonService.GetLookups(new[] { LookupConstants.RELATIONS, LookupConstants.VISTYPE });
            }

            lstVisitorDetails.ForEach(item =>
            {

                //Person details
                item.PersonIdentity = GetPersonBasicDetails(item.PersonId);
                if (!getRelationship) return;

                //Personal Relationship
                int relationshipId = lstVisitorToInmate.FirstOrDefault(w => w.VisitorId == item.PersonId
                    && w.InmateId == item.InmateId)?.VisitorRelationship ?? 0;

                if (relationshipId > 0)
                {
                    item.Relationship = lstLookup.SingleOrDefault(l => l.LookupIndex == relationshipId
                        && l.LookupType == LookupConstants.RELATIONS)?.LookupDescription;
                }

                if (item.VisitorType > 0)
                {
                    item.ProfessionalType = lstLookup.SingleOrDefault(l => l.LookupIndex == item.VisitorType &&
                        l.LookupType == LookupConstants.VISTYPE)?.LookupDescription;
                }

            });

            return lstVisitorDetails;
        }

        public async Task<int> UpdateConfirmIdDetails(int scheduleId)
        {
            Visit visit = _context.Visit.Single(w => w.ScheduleId == scheduleId);
            visit.ConfirmIdFlag = true;
            visit.ConfirmIdBy = _personnelId;
            visit.ConfirmIdDate = DateTime.Now;
            return await _context.SaveChangesAsync();
        }

        public async Task<int> UpdateAcknowledgementBackground(int scheduleId, bool backgroundFlag)
        {
            Visit visit = _context.Visit.Single(w => w.ScheduleId == scheduleId);
            visit.AcknowledgementBackground = backgroundFlag;
            return await _context.SaveChangesAsync();
        }

        #endregion

        #region Step 5 - Complete Visit Registration

        public async Task<int> UpdateCompleteRegistration(int scheduleId)
        {
            Visit visit = _context.Visit.Single(w => w.ScheduleId == scheduleId);
            visit.CompleteRegBy = _personnelId;
            visit.CompleteRegDate = DateTime.Now;
            visit.CompleteRegistration = true;
            return await _context.SaveChangesAsync();
        }

        public async Task<int> DeleteVisitRegistration(int scheduleId)
        {
            Schedule schedule = _context.Schedule.Single(w => w.ScheduleId == scheduleId);
            schedule.DeleteFlag = true;
            schedule.DeleteBy = _personnelId;
            schedule.DeleteDate = DateTime.Now;
            return await _context.SaveChangesAsync();
        }

        #endregion

        #region Old Code

        public List<RegisterDetails> GetVisitorList(SearchRegisterDetails searchDetails)
        {
            List<Lookup> lookUpList = GetLookupValue();

            List<int> lstVisitor = _context.VisitToVisitor.Select(s => s.PersonId).ToList();

            IQueryable<VisitToVisitor> visitList = _context.VisitToVisitor.Where(v =>
                !v.Visit.DeleteFlag && v.Visit.VisitDenyFlag == 0
                && DateTime.Now.Date <= v.Visit.StartDate.Date &&
                !v.Visit.EndDate.HasValue && lstVisitor.Contains(v.PersonId));

            if (searchDetails.FacilityId > 0)
            {
                visitList = visitList.Where(v => v.Visit.Inmate.FacilityId == searchDetails.FacilityId);
            }

            if (searchDetails.InmateId > 0)
            {
                visitList = visitList.Where(v => v.Visit.InmateId == searchDetails.InmateId);
            }

            if (!string.IsNullOrEmpty(searchDetails.HousingLocation) &&
                !string.IsNullOrEmpty(searchDetails.HousingNumber))
            {
                visitList = visitList.Where(v =>
                    v.Visit.Inmate.HousingUnit.HousingUnitLocation == searchDetails.HousingLocation &&
                    v.Visit.Inmate.HousingUnit.HousingUnitNumber == searchDetails.HousingNumber);
            }

            if (!string.IsNullOrEmpty(searchDetails.VisitorLastName))
            {
                visitList = visitList.Where(v => v.Visitor.PersonLastName.StartsWith(searchDetails.VisitorLastName));
            }

            if (!string.IsNullOrEmpty(searchDetails.VisitorFirstName))
            {
                visitList = visitList.Where(v => v.Visitor.PersonFirstName.StartsWith(searchDetails.VisitorFirstName));
            }

            if (!string.IsNullOrEmpty(searchDetails.VisitorIdNumber))
            {
                visitList = visitList.Where(v => v.VisitorIdNumber == searchDetails.VisitorIdNumber);
            }

            List<RegisterDetails> registerList = visitList.Select(v => new RegisterDetails
            {
                VisitorId = v.PersonId,
                PersonId = v.PersonId,
                InmateLocation = v.Visit.Inmate.InmateCurrentTrack,
                VisitorLocation = v.Visit.Location.PrivilegeDescription,
                VisitorLocationId = v.Visit.LocationId,
                VisitSecondaryFlag = v.Visit.VisitSecondaryFlag,
                VisitorTimeIn = v.Visit.StartDate.ToString(DateConstants.HOURSMINUTES),
                VisitorIdType = v.VisitorIdType,
                VisitorNumber = v.VisitorIdNumber,
                VisitorState = v.VisitorIdState,
                VisitorNotes = v.Visitor.VisitorNotes,
                VisitorDateOut = v.Visit.EndDate,
                VisitorDateIn = v.Visit.StartDate,
                VisitorSystemAlerts = v.Visit.VisitSystemFlagString,
                ExceedMaxTime = v.Visit.ExceedMaxTime,
                //Relationship = v.VisitorRelationship,
                InmateInfo = new PersonInfo
                {
                    PersonFirstName = v.Visitor.PersonFirstName,
                    PersonLastName = v.Visitor.PersonLastName,
                    PersonMiddleName = v.Visitor.PersonMiddleName,
                    PersonSuffix = v.Visitor.PersonSuffix,
                    InmateNumber = v.Visit.Inmate.InmateNumber,
                    InmateId = v.Visit.Inmate.InmateId
                },
                HousingInfo = new HousingDetail
                {
                    HousingUnitLocation = string.IsNullOrEmpty(v.Visit.Inmate.HousingUnit.HousingUnitLocation)
                        ? "NO HOUSING"
                        : v.Visit.Inmate.HousingUnit.HousingUnitLocation,
                    HousingUnitNumber = string.IsNullOrEmpty(v.Visit.Inmate.HousingUnit.HousingUnitLocation)
                        ? ""
                        : v.Visit.Inmate.HousingUnit.HousingUnitNumber,
                    HousingUnitBedLocation = string.IsNullOrEmpty(v.Visit.Inmate.HousingUnit.HousingUnitLocation)
                        ? ""
                        : v.Visit.Inmate.HousingUnit.HousingUnitBedLocation,
                    HousingUnitBedNumber = string.IsNullOrEmpty(v.Visit.Inmate.HousingUnit.HousingUnitLocation)
                        ? ""
                        : v.Visit.Inmate.HousingUnit.HousingUnitBedNumber
                },
                HousingUnitListId = v.Visit.Inmate.HousingUnit.HousingUnitListId,
                VisitorInfo = new PersonInfo
                {
                    PersonFirstName = v.Visitor.PersonFirstName,
                    PersonLastName = v.Visitor.PersonLastName,
                    PersonMiddleName = v.Visitor.PersonMiddleName,
                    PersonSuffix = v.Visitor.PersonSuffix
                },
                VisitorBooth = lookUpList.SingleOrDefault(l =>
                    l.LookupIndex == v.Visit.VisitBooth && l.LookupType == LookupConstants.VISBOOTH).LookupDescription,
                VisitorLocker = lookUpList.SingleOrDefault(l =>
                        l.LookupIndex == v.Visit.VisitorLocker && l.LookupType == LookupConstants.VISLOCKER)
                    .LookupDescription,
                VisitorBadgenumber = lookUpList.SingleOrDefault(l =>
                        l.LookupIndex == v.Visit.VisitorBadgeNumber && l.LookupType == LookupConstants.VISBADGE)
                    .LookupDescription,
                //VisitorStatus = v.InmateTrak.InmateTrakId > 0 ? VisitorStatus.InProgress : VisitorStatus.Registered
            }).ToList();

            registerList.ForEach(item =>
            {
                //item.VisitorListId = _context.Visitor.Where(v => v.PersonId == item.PersonId)
                //	.Select(x => x.VisitorId).FirstOrDefault();

                int relationshipId = _context.VisitorToInmate.Where(v =>
                    v.InmateId == item.InmateInfo.InmateId && v.VisitorId == item.PersonId &&
                    item.VisitorProfessionalFlag.HasValue)
                    .Select(x => x.VisitorRelationship ?? 0).FirstOrDefault();
                if (relationshipId > 0)
                {
                    item.Relationship = lookUpList.SingleOrDefault(l =>
                        l.LookupIndex == relationshipId && l.LookupType == LookupConstants.RELATIONS)?.LookupDescription;
                }

                if (item.PersonnelOutId.HasValue)
                {
                    item.PersonnelOut = _context.Personnel.Where(x => x.PersonnelId == item.PersonnelOutId).Select(p =>
                        new PersonInfo
                        {
                            PersonFirstName = p.PersonNavigation.PersonFirstName,
                            PersonLastName = p.PersonNavigation.PersonLastName,
                            PersonMiddleName = p.PersonNavigation.PersonMiddleName
                        }).SingleOrDefault();
                }

                if (item.PersonnelInId.HasValue)
                {
                    item.PersonnelIn = _context.Personnel.Where(x => x.PersonnelId == item.PersonnelInId).Select(p =>
                        new PersonInfo
                        {
                            PersonFirstName = p.PersonNavigation.PersonFirstName,
                            PersonLastName = p.PersonNavigation.PersonLastName,
                            PersonMiddleName = p.PersonNavigation.PersonMiddleName
                        }).SingleOrDefault();
                }

            });
            return registerList;
        }

        private List<Lookup> GetLookupValue() => _context.Lookup.Where(x =>
                (x.LookupType.Contains(LookupConstants.RELATIONS) ||
                 x.LookupType.Contains(LookupConstants.VISBOOTH) ||
                 x.LookupType.Contains(LookupConstants.VISLOCKER) ||
                 x.LookupType.Contains(LookupConstants.VISBADGE) ||
                 x.LookupType.Contains(LookupConstants.VISPERIDTYPE) ||
                 x.LookupType.Contains(LookupConstants.VISREAS) ||
                 x.LookupType.Contains(LookupConstants.VISTYPE)) && x.LookupInactive == 0).ToList();

        private int GetExitVisitor(int inmateId) =>
            _context.VisitToVisitor.Count(v => v.Visit.InmateId == inmateId &&
            //v.InmateTrakId.HasValue &&
            !v.Visit.EndDate.HasValue && v.Visit.StartDate.Date == DateTime.Now.Date);

        public async Task<int> UpdateExitVisitor(KeyValuePair<int, int> exitVisitor)
        {
            int visitorCount = GetExitVisitor(exitVisitor.Key);

            VisitToVisitor visit = _context.VisitToVisitor.Single(v => v.ScheduleId == exitVisitor.Value);
            if (!visit.Visit.EndDate.HasValue)
            {
                visit.Visit.EndDate = DateTime.Now;
                visit.Visit.Duration = DateTime.Now.Subtract(visit.Visit.StartDate);
            }

            if (visitorCount == 0)
            {
                InmateTrak inmateTrak = _context.InmateTrak.SingleOrDefault(i =>
                i.InmateId == exitVisitor.Key && !i.InmateTrakDateIn.HasValue);
                if (inmateTrak != null)
                {
                    inmateTrak.InmateTrakDateIn = DateTime.Now;
                }

                Inmate inmate = _context.Inmate.Single(i => i.InmateId == exitVisitor.Key);
                inmate.InmateCurrentTrack = null;
                inmate.InmateCurrentTrackId = null;
            }
            return await _context.SaveChangesAsync();
        }

        public List<VisitDetails> GetScheduleVisitDetails(VisitParam objVisitParam)
        {
            List<VisitDetails> lstRegisterationDetails = _context.VisitToVisitor
                .Where(w => w.Visit.RegFacilityId == objVisitParam.FacilityId
                    && w.Visit.VisitDenyFlag == 0 && !w.Visit.CompleteVisitFlag &&
                    (!objVisitParam.AllActiveSchedule || !w.Visit.DeleteFlag)
                    && (!objVisitParam.CompleteRegistration || !w.Visit.CompleteRegistration)
                    && (!objVisitParam.PendingVisit || w.Visit.CompleteRegistration &&
                        (w.Visit.Location.VisitOpenScheduleFlag || w.Visit.StartDate >= DateTime.Now) &&
                        (!w.Visit.Location.VisitOpenScheduleFlag && !w.Visit.Location.VisitAllowProfOnlyFlag ||
                            !w.Visit.InmateTrackingStart.HasValue)) &&
                    (!objVisitParam.VisitsInProgress || w.Visit.CompleteRegistration &&
                        (w.Visit.Location.VisitOpenScheduleFlag || w.Visit.StartDate < DateTime.Now) &&
                        (!w.Visit.Location.VisitOpenScheduleFlag && !w.Visit.Location.VisitAllowProfOnlyFlag ||
                            w.Visit.InmateTrackingStart.HasValue)) &&
                    (!objVisitParam.PendingVisit || w.Visit.CompleteRegistration &&
                        (w.Visit.Location.VisitOpenScheduleFlag || !w.Visit.InmateTrackingStart.HasValue) &&
                        (!w.Visit.Location.VisitOpenScheduleFlag && !w.Visit.Location.VisitAllowPersonalOnlyFlag ||
                            !w.Visit.InmateTrackingStart.HasValue)) &&
                    (!objVisitParam.VisitsInProgress || w.Visit.CompleteRegistration &&
                        (w.Visit.Location.VisitOpenScheduleFlag || w.Visit.InmateTrackingStart.HasValue) &&
                        (!w.Visit.Location.VisitOpenScheduleFlag && !w.Visit.Location.VisitAllowPersonalOnlyFlag ||
                            w.Visit.InmateTrackingStart.HasValue)))
                .OrderBy(o => o.Visit.CreateDate)
                .Select(s => new VisitDetails
                {
                    VisitToVisitorId = s.VisitToVisitorId,
                    VisitorType = s.Visit.VisitorType ?? 0,
                    ScheduleId = s.ScheduleId,
                    PrimaryVisitor = new PersonInfoVm
                    {
                        PersonId = s.PersonId,
                        PersonLastName = s.Visitor.PersonLastName,
                        PersonFirstName = s.Visitor.PersonFirstName
                    },
                    InmateInfo = new PersonInfoVm
                    {
                        InmateId = s.Visit.InmateId,
                        InmateNumber = s.Visit.Inmate.InmateNumber,
                        PersonLastName = s.Visit.Inmate.Person.PersonLastName,
                        PersonFirstName = s.Visit.Inmate.Person.PersonFirstName
                    },
                    DeleteFlag = s.Visit.DeleteFlag,
                    Location = s.Visit.Location.PrivilegeDescription,
                    CompleteVisitReason = s.Visit.CompleteVisitReason,
                    VisitDenyReason = s.Visit.VisitDenyReason,
                    VisitDenyNote = s.Visit.VisitDenyNote,
                    ScheduleDateTime = s.Visit.StartDate,
                    CountAsVisit = s.Visit.CountAsVisit,
                    AdultVisitorsCount = s.Visit.VisitAdditionAdultCount,
                    ChildVisitorsCount = s.Visit.VisitAdditionChildCount,
                    CompleteRegistration = s.Visit.CompleteRegistration,
                    InmateTrackingStart = s.Visit.InmateTrackingStart,
                    StartDate = s.Visit.StartDate,
                    EndDate = s.Visit.EndDate,
                    Time = s.Visit.Time,
                    Duration = s.Visit.Duration,
                    LocationId = s.Visit.LocationId ?? 0,
                    HousingDetails = new HousingDetail()
                    {
                        HousingUnitId = s.Visit.Inmate.HousingUnit.HousingUnitId > 0
                            ? s.Visit.Inmate.HousingUnit.HousingUnitId : default,
                        HousingUnitLocation = s.Visit.Inmate.HousingUnit.HousingUnitLocation,
                        HousingUnitNumber = s.Visit.Inmate.HousingUnit.HousingUnitNumber,
                        HousingUnitBedLocation = s.Visit.Inmate.HousingUnit.HousingUnitBedLocation,
                        HousingUnitBedNumber = s.Visit.Inmate.HousingUnit.HousingUnitBedNumber,
                        FacilityAbbr = s.Visit.Inmate.Facility.FacilityAbbr
                    },
                    InmateCurrentTrack = s.Visit.Inmate.InmateCurrentTrack,
                    VisitBoothId = s.Visit.VisitBooth,
                    ReasonId = s.Visit.ReasonId,
                    TypeId = s.Visit.TypeId,
                    VisitOpenScheduleFlag = s.Visit.LocationId.HasValue ?
                        s.Visit.Location.VisitOpenScheduleFlag : default
                }).ToList();

            List<int> lstPersonId = lstRegisterationDetails.Select(s => s.PrimaryVisitor.PersonId).ToList();

            List<VisitorToInmate> lstVisitorToInmate = _context.VisitorToInmate
                .Where(w => lstPersonId.Contains(w.VisitorId ?? 0)).ToList();

            List<VisitorProfessional> lstVisitorProfessional = _context.VisitorProfessional
                .Where(w => lstPersonId.Contains(w.PersonId)).Select(s => new VisitorProfessional
                {
                    PersonId = s.PersonId,
                    VisitorType = s.VisitorType
                }).ToList();

            List<LookupVm> lstLookup = _commonService.GetLookups(new[]
                {LookupConstants.RELATIONS, LookupConstants.VISTYPE,LookupConstants.VISITTYPE,
            LookupConstants.VISBOOTH,LookupConstants.VISREAS});


            List<int> lstScheduleId = lstRegisterationDetails.Select(s => s.ScheduleId).ToList();

            List<AoWizardProgressVm> lstVisitRegistrationWizardProgress = GetVisitRegistrationWizardProgress(lstScheduleId);
            List<Privileges> lstPrivilages = _context.Privileges.Where(w => w.InactiveFlag == 0).ToList();
            lstRegisterationDetails.ForEach(item =>
            {
                //Personal Relationship
                int relationshipId = lstVisitorToInmate.FirstOrDefault(w => w.VisitorId == item.PrimaryVisitor.PersonId
                                            && w.InmateId == item.InmateInfo.InmateId)?.VisitorRelationship ?? 0;

                if (relationshipId > 0)
                {
                    item.Relationship = lstLookup.SingleOrDefault(l =>
                        l.LookupIndex == relationshipId && l.LookupType == LookupConstants.RELATIONS)?.LookupDescription;
                }
                //Getting Reason, Type and Visit Booth details

                item.Reason = item.ReasonId.HasValue
                    ? lstLookup.SingleOrDefault(w =>
                        w.LookupType == LookupConstants.VISREAS &&
                        w.LookupIndex == item.ReasonId)?.LookupDescription
                    : null;

                LookupVm lookupVm = item.TypeId.HasValue
                    ? lstLookup.SingleOrDefault(w =>
                        w.LookupType == LookupConstants.VISITTYPE &&
                        w.LookupIndex == item.TypeId)
                    : null;

                if (lookupVm != null)
                {
                    item.Type = lookupVm.LookupDescription;
                }

                item.VisitBooth = item.VisitBoothId.HasValue
                    ? lstLookup.SingleOrDefault(w =>
                        w.LookupType == LookupConstants.VISBOOTH &&
                        w.LookupIndex == item.VisitBoothId)?.LookupDescription
                    : null;

                //Professional Relationship - needs to be rechecked
                VisitorProfessional visitorProfessional = lstVisitorProfessional.SingleOrDefault(w =>
                    w.PersonId == item.PrimaryVisitor.PersonId);

                if (visitorProfessional != null)
                {
                    // item.VisitorType = visitorProfessional.VisitorType ?? 0;

                    if (item.VisitorType > 0)
                    {
                        item.Relationship = lstLookup.SingleOrDefault(l => l.LookupIndex == item.VisitorType
                                                        && l.LookupType == LookupConstants.VISTYPE)?.LookupDescription;
                    }
                }

                //Wizard details
                item.RegistrationProgress =
                    lstVisitRegistrationWizardProgress.FirstOrDefault(w => w.ScheduleId == item.ScheduleId);

                InmateTrak lstInmateTrak = _context.InmateTrak.FirstOrDefault(w =>
                    w.InmateId == item.InmateInfo.InmateId && !w.InmateTrakDateIn.HasValue);

                if (lstInmateTrak != null && lstInmateTrak.EnrouteFinalLocationId > 0)
                {
                    item.Destination = lstPrivilages.FirstOrDefault(s =>
                                                    s.PrivilegeId == lstInmateTrak.EnrouteFinalLocationId)?.PrivilegeDescription;
                    item.DestinationId = lstInmateTrak.EnrouteFinalLocationId;
                    item.EnrouteInFlag = lstInmateTrak.EnrouteInFlag;
                    item.EnrouteOutFlag = lstInmateTrak.EnrouteOutFlag;
                    item.EnrouteFinalFlag = lstInmateTrak.EnrouteFinalFlag;
                }

                if (item.LocationId == 0 || lstPrivilages.Count(f => f.PrivilegeId == item.LocationId) == 0) return;
                item.VisitOpenScheduleFlag = lstPrivilages.FirstOrDefault(f =>
                        f.PrivilegeId == item.LocationId && f.InactiveFlag != 1).VisitOpenScheduleFlag;
                item.VisitBoothManageFlag = lstPrivilages.FirstOrDefault(f =>
                        f.PrivilegeId == item.LocationId && f.InactiveFlag != 1).VisitBoothManageFlag;
            });

            return lstRegisterationDetails;
        }

        public List<CompleteVisitReason> GetCompleteVisits()
        {
            List<CompleteVisitReason> lstCompleteVisitReason = _context.Lookup.Where(w => w.LookupType == LookupConstants.VISITCOMPLETE
             && w.LookupInactive == 0).Select(s => new CompleteVisitReason
             {
                 LookupDescription = s.LookupDescription,
                 LookupFlag8 = s.LookupFlag8 ?? 0,
                 LookupIdentity = s.LookupIdentity,
                 LookupFlag9 = s.LookupFlag9,
                 LookupFlag6 = s.LookupFlag6 ?? 0
             }).ToList();
            return lstCompleteVisitReason;
        }

        public async Task<int> UpdateScheduleVisits(ScheduledVisits scheduledVisits)
        {
            if (scheduledVisits.ScheduleId > 0)
            {
                Visit visitScheduleDetails = _context.Visit.Single(s => s.ScheduleId == scheduledVisits.ScheduleId);
                visitScheduleDetails.CountAsVisit = scheduledVisits.CountAsVisits;
                visitScheduleDetails.CompleteVisitNote = scheduledVisits.ReasonNote;
                visitScheduleDetails.CompleteVisitReason = scheduledVisits.Reason;
                visitScheduleDetails.CompleteVisitFlag = true;
            }

            InmateTrak trackList = _context.InmateTrak
                .Where(x => x.InmateId == scheduledVisits.InmateId && !x.InmateTrakDateIn.HasValue)
                .OrderByDescending(o => o.InmateTrakId)
                .Select(x => x).FirstOrDefault();
            Inmate obInmate =
                _context.Inmate.Single(it => it.InmateId == scheduledVisits.InmateId);
            if (trackList != null && scheduledVisits.IsCheckInmate)
            {
                trackList.InmateTrakDateIn = DateTime.Now;
            }

            if (obInmate != null && scheduledVisits.IsCheckInmate)
            {
                obInmate.InmateCurrentTrack = null;
                obInmate.InmateCurrentTrackId = null;
            }

            return await _context.SaveChangesAsync();
        }
        public bool GetVisitationPermissionRights()
        {
            List<string> roleClaims = _iUserPermissionPolicy.GetClaims();
            return roleClaims.Contains(PermissionTypes.Admin) || roleClaims.Contains(
                PermissionTypes.FunctionPermission + OverrideConflictConstant.VisitationConflictOverRide +
                PermissionTypes.Edit);
        }

        #endregion
    }
}