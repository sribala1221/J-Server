using System;
using GenerateTables.Models;
using ServerAPI.ViewModels;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Data.SqlClient;
using System.Data;
using Microsoft.AspNetCore.Http;
using ServerAPI.Utilities;
using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
namespace ServerAPI.Services
{
    public class IntakeService : IIntakeService
    {
        private readonly AAtims _context;
        private readonly ICommonService _commonService;
        private readonly IPersonPhotoService _personPhotoService;
        private readonly int _personnelId;
        private readonly IAtimsHubService _atimsHubService;

        public IntakeService(AAtims context, ICommonService commonService, IPersonPhotoService personPhotoService,
            IHttpContextAccessor httpContextAccessor, IAtimsHubService atimsHubService)
        {
            _context = context;
            _commonService = commonService;
            _personPhotoService = personPhotoService;
            _personnelId = Convert.ToInt32(httpContextAccessor.HttpContext.User.FindFirst("personnelId")?.Value);
            _atimsHubService = atimsHubService;
        }

        //To get Intake in Progress details
        public IntakeInmate GetIntake(int facilityId, bool isShowAll)
        {
            IntakeInmate intakeInmate = new IntakeInmate();
            List<int> formRecords = _context.FormRecord.Where(w => w.InmatePrebook.PersonId.HasValue)
                .Select(s => s.InmatePrebook.PersonId ?? 0).ToList();

            // To get Intake based on facility
            intakeInmate.IntakeDetails = _context.Incarceration
                .Where(i => i.Inmate.InmateActive == 1
                            && i.Inmate.FacilityId == facilityId
                            && !i.ReleaseOut.HasValue
                            && (!i.BookAndReleaseFlag.HasValue || i.BookAndReleaseFlag == 0)
                            && (isShowAll
                                ? !i.IntakeCompleteFlag.HasValue || i.IntakeCompleteFlag == 1
                                : !i.IntakeCompleteFlag.HasValue || i.IntakeCompleteFlag == 0))

                .Select(i => new IntakeVm
                {
                    InmateId = i.InmateId.Value,
                    InmateNumber = i.Inmate.InmateNumber,
                    IncarcerationId = i.IncarcerationId,
                    DateIn = i.DateIn,
                    PersonId = i.Inmate.PersonId,
                    PersonFirstName = i.Inmate.Person.PersonFirstName,
                    PersonMiddleName = i.Inmate.Person.PersonMiddleName,
                    PersonLastName = i.Inmate.Person.PersonLastName,
                    BookandReleaseFlag = i.BookAndReleaseFlag ?? 0,
                    LocationId = i.Inmate.InmateCurrentTrackId,
                    BookingNo = i.BookingNo,
                    ExpediteBookingReason = i.ExpediteBookingReason,
                    ExpediteBookingFlag = i.ExpediteBookingFlag,
                    ExpediteBookingNote = i.ExpediteBookingNote,
                    ExpediteDate = i.ExpediteBookingDate,
                    ExpediteBy = i.ExpediteBookingBy > 0
                        ? new PersonnelVm
                        {
                            PersonFirstName = i.ExpediteBookingByNavigation.PersonNavigation.PersonFirstName,
                            PersonMiddleName = i.ExpediteBookingByNavigation.PersonNavigation.PersonMiddleName,
                            PersonLastName = i.ExpediteBookingByNavigation.PersonNavigation.PersonLastName,
                            OfficerBadgeNumber = i.ExpediteBookingByNavigation.OfficerBadgeNum,
                            PersonnelId = i.ExpediteBookingByNavigation.PersonnelId
                        }
                        : default
                }).ToList();
            List<int> incarcerationId= intakeInmate.IntakeDetails.Select(s => s.IncarcerationId).ToList();

           // int[] lstIncIds = lstIncarceration.Select(ii => ii.IncarcerationId).ToArray();

            var lstIncarcerationArrestXrefDs =
                _context.IncarcerationArrestXref.Where(iax => incarcerationId.Contains(iax.IncarcerationId.Value) &&
                                                              iax.ArrestId.HasValue).Select(iax => new
                {
                    IncarcerationId = iax.IncarcerationId.Value,
                    iax.Arrest.ArrestBookingNo,
                    iax.Arrest.ArrestType
                }).ToList();

            // To get Inmate Prebook details
            IEnumerable<IntakeInmatePrebookVm> lstInmatePrebook =
                _context.InmatePrebook
                    .Where(w => w.IncarcerationId.HasValue && incarcerationId.Contains(w.IncarcerationId.Value))
                    .Select(s => new IntakeInmatePrebookVm
                    {
                        InmatePrebookId = s.InmatePrebookId,
                        IncarcerationId = s.IncarcerationId,
                        PersonId = s.PersonId,
                        PreBookNumber = s.PreBookNumber
                    }).ToList();

            // To get Wizard Step details
            //why do we need to take all the wizard data? the stepcomplete,steporder and other id to initialize the wizard screen is enough right?
            IEnumerable<AoWizardProgressVm> wizardProgress =
            _context.AoWizardProgressIncarceration.Where(w => incarcerationId.Contains(w.Incarceration.IncarcerationId))
                       .Select(w => new AoWizardProgressVm
                       {
                           WizardProgressId = w.AoWizardProgressId,
                           WizardId = w.AoWizardId,
                           IncarcerationId = w.Incarceration.IncarcerationId,
                           WizardStepProgress = w.AoWizardStepProgress
                               .Where(p => p.AoWizardProgressId == w.AoWizardProgressId)
                               .Select(p => new AoWizardStepProgressVm
                               {
                                   WizardStepProgressId = p.AoWizardStepProgressId,
                                   AoWizardFacilityStepId = p.AoWizardFacilityStepId,
                                   WizardProgressId = p.AoWizardProgressId,
                                   ComponentId = p.AoComponentId,
                                   Component =  new AoComponentVm
                                       {
                                           AppAoFunctionalityId = p.AoComponent.AppAofunctionalityId,
                                           CanChangeVisibility = p.AoComponent.CanChangeVisibility,
                                           ComponentId = p.AoComponent.AoComponentId,
                                           ComponentName = p.AoComponent.ComponentName,
                                           CustomFieldAllowed = p.AoComponent.CustomFieldAllowed,
                                           CustomFieldKeyName = p.AoComponent.CustomFieldKeyName,
                                           CustomFieldTableName = p.AoComponent.CustomFieldTableName,
                                           DisplayName = p.AoComponent.DisplayName,
                                           HasConfigurableFields = p.AoComponent.HasConfigurableFields,
                                           IsLastScreen = p.AoComponent.IsLastScreen
                                       },
                                   StepComplete = p.StepComplete,
                                   StepCompleteBy = _context.Personnel
                                       .Where(c => c.PersonnelId == p.StepCompleteById)
                                       .Select(c => new PersonnelVm
                                       {
                                           PersonnelId = c.PersonnelId,
                                           OfficerBadgeNumber = c.OfficerBadgeNum,
                                           PersonLastName = c.PersonNavigation.PersonLastName,
                                           PersonFirstName = c.PersonNavigation.PersonFirstName,
                                           PersonMiddleName = c.PersonNavigation.PersonMiddleName
                                       }).FirstOrDefault(),
                                   StepCompleteDate = p.StepCompleteDate,
                                   StepCompleteNote=p.StepCompleteNote
                               }).ToList()
                       }).ToList();


            List<KeyValuePair<int, string>> lstAoComponent = _context.AoComponent
                .Select(comp => new KeyValuePair<int, string>(comp.AoComponentId, comp.ComponentName)).ToList();

            string valType = _commonService.GetValidationType(TaskValidateType.IntakeComplete);
            var lstAoTaskLookups = _context.AoTaskLookupAssign.Where(lookAssign =>
                    lookAssign.TaskValidateLookup == valType && !lookAssign.DeleteFlag &&
                    lookAssign.FacilityId == facilityId && !lookAssign.AoTaskLookup.DeleteFlag)
                .Select(lookAssign => new
                {
                    lookAssign.AoTaskLookupId,
                    lookAssign.AoTaskLookup.TaskName,
                    ComponentName = lookAssign.AoTaskLookup.AoComponentId.HasValue
                        ? lstAoComponent.Find(com => com.Key == lookAssign.AoTaskLookup.AoComponentId).Value
                        : null
                }).AsEnumerable().Distinct().ToList();

            int[] arrTaskIds = lstAoTaskLookups.Select(task => task.AoTaskLookupId).ToArray();

            int[] inmateIds = intakeInmate.IntakeDetails.Select(i =>
            {
                Debug.Assert(i.InmateId != null, "i.InmateId != null");
                return i.InmateId.Value;
            }).ToArray();

            List<TasksCountVm> lstAoTaskQueue = _context.AoTaskQueue
                .Where(inm =>
                    inmateIds.Contains(inm.InmateId) && !inm.CompleteFlag && arrTaskIds.Contains(inm.AoTaskLookupId))
                .Select(que => new TasksCountVm
                    {
                        ComponentId = que.AoTaskLookup.AoComponentId,
                        TaskInstruction = que.AoTaskLookup.TaskInstructions,
                        InmateId = que.InmateId,
                        TaskLookupId = que.AoTaskLookupId,
                        ComponentName = lstAoTaskLookups.Find(look => look.AoTaskLookupId == que.AoTaskLookupId)
                            .ComponentName,
                        TaskName = que.AoTaskLookup.TaskName,
                        PriorityFlag = que.PriorityFlag
                    }
                ).ToList();

            foreach (IntakeVm ivm in intakeInmate.IntakeDetails)
            {
                //// To get the Inmate Prebook based on person and incarceration.
                IntakeInmatePrebookVm intakeInmatePrebookVm = lstInmatePrebook.FirstOrDefault(w =>
                    w.PersonId == ivm.PersonId && w.IncarcerationId>0);
                ivm.WizardProgress = wizardProgress.FirstOrDefault(w => w.IncarcerationId.HasValue && w.IncarcerationId== ivm.IncarcerationId);

                ivm.PrebookNumber = intakeInmatePrebookVm?.PreBookNumber;
                ivm.PreBookId = intakeInmatePrebookVm?.InmatePrebookId ?? 0;

                ivm.PreBookCount = formRecords.Count(c => c == ivm.PersonId);
                ivm.BookingTasks = lstAoTaskQueue.Where(qu => qu.InmateId == ivm.InmateId).ToList();

                ivm.CaseType = lstIncarcerationArrestXrefDs.Where(
                    iax => ivm.IncarcerationId == iax.IncarcerationId).Select(iax => iax.ArrestType).ToArray();
            }

            intakeInmate.TaskDetails = GetTasksDetails(facilityId, inmateIds);

         
            return intakeInmate;
        }

        //To get Intake in Progress details
        public IntakeVm GetInmateIdFromPrebook(int inmatePrebookId)
        {

            int incarcerationId = _context.InmatePrebook.Find(inmatePrebookId).IncarcerationId??0;

            // To get Intake based on facility
            IntakeVm intakeVm = _context.Incarceration
                .Where(i => i.IncarcerationId == incarcerationId)
                .Select(i => new IntakeVm
                {
                    InmateId = i.InmateId.Value,
                    IncarcerationId = i.IncarcerationId,
                    PersonId = i.Inmate.PersonId
                }).Single();           
            
            return intakeVm;
        }

        public List<TasksCountVm> GetTasksDetails(int facilityId, int[] inmateIds)
        {
            List<KeyValuePair<bool, int>> lstAoTaskQueue = _context.AoTaskQueue
                .Where(queue => !queue.CompleteFlag && !queue.AoTaskLookup.DeleteFlag && inmateIds.Contains(queue.InmateId))
                .Select(queue =>
                    new KeyValuePair<bool, int>(queue.PriorityFlag, queue.AoTaskLookupId)).ToList();

            List<TasksCountVm> tasksCountList = _context.AoTaskLookupAssign.Where(lookAssign =>
                    lookAssign.TaskValidateLookup == _commonService.GetValidationType(TaskValidateType.IntakeComplete)
                    && !lookAssign.DeleteFlag && lookAssign.FacilityId == facilityId &&
                    !lookAssign.AoTaskLookup.DeleteFlag)
                .Select(
                    look => new TasksCountVm
                    {
                        TaskLookupId = look.AoTaskLookupId,
                        TaskName = look.AoTaskLookup.TaskName
                    }).Distinct().ToList();
            tasksCountList.ForEach(task =>
            {
                task.InmateCount = lstAoTaskQueue.Count(queue => queue.Value == task.TaskLookupId);
                task.TaskPriorityCount = lstAoTaskQueue.Count(queue => queue.Value == task.TaskLookupId && queue.Key);
            });

            return tasksCountList;
        }

        public List<InmatePrebookStagingVm> GetInmatePrebookStaging(InmatePrebookStagingVm obj)
        {
            IQueryable<InmatePrebookStagingVm> qryInmatePrebookStagingVms = 
                _context.InmatePrebookStaging
                        .Where(a => a.ParsedRecordType == "P")
                        .Select(i => new InmatePrebookStagingVm
                        {
                            InmatePrebookStagingId = i.InmatePrebookStagingId,
                            CaseNumber = i.ParsedCaseNumber,
                            CreateDate = i.CreateDate,
                            StateNumber = i.ParsedStateNumber,
                            InmateNumber = i.ParsedInmateNumber,
                            LastName = i.ParsedLastName,
                            FirstName = i.ParsedFirstName,
                            MiddleName = i.ParsedMiddleName,
                            Gender = i.ParsedGender,
                            DOB = i.ParsedDOB,
                            RawData = i.RawData,
                            DeleteFlag = i.DeleteFlag,
                            DeleteReason = i.DeleteReason,
                            InmatePrebookId=i.InmatePrebookId
                        });

            DateTime dateTime = DateTime.Now.AddDays(-3);
            IEnumerable<InmatePrebookStagingVm> lstInmatePrebookStagingVms;
            switch (obj.SelectFlag)
            {
                case InmatePrebookStagingAlert.Active:
                    lstInmatePrebookStagingVms = qryInmatePrebookStagingVms.Where(i => !i.DeleteFlag
                        && i.CreateDate > dateTime);
                    break;
                case InmatePrebookStagingAlert.Deleted:
                    lstInmatePrebookStagingVms = qryInmatePrebookStagingVms.Where(i => i.DeleteFlag 
                        && i.DeleteReason == InmatePrebookStagingReason.Replaced.ToString()
                        && obj.FromDate.Value.Date <= i.CreateDate.Date
                        && obj.ToDate.Value.Date >= i.CreateDate.Date);
                    break;
                default:
                    lstInmatePrebookStagingVms = qryInmatePrebookStagingVms.Where(i => i.DeleteFlag 
                        && i.DeleteReason == InmatePrebookStagingReason.Consumed.ToString()
                        && obj.FromDate.Value.Date <= i.CreateDate.Date
                        && obj.ToDate.Value.Date >= i.CreateDate.Date);
                    break;
            }

            List<InmatePrebookStagingVm> inmatePrebookStagingVms = lstInmatePrebookStagingVms.ToList();
            int[] inmatePrebookStagingIds = inmatePrebookStagingVms.Select(i => i.InmatePrebookStagingId).ToArray();

            List<AoWizardProgressInmatePrebookStaging> lstAoWizardProgress = _context.AoWizardProgressInmatePrebookStaging.Where(awp =>
                inmatePrebookStagingIds.Contains(awp.InmatePrebookStagingId) &&
                awp.AoWizardId == (int?)Wizards.rmsIntake).ToList();

            List<WizardStep> lstAoWizardStepProgress = _context.AoWizardStepProgress
                .Where(aws => lstAoWizardProgress.Select(i => i.AoWizardProgressId).Contains(aws.AoWizardProgressId))
                .Select(aw => new WizardStep
                {
                    ComponentId = aw.AoComponentId,
                    AoWizardFacilityStepId = aw.AoWizardFacilityStepId,
                    WizardProgressId = aw.AoWizardProgressId,
                    WizardStepProgressId = aw.AoWizardStepProgressId,
                    StepComplete = aw.StepComplete
                }).ToList();

            inmatePrebookStagingVms.ForEach(item =>
            { 
                item.WizardProgressId = lstAoWizardProgress.FirstOrDefault(awp =>
                    awp.InmatePrebookStagingId == item.InmatePrebookStagingId)?.AoWizardProgressId;

                if (item.WizardProgressId.HasValue)
                {
                    item.LastStep = lstAoWizardStepProgress
                        .Where(aws => aws.WizardProgressId == item.WizardProgressId.Value).ToList();
                }    
            });

            return inmatePrebookStagingVms;
        }

        public PersonInfoVm GetPersonByRms(string sid)
        {
            PersonInfoVm personInfoVm = _context.Person.Where(p => p.PersonCii == sid)
                .Select(p => new PersonInfoVm
                {
                    PersonId = p.PersonId,
                    PersonLastName = p.PersonLastName,
                    PersonFirstName = p.PersonFirstName,
                    PersonMiddleName = p.PersonMiddleName,
                    PersonDob = p.PersonDob
                }).LastOrDefault();

            if (personInfoVm == null) return null;
            List<IdentifierVm> lstIdentifierVms = _personPhotoService.GetIdentifier(personInfoVm.PersonId);
            lstIdentifierVms = lstIdentifierVms
                .Where(i => i.IdentifierType == ((int?)IdentifierType.FrontView).ToString()).ToList();
            if (lstIdentifierVms.Count > 0)
            {
                personInfoVm.PhotoPath = lstIdentifierVms[0].PhotographRelativePath;
            }
            return personInfoVm;
        }

        public string InsertRmsChargesAndWarrants(int inmatePrebookStagingId, int inmatePrebookId, int personId, int facilityId)
        {
            SqlConnection connection = new SqlConnection(Startup.ConnectionString);
            connection.Open();
            SqlCommand command = new SqlCommand("CCD_RMSChargesAndWarrants", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            command.Parameters.AddWithValue("@InmatePrebookStagingId", inmatePrebookStagingId);
            command.Parameters.AddWithValue("@InmatePrebookId", inmatePrebookId);
            command.Parameters.AddWithValue("@createdBy", _personnelId);
            command.Parameters.Add("@ErrorMessage", SqlDbType.Char, 150);
            command.Parameters["@ErrorMessage"].Direction = ParameterDirection.Output;
            command.Parameters.Add("@IsSuccess", SqlDbType.Bit);
            command.Parameters["@IsSuccess"].Direction = ParameterDirection.Output;
            command.Parameters.Add("@NotProcessedUDF", SqlDbType.NVarChar, 500);
            command.Parameters["@NotProcessedUDF"].Direction = ParameterDirection.Output;
            command.ExecuteNonQuery();
            bool isSuccess = (bool)command.Parameters["@IsSuccess"].Value;
            string notProcessedUdfNo = command.Parameters["@NotProcessedUDF"].Value.ToString().Trim();
            connection.Close();

            InmatePrebookStaging inmatePrebookStaging = _context.InmatePrebookStaging.Find(inmatePrebookStagingId);
            inmatePrebookStaging.InmatePrebookId = inmatePrebookId;
            _context.SaveChanges();

            if (isSuccess)
            {
                connection = new SqlConnection(Startup.ConnectionString);
                connection.Open();
                command = new SqlCommand("CCD_RMSPayloadData", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };
                command.Parameters.AddWithValue("@inmatePrebookStagingId", inmatePrebookStagingId);
                command.Parameters.AddWithValue("@personId", personId);
                command.Parameters.AddWithValue("@createdBy", _personnelId);
                command.Parameters.Add("@ErrorMessage", SqlDbType.Char, 150);
                command.Parameters["@ErrorMessage"].Direction = ParameterDirection.Output;
                command.Parameters.Add("@InmateNumber", SqlDbType.VarChar, 500);
                command.Parameters["@InmateNumber"].Direction = ParameterDirection.Output;
                command.Parameters.Add("@IsSuccess", SqlDbType.Bit);
                command.Parameters["@IsSuccess"].Direction = ParameterDirection.Output;
                command.ExecuteNonQuery();
                isSuccess = (bool)command.Parameters["@IsSuccess"].Value;
                string inmateNumber = command.Parameters["@InmateNumber"].Value.ToString();
                connection.Close();

                if (isSuccess && inmateNumber != "")
                {
                    Person person = _context.Person.Find(personId);

                    IntakeEntryVm intakeEntryVm = new IntakeEntryVm()
                    {
                        PersonId = personId,
                        FacilityId = facilityId,
                        PersonLastName = person.PersonLastName,
                        PersonFirstName = person.PersonFirstName,
                        PersonMiddleName = person.PersonMiddleName,
                        PersonSuffix = person.PersonSuffix,
                        PersonDob = person.PersonDob,
                        InmatePreBookId = inmatePrebookId,
                        ReactivateArrestId = 0
                    };

                    Inmate inmate = _context.Inmate.Where(i => i.PersonId == personId).SingleOrDefault();
                    if (inmate == null)
                    {
                        int age = _commonService.GetAgeFromDob(person.PersonDob);
                        inmate = new Inmate
                        {
                            PersonId = personId,
                            InmateNumber = inmateNumber,
                            InmateReceivedDate = DateTime.Now,
                            InmateOfficerId = _personnelId,
                            FacilityId = facilityId,
                            InmateJuvenileFlag = age < 18 ? 1 : 0
                        };
                        _context.Inmate.Add(inmate);
                    }
                    else
                    {
                        if (inmateNumber != inmate.InmateNumber)
                        {
                            Aka aka = new Aka()
                            {
                                AkaInmateNumber = inmate.InmateNumber,
                                CreateDate = DateTime.Now,
                                CreatedBy = _personnelId,
                                PersonId = personId
                            };
                            _context.Aka.Add(aka);
                        }
                    }
                    inmate.InmateNumber = inmateNumber;
                    _context.SaveChanges();
                }
            }

            return notProcessedUdfNo;
        }

        public async Task<int> CompleteRmsPrebook(int inmatePrebookStagingId, int inmatePrebookId)
        {
            InmatePrebookStaging inmatePrebookStaging = _context.InmatePrebookStaging.Find(inmatePrebookStagingId);
            inmatePrebookStaging.DeleteFlag = true;
            inmatePrebookStaging.DeleteDate = DateTime.Now;
            inmatePrebookStaging.DeleteReason = InmatePrebookStagingReason.Consumed.ToString();

            InmatePrebook inmatePrebook = _context.InmatePrebook.Find(inmatePrebookId);
            inmatePrebook.IntakeReviewAccepted = true;
            inmatePrebook.IntakeReviewBy = _personnelId;
            inmatePrebook.IntakeReviewDate = DateTime.Now;
            inmatePrebook.IdentificationAccepted = true;
            inmatePrebook.IdentificationAcceptedBy = _personnelId;
            inmatePrebook.IdentificationAcceptedDate = DateTime.Now;
            inmatePrebook.CompleteFlag = 1;
            inmatePrebook.CompleteBy = _personnelId;
            inmatePrebook.CompleteDate = DateTime.Now;

            return await _context.SaveChangesAsync();
        }

        public async Task<int> DeleteInmatePrebookStaging(int inmatePrebookStagingId)
        {
            InmatePrebookStaging inmatePrebookStaging =
                _context.InmatePrebookStaging.Single(c => c.InmatePrebookStagingId == inmatePrebookStagingId);
            inmatePrebookStaging.DeleteFlag = true;
            inmatePrebookStaging.DeleteDate = DateTime.Now;
            inmatePrebookStaging.DeleteReason = InmatePrebookStagingReason.Replaced.ToString();
            return await _context.SaveChangesAsync();
        }
    }
}