using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using GenerateTables.Models;
using ServerAPI.ViewModels;
using ServerAPI.Utilities;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace ServerAPI.Services
{
    public class BookingTaskService : IBookingTaskService
    {
        private readonly AAtims _context;
        private readonly ICommonService _commonService;
        private readonly IBookingService _bookingService;
        private readonly IInmateService _inmateService;
        private readonly int _personnelId;
        private readonly IPersonService _iPersonService;
        private readonly IInterfaceEngineService _interfaceEngine;

        public BookingTaskService(AAtims context, ICommonService commonService,
            IHttpContextAccessor httpContextAccessor, IPersonService personService, IInmateService inmateService,
            IBookingService bookingService, IInterfaceEngineService interfaceEngineService)
        {
            _context = context;
            _commonService = commonService;
            _personnelId = Convert.ToInt32(httpContextAccessor.HttpContext.User.FindFirst("personnelId")?.Value);
            _iPersonService = personService;
            _inmateService = inmateService;
            _bookingService = bookingService;
            _interfaceEngine = interfaceEngineService;
        }

        public List<KeyValuePair<int, string>> GetAllTasks(int facilityId) =>
            _context.AoTaskLookup.Where(task => !task.DeleteFlag && !task.DeleteFlag)
                .Select(task => new KeyValuePair<int, string>(task.AoTaskLookupId, task.TaskName))
                .Distinct().ToList();

        public List<TaskOverview> GetTaskInmates(int taskLookupId, int facilityId)
        {
            List<AoTaskLookupAssign> taskDetails = _context.AoTaskLookupAssign.Where(s =>
                                    s.AoTaskLookupId == taskLookupId && s.FormTemplatesId.HasValue).ToList();

            List<TaskOverview> lstAoTaskQueue =
                _context.AoTaskQueue
                    .Where(queue => queue.AoTaskLookupId == taskLookupId && queue.Inmate.FacilityId==facilityId)
                    .Select(task =>
                        new TaskOverview
                        {
                            InmateId = task.InmateId,
                            CompleteById = task.CompleteBy,
                            CompleteDate = task.CompleteDate,
                            CompleteNote = task.CompleteNote,
                            PriorityFlag = task.PriorityFlag,
                            CreateDate = task.CreateDate,
                            TaskName = task.AoTaskLookup.TaskName,
                            TaskLookupId = task.AoTaskLookupId,
                            ComponentId = task.AoTaskLookup.AoComponentId,
                            TaskInstruction = task.AoTaskLookup.TaskInstructions,
                            IncarcerationId = task.Inmate.Incarceration.Where(w => w.InmateId == task.InmateId)
                                .Select(s => s.IncarcerationId).FirstOrDefault(),
                            ArrestId = _context.IncarcerationArrestXref
                                .Where(wi => wi.Incarceration.InmateId == task.InmateId)
                                .Select(s => s.ArrestId).FirstOrDefault(),
                            IncarcerationArrestXrefId = _context.IncarcerationArrestXref
                                .Where(wi => wi.Incarceration.InmateId == task.InmateId)
                                .Select(s => s.IncarcerationArrestXrefId).FirstOrDefault(),
                            //FormCategoryId = taskDetails.Count>0 ? taskDetails[0].FormTemplates.FormCategoryId: default,
                            FormCategoryId = _context.AoTaskLookupAssign.Where(w => w.AoTaskLookupId == taskLookupId)
                            .FirstOrDefault().FormTemplates.FormCategoryId,
                            FormTemplateId = taskDetails.FirstOrDefault(s => s.AoTaskLookupId == taskLookupId).FormTemplatesId,
                            FormTemplateName = taskDetails.FirstOrDefault(s => s.AoTaskLookupId == taskLookupId).FormTemplates.DisplayName,
                            // FormTemplateDetails = taskDetails.Select(s =>
                            //         new KeyValuePair<int, string>(s.FormTemplatesId.Value, s.FormTemplates.DisplayName))
                            //     .ToList()
                        }).ToList();

            List<int> officerIds =
                lstAoTaskQueue.Select(i => new[] { i.CompleteById, i.ExpediteById })
                    .SelectMany(i => i).Where(i => i.HasValue)
                    .Select(i => i.Value).Distinct().ToList();

            int[] inmateIds = lstAoTaskQueue.Select(i => i.InmateId).ToArray();

            List<InmateHousing> lstPersonDetails = _bookingService.GetInmateDetails(inmateIds.ToList());

            IQueryable<Incarceration> lstIncarceration = _context.Incarceration.Where(inc =>
                inmateIds.Contains(inc.InmateId.Value) &&
                !inc.ReleaseOut.HasValue);

            officerIds.AddRange(lstIncarceration.Select(i => new[] { i.ExpediteBookingBy })
                    .SelectMany(i => i).Where(i => i.HasValue)
                    .Select(i => i.Value).Distinct().ToList());

            List<PersonnelVm> officersList = _context.Personnel.Where(off => officerIds.Contains(off.PersonnelId))
                .Select(s => new PersonnelVm
                {
                    ArrestTransportOfficerFlag = s.ArrestTransportOfficerFlag,
                    PersonFirstName = s.PersonNavigation.PersonFirstName,
                    PersonLastName = s.PersonNavigation.PersonLastName,
                    PersonMiddleName = s.PersonNavigation.PersonMiddleName,
                    PersonId = s.PersonId,
                    PersonnelId = s.PersonnelId,
                    OfficerBadgeNumber = s.OfficerBadgeNum,
                    PersonnelAgencyFlag = s.PersonnelAgencyGroupFlag
                }).ToList();

            lstAoTaskQueue.ForEach(task =>
            {
                if (task.CompleteById.HasValue)
                {
                    task.CompleteBy = officersList.Single(off => off.PersonnelId == task.CompleteById.Value);
                }

                task.PersonDetails = lstPersonDetails.First(per => per.InmateId == task.InmateId);

                Incarceration incDetails = lstIncarceration.SingleOrDefault(inc => inc.InmateId == task.InmateId);
                if (!(incDetails is null))
                {
                    task.NoKeeperFlag = incDetails.NoKeeper;
                    task.ExpediteBookingReason = incDetails.ExpediteBookingReason;
                    task.ExpediteBookingFlag = incDetails.ExpediteBookingFlag;
                    task.ExpediteBookingNote = incDetails.ExpediteBookingNote;
                    task.ExpediteDate = incDetails.ExpediteBookingDate;
                    task.ExpediteBy = incDetails.ExpediteBookingBy > 0
                        ? officersList.Where(per => per.PersonnelId == incDetails.ExpediteBookingBy).Select(per =>
                            new PersonnelVm
                            {
                                PersonFirstName = per.PersonFirstName,
                                PersonMiddleName = per.PersonMiddleName,
                                PersonLastName = per.PersonLastName,
                                OfficerBadgeNumber = per.OfficerBadgeNumber,
                                PersonnelId = per.PersonnelId
                            }).First()
                        : new PersonnelVm();
                }

                if (task.ComponentId.HasValue)
                {
                    task.ComponentName = _context.AoComponent.Single(acc => acc.AoComponentId == task.ComponentId)
                        .ComponentName;
                }
            });

            return lstAoTaskQueue;
        }

        public async Task<int> AssignTaskAsync(TaskOverview taskOverview)
        {
            int inmateTaskCount = _context.AoTaskQueue.Count(queue =>
                queue.InmateId == taskOverview.InmateId && queue.AoTaskLookupId == taskOverview.TaskLookupId &&
                !queue.CompleteFlag);
            if (inmateTaskCount != 0) return 0;
            AoTaskQueue insTaskQueue = new AoTaskQueue
            {
                AoTaskLookupId = taskOverview.TaskLookupId,
                AoTaskLookup =
                    _context.AoTaskLookup.Single(task => task.AoTaskLookupId == taskOverview.TaskLookupId),
                InmateId = taskOverview.InmateId,
                CreateDate = DateTime.Now,
                CreateBy = _personnelId,
                PriorityFlag = taskOverview.PriorityFlag,
                ManualFlag = true
            };
            _context.AoTaskQueue.Add(insTaskQueue);
            return await _context.SaveChangesAsync();
        }

        public List<TaskOverview> GetInmateAllTasks(int inmateId, int taskLookupId = 0)
        {
            List<TaskOverview> lstAoTaskQueue = _context.AoTaskQueue.Where(queue =>
                queue.InmateId == inmateId).Select(task => new TaskOverview
                {
                    InmateId = task.InmateId,
                    TaskLookupId = task.AoTaskLookupId,
                    TaskName = task.AoTaskLookup.TaskName,
                    ComponentId = task.AoTaskLookup.AoComponentId,
                    CompleteById = task.CompleteBy,
                    CompleteDate = task.CompleteDate,
                    CompleteNote = task.CompleteNote,
                    PriorityFlag = task.PriorityFlag,
                    CreateDate = task.CreateDate,
                    TaskInstruction = task.AoTaskLookup.TaskInstructions,
                    IncarcerationId=_context.Incarceration.FirstOrDefault(f=>f.InmateId==task.InmateId && !f.ReleaseOut.HasValue).IncarcerationId,
                    FormCategoryId = _context.AoTaskLookupAssign.Where(w => w.AoTaskLookupId == taskLookupId)
                            .FirstOrDefault().FormTemplates.FormCategoryId,                            
                }).ToList();

            if (taskLookupId > 0)
            {
                lstAoTaskQueue.ForEach(item =>
                {
                    item.TaskValidateLookup = _context.AoTaskLookupAssign
                        .Where(ii => ii.AoTaskLookupId == item.TaskLookupId).Select(ii => ii.TaskValidateLookup).ToArray();

                    int completedCnt = lstAoTaskQueue.Count(a =>
                        !a.CompleteById.HasValue && a.TaskLookupId == taskLookupId);

                    int facilityId = _context.Inmate.Single(a => a.InmateId == inmateId).FacilityId;

                    AoTaskLookupAssign aoTaskLookupAssign = null;

                    if (completedCnt == 0)
                    {
                        aoTaskLookupAssign = _context.AoTaskLookupAssign.SingleOrDefault(i =>
                            i.AoTaskLookupId == taskLookupId && !i.DeleteFlag && i.FacilityId == facilityId &&
                            i.TaskValidateLookup == EventNameConstants.INTAKECOMPLETE);
                    }

                    if (aoTaskLookupAssign == null && completedCnt == 1)
                    {
                        aoTaskLookupAssign = _context.AoTaskLookupAssign.SingleOrDefault(i =>
                            i.AoTaskLookupId == taskLookupId && !i.DeleteFlag && i.FacilityId == facilityId &&
                            i.TaskValidateLookup == EventNameConstants.BOOKINGCOMPLETE);
                    }

                    if (aoTaskLookupAssign == null && completedCnt == 2)
                    {
                        aoTaskLookupAssign = _context.AoTaskLookupAssign.SingleOrDefault(i =>
                            i.AoTaskLookupId == taskLookupId && !i.DeleteFlag && i.FacilityId == facilityId &&
                            i.TaskValidateLookup == EventNameConstants.ASSESSMENTCOMPLETE);
                    }

                    if (aoTaskLookupAssign == null && completedCnt == 3)
                    {
                        aoTaskLookupAssign = _context.AoTaskLookupAssign
                            .SingleOrDefault(i => i.AoTaskLookupId == taskLookupId && !i.DeleteFlag &&
                                                  i.FacilityId == facilityId &&
                                                  i.TaskValidateLookup == EventNameConstants.DORELEASE);
                    }

                    if (aoTaskLookupAssign == null && completedCnt == 4)
                    {
                        aoTaskLookupAssign = _context.AoTaskLookupAssign.SingleOrDefault(i =>
                            i.AoTaskLookupId == taskLookupId && !i.DeleteFlag && i.FacilityId == facilityId &&
                            i.TaskValidateLookup == EventNameConstants.HOUSINGASSIGNFROMTRANSFER);
                    }

                    if (aoTaskLookupAssign?.FormTemplatesId != null)
                    {
                        item.FormTemplateId = aoTaskLookupAssign.FormTemplatesId;
                        item.FormTemplateName = _context.FormTemplates
                            .Single(a => a.FormTemplatesId == item.FormTemplateId)
                            .DisplayName;
                    }
                });
            }

            List<int> officerIds = lstAoTaskQueue.Where(i => i.CompleteById.HasValue).Select(i => i.CompleteById.Value)
                .ToList();

            List<PersonnelVm> completeByOfficers = _iPersonService.GetPersonNameList(officerIds.ToList());

            lstAoTaskQueue.ForEach(task =>
            {
                if (task.CompleteById.HasValue)
                {
                    task.CompleteBy = completeByOfficers.Single(off => off.PersonnelId == task.CompleteById.Value);
                }

                if (task.ComponentId.HasValue)
                {
                    task.ComponentName = _context.AoComponent.Single(acc => acc.AoComponentId == task.ComponentId)
                        .ComponentName;
                }
            });

            return lstAoTaskQueue;
        }

        public async Task<int> UpdateTaskPriority(TaskOverview taskOverview)
        {
            AoTaskQueue incDetails = _context.AoTaskQueue.Single(inc =>
                inc.InmateId == taskOverview.InmateId &&
                inc.AoTaskLookupId == taskOverview.TaskLookupId &&
                !inc.CompleteFlag);

            incDetails.PriorityFlag = !taskOverview.PriorityFlag;

            return await _context.SaveChangesAsync();
        }

        public AoTaskLookupVm GetCompleteTask(int aoTaskLookupId) =>
            _context.AoTaskLookup.Where(w => w.AoTaskLookupId == aoTaskLookupId)
                .Select(s => new AoTaskLookupVm
                {
                    AoTaskLookupId = s.AoTaskLookupId,
                    AoComponentId = s.AoComponentId,
                    TaskName = s.TaskName,
                    TaskInstruction = s.TaskInstructions
                }).SingleOrDefault();

        public int UpdateCompleteTasks(TasksCountVm taskDetails)
        {
            AoTaskQueue aoTaskQueue = _context.AoTaskQueue.First(queue =>
                queue.InmateId == taskDetails.InmateId && queue.AoTaskLookupId == taskDetails.TaskLookupId &&
                !queue.CompleteFlag);
            aoTaskQueue.CompleteFlag = true;
            aoTaskQueue.CompleteNote = taskDetails.CompleteNote;
            aoTaskQueue.CompleteBy = _personnelId;
            aoTaskQueue.CompleteDate = DateTime.Now;
            return _context.SaveChanges();
        }

        public KeeperNoKeeperDetails GetKeeperNoKeeper(int incarcerationId)
        {
            KeeperNoKeeperDetails keeper = new KeeperNoKeeperDetails
            {
                NoKeeperFlag = _context.Incarceration.Find(incarcerationId).NoKeeper,
                lstNoKeeperHistory = _context.IncarcerationNoKeeperHistory
                    .Where(inc => inc.IncarcerationId == incarcerationId).Select(incKeep =>
                        new NoKeeperHistory
                        {
                            KeeperFlag = incKeep.Keeper,
                            NoKeeperFlag = incKeep.NoKeeper,
                            NoKeeperReason = incKeep.NoKeeperReason,
                            NoKeeperNote = incKeep.NoKeeperNote,
                            NoKeeperDate = incKeep.CreateDate,
                            NoKeeperById = incKeep.CreateBy,
                            NoKeeperBy = _context.Personnel.Where(pers => pers.PersonnelId == incKeep.CreateBy).Select(
                                per => new PersonnelVm
                                {
                                    PersonLastName = per.PersonNavigation.PersonLastName,
                                    PersonFirstName = per.PersonNavigation.PersonFirstName
                                }).Single()
                        }
                    ).ToList()
            };

            return keeper;
        }

        public int SaveNoKeeperValues(NoKeeperHistory noKeeperValues)
        {
            Incarceration incarceration = _context.Incarceration.Find(noKeeperValues.incarcerationId);

            if (incarceration.NoKeeper == noKeeperValues.KeeperFlag)
            {
                int personId = _context.Inmate.Find(incarceration.InmateId).PersonId;
                string eventName = noKeeperValues.KeeperFlag
                    ? EventNameConstants.KEEPERSET
                    : EventNameConstants.NOKEEPERSET;

                _interfaceEngine.Export(new ExportRequestVm
                    {
                        EventName = eventName,
                        PersonnelId = _personnelId,
                        Param1 = personId.ToString(),
                        Param2 = noKeeperValues.incarcerationId.ToString()
                    });
            }
            else
            {
                if (_context.IncarcerationNoKeeperHistory.Count
                        (x => x.IncarcerationId == noKeeperValues.incarcerationId) == 0)
                {
                    int personId = _context.Inmate.Find(incarceration.InmateId).PersonId;
                    string eventName = noKeeperValues.KeeperFlag
                        ? EventNameConstants.KEEPERSET
                        : EventNameConstants.NOKEEPERSET;
                     _interfaceEngine.Export(new ExportRequestVm
                    {
                        EventName = eventName,
                        PersonnelId = _personnelId,
                        Param1 = personId.ToString(),
                        Param2 = noKeeperValues.incarcerationId.ToString()
                    });
                }
            }

            if (noKeeperValues.KeeperFlag)
            {
                incarceration.NoKeeper = false;
                incarceration.NoKeeperReason = noKeeperValues.NoKeeperReason;
                incarceration.NoKeeperNote = noKeeperValues.NoKeeperNote;
            }
            else
            {
                incarceration.NoKeeper = noKeeperValues.NoKeeperFlag;
                incarceration.NoKeeperReason = noKeeperValues.NoKeeperReason;
                incarceration.NoKeeperNote = noKeeperValues.NoKeeperNote;
            }

            IncarcerationNoKeeperHistory incNoKeep = new IncarcerationNoKeeperHistory
            {
                Keeper = noKeeperValues.KeeperFlag,
                NoKeeper = noKeeperValues.NoKeeperFlag,
                NoKeeperNote = noKeeperValues.NoKeeperNote,
                NoKeeperReason = noKeeperValues.NoKeeperReason,
                IncarcerationId = noKeeperValues.incarcerationId,
                CreateDate = DateTime.Now,
                CreateBy = _personnelId
            };
            _context.IncarcerationNoKeeperHistory.Add(incNoKeep);
            return _context.SaveChanges();
        }

        #region AssessmentOverview

        public BookingOverviewVm GetAssessmentDetails(int facilityId)
        {
            BookingOverviewVm assessmentOverviewVm = new BookingOverviewVm
            {
                BookingOverviewDetails = _context.Incarceration
                    .Where(i => i.Inmate.InmateActive == 1
                                && i.Inmate.FacilityId == facilityId
                                && i.IntakeCompleteFlag == 1
                                && i.BookCompleteFlag == 1
                                && !i.ReleaseOut.HasValue
                                && (!i.BookAndReleaseFlag.HasValue || i.BookAndReleaseFlag == 0)
                                && !i.AssessmentCompleteFlag)
                    .Select(i => new BookingOverviewDetails
                    {
                        InmateId = i.InmateId.Value,
                        IncarcerationId = i.IncarcerationId,
                        TransportFlag = i.TransportFlag == 1,
                        CreateDate = i.DateIn.Value,
                        NoKeeper = i.NoKeeper,
                        ExpediteBookingReason = i.ExpediteBookingReason,
                        ExpediteBookingFlag = i.ExpediteBookingFlag,
                        ExpediteBookingNote = i.ExpediteBookingNote,
                        ExpediteDate = i.ExpediteBookingDate,
                        ExpediteById = i.ExpediteBookingBy,
                        ReleaseOut = i.ReleaseOut
                    }).ToList()
            };

            List<int> officerIds = assessmentOverviewVm.BookingOverviewDetails.Where(i => i.ExpediteById.HasValue)
                    .Select(i => i.ExpediteById.Value).ToList();

            List<PersonnelVm> expediteOfficer = _iPersonService.GetPersonNameList(officerIds.ToList());

            int[] lstIncIds = assessmentOverviewVm.BookingOverviewDetails.Select(inc => inc.IncarcerationId).ToArray();
            int[] lstInmIds = assessmentOverviewVm.BookingOverviewDetails.Select(inc => inc.InmateId).ToArray();

            var lstIncarcerationArrestXrefDs =
                _context.IncarcerationArrestXref.Where(iax => lstIncIds.Contains(iax.IncarcerationId.Value) &&
                                                              iax.ArrestId.HasValue).Select(iax => new
                                                              {
                                                                  IncarcerationId = iax.IncarcerationId.Value,
                                                                  iax.Arrest.ArrestBookingNo,
                                                                  iax.Arrest.ArrestType
                                                              }).ToList();


            List<InmateHousing> lstPersonDetails = _bookingService.GetInmateDetails(lstInmIds.ToList()).ToList();

            List<KeyValuePair<int, string>> lstPrebook = _context.InmatePrebook
                .Where(ip => ip.IncarcerationId.HasValue
                             && lstIncIds.Contains(ip.IncarcerationId.Value)
                             && (!ip.CourtCommitFlag.HasValue || ip.CourtCommitFlag == 0)
                             && ip.DeleteFlag != 1).Select(inc => new
                    KeyValuePair<int, string>(inc.IncarcerationId.Value, inc.PreBookNumber)).ToList();

            List<AoWizardProgressIncarceration> lstAoWizardProgressIncarceration =
                _context.AoWizardProgressIncarceration.Where(awp =>
                    lstIncIds.Contains(awp.IncarcerationId)).ToList();

            int[] lstWizardProgressIds =
                lstAoWizardProgressIncarceration.Select(inc => inc.AoWizardProgressId).ToArray();

            List<AoWizardStepProgress> lstAoWizardStepProgress = _context.AoWizardStepProgress
                .Where(aws => lstWizardProgressIds.Contains(aws.AoWizardProgressId)).ToList();

            int[] arrTaskIds = _context.AoTaskLookupAssign.Where(lookAssign =>
                    lookAssign.TaskValidateLookup ==
                    _commonService.GetValidationType(TaskValidateType.AssessmentComplete) &&
                    !lookAssign.DeleteFlag &&
                    lookAssign.FacilityId == facilityId &&
                    !lookAssign.AoTaskLookup.DeleteFlag)
                .Select(task => task.AoTaskLookupId).ToArray();

            List<AoComponent> lstAoComponent = _context.AoComponent.ToList();

            List<TasksCountVm> lstAoTaskQueue = _context.AoTaskQueue.Where(inm =>
                    lstInmIds.Contains(inm.InmateId) && !inm.CompleteFlag && arrTaskIds.Contains(inm.AoTaskLookupId))
                .Select(que => new TasksCountVm
                {
                    InmateId = que.InmateId,
                    TaskLookupId = que.AoTaskLookupId,
                    ComponentId = que.AoTaskLookup.AoComponentId,
                    ComponentName = que.AoTaskLookup.AoComponentId.HasValue
                            ? lstAoComponent.Single(look =>
                                look.AoComponentId == que.AoTaskLookup.AoComponentId).ComponentName
                            : null,
                    TaskName = que.AoTaskLookup.TaskName,
                    PriorityFlag = que.PriorityFlag,
                    TaskIconPath = que.AoTaskLookup.AoComponentId.HasValue
                            ? lstAoComponent.Single(look => look.AoComponentId == que.AoTaskLookup.AoComponentId)
                                .StepIcon
                            : null,
                    TaskInstruction = que.AoTaskLookup.TaskInstructions
                }
                ).ToList();

            assessmentOverviewVm.TasksCount = _context.AoTaskLookupAssign.Where(lookAssign =>
                    !lookAssign.DeleteFlag && lookAssign.FacilityId == facilityId &&
                    !lookAssign.AoTaskLookup.DeleteFlag &&
                    lookAssign.TaskValidateLookup ==
                    _commonService.GetValidationType(TaskValidateType.AssessmentComplete))
                .Select(
                    look => new TasksCountVm
                    {
                        TaskLookupId = look.AoTaskLookupId,
                        TaskName = look.AoTaskLookup.TaskName
                    }).Distinct().ToList();

            assessmentOverviewVm.TasksCount.ForEach(task =>
            {
                task.InmateCount = lstAoTaskQueue.Count(queue => queue.TaskLookupId == task.TaskLookupId);
                task.TaskPriorityCount = lstAoTaskQueue.Count(queue =>
                    queue.TaskLookupId == task.TaskLookupId && queue.PriorityFlag);
            });

            assessmentOverviewVm.BookingOverviewDetails.ForEach(
                inm =>
                {
                    inm.PrebookNumber = lstPrebook.FirstOrDefault(pre => pre.Key == inm.IncarcerationId).Value;
                    inm.PersonDetails = lstPersonDetails.Single(per => per.InmateId == inm.InmateId);
                    inm.InmateTasks = lstAoTaskQueue.Where(tas => tas.InmateId == inm.InmateId).ToList();

                    inm.AssessmentWizardProgressId = inm.NoKeeper
                        ? lstAoWizardProgressIncarceration.FirstOrDefault(awp =>
                            awp.IncarcerationId == inm.IncarcerationId && awp.AoWizardId == 16)?.AoWizardProgressId
                        : lstAoWizardProgressIncarceration.FirstOrDefault(awp =>
                            awp.IncarcerationId == inm.IncarcerationId && awp.AoWizardId == 15)?.AoWizardProgressId;

                    if (inm.AssessmentWizardProgressId.HasValue)
                    {
                        inm.AssessmentWizards = lstAoWizardStepProgress
                            .Where(aws => aws.AoWizardProgressId == inm.AssessmentWizardProgressId.Value).Select(aw =>
                                new WizardStep
                                {
                                    ComponentId = aw.AoComponentId,
                                    WizardProgressId = aw.AoWizardProgressId,
                                    AoWizardFacilityStepId = aw.AoWizardFacilityStepId,
                                    WizardStepProgressId = aw.AoWizardStepProgressId,
                                    StepComplete = aw.StepComplete
                                }).ToList();
                    }

                    if (inm.ExpediteById.HasValue)
                    {
                        inm.ExpediteBy = expediteOfficer.Single(ao => ao.PersonnelId == inm.ExpediteById.Value);
                    }

                    inm.CaseType = lstIncarcerationArrestXrefDs.Where(
                        iax => inm.IncarcerationId == iax.IncarcerationId).Select(iax => iax.ArrestType).ToArray();
                }
            );

            return assessmentOverviewVm;
        }

        public async Task<int> UpdateAssessmentComplete(BookingComplete assessmentComplete)
        {
            Incarceration incDetails = _context.Incarceration.Single(inc =>
                inc.InmateId == assessmentComplete.InmateId &&
                inc.IncarcerationId == assessmentComplete.IncarcerationId);

            incDetails.AssessmentCompleteFlag = assessmentComplete.IsComplete;
            incDetails.AssessmentCompleteDate = DateTime.Now;
            incDetails.AssessmentCompleteBy = _personnelId;
            incDetails.AssessmentCompleteByNavigation =
                _context.Personnel.Single(per => per.PersonnelId == _personnelId);

            int personId = _iPersonService.GetInmateDetails(incDetails.InmateId ?? 0).PersonId;
            //EventVm evenHandle = new EventVm
            //{
            //    CorresId = incDetails.IncarcerationId,
            //    EventName = EventNameConstants.ASSESSMENTCOMPLETED,
            //    PersonId = personId
            //};
            ////Insert into Web Service Event Type
            //_commonService.EventHandle(evenHandle);
            //await _interfaceEngine.Export(new ExportRequestVm
            //{
            //    EventName = EventNameConstants.ASSESSMENTCOMPLETED,
            //    PersonnelId = _personnelId,
            //    Param1 = personId.ToString(),
            //    Param2 = incDetails.IncarcerationId.ToString()
            //});

            if (assessmentComplete.IsComplete)
            {
                WebServiceEventType webServiceEventType = _context.WebServiceEventType
                    .FirstOrDefault(wse => wse.WebServiceEventName == LookupConstants.ASSESSMENTCOMPLETE);

                bool eventQueueFlag = (_context.WebServiceEventSetting.FirstOrDefault(wse => wse.EventQueueFlag == 1)
                    ?.EventQueueFlag).HasValue;

                if (eventQueueFlag && webServiceEventType != null)
                {
                    List<WebServiceEventAssign> lstWebServiceEventAssign = _context.WebServiceEventAssign.Where(wsea =>
                        wsea.WebServiceEventTypeId == webServiceEventType.WebServiceEventTypeId &&
                        wsea.WebServiceEventInactive == 0).ToList();

                    lstWebServiceEventAssign.ForEach(lws =>
                    {
                        WebServiceEventQueue wseq = new WebServiceEventQueue
                        {
                            WebServiceEventAssignId = lws.WebServiceEventAssignId,
                            CreateDate = DateTime.Now,
                            CreateBy = _personnelId,
                            WebServiceEventParameter1 = personId.ToString(),
                            WebServiceEventParameter2 = assessmentComplete.IncarcerationId.ToString()
                        };
                        _context.Add(wseq);

                        if (webServiceEventType.WebServiceEventRunHistory != 1) return;
                        WebServiceEventTypeHistory wseth = new WebServiceEventTypeHistory
                        {
                            WebServiceEventTypeId = lws.WebServiceEventAssignId,
                            CreateDate = DateTime.Now,
                            CreateBy = _personnelId,
                            WebServiceEventParameter1 = personId.ToString(),
                            WebServiceEventParameter2 = assessmentComplete.IncarcerationId.ToString()
                        };
                        _context.Add(wseth);
                    });
                }

                _inmateService.CreateTask(assessmentComplete.InmateId, incDetails.NoKeeper
                    ? TaskValidateType.AssessmentCompleteNonKeeperEvent
                    : TaskValidateType.AssessmentCompleteKeeperEvent);
            }

            return await _context.SaveChangesAsync();
        }

        #endregion
    }
}
