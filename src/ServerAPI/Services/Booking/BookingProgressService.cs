using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using GenerateTables.Models;
using ServerAPI.ViewModels;
using ServerAPI.Utilities;
using System.IO;
using JetBrains.Annotations;

namespace ServerAPI.Services
{
    [UsedImplicitly]
    public class BookingProgressService : IBookingProgressService
    {
        private readonly AAtims _context;
        private readonly ICommonService _commonService;
        private readonly IRequestService _requestService;
        private readonly IPersonService _iPersonService;
        private readonly IBookingService _iBookingService;
        private readonly IRecordsCheckService _recordsCheckService;
        private readonly IAppletsSavedService _appletsSavedService;

        public BookingProgressService(AAtims context, ICommonService commonService,
            IPersonService personService, IRecordsCheckService recordsCheckService, IRequestService requestService,
            IBookingService bookingService, IAppletsSavedService appletsSavedService)
        {
            _context = context;
            _commonService = commonService;
            _iPersonService = personService;
            _recordsCheckService = recordsCheckService;
            _requestService = requestService;
            _iBookingService = bookingService;
            _appletsSavedService = appletsSavedService;
        }

        // Get Booking in progress counts to bind left side grid.
        public BookingInProgressCountVm GetBookingInProgress(BookingInputsVm iBooking)
        {
            List<Incarceration> lstIncarcerations = GetOverallBookings(iBooking).ToList();

            BookingInProgressCountVm bookings = new BookingInProgressCountVm
            {
                BookingCounts = new List<KeyValuePair<BookingTypeEnum, int>>
                {
                    new KeyValuePair<BookingTypeEnum, int>(BookingTypeEnum.OverallBookingProgress,
                        lstIncarcerations.Count),
                    new KeyValuePair<BookingTypeEnum, int>(BookingTypeEnum.BookingDataProgress,
                        GetBookingDataProgress(iBooking).Count())
                }
            };

            //42057 - Records Check submodule Id Number
            bool bRecordCheck = _context.AppAoSubModule.SingleOrDefault(app =>
                                    app.AppAoSubModuleId == 42057)?.AppAoSubModuleVisible == 1;

            if (bRecordCheck)
            {
                bookings.BookingCounts.Add(new KeyValuePair<BookingTypeEnum, int>(BookingTypeEnum.RecordsCheckResponsess,
                    _recordsCheckService.GetRecordsCheckResponseCount(iBooking.FacilityId).Count()));
            }

            bookings.BookingCounts.Add(new KeyValuePair<BookingTypeEnum, int>(BookingTypeEnum.ExternalAttachments,
                GetAttachmentsCount(iBooking)));

            // 7 - Request Type to be show in Booking progress page
            bookings.PendingQueue = _requestService.GetBookingPendingReq(iBooking.FacilityId, 7);
            bookings.AssignedQueue = _requestService.GetBookingAssignedReq(7);
            int[] incInmateInt = lstIncarcerations.Select(inc => inc.InmateId ?? 0).ToArray();
            bookings.TasksCount = GetTasksDetails(iBooking.FacilityId, incInmateInt);
            bookings.BookingDetails = GetBookingInProgressDetails(iBooking);

            return bookings;
        }

        // To Get Booking in progress details to bind right side grid.
        public BookingInProgressDetailsVm GetBookingInProgressDetails(BookingInputsVm iBooking)
        {
            BookingInProgressDetailsVm bookingDetails = new BookingInProgressDetailsVm();
            switch (iBooking.BookingType)
            {
                case BookingTypeEnum.OverallBookingProgress:
                    bookingDetails.OverallBookingProgress = GetBookingProgress(iBooking);
                    break;
                case BookingTypeEnum.BookingDataProgress:
                    bookingDetails.BookingDataProgress = GetBookDataProgressDetails(iBooking);
                    break;
                case BookingTypeEnum.RecordsCheckResponsess:
                    bookingDetails.RecordsCheckResponse =
                        _recordsCheckService.GetRecordsCheckResponse(iBooking.FacilityId);
                    break;
                case BookingTypeEnum.ExternalAttachments:
                    bookingDetails.ExternalAttachments = GetExternalAttachments(iBooking);
                    break;
                case BookingTypeEnum.Pending:
                    // 7 - List of Request Type to be show in Booking progress page
                    bookingDetails.PendingRequest = _requestService.GetBookingPendingReqDetail(iBooking.FacilityId,
                        iBooking.RequestLookupId, 7);
                    break;
                case BookingTypeEnum.Assigned:
                    // 7 - List of Request Type to be show in Booking progress page
                    bookingDetails.AssignedRequest =
                        _requestService.GetBookingAssignedReqDetail(iBooking.RequestLookupId, 7);
                    break;
                case BookingTypeEnum.Tasks:
                    bookingDetails.OverallBookingProgress = GetBookingProgress(iBooking);
                    break;
            }

            return bookingDetails;
        }

        #region LeftSideGridExplanation

        // Get Overall Bookings in a facility
        private IQueryable<Incarceration> GetOverallBookings(BookingInputsVm iBooking)
        {
            IQueryable<Incarceration> lstIncarceration =
                _context.Incarceration.Where(inc => !inc.ReleaseOut.HasValue
                                                    && inc.Inmate.InmateActive == 1 &&
                                                    inc.Inmate.FacilityId == iBooking.FacilityId);

            if (iBooking.ShowIntake && !iBooking.ActiveOnly)
            {
                lstIncarceration = lstIncarceration.Where(inc => !inc.BookCompleteFlag.HasValue);
            }

            if (!iBooking.ShowIntake && !iBooking.ActiveOnly)
            {
                lstIncarceration = lstIncarceration.Where(inc =>
                    inc.IntakeCompleteFlag.HasValue && inc.IntakeCompleteFlag == 1
                                                    && !inc.BookCompleteFlag.HasValue ||
                    inc.BookAndReleaseFlag.HasValue && inc.BookAndReleaseFlag == 1);
            }

            return lstIncarceration;
        }

        // Get Booking Data Details in a facility
        private IQueryable<ArrestDetailsVm> GetBookingDataProgress(BookingInputsVm iBooking)
        {
            IQueryable<Incarceration> lstIncarceration =
                _context.Incarceration.Where(inc => inc.Inmate.InmateActive == 1
                                                    && inc.Inmate.FacilityId == iBooking.FacilityId);

            if (iBooking.ShowIntake && !iBooking.ActiveOnly)
            {
                lstIncarceration = lstIncarceration.Where(inc => !inc.BookCompleteFlag.HasValue);
            }

            if (!iBooking.ShowIntake && !iBooking.ActiveOnly)
            {
                lstIncarceration =
                    lstIncarceration.Where(inc => inc.IntakeCompleteFlag.HasValue && inc.IntakeCompleteFlag == 1 ||
                                                  inc.BookAndReleaseFlag.HasValue && inc.BookAndReleaseFlag == 1);
            }

            IQueryable<IncarcerationArrestXref> incArrXref = lstIncarceration.SelectMany(inc =>
                inc.IncarcerationArrestXref.Where(iax =>
                    iax.IncarcerationId == inc.IncarcerationId && !iax.ReleaseDate.HasValue && iax.ArrestId.HasValue));

            IQueryable<ArrestDetailsVm> arrestDetail = incArrXref.Where(iax => !iax.Arrest.BookingCompleteFlag.HasValue)
                .Select(i => new ArrestDetailsVm
                {
                    IncarcerationArrestXrefId = i.IncarcerationArrestXrefId,
                    IncarcerationId = i.IncarcerationId.Value,
                    ReleaseDate = i.ReleaseDate,
                    ReleaseReason = i.ReleaseReason,
                    ArrestId = i.ArrestId.Value,
                    BookingNumber = i.Arrest.ArrestBookingNo,
                    ArrestTypeId = i.Arrest.ArrestType,
                    ArrestBookingDate = i.Arrest.ArrestBookingDate,
                    ArrestCaseNumber = i.Arrest.ArrestCaseNumber,
                    ArrestCourtDocket = i.Arrest.ArrestCourtDocket,
                    ArrestActive = i.Arrest.ArrestActive == 1,
                    InmateId = i.Arrest.InmateId,
                    ArrestAgencyId = i.Arrest.ArrestingAgencyId,
                    BillingAgencyId = i.Arrest.ArrestBillingAgencyId,
                    ArrestCourtJurisdictionId = i.Arrest.ArrestCourtJurisdictionId
                });

            return arrestDetail;
        }

        // Get External Attachments in a facility
        private int GetAttachmentsCount(BookingInputsVm iBooking)
        {
            IQueryable<AppletsSaved> appSaved = GetAppletsSaved();

            int inmateCount = _context.Inmate.SelectMany(inm => appSaved.Where(app =>
                app.ExternalInmateId.Value == inm.InmateId && inm.FacilityId == iBooking.FacilityId)).Count();

            return inmateCount;
        }

        private List<TasksCountVm> GetTasksDetails(int facilityId, int[] inmateIds)
        {
            List<KeyValuePair<bool, int>> lstAoTaskQueue = _context.AoTaskQueue.Where(queue =>
                    !queue.CompleteFlag && !queue.AoTaskLookup.DeleteFlag && inmateIds.Contains(queue.InmateId))
                .Select(queue =>
                    new KeyValuePair<bool, int>(queue.PriorityFlag, queue.AoTaskLookupId)).ToList();

            List<TasksCountVm> tasksCount = _context.AoTaskLookupAssign.Where(lookAssign =>
                    lookAssign.TaskValidateLookup == _commonService.GetValidationType(TaskValidateType.BookingComplete)
                    && !lookAssign.DeleteFlag && lookAssign.FacilityId == facilityId &&
                    !lookAssign.AoTaskLookup.DeleteFlag)
                .Select(look => new TasksCountVm
                {
                    TaskLookupId = look.AoTaskLookupId,
                    TaskName = look.AoTaskLookup.TaskName,
                    InmateCount = lstAoTaskQueue.Count(queue => queue.Value == look.AoTaskLookupId),
                    TaskPriorityCount =
                        lstAoTaskQueue.Count(queue => queue.Value == look.AoTaskLookupId && queue.Key),
                    FormTemplateId = look.FormTemplatesId,
                    FormTemplateName = look.FormTemplates.DisplayName
                }).Distinct().ToList();

            return tasksCount;
        }

        #endregion

        // right side grid explanation

        #region RightSideGridExplanation

        // Get booking in progress details for right side binding.
        private List<BookingVm> GetBookingProgress(BookingInputsVm iBooking)
        {
            IQueryable<Incarceration> lstIncarceration = GetOverallBookings(iBooking);

            List<InmateHousing> lstPersonDetails =
              _iBookingService.GetInmateDetails(lstIncarceration.Select(i => i.InmateId.Value).ToList());

            List<BookingVm> overallBookings = lstIncarceration.Select(inc => new BookingVm
            {
                IncarcerationId = inc.IncarcerationId,
                InmateId = inc.InmateId.Value,
                IntakeCompleteFlag = inc.IntakeCompleteFlag == 1,
                BookReleaseFlag = inc.BookAndReleaseFlag == 1,
                DateIn = inc.DateIn,
                BookCompleteFlag = inc.BookCompleteFlag == 1,
                BookingWizardLastStepId = inc.BookingWizardLastStepId,
                NoKeeper = inc.NoKeeper,
                PersonDetails = lstPersonDetails.Single(inm => inm.InmateId == inc.InmateId),
                ExpediteBookingReason = inc.ExpediteBookingReason,
                ExpediteBookingFlag = inc.ExpediteBookingFlag,
                ExpediteBookingNote = inc.ExpediteBookingNote,
                ExpediteDate = inc.ExpediteBookingDate,
                ExpediteById = inc.ExpediteBookingBy,
                VerifyId = inc.VerifyIDFlag
            }).ToList();

            int[] lstIncIds = lstIncarceration.Select(ii => ii.IncarcerationId).ToArray();

            var lstIncarcerationArrestXrefDs =
                _context.IncarcerationArrestXref.Where(iax => lstIncIds.Contains(iax.IncarcerationId.Value) &&
                                                              iax.ArrestId.HasValue).Select(iax => new
                {
                    IncarcerationId = iax.IncarcerationId.Value,
                    iax.Arrest.ArrestBookingNo,
                    iax.Arrest.ArrestType
                }).ToList();

            var lstPrebook = _context.InmatePrebook
                .Where(ip => ip.IncarcerationId.HasValue
                             && lstIncIds.Contains(ip.IncarcerationId.Value)
                             && !ip.DeleteFlag.HasValue
                             && (!ip.CourtCommitFlag.HasValue || ip.CourtCommitFlag == 0)).Select(inc => new
                {
                    inc.IncarcerationId,
                    inc.PreBookNumber
                });

            List<AoWizardProgressIncarceration> lstAoWizardProgressIncarceration =
                _context.AoWizardProgressIncarceration.Where(awp =>
                    lstIncIds.Contains(awp.IncarcerationId)).ToList();

            List<AoComponent> lstAoComponent = _context.AoComponent.ToList();

            int[] arrTaskIds = _context.AoTaskLookupAssign.Where(lookAssign =>
                    lookAssign.TaskValidateLookup ==
                    _commonService.GetValidationType(TaskValidateType.BookingComplete) &&
                    !lookAssign.DeleteFlag &&
                    lookAssign.FacilityId == iBooking.FacilityId &&
                    !lookAssign.AoTaskLookup.DeleteFlag)
                .Select(task => task.AoTaskLookupId).ToArray();

            int[] inmateIds = lstIncarceration.Select(i => i.InmateId.Value).ToArray();

            List<TasksCountVm> lstAoTaskQueue = _context.AoTaskQueue.Where(inm =>
                    inmateIds.Contains(inm.InmateId) && !inm.CompleteFlag && arrTaskIds.Contains(inm.AoTaskLookupId))
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

            List<int> officerIds = overallBookings.Where(i => i.ExpediteById.HasValue)
                .Select(i => i.ExpediteById.Value).ToList();

            List<PersonnelVm> expediteOfficer = _iPersonService.GetPersonNameList(officerIds.ToList());

            overallBookings.ForEach(ovp =>
            {
                ovp.PreBookNumber = lstPrebook.FirstOrDefault(pre => pre.IncarcerationId == ovp.IncarcerationId)
                    ?.PreBookNumber;
                ovp.OriginalBookingNumber = lstIncarcerationArrestXrefDs.FirstOrDefault(
                    iax => ovp.IncarcerationId == iax.IncarcerationId)?.ArrestBookingNo;

                if (ovp.ExpediteById.HasValue)
                {
                    ovp.ExpediteBy = expediteOfficer.Single(ao => ao.PersonnelId == ovp.ExpediteById.Value);
                }

                Wizards wizardId = ovp.BookReleaseFlag ? Wizards.bookAndRelease : Wizards.booking;
                ovp.WizardProgressId = lstAoWizardProgressIncarceration.FirstOrDefault(awp =>
                    awp.IncarcerationId == ovp.IncarcerationId && awp.AoWizardId == (int)wizardId)?.AoWizardProgressId;

                if (ovp.WizardProgressId.HasValue)
                {
                    ovp.LastStep = _context.AoWizardStepProgress
                        .Where(aws => aws.AoWizardProgressId == ovp.WizardProgressId.Value).Select(aw => new WizardStep
                        {
                            ComponentId = aw.AoComponentId,
                            WizardProgressId = aw.AoWizardProgressId,
                            AoWizardFacilityStepId = aw.AoWizardFacilityStepId,
                            WizardStepProgressId = aw.AoWizardStepProgressId,
                            StepComplete = aw.StepComplete
                        }).ToList();
                }

                ovp.BookingTasks = lstAoTaskQueue.Where(qu => qu.InmateId == ovp.InmateId).ToList();

                ovp.CaseType = lstIncarcerationArrestXrefDs.Where(
                    iax => ovp.IncarcerationId == iax.IncarcerationId).Select(iax => iax.ArrestType).ToArray();

            });

            return overallBookings;
        }

        // Get booking data details for right side binding.
        private List<ArrestDetailsVm> GetBookDataProgressDetails(BookingInputsVm iBooking)
        {
            List<ArrestDetailsVm> bookingData = GetBookingDataProgress(iBooking).ToList();

            List<int> officerIds =
                bookingData.Select(i => new[] { i.ArrestAgencyId, i.BillingAgencyId, i.ArrestCourtJurisdictionId })
                    .SelectMany(i => i).Where(i => i.HasValue)
                    .Select(i => i.Value).Distinct().ToList();

            List<AgencyVm> lstAgency = _commonService.GetAgencyNameList(officerIds).ToList();

            string[] arrestTypes = bookingData.Select(i => i.ArrestTypeId)
                .Where(i => i != null).Distinct().ToArray();

            List<KeyValuePair<int, string>> lstLookups = _context.Lookup
                .Where(look => look.LookupType == LookupConstants.ARRTYPE &&
                               arrestTypes.Contains(Convert.ToString(look.LookupIndex,
                                   CultureInfo.InvariantCulture)) && look.LookupInactive == 0)
                .Select(look => new KeyValuePair<int, string>(look.LookupIndex, look.LookupDescription))
                .ToList();

            List<InmateHousing> lstPersonDetails = _iBookingService.GetInmateDetails(bookingData.Where(i => i.InmateId.HasValue)
                .Select(i => i.InmateId.Value).ToList());


            List<AoWizardProgressArrest> lstAoWizardProgress = _context.AoWizardProgressArrest.Where(awp =>
                bookingData.Select(i => i.ArrestId).Contains(awp.ArrestId) &&
                awp.AoWizardId == 14).ToList();

            List<WizardStep> lstAoWizardStepProgress = _context.AoWizardStepProgress
                .Where(aws => lstAoWizardProgress.Select(i => i.AoWizardProgressId).Contains(aws.AoWizardProgressId))
                .Select(aw => new WizardStep
                {
                    ComponentId = aw.AoComponentId,
                    AoWizardFacilityStepId = aw.AoWizardFacilityStepId,
                    WizardProgressId = aw.AoWizardProgressId,
                    WizardStepProgressId = aw.AoWizardStepProgressId,
                    StepComplete = aw.StepComplete,

                }).ToList();

            bookingData.ForEach(bookings =>
            {
                bookings.ArrestType = bookings.ArrestTypeId != null
                    ? lstLookups.SingleOrDefault(look =>
                        Convert.ToString(look.Key, CultureInfo.InvariantCulture) == bookings.ArrestTypeId.Trim()).Value
                    : null;

                bookings.PersonDetails = lstPersonDetails.Single(lpd =>
                    bookings.InmateId != null && lpd.InmateId == bookings.InmateId.Value);

                bookings.ArrestAgencyName =
                    lstAgency.Single(la => la.AgencyId == bookings.ArrestAgencyId).AgencyAbbreviation;

                if (bookings.BillingAgencyId.HasValue)
                {
                    bookings.BillingAgencyName = lstAgency.Single(la => la.AgencyId == bookings.BillingAgencyId)
                        .AgencyAbbreviation;
                }

                if (bookings.ArrestCourtJurisdictionId.HasValue)
                {
                    bookings.ArrestCourtJurisdictionName = lstAgency
                        .Single(la => la.AgencyId == bookings.ArrestCourtJurisdictionId).AgencyAbbreviation;
                }

                bookings.WizardProgressId = lstAoWizardProgress.FirstOrDefault(awp =>
                    awp.ArrestId == bookings.ArrestId)?.AoWizardProgressId;

                if (bookings.WizardProgressId.HasValue)
                {
                    bookings.LastStep = lstAoWizardStepProgress
                        .Where(aws => aws.WizardProgressId == bookings.WizardProgressId.Value).ToList();
                }

                if(bookings.ArrestTypeId != null)
                {
                    bookings.CaseType = new[] { bookings.ArrestTypeId.ToString() }; 
                }                
            });
            return bookingData;
        }

        // Get external attachments details.
        private List<ExternalAttachmentsVm> GetExternalAttachments(BookingInputsVm iBooking)
        {
            string facilityAbbr = _commonService.GetFacilities()
                .Single(fac => fac.FacilityId == iBooking.FacilityId).FacilityAbbr;

            List<ExternalAttachmentsVm> lstExternalAttachments = GetAppletsSaved().Where(apps =>
                    apps.ExternalInmateId.HasValue && apps.ExternalInmate.FacilityId == iBooking.FacilityId)
                .Select(apps => new ExternalAttachmentsVm
                {
                    AppletsSavedId = apps.AppletsSavedId,
                    InmateId = apps.ExternalInmateId,
                    FacilityId = iBooking.FacilityId,
                    FacilityAbbr = facilityAbbr,
                    UpdateDate = apps.UpdateDate,
                    CreateDate = apps.CreateDate,
                    LastAccessDate = apps.LastAccessDate,
                    CreatedBy = apps.CreatedBy,
                    LastAccessBy = apps.LastAccessBy,
                    UpdatedBy = apps.UpdatedBy,
                    AppletsSavedType = apps.AppletsSavedType,
                    AppletsSavedTitle = apps.AppletsSavedTitle,

                    AppletsSavedDescription = apps.AppletsSavedDescription,
                    AppletsSavedKeyword1 = apps.AppletsSavedKeyword1,
                    AppletsSavedKeyword2 = apps.AppletsSavedKeyword2,
                    AppletsSavedKeyword3 = apps.AppletsSavedKeyword3,
                    AppletsSavedKeyword4 = apps.AppletsSavedKeyword4,
                    AppletsSavedKeyword5 = apps.AppletsSavedKeyword5,
                    AppletsSavedPath = apps.AppletsSavedIsExternal
                        ? _appletsSavedService.GetExternalPath() + apps.AppletsSavedPath : apps.AppletsSavedPath,
                    AppletsDeleteFlag = apps.AppletsDeleteFlag == 1,
                    ExternalAcceptFlag = apps.ExternalAcceptFlag == 1,
                    HousingUnitLocation = apps.HousingUnitLocation,
                    HousingUnitNumber = apps.HousingUnitNumber,
                    AttachmentFile = Path.GetFileName(apps.AppletsSavedPath)
                }).ToList();

            List<int> officerIds =
                lstExternalAttachments.Select(i => new[] { i.CreatedBy, i.LastAccessBy, i.UpdatedBy })
                    .SelectMany(i => i)
                    .Where(i => i.HasValue)
                    .Select(i => i.Value)
                    .ToList();

            List<PersonnelVm> attachOfficer = _iPersonService.GetPersonNameList(officerIds.ToList());

            lstExternalAttachments.ForEach(lea =>
            {
                if (lea.InmateId.HasValue)
                {
                    lea.PersonDetails = _iPersonService.GetInmateDetails(lea.InmateId.Value);
                }

                if (lea.CreatedBy.HasValue)
                {
                    lea.CreatedOfficer = attachOfficer.Single(ao => ao.PersonnelId == lea.CreatedBy.Value);
                }

                if (lea.LastAccessBy.HasValue)
                {
                    lea.LastAccessOfficer = attachOfficer.Single(ao => ao.PersonnelId == lea.LastAccessBy.Value);
                }

                if (lea.UpdatedBy.HasValue)
                {
                    lea.UpdatedOfficer = attachOfficer.Single(ao => ao.PersonnelId == lea.UpdatedBy.Value);
                }
            }
            );

            return lstExternalAttachments;
        }
        
        // get attachment details
        private IQueryable<AppletsSaved> GetAppletsSaved() => _context.AppletsSaved.Where(apps =>
            apps.ExternalInmateId.HasValue && !apps.ExternalAcceptFlag.HasValue && apps.AppletsDeleteFlag == 0);

        #endregion

        #region BookingOverview

        private List<InmateHousing> GetPersonDetails(List<int> personIds) =>
            _context.Person.Where(x => personIds.Contains(x.PersonId))
                .Select(a => new InmateHousing
                {
                    PersonId = a.PersonId,
                    PersonFirstName = a.PersonFirstName,
                    PersonLastName = a.PersonLastName,
                    PersonMiddleName = a.PersonMiddleName,
                    PersonSexLast = a.PersonSexLast
                }).ToList();

        public BookingOverviewVm GetBookingOverviewDetails(int facilityId)
        {
            BookingOverviewVm bookingOverviewVm = new BookingOverviewVm
            {
                BookingOverviewDetails = _context.Incarceration
                    .Where(i => i.Inmate.InmateActive == 1
                                && i.Inmate.FacilityId == facilityId
                                && !i.ReleaseOut.HasValue
                                && (!i.BookAndReleaseFlag.HasValue || i.BookAndReleaseFlag == 0))
                    .Select(i => new BookingOverviewDetails
                    {
                        InmateId = i.InmateId.Value,
                        IncarcerationId = i.IncarcerationId,
                        IntakeCompleteFlag = i.IntakeCompleteFlag == 1,
                        BookAndReleaseFlag = i.BookAndReleaseFlag == 1,
                        BookCompleteFlag = i.BookCompleteFlag == 1,
                        ReleaseCompleteFlag = i.ReleaseCompleteFlag == 1,
                        AssessmentCompleteFlag = i.AssessmentCompleteFlag,
                        OverallFinalReleaseDate = i.OverallFinalReleaseDate,
                        TransportFlag = i.TransportFlag == 1,
                        TransportDate = i.TransportScheduleDate,
                        CreateDate = i.DateIn.Value,
                        PrebookCompleteFlag = true,
                        ReleaseClearFlag = i.ReleaseClearFlag == 1,
                        NoKeeper = i.NoKeeper,
                        ExpediteBookingReason = i.ExpediteBookingReason,
                        ExpediteBookingFlag = i.ExpediteBookingFlag,
                        ExpediteBookingNote = i.ExpediteBookingNote,
                        ExpediteDate = i.ExpediteBookingDate,
                        ExpediteById = i.ExpediteBookingBy,
                        ReleaseOut = i.ReleaseOut,
                        VerifyId = i.VerifyIDFlag
                    }).ToList()
            };

            bookingOverviewVm.BookingOverviewDetails.AddRange(_context.InmatePrebook
                .Where(ip => ip.FacilityId == facilityId && (!ip.DeleteFlag.HasValue || ip.DeleteFlag == 0) &&
                             ip.CompleteFlag == 1 && (!ip.IncarcerationId.HasValue || ip.IncarcerationId == 0) &&
                             (!ip.TempHoldId.HasValue || ip.TempHoldId == 0)
                             && (!ip.CourtCommitFlag.HasValue || ip.CourtCommitFlag == 0)).Select(ip =>
                    new BookingOverviewDetails
                    {
                        PrebookId = ip.InmatePrebookId,
                        PrebookNumber = ip.PreBookNumber,
                        CreateDate = ip.PrebookDate,
                        PersonId = ip.PersonId,
                        PrebookCompleteFlag = false
                    }).ToList());

            int[] lstInmIds = bookingOverviewVm.BookingOverviewDetails.Select(inc => inc.InmateId).ToArray();

            List<TasksCountVm> lstAoTaskQueue = _context.AoTaskQueue
                .Where(inm => lstInmIds.Contains(inm.InmateId) && !inm.CompleteFlag && !inm.AoTaskLookup.DeleteFlag)
                .Select(que => new TasksCountVm
                {
                    InmateId = que.InmateId,
                    TaskLookupId = que.AoTaskLookupId,
                    ComponentId = que.AoTaskLookup.AoComponentId,
                    ComponentName = que.AoTaskLookup.AoComponent.ComponentName,
                    TaskName = que.AoTaskLookup.TaskName,
                    PriorityFlag = que.PriorityFlag,
                    TaskIconPath = que.AoTaskLookup.AoComponent.StepIcon
                }).ToList();

            bookingOverviewVm.TasksCount = _context.AoTaskLookupAssign.Where(lookAssign =>
                    !lookAssign.DeleteFlag && lookAssign.FacilityId == facilityId &&
                    !lookAssign.AoTaskLookup.DeleteFlag)
                .Select(
                    look => new TasksCountVm
                    {
                        TaskLookupId = look.AoTaskLookupId,
                        TaskName = look.AoTaskLookup.TaskName
                    }).Distinct().ToList();

            bookingOverviewVm.TasksCount.ForEach(task =>
            {
                task.InmateCount = lstAoTaskQueue.Count(queue => queue.TaskLookupId == task.TaskLookupId);
                task.TaskPriorityCount = lstAoTaskQueue.Count(queue =>
                    queue.TaskLookupId == task.TaskLookupId && queue.PriorityFlag);
            });

            bookingOverviewVm.BookingOverviewDetails.ForEach(inm =>
            {
                inm.InmateTasks = lstAoTaskQueue.Where(tas => tas.InmateId == inm.InmateId).ToList();
            });

            return bookingOverviewVm;
        }

        public BookingOverviewVm GetBookingOverview(int facilityId , int inmateId)
        {

            BookingOverviewVm bookingOverviewVm = new BookingOverviewVm
            {
                BookingOverviewDetails = inmateId > 0
                    ? _context.Incarceration
                        .Where(i => i.Inmate.InmateActive == 1 && i.InmateId == inmateId
                            && i.Inmate.FacilityId == facilityId
                            && !i.ReleaseOut.HasValue
                            && (inmateId != 0 || !i.BookAndReleaseFlag.HasValue || i.BookAndReleaseFlag == 0))
                        .Select(i => new BookingOverviewDetails
                        {
                            InmateId = i.InmateId.Value,
                            IncarcerationId = i.IncarcerationId,
                            IntakeCompleteFlag = i.IntakeCompleteFlag == 1,
                            BookAndReleaseFlag = i.BookAndReleaseFlag == 1,
                            BookCompleteFlag = i.BookCompleteFlag == 1,
                            AssessmentCompleteFlag = i.AssessmentCompleteFlag,
                            ReleaseCompleteFlag = i.ReleaseCompleteFlag == 1,
                            PrebookCompleteFlag = true,
                            NoKeeper = i.NoKeeper,
                            ExpediteById = i.ExpediteBookingBy
                        }).ToList()
                    : _context.Incarceration
                        .Where(i => i.Inmate.InmateActive == 1 && i.Inmate.FacilityId == facilityId
                            && !i.ReleaseOut.HasValue && (!i.BookAndReleaseFlag.HasValue || i.BookAndReleaseFlag == 0))
                        .Select(i => new BookingOverviewDetails
                        {
                            InmateId = i.InmateId.Value,
                            IncarcerationId = i.IncarcerationId,
                            IntakeCompleteFlag = i.IntakeCompleteFlag == 1,
                            BookAndReleaseFlag = i.BookAndReleaseFlag == 1,
                            BookCompleteFlag = i.BookCompleteFlag == 1,
                            PrebookCompleteFlag = true,
                            NoKeeper = i.NoKeeper,
                            ExpediteById = i.ExpediteBookingBy
                        }).ToList()
            };


            List<int> officerIds = bookingOverviewVm.BookingOverviewDetails.Where(i => i.ExpediteById.HasValue)
                    .Select(i => i.ExpediteById.Value).ToList();

            List<PersonnelVm> expediteOfficer = _iPersonService.GetPersonNameList(officerIds.ToList());

            int[] lstIncIds = bookingOverviewVm.BookingOverviewDetails.Select(inc => inc.IncarcerationId).ToArray();
            int[] lstInmIds = bookingOverviewVm.BookingOverviewDetails.Select(inc => inc.InmateId).ToArray();

            var lstIncarcerationArrestXrefDs = _context.IncarcerationArrestXref.Where(iax =>
                lstIncIds.Contains(iax.IncarcerationId.Value) && iax.ArrestId.HasValue)
                .Select(iax => new
                {
                    IncarcerationId = iax.IncarcerationId.Value,
                    iax.Arrest.ArrestBookingNo,
                    iax.Arrest.ArrestType
                }).ToList();

            List<InmateHousing> lstPersonDetails = _iBookingService.GetInmateDetails(lstInmIds.ToList()).ToList();

            List<KeyValuePair<int, string>> lstPrebook = _context.InmatePrebook
                .Where(ip => ip.IncarcerationId.HasValue &&
                lstIncIds.Contains(ip.IncarcerationId.Value)
                             && (!ip.CourtCommitFlag.HasValue || ip.CourtCommitFlag == 0)).Select(inc => new
                    KeyValuePair<int, string>(inc.IncarcerationId.Value, inc.PreBookNumber)).ToList();

            List<AoWizardProgressIncarceration> lstAoWizardProgressIncarceration =
                _context.AoWizardProgressIncarceration.Where(awp =>
                    lstIncIds.Contains(awp.IncarcerationId)).ToList();

            int[] lstWizardProgressIds =
                lstAoWizardProgressIncarceration.Select(inc => inc.AoWizardProgressId).ToArray();

            List<AoWizardStepProgress> lstAoWizardStepProgress = _context.AoWizardStepProgress
                .Where(aws => lstWizardProgressIds.Contains(aws.AoWizardProgressId)).ToList();

            List<InmatePrebook> lstInmatePrebook = _context.InmatePrebook
                .Where(ip => ip.FacilityId == facilityId && (!ip.DeleteFlag.HasValue || ip.DeleteFlag == 0) &&
                     ip.CompleteFlag == 1 && (!ip.IncarcerationId.HasValue || ip.IncarcerationId == 0) &&
                     (!ip.TempHoldId.HasValue || ip.TempHoldId == 0)
                     && (!ip.CourtCommitFlag.HasValue || ip.CourtCommitFlag == 0)).ToList();

            List<BookingOverviewDetails> lstPrebookDetails = lstInmatePrebook.Select(ip =>
                new BookingOverviewDetails
                {
                    PrebookId = ip.InmatePrebookId,
                    PrebookNumber = ip.PreBookNumber,
                    CreateDate = ip.PrebookDate,
                    PersonId = ip.PersonId,
                    PrebookCompleteFlag = false
                }).ToList();

            int[] lstPersonIds =
                lstPrebookDetails.Where(inc => inc.PersonId.HasValue).Select(inc => inc.PersonId.Value).ToArray();

            List<InmateHousing> lstPrePersonDetails = GetPersonDetails(lstPersonIds.ToList()).ToList();

            bookingOverviewVm.BookingOverviewDetails.AddRange(lstPrebookDetails);

            bookingOverviewVm.BookingOverviewDetails.ForEach(inm =>
            {
                if (inm.InmateId == 0)
                {
                    inm.PersonDetails = !inm.PersonId.HasValue
                        ? lstInmatePrebook.Where(ipm => ipm.InmatePrebookId == inm.PrebookId)
                            .Select(imp => new InmateHousing
                            {
                                PersonFirstName = imp.PersonFirstName,
                                PersonLastName = imp.PersonLastName,
                                PersonMiddleName = imp.PersonMiddleName,
                                PersonDob = imp.PersonDob,
                                PersonSuffix = imp.PersonSuffix
                            }).Single()
                        : lstPrePersonDetails.Single(per => per.PersonId == inm.PersonId);
                }
                else
                {
                    inm.PrebookNumber = lstPrebook.FirstOrDefault(pre => pre.Key == inm.IncarcerationId).Value;
                    inm.PersonDetails = lstPersonDetails.Single(per => per.InmateId == inm.InmateId);

                    inm.IntakeWizardProgressId = lstAoWizardProgressIncarceration.FirstOrDefault(awp =>
                        awp.IncarcerationId == inm.IncarcerationId &&
                        awp.AoWizardId == (int)Wizards.intake)?.AoWizardProgressId;

                    inm.ReleaseWizardProgressId = lstAoWizardProgressIncarceration.FirstOrDefault(awp =>
                        awp.IncarcerationId == inm.IncarcerationId &&
                        awp.AoWizardId == (int)Wizards.release)?.AoWizardProgressId;

                    inm.AssessmentWizardProgressId = inm.NoKeeper
                        ? lstAoWizardProgressIncarceration.FirstOrDefault(awp =>
                            awp.IncarcerationId == inm.IncarcerationId &&
                            awp.AoWizardId == (int)Wizards.assessmentNonKeeper)?.AoWizardProgressId
                        : lstAoWizardProgressIncarceration.FirstOrDefault(awp =>
                            awp.IncarcerationId == inm.IncarcerationId &&
                            awp.AoWizardId == (int)Wizards.assessmentKeeper)?.AoWizardProgressId;
                }

                if (inm.ExpediteById.HasValue)
                {
                    inm.ExpediteBy = expediteOfficer.Single(ao => ao.PersonnelId == inm.ExpediteById.Value);
                }

                if (inm.IntakeWizardProgressId.HasValue)
                {
                    inm.IntakeWizards = lstAoWizardStepProgress
                        .Where(aws => aws.AoWizardProgressId == inm.IntakeWizardProgressId.Value).Select(aw =>
                            new WizardStep
                            {
                                ComponentId = aw.AoComponentId,
                                WizardProgressId = aw.AoWizardProgressId,
                                AoWizardFacilityStepId = aw.AoWizardFacilityStepId,
                                WizardStepProgressId = aw.AoWizardStepProgressId,
                                StepComplete = aw.StepComplete
                            }).ToList();
                }

                if (inm.IntakeCompleteFlag)
                {
                    Wizards wizardId = inm.BookAndReleaseFlag ? Wizards.bookAndRelease : Wizards.booking;
                    inm.BookingWizardProgressId = lstAoWizardProgressIncarceration.FirstOrDefault(awp =>
                        awp.IncarcerationId == inm.IncarcerationId &&
                        awp.AoWizardId == (int)wizardId)?.AoWizardProgressId;
                    if (inm.BookingWizardProgressId.HasValue)
                    {
                        inm.BookingWizards = lstAoWizardStepProgress
                            .Where(aws => aws.AoWizardProgressId == inm.BookingWizardProgressId.Value).Select(aw =>
                                new WizardStep
                                {
                                    ComponentId = aw.AoComponentId,
                                    WizardProgressId = aw.AoWizardProgressId,
                                    AoWizardFacilityStepId = aw.AoWizardFacilityStepId,
                                    WizardStepProgressId = aw.AoWizardStepProgressId,
                                    StepComplete = aw.StepComplete
                                }).ToList();
                    }
                }

                if (inm.ReleaseWizardProgressId.HasValue)
                {
                    inm.ReleaseWizards = lstAoWizardStepProgress
                        .Where(aws => aws.AoWizardProgressId == inm.ReleaseWizardProgressId.Value).Select(aw =>
                            new WizardStep
                            {
                                ComponentId = aw.AoComponentId,
                                WizardProgressId = aw.AoWizardProgressId,
                                AoWizardFacilityStepId = aw.AoWizardFacilityStepId,
                                WizardStepProgressId = aw.AoWizardStepProgressId,
                                StepComplete = aw.StepComplete
                            }).ToList();
                }

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

                inm.CaseType = lstIncarcerationArrestXrefDs.Where(
                    iax => inm.IncarcerationId == iax.IncarcerationId).Select(iax => iax.ArrestType).ToArray();
            }
            );

            return bookingOverviewVm;
        }

        #endregion
    }
}
