using System;
using System.Data;
using GenerateTables.Models;
using Microsoft.ApplicationInsights.AspNetCore.TelemetryInitializers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using ScheduleWidget.Common;

// ReSharper disable once CheckNamespace
namespace ServerAPI.Tests
{
    public partial class DbInitialize
    {

        private void AppAoDetails()
        {
            Db.AppAoUserControlFields.AddRange(
                new AppAoUserControlFields
                {
                    AppAoUserControlFieldsId = 5,
                    AppAoUserControlId = 10,
                    AppAoFieldLabelId = 30,
                    CreateDate = DateTime.Now.AddDays(-2),
                    UpdateDate = DateTime.Now,
                    CreateBy = 11,
                    UpdateBy = 11,
                    FieldTagId = "txtbookdetails",
                    FieldVisible = 1,
                    FieldRequired = null,
                    ValidateCompleteWarning = 1,
                    ValidateCompleteRequired = 1,
                    ValidateCompleteBookTypeString = "OVERVIEW OF BOOKING DETAILS"
                },
                new AppAoUserControlFields
                {
                    AppAoUserControlFieldsId = 6,
                    AppAoUserControlId = 15,
                    AppAoFieldLabelId = 31,
                    CreateDate = DateTime.Now,
                    UpdateDate = DateTime.Now,
                    CreateBy = 12,
                    UpdateBy = 11,
                    FieldTagId = "txtbooks",
                    FieldVisible = 1,
                    FieldRequired = null,
                    ValidateCompleteBookTypeString = "BOOKING OFFICER ONLY HAVE FULL RIGHTS"
                },
                new AppAoUserControlFields
                {
                    AppAoUserControlFieldsId = 7,
                    AppAoUserControlId = 15,
                    AppAoFieldLabelId = 31,
                    CreateDate = DateTime.Now.AddDays(-1),
                    UpdateDate = DateTime.Now,
                    CreateBy = 12,
                    UpdateBy = 11,
                    FieldTagId = "drpcharges",
                    FieldVisible = 1,
                    FieldRequired = null,
                    ValidateCompleteBookTypeString = "LOCAL WARRENT WITH CHARGES",
                    ValidateCompleteWarning = 1,
                    ValidateCompleteRequired = 1
                },
                new AppAoUserControlFields
                {
                    AppAoUserControlFieldsId = 8,
                    AppAoUserControlId = 26,
                    AppAoFieldLabelId = 31,
                    CreateDate = DateTime.Now,
                    UpdateDate = DateTime.Now,
                    CreateBy = 12,
                    UpdateBy = 12,
                    FieldTagId = "TxtInmateNumber",
                    FieldVisible = 1,
                    FieldRequired = 1,
                    ValidateCompleteBookTypeString = null,
                    FieldLabel = "INMATE NUMBER"
                },
                new AppAoUserControlFields
                {
                    AppAoUserControlFieldsId = 9,
                    AppAoUserControlId = 26,
                    AppAoFieldLabelId = 11,
                    CreateDate = DateTime.Now,
                    UpdateDate = DateTime.Now,
                    CreateBy = 11,
                    UpdateBy = 12,
                    FieldTagId = "TxtSiteInmate",
                    FieldVisible = 1,
                    FieldRequired = 1,
                    ValidateCompleteBookTypeString = "TEXT VALUE",
                    FieldLabel = "SITE INMATE"
                },
                new AppAoUserControlFields
                {
                    AppAoUserControlFieldsId = 10,
                    AppAoUserControlId = 2,
                    AppAoFieldLabelId = 15,
                    CreateDate = DateTime.Now,
                    UpdateDate = DateTime.Now,
                    CreateBy = 11,
                    UpdateBy = 12,
                    FieldTagId = "TxtAKAInmateNum",
                    FieldVisible = null,
                    FieldRequired = 1,
                    ValidateCompleteBookTypeString = "INMATE NUMBER",
                    FieldLabel = "AKA INMATE NUMBER"
                },
                new AppAoUserControlFields
                {
                    AppAoUserControlFieldsId = 11,
                    AppAoUserControlId = 2,
                    AppAoFieldLabelId = 12,
                    CreateDate = DateTime.Now,
                    UpdateDate = DateTime.Now,
                    CreateBy = 11,
                    UpdateBy = 11,
                    FieldTagId = "TxtAKASiteInmateNum",
                    FieldVisible = 1,
                    FieldRequired = null,
                    FieldLabel = "AKA SITE INMATE NUMBER"
                },
                new AppAoUserControlFields
                {
                    AppAoUserControlFieldsId = 12,
                    AppAoUserControlId = 2,
                    AppAoFieldLabelId = 15,
                    CreateDate = DateTime.Now,
                    UpdateDate = DateTime.Now,
                    CreateBy = 12,
                    UpdateBy = 11,
                    FieldTagId = "TxtOtherPhoneNum",
                    FieldVisible = 1,
                    FieldRequired = 1,
                    ValidateCompleteBookTypeString = "PHONE",
                    FieldLabel = "OTHER PHONE NUMBER"
                },
                new AppAoUserControlFields
                {
                    AppAoUserControlFieldsId = 13,
                    AppAoUserControlId = 2,
                    AppAoFieldLabelId = 20,
                    CreateDate = DateTime.Now,
                    UpdateDate = DateTime.Now,
                    CreateBy = 12,
                    UpdateBy = 12,
                    FieldTagId = "TxtAFISNumber",
                    FieldVisible = null,
                    FieldRequired = 1,
                    ValidateCompleteBookTypeString = null,
                    FieldLabel = "AFIS NUMER"
                });

            Db.AppAoUserControlFieldsRestrict.AddRange(
                new AppAoUserControlFieldsRestrict
                {
                    AppAoUserControlFieldsRestrictId = 10,
                    CreateBy = 12,
                    UpdateBy = 12,
                    CreateDate = DateTime.Now.AddDays(-3),
                    UpdateDate = DateTime.Now.AddDays(-1),
                    AppAoUserControlFieldsId = 6,
                    ProtectFlag = 1,
                    RestrictFlag = 1,
                    GroupId = 11
                },
                new AppAoUserControlFieldsRestrict
                {
                    AppAoUserControlFieldsRestrictId = 11,
                    CreateBy = 11,
                    UpdateBy = 12,
                    CreateDate = DateTime.Now.AddDays(-1),
                    UpdateDate = DateTime.Now,
                    AppAoUserControlFieldsId = 5,
                    ProtectFlag = 1,
                    RestrictFlag = 1,
                    GroupId = 11
                }
            );

            Db.AoTaskQueue.AddRange(
                new AoTaskQueue
                {
                    AoTaskQueueId = 15,
                    CreateDate = DateTime.Now.AddDays(-10),
                    CreateBy = 11,
                    InmateId = 105,
                    CompleteBy = null,
                    CompleteFlag = false,
                    AoTaskLookupId = 10
                },
                new AoTaskQueue
                {
                    AoTaskQueueId = 16,
                    CreateDate = DateTime.Now.AddDays(-10),
                    CreateBy = 11,
                    InmateId = 105,
                    CompleteBy = null,
                    CompleteFlag = false,
                    AoTaskLookupId = 11,
                    PriorityFlag = false
                },
                new AoTaskQueue
                {
                    AoTaskQueueId = 17,
                    CreateDate = DateTime.Now.AddDays(-10),
                    CreateBy = 11,
                    InmateId = 105,
                    CompleteBy = null,
                    CompleteFlag = false,
                    AoTaskLookupId = 12,
                    PriorityFlag = true
                },
                new AoTaskQueue
                {
                    AoTaskQueueId = 18,
                    CreateDate = DateTime.Now.AddDays(-10),
                    CreateBy = 12,
                    InmateId = 103,
                    CompleteBy = null,
                    CompleteFlag = false,
                    AoTaskLookupId = 10,
                    PriorityFlag = true
                },
                new AoTaskQueue
                {
                    AoTaskQueueId = 19,
                    CreateDate = DateTime.Now.AddDays(-10),
                    CreateBy = 11,
                    InmateId = 104,
                    CompleteBy = 12,
                    CompleteFlag = false,
                    AoTaskLookupId = 10,
                    PriorityFlag = true
                }
                );

            Db.AoTaskLookup.AddRange(
                new AoTaskLookup
                {
                    AoTaskLookupId = 10,
                    DeleteFlag = false,
                    DeleteDate = null,
                    DeleteBy = 12,
                    TaskName = "MEDICAL FORM"
                },
                new AoTaskLookup
                {
                    AoTaskLookupId = 11,
                    DeleteFlag = false,
                    DeleteDate = null,
                    DeleteBy = 11,
                    TaskName = "LIVE SCAN"
                },
                new AoTaskLookup
                {
                    AoTaskLookupId = 12,
                    DeleteFlag = false,
                    DeleteDate = null,
                    DeleteBy = 12,
                    TaskName = "PRE CLASS"
                }
                );
            Db.AoWizardProgressIncarceration.AddRange(
                new AoWizardProgressIncarceration
                {
                    AoWizardProgressId = 100,
                    AoWizardId = 4,
                    IncarcerationId = 27
                },
                new AoWizardProgressIncarceration
                {
                    AoWizardProgressId = 101,
                    AoWizardId = 5,
                    IncarcerationId = 12
                },
                new AoWizardProgressIncarceration
                {
                    AoWizardProgressId = 102,
                    AoWizardId = 5,
                    IncarcerationId = 10
                },
                new AoWizardProgressIncarceration
                {
                    AoWizardProgressId = 103,
                    AoWizardId = 11,
                    IncarcerationId = 15
                }
            );
            Db.AoWizardProgressTempHold.AddRange(
                new AoWizardProgressTempHold
                {
                    TempHoldId = 13,
                    AoWizardProgressId = 11,
                    AoWizardId = 11
                }
                );

            Db.AoWizardStepProgress.AddRange(
                new AoWizardStepProgress
                {
                    AoWizardProgressId = 100,
                    AoWizardFacilityStepId = 10,
                    AoComponentId = 20,
                    AoWizardStepProgressId = 15,
                    StepCompleteById = 11
                },
                new AoWizardStepProgress
                {
                    AoWizardProgressId = 102,
                    AoWizardFacilityStepId = 10,
                    AoComponentId = 21,
                    AoWizardStepProgressId = 16,
                    StepCompleteById = 12
                }
                );

            Db.AoComponent.AddRange(
                new AoComponent
                {
                    AoComponentId = 20,
                    AppAofunctionalityId = null,
                    CanChangeVisibility = true,
                    ComponentName = "PersonIdentityComponent",
                    DisplayName = "Person Identity",
                    IsBookingData = false,
                    IsEntryScreen = true,
                    IsLastScreen = false
                },
                new AoComponent
                {
                    AoComponentId = 21,
                    AppAofunctionalityId = null,
                    CanChangeVisibility = true,
                    ComponentName = "PersonAkaComponent",
                    DisplayName = "Person - AKA",
                    IsBookingData = false,
                    IsEntryScreen = false,
                    IsLastScreen = false
                }
                );


            Db.AoTaskLookupAssign.AddRange(

                 new AoTaskLookupAssign
                 {
                     AoTaskLookupAssignId = 15,
                     CreateDate = DateTime.Now.AddDays(-10),
                     DeleteFlag = false,
                     FacilityId = 2,
                     CreateBy = 12,
                     DeleteDate = null,
                     DeleteBy = null,
                     AoTaskLookupId = 12,
                     TaskValidateLookup = "INTAKE COMPLETE"
                 },
                 new AoTaskLookupAssign
                 {
                     AoTaskLookupAssignId = 16,
                     CreateDate = DateTime.Now.AddDays(-10),
                     DeleteFlag = false,
                     FacilityId = 1,
                     CreateBy = 11,
                     DeleteDate = null,
                     DeleteBy = null,
                     AoTaskLookupId = 10
                 },
                 new AoTaskLookupAssign
                 {
                     AoTaskLookupAssignId = 17,
                     CreateDate = DateTime.Now.AddDays(-15),
                     DeleteFlag = false,
                     FacilityId = 2,
                     CreateBy = 12,
                     DeleteBy = null,
                     AoTaskLookupId = 10,
                     TaskCreateLookup = "BOOKING COMPLETE KEEPER EVENT"
                 }

             );


            Db.AoWizard.AddRange(
                new AoWizard
                {
                    AoWizardId = 1,
                    WizardName = "INTAKE",
                    IsActive = true,
                    IsSubWizard = false
                },
                new AoWizard
                {
                    AoWizardId = 2,
                    WizardName = "BOOKING",
                    IsActive = true,
                    IsSubWizard = false
                },
                new AoWizard
                {
                    AoWizardId = 3,
                    WizardName = "RELEASE",
                    IsActive = true,
                    IsSubWizard = false
                },
                new AoWizard
                {
                    AoWizardId = 4,
                    WizardName = "BOOK AND RELEASE",
                    IsActive = true,
                    IsSubWizard = false
                },
                new AoWizard
                {
                    AoWizardId = 11,
                    WizardName = "PRE-BOOK",
                    IsActive = true,
                    IsSubWizard = false

                },
                new AoWizard
                {
                    AoWizardId = 10,
                    WizardName = "COURT COMMIT",
                    IsActive = true,
                    IsSubWizard = false
                }
                );
            Db.AppAoWizard.AddRange(
               new AppAoWizard
               {
                   AppAoWizardId = 1,
                   WizardName = "INTAKE WIZARD",
                   ActiveFlag = 1,
                   AppAoId = 4
               },
               new AppAoWizard
               {
                   AppAoWizardId = 2,
                   WizardName = "BOOKING WIZARD",
                   ActiveFlag = 1,
                   AppAoId = 4
               }
           );
            Db.ScheduleProgram.AddRange(
                new ScheduleProgram
                {
                    // ProgramId = 9,
                    ScheduleId = 500

                }

                );


            Db.ScheduleInmate.AddRange(
                new ScheduleInmate
                {
                    ScheduleId = 13,
                    LocationId = 7,
                    StartDate = DateTime.Now,
                    EndDate = DateTime.Now,
                    IsSingleOccurrence = false,
                    DayInterval = DayInterval.Wed,
                    FrequencyType = FrequencyType.Weekly,
                    InmateId = 100

                },
                new ScheduleInmate
                {
                    ScheduleId = 14,
                    LocationId = 5,
                    StartDate = DateTime.Now.Date.AddHours(2).AddMinutes(15),
                    IsSingleOccurrence = false,
                    InmateId = 105

                }

                );


            Db.Schedule.AddRange(
                new Schedule
                {
                    ScheduleId = 10,
                    LocationId = 7,
                    EndDate = DateTime.Now.AddDays(4),
                    StartDate = DateTime.Now,
                    IsSingleOccurrence = false,
                    DayInterval = DayInterval.Sun,
                    FrequencyType = FrequencyType.Weekly

                },
                new Schedule
                {
                    ScheduleId = 11,
                    LocationId = 7,
                    EndDate = DateTime.Now.AddDays(1),
                    StartDate = DateTime.Now,
                    IsSingleOccurrence = false,
                    DayInterval = DayInterval.Tue,
                    FrequencyType = FrequencyType.Weekly
                },
                new Schedule
                {
                    ScheduleId = 12,
                    LocationId = 5,
                    StartDate = DateTime.Now,
                    EndDate = DateTime.Now,
                    IsSingleOccurrence = true
                },

                new Schedule
                {
                    ScheduleId = 15,
                    LocationId = 7,
                    StartDate = DateTime.Now.Date.AddHours(2).AddMinutes(15),
                    IsSingleOccurrence = false,
                },
                new Schedule
                {
                    ScheduleId = 16,
                    LocationId = 5,
                    StartDate = DateTime.Now.Date.AddHours(2).AddMinutes(15),
                    EndDate = DateTime.Now,
                    IsSingleOccurrence = false,
                }
                );

            Db.AoWizardFacility.AddRange(

                new AoWizardFacility
                {
                    AoWizardFacilityId = 10,
                    FacilityId = 1,
                    AoWizardId = 10,
                    IsSequential = true
                },
                new AoWizardFacility
                {
                    AoWizardFacilityId = 11,
                    FacilityId = 1,
                    AoWizardId = 11,
                    IsSequential = true
                }
                );

            Db.AttendanceAo.AddRange(
                new AttendanceAo
                {
                    AttendanceAoId = 10,
                    PersonnelId = 1,
                    AttendanceAoLastHousingDate = DateTime.Now,
                    AttendanceAoLastHousingNote = "ANOTHER WORK",
                    AttendanceAoLastHousingNumber = null
                },
                new AttendanceAo
                {
                    AttendanceAoId = 12,
                    AttendanceAoLastHousingNumber = "AO1040",
                    AttendanceAoLastHousingDate = DateTime.Now,
                    AttendanceAoLastHousingOfficerId = 12,
                    PersonnelId = 5
                },
                new AttendanceAo
                {
                    AttendanceAoId = 13,
                    PersonnelId = 11,
                    AttendanceAoLastHousingDate = DateTime.Now,
                    AttendanceAoLastHousingNumber = "A01041"
                }
            );

            Db.AttendanceAoHistory.AddRange(
                new AttendanceAoHistory
                {
                    AttendanceAoHistoryId = 10,
                    AttendanceAoId = 11,
                    AttendanceAoLastHousingFacilityId = 1,
                    AttendanceAoStatusDate = DateTime.Now.AddDays(-1)
                },
                new AttendanceAoHistory
                {
                    AttendanceAoHistoryId = 15,
                    AttendanceAoId = 10,
                    AttendanceAoStatusDate = DateTime.Now.AddDays(-2),
                    AttendanceAoLastHousingFacilityId = 2
                },
                new AttendanceAoHistory
                {
                    AttendanceAoHistoryId = 16,
                    AttendanceAoId = 13,
                    AttendanceAoStatus = 1,
                    AttendanceAoStatusDate = DateTime.Now.AddDays(-3),
                    AttendanceAoLastHousingFacilityId = 2,
                    AttendanceAoStatusNote = "CLOCK IN",
                    AttendanceAoStatusOfficerId = 11,
                    AttendanceAoLastLocDate = DateTime.Now.AddDays(-1)
                },
             new AttendanceAoHistory
             {
                 AttendanceAoHistoryId = 17,
                 AttendanceAoId = 13,
                 AttendanceAoStatus = 2,
                 AttendanceAoLastHousingFacilityId = 2,
                 AttendanceAoStatusNote = "CLOCK OUT",
                 AttendanceAoLastHousingOfficerId = 11,
                 AttendanceAoLastHousingLocation = "DOWN BED LOC1",
                 AttendanceAoLastLocTrack = "CHENNAI",
                 AttendanceAoLastLocOfficerId = 11,
                 AttendanceAoStatusDate = DateTime.Now.AddDays(-1),
                 AttendanceAoLastLocDate = DateTime.Now.AddDays(-2),
                 AttendanceAoStatusOfficerId = 11,
             }
            );

            Db.AppAoUserControl.AddRange(
                new AppAoUserControl
                {
                    AppAoUserControlId = 6,
                    ControlName = "Usercontrols/Person/personaka.ascx",
                    DisplayName = "PERSON - AKA",
                    ShowInIntakeWizard = 1,
                    ShowInReleaseWizard = 1
                },
                new AppAoUserControl
                {
                    AppAoUserControlId = 7,
                    ControlName = "Usercontrols/Person/personcontacts.ascx",
                    DisplayName = "PERSON - CONTACTS",
                    ShowInBookingWizard = 1
                }
            );

            Db.AppAoDetailChildXref.AddRange(
                new AppAoDetailChildXref
                {
                    AppAoSubModuleId = 15,
                    AppAoDetailChildXrefId = 5,
                    AppAoDetailChildId = 6,
                    AppAoDetailChildVisible = 0
                },
                new AppAoDetailChildXref
                {
                    AppAoSubModuleId = 20,
                    AppAoDetailChildXrefId = 6,
                    AppAoDetailChildId = 5,
                    AppAoDetailChildVisible = 1
                });
            Db.AppAoDetailChild.AddRange(
                new AppAoDetailChild
                {
                    AppAoDetailChildId = 5,
                    AppAoDetailParentId = 10,
                    AppAoDetailChildName = "ADDRESS"
                },
                new AppAoDetailChild
                {
                    AppAoDetailChildId = 6,
                    AppAoDetailParentId = 10,
                    AppAoDetailChildName = "CHAR"
                });

            Db.AppAoDetailParent.AddRange(
                new AppAoDetailParent
                {
                    AppAoDetailParentId = 10,
                    AppAoDetailParentName = "PERSON"
                },
                new AppAoDetailParent
                {
                    AppAoDetailParentId = 11,
                    AppAoDetailParentName = "BOOKING"
                });

            Db.AppAoWizardFixed.AddRange(

                new AppAoWizardFixed
                {
                    AppAoWizardFixedId = 1,
                    WizardName = "PRE-BOOK"
                },
                new AppAoWizardFixed
                {
                    AppAoWizardFixedId = 2,
                    WizardName = "GRIEVANCE"
                });


            Db.AppAoWizardSteps.AddRange(
                new AppAoWizardSteps
                {
                    AppAoWizardStepsId = 5,
                    FacilityId = 1,
                    AppAoUserControlId = 6,
                    OrderBy = 11,
                    AppAoWizardId = 2,
                    AppAoUserControlParam = 4
                },
                new AppAoWizardSteps
                {
                    AppAoWizardStepsId = 6,
                    FacilityId = 1,
                    AppAoUserControlId = 6,
                    OrderBy = 12,
                    AppAoWizardId = 3,
                    AppAoUserControlParam = 4
                },
                new AppAoWizardSteps
                {
                    AppAoWizardStepsId = 7,
                    FacilityId = 2,
                    AppAoUserControlId = 6,
                    OrderBy = 12,
                    AppAoWizardId = 1,
                    AppAoUserControlParam = 5
                });

            Db.AoWizardFacilityStep.AddRange(
                new AoWizardFacilityStep
                {
                    AoWizardFacilityStepId = 15,
                    CreateDate = DateTime.Now.AddDays(-25),
                    UpdateDate = DateTime.Now.AddDays(-11),
                    CreateById = 12,
                    UpdateById = 11,
                    AoWizardFacilityId = 10,
                    AoComponentParamId = 7
                }

                );

            Db.AppAoSubModule.AddRange(
                new AppAoSubModule
                {
                    AppAoSubModuleId = 15,
                    AppAoSubModuleVisible = 1,
                    AppAoSubModuleName = "PreBook",
                    AppAoModuleId = 10
                },
                new AppAoSubModule
                {
                    AppAoSubModuleId = 16,
                    AppAoSubModuleVisible = 1,
                    AppAoSubModuleName = "Class File",
                    AppAoModuleId = 12
                },
                new AppAoSubModule
                {
                    AppAoSubModuleId = 17,
                    AppAoSubModuleVisible = 1,
                    AppAoSubModuleName = "Booking",
                    AppAoModuleId = 11
                },
                new AppAoSubModule
                {
                    AppAoSubModuleId = 18,
                    AppAoModuleId = 13,
                    AppAoSubModuleVisible = 1,
                    AppAoSubModuleName = "File"
                },
                new AppAoSubModule
                {
                    AppAoSubModuleId = 42057,
                    AppAoModuleId = 14,
                    AppAoSubModuleVisible = 1,
                    AppAoSubModuleName = "Records Check",
                    AppAoSubModuleRoute = "JMS/Records/RecChk"
                });

            Db.AppAoModule.AddRange(
                new AppAoModule
                {
                    AppAoModuleId = 10,
                    AppAoModuleVisible = 1,
                    AppAoModuleName = "Intake"
                },
                new AppAoModule
                {
                    AppAoModuleId = 11,
                    AppAoModuleVisible = 1,
                    AppAoModuleName = "Booking"
                },
                new AppAoModule
                {
                    AppAoModuleId = 12,
                    AppAoModuleVisible = 1,
                    AppAoModuleName = "Classify"
                },
                new AppAoModule
                {
                    AppAoModuleId = 13,
                    AppAoModuleVisible = 1,
                    AppAoModuleName = "Records"
                },
                new AppAoModule
                {
                    AppAoModuleId = 14,
                    AppAoModuleVisible = 1,
                    AppAoModuleName = "Facility"
                }
            );

            Db.AppAoWizardFixedSteps.AddRange(
                new AppAoWizardFixedSteps
                {
                    AppAoWizardFixedId = 1,
                    CreateBy = 12,
                    CreateDate = DateTime.Now.AddDays(-2),
                    UpdateBy = 11,
                    UpdateDate = DateTime.Now,
                    AppAoWizardFixedStepsId = 10,
                    StepName = "Medical Prescreening",
                    StepIcon = "search",
                    StepInvisible = 1
                },
                new AppAoWizardFixedSteps
                {
                    AppAoWizardFixedStepsId = 11,
                    AppAoWizardFixedId = 2,
                    CreateBy = 12,
                    CreateDate = DateTime.Now.AddDays(-5),
                    UpdateBy = 12,
                    UpdateDate = DateTime.Now,
                    StepName = "Medical Prescreening",
                    StepIcon = null,
                    StepInvisible = 1
                },
                new AppAoWizardFixedSteps
                {
                    AppAoWizardFixedStepsId = 110,
                    AppAoWizardFixedId = 5,
                    CreateBy = 11,
                    CreateDate = DateTime.Now.AddDays(-4),
                    UpdateBy = 12,
                    UpdateDate = DateTime.Now,
                    StepName = "Medical Prescreening",
                    StepInvisible = 1
                });

            Db.AccountAoFee.AddRange(
                new AccountAoFee
                {
                    AccountAoFeeId = 10,
                    CreateDate = DateTime.Now.AddDays(-10),
                    InmateId = 100,
                    UpdateDate = DateTime.Now.AddDays(-10),
                    UpdatedBy = 11,
                    CreatedBy = 12,
                    AccountAoFeeTypeId = null,
                    AccountAoFundId = 10,
                    TransactionVoidBy = 12,
                    TransactionOfficerId = 12,
                    TransactionFee = Convert.ToDecimal(122.00),
                    BalanceFundFee = Convert.ToDecimal(40.10),
                    BalanceAccountFee = Convert.ToDecimal(15000.00)
                },
                new AccountAoFee
                {
                    AccountAoFeeId = 11,
                    CreateDate = DateTime.Now.AddDays(-10),
                    InmateId = 100,
                    UpdateDate = DateTime.Now.AddDays(-10),
                    UpdatedBy = 11,
                    CreatedBy = 12,
                    AccountAoFeeTypeId = null,
                    AccountAoFundId = 10,
                    TransactionVoidBy = 11,
                    TransactionOfficerId = 11,
                    TransactionFee = Convert.ToDecimal(225.10),
                    BalanceAccountFee = Convert.ToDecimal(5000.02)
                }
                );

            Db.AccountAoFund.AddRange(
                new AccountAoFund
                {
                    AccountAoFundId = 10,
                    CreateDate = DateTime.Now.AddDays(-10),
                    UpdateDate = DateTime.Now.AddDays(-10),
                    UpdateBy = 11,
                    CreatedBy = 12,
                    AccountAoBankId = 10,
                    BalanceFund = Convert.ToDecimal(150.01),
                    BalanceFundFee = null,
                    BalanceFundPending = Convert.ToDecimal(20.01),
                    FundAllowBypassDepository = 0,
                    FundAllowFee = 0,
                    FundAllowFeeOrder = 1,
                    FundAllowFeePayDebtPercentage = null,
                    FundDescription = "INMATE",
                    FundName = "INMATE",
                    FundInmateOnlyFlag = 0,
                    FundInmateMinBalance=Convert.ToDecimal(11.01),
                    FundAllowFeeWaiveDebt = 1
                },
                new AccountAoFund
                {
                    AccountAoFundId = 11,
                    CreateDate = DateTime.Now.AddDays(-10),
                    UpdateDate = DateTime.Now.AddDays(-10),
                    UpdateBy = 12,
                    CreatedBy = 12,
                    AccountAoBankId = 10,
                    BalanceFund = Convert.ToDecimal(150.01),
                    BalanceFundFee = null,
                    BalanceFundPending = Convert.ToDecimal(20.01),
                    FundAllowBypassDepository = 0,
                    FundAllowFee = 1,
                    FundAllowFeeOrder = 0,
                    FundAllowFeePayDebtPercentage = null,
                    FundDescription = "WELFARE",
                    FundName = "WELFARE",
                    FundInmateOnlyFlag = 1,
                    FundInmateMinBalance =Convert.ToDecimal(10.00)
                }

                );

            Db.AccountAoBank.AddRange(
                new AccountAoBank
                {
                    AccountAoBankId = 10,
                    CreateDate = DateTime.Now.AddDays(-10),
                    UpdateDate = DateTime.Now.AddDays(-10),
                    BalanceAccount = Convert.ToDecimal(200.20),
                    BalanceAccountFee = Convert.ToDecimal(0.00),
                    BalanceAccountPending = Convert.ToDecimal(10.01),
                    BankAccountAbbr = "SBI",
                    BankName = "STATE BANK OF INDIA",
                    BankAccountNum = "SB100100",
                    CashDrawerMinBalance = Convert.ToDecimal(1.12),
                    ClearCheckAfterDays = null,
                    CreatedBy = 12,
                    UpdateBy = 11,
                    FacilityId = 1,
                    InmateMaxBalance = Convert.ToDecimal(12.01),
                    NextCheckNum = 5,
                    ReceiptShowOwed = null
                },
                new AccountAoBank
                {
                    AccountAoBankId = 11,
                    CreateDate = DateTime.Now.AddDays(-10),
                    UpdateDate = DateTime.Now.AddDays(-10),
                    BalanceAccount = Convert.ToDecimal(100.20),
                    BalanceAccountFee = Convert.ToDecimal(0.00),
                    BalanceAccountPending = Convert.ToDecimal(5.01),
                    BankAccountAbbr = "CUB",
                    BankName = "CITY UNION BANK",
                    BankAccountNum = "SB100100",
                    CashDrawerMinBalance = Convert.ToDecimal(1.12),
                    ClearCheckAfterDays = null,
                    CreatedBy = 12,
                    UpdateBy = 11,
                    FacilityId = 1,
                    InmateMaxBalance = Convert.ToDecimal(0.00),
                    NextCheckNum = 5,
                    ReceiptShowOwed = null
                }

                );

            Db.AccountAoInmate.AddRange(
                new AccountAoInmate
                {
                    AccountAoInmateId = 10,
                    AccountAoBankId = 10,
                    InmateId = 100,
                    BalanceInmateFee = 1500,
                    BalanceInmatePending = 500,
                    BalanceInmate = Convert.ToDecimal(5.23)
                },
                new AccountAoInmate
                {
                    AccountAoInmateId = 11,
                    AccountAoBankId = 11,
                    InmateId = 101,
                    BalanceInmateFee = 500,
                    BalanceInmatePending = 2000,
                    BalanceInmate = Convert.ToDecimal(125.25)
                });

            Db.AccountAoTransaction.AddRange(
                new AccountAoTransaction
                {
                    AccountAoTransactionId = 10,
                    InmateId = 100,
                    BalanceInmate = 120
                },
                new AccountAoTransaction
                {
                    AccountAoTransactionId = 6,
                    InmateId = 101,
                    BalanceInmate = 250
                });



            Db.Aka.AddRange(
                new Aka
                {
                    AkaId = 15,
                    PersonId = 50,
                    AkaFirstName = "SANGEETHA",
                    AkaDob = DateTime.Now.AddYears(-40),
                    CreateDate = DateTime.Now.AddDays(-20),
                    UpdateDate = null,
                    PersonGangName = "CHAIN SNATCHER",
                    AkaLastName = "VIJAYA",
                    AkaMiddleName = "PALANI",
                    AkaSuffix = "V",
                    AkaInmateNumber = null,
                    AkaSiteInmateNumber = "SVK661",
                    UpdatedBy = 12,
                    CreatedBy = 11
                },
                new Aka
                {
                    AkaId = 16,
                    PersonId = 55,
                    AkaFirstName = "SUGIR",
                    AkaLastName = "KRISHNA",
                    AkaMiddleName = "SURUTHI",
                    AkaSuffix = "S",
                    AkaDob = DateTime.Now.AddYears(-30),
                    CreateDate = DateTime.Now.AddDays(-19),
                    UpdateDate = null,
                    AkaInmateNumber = null,
                    PersonGangName = "ROCK BOYS",
                    UpdatedBy = 11,
                    CreatedBy = 12
                },
                new Aka
                {
                    AkaId = 17,
                    PersonId = 60,
                    AkaFirstName = "VIJAI",
                    AkaLastName = "VIDYA",
                    AkaMiddleName = "MOHAN",
                    AkaSuffix = "vv",
                    AkaDob = DateTime.Now.AddYears(-20),
                    CreateDate = DateTime.Now.AddDays(-18),
                    UpdateDate = null,
                    PersonGangName = "LATE PIC UP",
                    AkaInmateNumber = "SVK661",
                    UpdatedBy = 12,
                    CreatedBy = 11
                },
                new Aka
                {
                    AkaId = 18,
                    PersonId = 100,
                    AkaFirstName = "DENESH",
                    AkaLastName = "DEVA",
                    AkaMiddleName = "RAVII",
                    AkaSuffix = "DD",
                    AkaDob = DateTime.Now.AddYears(-35),
                    CreateDate = DateTime.Now.AddDays(-18),
                    UpdateDate = null,
                    AkaInmateNumber = "SVK661",
                    CreatedBy = 12,
                    UpdatedBy = 11
                },
                new Aka
                {
                    AkaId = 19,
                    PersonId = 101,
                    AkaFirstName = "KATHIK",
                    AkaLastName = "JATHAV",
                    AkaMiddleName = null,
                    AkaSuffix = "KJ",
                    AkaDob = DateTime.Now.AddYears(-35),
                    CreateDate = DateTime.Now.AddDays(-18),
                    UpdateDate = null,
                    AkaInmateNumber = "SVK742",
                    CreatedBy = 12,
                    UpdatedBy = 12
                },
                new Aka
                {
                    AkaId = 20,
                    PersonId = 112,
                    AkaFirstName = "ASHOK",
                    AkaLastName = "MANI",
                    AkaMiddleName = null,
                    AkaSuffix = "AM",
                    AkaDob = DateTime.Now.AddYears(-45),
                    CreateDate = DateTime.Now.AddDays(-20),
                    UpdateDate = DateTime.Now,
                    AkaInmateNumber = "SVK743",
                    CreatedBy = 12,
                    UpdatedBy = 11
                });
            Db.AkaHistory.AddRange(
                new AkaHistory
                {
                    AkaHistoryId = 20,
                    CreateDate = DateTime.Now.AddDays(-20),
                    PersonnelId = 11,
                    AkaId = 15,
                    AkaHistoryList = "{'FKNLAST NAME':'VIJAYA','FKNFIRST NAME'':'','FKNMIDDLE NAME':'PALANI','FKNSUFFIX':'PV'}"
                },
                new AkaHistory
                {
                    AkaHistoryId = 21,
                    CreateDate = DateTime.Now.AddDays(-20),
                    PersonnelId = 11,
                    AkaId = 16,
                    AkaHistoryList = "{'FKNLAST NAME':'KRISHNA','FKNFIRST NAME':'SUGIR','FKNMIDDLE NAME':'SURUTHI','FKNSUFFIX':'KS'}"
                },
                new AkaHistory
                {
                    AkaHistoryId = 22,
                    CreateDate = DateTime.Now.AddDays(-20),
                    PersonnelId = 11,
                    AkaId = 16,
                    AkaHistoryList = "{'DELETE FLAG':'','DELETE BY':'','DELETE DATE':''}"
                }
            );
            Db.UserGroups.AddRange(
                new UserGroups
                {
                    GroupId = 10,
                    CreateDate = DateTime.Now.AddDays(-5),
                    UpdateDate = DateTime.Now.AddDays(-2),
                    GroupDescription = "ADMIN"
                },
                new UserGroups
                {
                    GroupId = 11,
                    CreateDate = DateTime.Now.AddDays(-3),
                    UpdateDate = DateTime.Now.AddDays(-1),
                    GroupDescription = "JAIL PROPERTY"
                }
            );

        }
    }

}
