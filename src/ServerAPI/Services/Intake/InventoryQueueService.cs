using System;
using System.Collections.Generic;
using System.Linq;
using GenerateTables.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using ServerAPI.ViewModels;
using ServerAPI.Utilities;
using System.Threading.Tasks;

namespace ServerAPI.Services
{
    public class InventoryQueueService : IInventoryQueueService
    {

        private readonly AAtims _context;
        private readonly ICommonService _commonService;
        private readonly IInventoryInmateService _invInmateService;
        private readonly IBookingTaskService _bookingTaskService;

        private readonly IFormsService _formsService;
        private readonly int _personnelId;
        private List<InventoryQueueIntakeDetails> _lstQueueIntake = new List<InventoryQueueIntakeDetails>();

        public InventoryQueueService(AAtims context, ICommonService commonService,
            IInventoryInmateService invInmateService, IBookingTaskService bookingTaskService,
            IHttpContextAccessor httpContextAccessor, IFormsService formsService)
        {
            _context = context;
            _commonService = commonService;
            _invInmateService = invInmateService;
            _bookingTaskService = bookingTaskService;
             _formsService = formsService;
            _personnelId = Convert.ToInt32(httpContextAccessor.HttpContext.User
                .FindFirst(AuthDetailConstants.PERSONNELID)?.Value);
        }

        //load the inventory queue In progress
        public InventoryQueueVm GetInventoryQueue(int facilityId, int value, int scheduled)
        {      
            ////SiteOptions value for Inventory          
            string siteOptionId = _context.SiteOptions
                .SingleOrDefault(s => s.SiteOptionsName == SiteOptionsConstants.INVENTORYDAYSAFTERRELEASEDEFAULT && s.SiteOptionsStatus == "1")?.SiteOptionsValue;
            if (siteOptionId == null)
            {
                siteOptionId = "0";
            }

            if (value == -1)
            {
                Int32.TryParse(siteOptionId, out value);
            }

            List<QueueInProgress> lstQueue = new List<QueueInProgress>();
            List<QueueInRelease> lstQueueRelease = new List<QueueInRelease>();
            List<QueueInRelease> lstScheduled = new List<QueueInRelease>();

            IQueryable<Incarceration> incarceration =
                _context.Incarceration.Where(s => s.Inmate.InmateActive == 1 && !s.ReleaseOut.HasValue && s.Inmate.FacilityId == facilityId);

            QueueInProgress queueIntake = new QueueInProgress
            {
                QueueInmateType = InventoryQueueConstants.INTAKE,
                QueueInmateCount = incarceration.Count(s => !s.IntakeCompleteFlag.HasValue && (s.BookAndReleaseFlag == 0 || !s.BookAndReleaseFlag.HasValue))
            };
            lstQueue.Add(queueIntake);

            QueueInProgress queueBooking = new QueueInProgress
            {
                QueueInmateType = InventoryQueueConstants.BOOKING,
                QueueInmateCount = incarceration.Count(s => s.IntakeCompleteFlag == 1 && (s.BookAndReleaseFlag == 0 || !s.BookAndReleaseFlag.HasValue)
                         && !s.BookCompleteFlag.HasValue)
            };
            lstQueue.Add(queueBooking);


            //BookingOverviewVm listBookingOverview = _bookingTaskService.GetAssessmentDetails(facilityId);
            QueueInProgress queueAssessment = new QueueInProgress
            {
                QueueInmateType = InventoryQueueConstants.ASSESSMENT,
                QueueInmateCount = incarceration.Count(
                    i => i.IntakeCompleteFlag == 1 && i.BookCompleteFlag == 1 
                                && (i.BookAndReleaseFlag == 0 || !i.BookAndReleaseFlag.HasValue)
                                && !i.AssessmentCompleteFlag)
            };
            lstQueue.Add(queueAssessment);

            IQueryable<Incarceration> incarcerationNo =
                _context.Incarceration.Where(s => !s.ReleaseOut.HasValue && s.Inmate.FacilityId == facilityId);


            QueueInProgress queueHousing = new QueueInProgress
            {
                QueueInmateType = InventoryQueueConstants.NOHOUSING,
                QueueInmateCount = incarcerationNo.Count(
                    s => !s.Inmate.HousingUnitId.HasValue && (s.BookAndReleaseFlag == 0 || !s.BookAndReleaseFlag.HasValue))
            };
            lstQueue.Add(queueHousing);

            QueueInProgress queueRelease = new QueueInProgress
            {
                QueueInmateType = InventoryQueueConstants.RELEASE,
                QueueInmateCount = incarceration.Count(s => (s.OverallFinalReleaseDate <= DateTime.Now
                                                                              && s.OverallFinalReleaseDate.HasValue) ||
                                                                             s.ReleaseClearFlag == 1 ||
                                                                             s.BookAndReleaseFlag == 1)
            };
            lstQueue.Add(queueRelease);

            QueueInProgress queueBookRelease = new QueueInProgress
            {
                QueueInmateType = InventoryQueueConstants.BOOKANDRELEASE,
                QueueInmateCount = incarceration.Count(s => s.BookAndReleaseFlag == 1)
            };
            lstQueue.Add(queueBookRelease);

            DateTime? endDateTime = DateTime.Today.AddHours(23).AddMinutes(59).AddSeconds(59).AddDays(scheduled);
                QueueInRelease queueScheduledrelease = new QueueInRelease
                {
                    QueueInmateType = InventoryQueueConstants.STANDARDRELEASE,
                QueueInmateCount = incarceration.Where(s => (s.OverallFinalReleaseDate.HasValue &&
                                                            s.OverallFinalReleaseDate.Value <=endDateTime.Value)
                                                                && (!s.TransportFlag.HasValue || s.TransportFlag == 0))
                                                                         .Select(s => new Incarceration
                                                                         {
                                                                             InmateId = s.InmateId
                                                                         }).Distinct().Count(),
                    InventorySiteOptions = siteOptionId
                };
                lstScheduled.Add(queueScheduledrelease);

                QueueInRelease queueScheduledTransportRelease = new QueueInRelease
                {
                    QueueInmateType = InventoryQueueConstants.TRANSPORTRELEASE,
                QueueInmateCount = incarceration.Where(s => (s.OverallFinalReleaseDate.HasValue &&
                                                            s.OverallFinalReleaseDate.Value <= endDateTime.Value)
                                                                && s.TransportFlag == 1)
                                                                .Select(s => new Incarceration
                                                                {
                                                                    InmateId = s.InmateId
                                                                }).Distinct().Count(),
                    InventorySiteOptions = siteOptionId
                };
                lstScheduled.Add(queueScheduledTransportRelease);

                IQueryable<Incarceration> incarcerationStd = _context.Incarceration.Where(s => s.Inmate.InmateActive == 0 && s.ReleaseOut.HasValue
                                                               && s.Inmate.FacilityId == facilityId);

            QueueInRelease queueStandardRelease = new QueueInRelease
            {
                QueueInmateType = InventoryQueueConstants.STANDARDRELEASE,
                QueueInmateCount = incarcerationStd.Where(s => s.ReleaseOut.Value <= DateTime.Now.AddDays(-value) 
                                                            && (!s.TransportFlag.HasValue || s.TransportFlag == 0)
                                                            && s.Inmate.PersonalInventory.Count(c => c.InmateId == s.InmateId && c.InventoryDispositionCode == 4) > 0)
                                                            .Select(s => new Incarceration
                                                            {
                                                                InmateId = s.InmateId
                                                            }).Distinct().Count(),
                InventorySiteOptions = siteOptionId
            };
            lstQueueRelease.Add(queueStandardRelease);

            QueueInRelease queueTransportRelease = new QueueInRelease
            {
                QueueInmateType = InventoryQueueConstants.TRANSPORTRELEASE,
                QueueInmateCount = incarcerationStd.Where(s =>  s.ReleaseOut.Value <= DateTime.Now.AddDays(-value) && s.TransportFlag == 1
                                                            && s.Inmate.PersonalInventory
                                                            .Count(i => i.InmateId == s.InmateId && i.InventoryDispositionCode == 4) > 0)
                                                            .Select(s => new Incarceration
                                                            {
                                                                InmateId = s.InmateId
                                                            }).Distinct().Count(),
                InventorySiteOptions = siteOptionId
            };
            lstQueueRelease.Add(queueTransportRelease);

            BinInventoryVm lstBinItems = _invInmateService.AvailableBinItems(facilityId);

            List<InventoryQueueForms> forms = _context.FormTemplates.Where(w => w.FormCategoryFilterId == 1 && w.FormCategoryId == 10
                                                                            && w.RouteToHousingFlag == 1 && w.RouteToHousingReturn)
            .Select(s => new InventoryQueueForms
            {
                    PropertyFormName = s.DisplayName,
                    PendingQueueCount = s.FormRecord.Count(c => c.DeleteFlag == 0 && c.FormHousingRoute == 1 && c.FormHousingClear == 0),
                    ReadyQueueCount = s.FormRecord.Count(cc => cc.DeleteFlag == 0 && cc.FormHousingRoute == 1 && cc.FormHousingClear == 1
                                                                && !cc.FormHousingReturnClear)
            }).ToList();

            InventoryQueueVm lstQueueDetails = new InventoryQueueVm
            {
                QueueInProgress = lstQueue,
                BinReceivingDetails = lstBinItems.BinReceivingDetails,
                BinFacilityTransferDetails = lstBinItems.BinFacilityTransferDetails,
                QueueInRelease = lstQueueRelease,
                QueueScheduled = lstScheduled,
                InventoryQueueForms = forms,
                SiteOptionId = siteOptionId
            };

            return lstQueueDetails;
        }

        public InventoryQueueDetailsVm GetInventoryInmateDetails(int facilityId, InventoryQueue value,
            int personalInventoryBinId, int selected , int schSelected)
        {
            IQueryable<Incarceration> incarceration =
                _context.Incarceration.Where(s => s.Inmate.FacilityId == facilityId);

            List<InventoryQueueIntakeDetails> lstInventoryQueueDetail = _context.PersonalInventory.AsNoTracking().Where(i => i.DeleteFlag == 0 && i.InmateId.HasValue &&
               i.InventoryDispositionCode == (int?)Disposition.Storage).Select(
                    s => new InventoryQueueIntakeDetails
                    {
                        PersonalInventoryBinId = s.PersonalInventoryBinId??0,
                        InmateId = s.InmateId,
                        DeleteFlag = s.DeleteFlag,
                        HousingUnitId = s.Inmate.HousingUnitId,
                        InmateCurrentTrakId = s.Inmate.InmateCurrentTrackId,
                        InmateNumber = s.Inmate.InmateNumber,
                        FacilityId = s.PersonalInventoryBin.FacilityId,
                        InventoryReturnDate = s.InventoryReturnDate,
                        BinName = s.PersonalInventoryBin.BinName,
                        PersonInfoDetails = new PersonInfoVm
                        {
                            PersonId = s.Inmate.PersonId,
                            PersonLastName = s.Inmate.Person.PersonLastName,
                            PersonMiddleName = s.Inmate.Person.PersonMiddleName,
                            PersonFirstName = s.Inmate.Person.PersonFirstName
                        },
                        HousingDetails = new HousingDetail
                        {
                            HousingUnitLocation = s.Inmate.HousingUnit.HousingUnitLocation,
                            HousingUnitNumber = s.Inmate.HousingUnit.HousingUnitNumber,
                            HousingUnitBedNumber = s.Inmate.HousingUnit.HousingUnitBedNumber
                        },
                        PrivilegesDetails = new PrivilegeDetailsVm
                        {
                            PrivilegeId = s.Inmate.InmateCurrentTrackId ?? 0,
                            PrivilegeDescription = s.Inmate.InmateCurrentTrackNavigation.PrivilegeDescription
                        },
  
                    }).Distinct().ToList();

            if (InventoryQueue.Intake == value || InventoryQueue.Booking == value || InventoryQueue.Assessment == value ||
                InventoryQueue.BookAndRelease == value || InventoryQueue.NoHousing == value ||
                InventoryQueue.Release == value || InventoryQueue.StandardRelease == value || InventoryQueue.TransportRelease == value
                || InventoryQueue.SchStandardRelease == value|| InventoryQueue.SchTransportRelease == value)
            {
                //switch case statement//
                switch (value)
                {
                    case InventoryQueue.Intake:
                        //Intake
                        _lstQueueIntake = incarceration
                            .Where(i => i.Inmate.InmateActive == 1 && !i.IntakeCompleteFlag.HasValue && !i.ReleaseOut.HasValue && i.Inmate.FacilityId == facilityId &&
                                        (!i.BookAndReleaseFlag.HasValue || i.BookAndReleaseFlag == 0))
                            .Select(s => new InventoryQueueIntakeDetails
                            {
                                InmateId = s.Inmate.InmateId,
                                IncarcerationId = s.IncarcerationId,
                                DateIn = s.DateIn,
                                InmateNumber = s.Inmate.InmateNumber,
                                PersonInfoDetails = new PersonInfoVm
                                {
                                    PersonId = s.Inmate.Person.PersonId,
                                    PersonLastName = s.Inmate.Person.PersonLastName,
                                    PersonMiddleName = s.Inmate.Person.PersonMiddleName,
                                    PersonFirstName = s.Inmate.Person.PersonFirstName
                                },
                                HousingDetails = new HousingDetail
                                {
                                    HousingUnitLocation = s.Inmate.HousingUnit.HousingUnitLocation,
                                    HousingUnitNumber = s.Inmate.HousingUnit.HousingUnitNumber,
                                    HousingUnitBedNumber = s.Inmate.HousingUnit.HousingUnitBedNumber
                                },
                                PrivilegesDetails = new PrivilegeDetailsVm
                                {
                                    PrivilegeId = s.Inmate.InmateCurrentTrackId ?? 0,
                                    PrivilegeDescription = s.Inmate.InmateCurrentTrack
                                }

                            }).OrderBy(i => i.DateIn).ToList();
                        break;

                    case InventoryQueue.Booking:
                        //Booking
                        _lstQueueIntake = incarceration
                            .Where(s => s.Inmate.InmateActive == 1 && s.IntakeCompleteFlag == 1 && !s.ReleaseOut.HasValue && s.Inmate.FacilityId == facilityId &&
                                        (s.BookAndReleaseFlag == 0 || !s.BookAndReleaseFlag.HasValue) && !s.BookCompleteFlag.HasValue)
                            .Select(s => new InventoryQueueIntakeDetails
                            {
                                InmateId = s.Inmate.InmateId,
                                IncarcerationId = s.IncarcerationId,
                                DateIn = s.DateIn,
                                InmateNumber = s.Inmate.InmateNumber,
                                PersonInfoDetails = new PersonInfoVm
                                {
                                    PersonId = s.Inmate.Person.PersonId,
                                    PersonLastName = s.Inmate.Person.PersonLastName,
                                    PersonMiddleName = s.Inmate.Person.PersonMiddleName,
                                    PersonFirstName = s.Inmate.Person.PersonFirstName
                                },
                                HousingDetails = new HousingDetail
                                {
                                    HousingUnitLocation = s.Inmate.HousingUnit.HousingUnitLocation,
                                    HousingUnitNumber = s.Inmate.HousingUnit.HousingUnitNumber,
                                    HousingUnitBedNumber = s.Inmate.HousingUnit.HousingUnitBedNumber
                                },
                                PrivilegesDetails = new PrivilegeDetailsVm
                                {
                                    PrivilegeId = s.Inmate.InmateCurrentTrackId ?? 0,
                                    PrivilegeDescription = s.Inmate.InmateCurrentTrack
                                }

                            }).OrderBy(i => i.DateIn).ToList();
                        break;

                    case InventoryQueue.NoHousing:
                        //No Housing
                        _lstQueueIntake = incarceration
                            .Where(i => (i.Inmate.InmateActive == 1 || i.Inmate.InmateActive == 0)
                                        && !i.Inmate.HousingUnitId.HasValue && !i.ReleaseOut.HasValue && i.Inmate.FacilityId == facilityId &&
                                        (!i.BookAndReleaseFlag.HasValue || i.BookAndReleaseFlag == 0))
                            .Select(s => new InventoryQueueIntakeDetails
                            {
                                InmateId = s.Inmate.InmateId,
                                IncarcerationId = s.IncarcerationId,
                                DateIn = s.DateIn,
                                InmateNumber = s.Inmate.InmateNumber,
                                PersonInfoDetails = new PersonInfoVm
                                {
                                    PersonId = s.Inmate.Person.PersonId,
                                    PersonLastName = s.Inmate.Person.PersonLastName,
                                    PersonMiddleName = s.Inmate.Person.PersonMiddleName,
                                    PersonFirstName = s.Inmate.Person.PersonFirstName
                                },
                                HousingDetails = new HousingDetail
                                {
                                    HousingUnitLocation = s.Inmate.HousingUnit.HousingUnitLocation,
                                    HousingUnitNumber = s.Inmate.HousingUnit.HousingUnitNumber,
                                    HousingUnitBedNumber = s.Inmate.HousingUnit.HousingUnitBedNumber
                                },
                                PrivilegesDetails = new PrivilegeDetailsVm
                                {
                                    PrivilegeId = s.Inmate.InmateCurrentTrackId ?? 0,
                                    PrivilegeDescription = s.Inmate.InmateCurrentTrack
                                }

                            }).OrderBy(i => i.DateIn).ToList();
                        break;

                    case InventoryQueue.Release:
                        //Release
                        _lstQueueIntake = incarceration
                            .Where(i => i.Inmate.InmateActive == 1 && !i.ReleaseOut.HasValue && i.Inmate.FacilityId == facilityId &&
                                        ((i.OverallFinalReleaseDate <= DateTime.Now && i.OverallFinalReleaseDate.HasValue) ||
                                         i.ReleaseClearFlag == 1 || i.BookAndReleaseFlag == 1))
                            .Select(s => new InventoryQueueIntakeDetails
                            {
                                InmateId = s.Inmate.InmateId,
                                IncarcerationId = s.IncarcerationId,
                                DateIn = s.DateIn,
                                InmateNumber = s.Inmate.InmateNumber,
                                PersonInfoDetails = new PersonInfoVm
                                {
                                    PersonId = s.Inmate.Person.PersonId,
                                    PersonLastName = s.Inmate.Person.PersonLastName,
                                    PersonMiddleName = s.Inmate.Person.PersonMiddleName,
                                    PersonFirstName = s.Inmate.Person.PersonFirstName
                                },
                                HousingDetails = new HousingDetail
                                {
                                    HousingUnitLocation = s.Inmate.HousingUnit.HousingUnitLocation,
                                    HousingUnitNumber = s.Inmate.HousingUnit.HousingUnitNumber,
                                    HousingUnitBedNumber = s.Inmate.HousingUnit.HousingUnitBedNumber
                                },
                                PrivilegesDetails = new PrivilegeDetailsVm
                                {
                                    PrivilegeId = s.Inmate.InmateCurrentTrackId ?? 0,
                                    PrivilegeDescription = s.Inmate.InmateCurrentTrack
                                }

                            }).OrderBy(i => i.DateIn).ToList();
                        break;

                    case InventoryQueue.Assessment:
                        //Book & Release
                        _lstQueueIntake = incarceration
                            .Where(i => i.Inmate.InmateActive == 1 && i.IntakeCompleteFlag == 1 && i.Inmate.FacilityId == facilityId
                                        && i.BookCompleteFlag == 1 && !i.ReleaseOut.HasValue && (!i.BookAndReleaseFlag.HasValue || i.BookAndReleaseFlag == 0)
                                        && !i.AssessmentCompleteFlag)
                            .Select(s => new InventoryQueueIntakeDetails
                            {
                                InmateId = s.Inmate.InmateId,
                                IncarcerationId = s.IncarcerationId,
                                DateIn = s.DateIn,
                                InmateNumber = s.Inmate.InmateNumber,
                                PersonInfoDetails = new PersonInfoVm
                                {
                                    PersonId = s.Inmate.Person.PersonId,
                                    PersonLastName = s.Inmate.Person.PersonLastName,
                                    PersonMiddleName = s.Inmate.Person.PersonMiddleName,
                                    PersonFirstName = s.Inmate.Person.PersonFirstName
                                },
                                HousingDetails = new HousingDetail
                                {
                                    HousingUnitLocation = s.Inmate.HousingUnit.HousingUnitLocation,
                                    HousingUnitNumber = s.Inmate.HousingUnit.HousingUnitNumber,
                                    HousingUnitBedNumber = s.Inmate.HousingUnit.HousingUnitBedNumber
                                },
                                PrivilegesDetails = new PrivilegeDetailsVm
                                {
                                    PrivilegeId = s.Inmate.InmateCurrentTrackId ?? 0,
                                    PrivilegeDescription = s.Inmate.InmateCurrentTrack
                                }
                            }).OrderBy(i => i.DateIn).ToList();
                        break;

                    case InventoryQueue.BookAndRelease:
                        //Book & Release
                        _lstQueueIntake = incarceration
                            .Where(i => i.Inmate.InmateActive == 1 && i.Inmate.FacilityId == facilityId && i.BookAndReleaseFlag == 1 && !i.ReleaseOut.HasValue)
                            .Select(s => new InventoryQueueIntakeDetails
                            {
                                InmateId = s.Inmate.InmateId,
                                IncarcerationId = s.IncarcerationId,
                                DateIn = s.DateIn,
                                InmateNumber = s.Inmate.InmateNumber,
                                PersonInfoDetails = new PersonInfoVm
                                {
                                    PersonId = s.Inmate.Person.PersonId,
                                    PersonLastName = s.Inmate.Person.PersonLastName,
                                    PersonMiddleName = s.Inmate.Person.PersonMiddleName,
                                    PersonFirstName = s.Inmate.Person.PersonFirstName
                                },
                                HousingDetails = new HousingDetail
                                {
                                    HousingUnitLocation = s.Inmate.HousingUnit.HousingUnitLocation,
                                    HousingUnitNumber = s.Inmate.HousingUnit.HousingUnitNumber,
                                    HousingUnitBedNumber = s.Inmate.HousingUnit.HousingUnitBedNumber
                                },
                                PrivilegesDetails = new PrivilegeDetailsVm
                                {
                                    PrivilegeId = s.Inmate.InmateCurrentTrackId ?? 0,
                                    PrivilegeDescription = s.Inmate.InmateCurrentTrack
                                }
                            }).OrderBy(i => i.DateIn).ToList();
                        break;

                    case InventoryQueue.SchStandardRelease:
                        //Standard & Release
                        DateTime? endSchDateTime = DateTime.Today.AddHours(23).AddMinutes(59).AddSeconds(59)
                            .AddDays(schSelected);

                        _lstQueueIntake = incarceration.Where(s => s.Inmate.InmateActive == 1 && s.Inmate.FacilityId == facilityId 
                                                                   && ((s.OverallFinalReleaseDate.HasValue &&
                                                                   s.OverallFinalReleaseDate.Value <= endSchDateTime.Value)
                                                                   && !s.ReleaseOut.HasValue) && (!s.TransportFlag.HasValue || s.TransportFlag == 0))
                            .Select(s => new InventoryQueueIntakeDetails
                            {
                                InmateId = s.InmateId,
                                InmateNumber = s.Inmate.InmateNumber,
                                PersonInfoDetails = new PersonInfoVm
                                {
                                    PersonId = s.Inmate.Person.PersonId,
                                    PersonLastName = s.Inmate.Person.PersonLastName,
                                    PersonMiddleName = s.Inmate.Person.PersonMiddleName,
                                    PersonFirstName = s.Inmate.Person.PersonFirstName
                                },
                                HousingDetails = new HousingDetail
                                {
                                    HousingUnitLocation = s.Inmate.HousingUnit.HousingUnitLocation,
                                    HousingUnitNumber = s.Inmate.HousingUnit.HousingUnitNumber,
                                    HousingUnitBedNumber = s.Inmate.HousingUnit.HousingUnitBedNumber
                                },
                                PrivilegesDetails = new PrivilegeDetailsVm
                                {
                                    PrivilegeId = s.Inmate.InmateCurrentTrackId ?? 0,
                                    PrivilegeDescription = s.Inmate.InmateCurrentTrack
                                }
                            }).Distinct().ToList();
                        break;

                    case InventoryQueue.SchTransportRelease:
                        //Transport & Release
                        DateTime? endTranDateTime = DateTime.Today.AddHours(23).AddMinutes(59).AddSeconds(59)
                            .AddDays(schSelected);

                        _lstQueueIntake = incarceration.Where(s => s.Inmate.InmateActive == 1 && s.Inmate.FacilityId == facilityId
                                                           && ((s.OverallFinalReleaseDate.HasValue && s.OverallFinalReleaseDate.Value <= endTranDateTime.Value)
                                                           && !s.ReleaseOut.HasValue) && s.TransportFlag == 1)
                            .Select(s => new InventoryQueueIntakeDetails
                            {
                                InmateId = s.Inmate.InmateId,
                                InmateNumber = s.Inmate.InmateNumber,
                                PersonInfoDetails = new PersonInfoVm
                                {
                                    PersonId = s.Inmate.Person.PersonId,
                                    PersonLastName = s.Inmate.Person.PersonLastName,
                                    PersonMiddleName = s.Inmate.Person.PersonMiddleName,
                                    PersonFirstName = s.Inmate.Person.PersonFirstName
                                },
                                HousingDetails = new HousingDetail
                                {
                                    HousingUnitLocation = s.Inmate.HousingUnit.HousingUnitLocation,
                                    HousingUnitNumber = s.Inmate.HousingUnit.HousingUnitNumber,
                                    HousingUnitBedNumber = s.Inmate.HousingUnit.HousingUnitBedNumber
                                },
                                PrivilegesDetails = new PrivilegeDetailsVm
                                {
                                    PrivilegeId = s.Inmate.InmateCurrentTrackId ?? 0,
                                    PrivilegeDescription = s.Inmate.InmateCurrentTrack
                                }
                            }).Distinct().OrderBy(i => i.InmateId).ToList();
                        break;

                    case InventoryQueue.StandardRelease:
                        //Standard & Release
                        _lstQueueIntake = incarceration.AsNoTracking().Where(s => s.Inmate.InmateActive == 0 && s.Inmate.FacilityId == facilityId 
                                                           && (s.ReleaseOut.Value <= DateTime.Now.AddDays(selected) &&
                                                           s.ReleaseOut.HasValue) && (!s.TransportFlag.HasValue || s.TransportFlag == 0)
                                                           && s.Inmate.PersonalInventory .Count(i => i.InmateId == s.InmateId && i.InventoryDispositionCode == 4) > 0)
                            .Select(s => new InventoryQueueIntakeDetails
                            {
                                InmateId = s.InmateId,
                                InmateNumber = s.Inmate.InmateNumber,
                                PersonInfoDetails = new PersonInfoVm
                                {
                                    PersonId = s.Inmate.Person.PersonId,
                                    PersonLastName = s.Inmate.Person.PersonLastName,
                                    PersonMiddleName = s.Inmate.Person.PersonMiddleName,
                                    PersonFirstName = s.Inmate.Person.PersonFirstName
                                },
                                HousingDetails = new HousingDetail
                                {
                                    HousingUnitLocation = s.Inmate.HousingUnit.HousingUnitLocation,
                                    HousingUnitNumber = s.Inmate.HousingUnit.HousingUnitNumber,
                                    HousingUnitBedNumber = s.Inmate.HousingUnit.HousingUnitBedNumber
                                },
                                PrivilegesDetails = new PrivilegeDetailsVm
                                {
                                    PrivilegeId = s.Inmate.InmateCurrentTrackId ?? 0,
                                    PrivilegeDescription = s.Inmate.InmateCurrentTrack
                                }
                            }).Distinct().ToList();
                        break;

                    case InventoryQueue.TransportRelease:
                        //Transport & Release
                        _lstQueueIntake = incarceration.Where(s => s.Inmate.InmateActive == 0 && s.Inmate.FacilityId == facilityId
                                                                   && (s.ReleaseOut.Value <= DateTime.Now.AddDays(selected) &&
                                                                   s.ReleaseOut.HasValue) && s.TransportFlag == 1
                                                                   && s.Inmate.PersonalInventory.Count(i => i.InmateId == s.InmateId &&i.InventoryDispositionCode == 4) > 0)
                            .Select(s => new InventoryQueueIntakeDetails
                            {
                                InmateId = s.Inmate.InmateId,
                                InmateNumber = s.Inmate.InmateNumber,
                                PersonInfoDetails = new PersonInfoVm
                                {
                                    PersonId = s.Inmate.Person.PersonId,
                                    PersonLastName = s.Inmate.Person.PersonLastName,
                                    PersonMiddleName = s.Inmate.Person.PersonMiddleName,
                                    PersonFirstName = s.Inmate.Person.PersonFirstName
                                },
                                HousingDetails = new HousingDetail
                                {
                                    HousingUnitLocation = s.Inmate.HousingUnit.HousingUnitLocation,
                                    HousingUnitNumber = s.Inmate.HousingUnit.HousingUnitNumber,
                                    HousingUnitBedNumber = s.Inmate.HousingUnit.HousingUnitBedNumber
                                },
                                PrivilegesDetails = new PrivilegeDetailsVm
                                {
                                    PrivilegeId = s.Inmate.InmateCurrentTrackId ?? 0,
                                    PrivilegeDescription = s.Inmate.InmateCurrentTrack
                                }
                            }).Distinct().OrderBy(i => i.InmateId).ToList();
                        break;

                }
            }
            else
            {
                _lstQueueIntake = lstInventoryQueueDetail. Where(i => i.PersonalInventoryBinId == personalInventoryBinId 
                                                           && !i.InventoryReturnDate.HasValue).Distinct().OrderBy(i => i.InmateId).ToList();
            }

            IList<BinNameVm> binNameVmVm = lstInventoryQueueDetail
            .Where(w => _lstQueueIntake.Select(s => s.InmateId).Contains(w.InmateId))
            .GroupBy(h => h.InmateId).Select(s => new BinNameVm
            {
                     InmateId = s.Key ?? 0,
                     BinName = s.Select(a => a.BinName).Distinct().ToList(),
                     BinCount = s.Select(a => a.BinName).Distinct().Count()
                 }).ToList();

            InventoryQueueDetailsVm lstInventoryVmDetails = new InventoryQueueDetailsVm
            {
                InventoryQueueIntakeDetails = _lstQueueIntake,
                BinNameList = binNameVmVm.ToList(),

            };

            return lstInventoryVmDetails;
        }

        public List<string> InventoryDetailsRelease(int inmateId)
        {
            //sub grid for inventory 
            List<string> lstInventory = _context.PersonalInventory
                .Where(s => s.InmateId == inmateId && s.InventoryDispositionCode == 4 && s.DeleteFlag == 0)
                .Select(s => new
                {
                    s.PersonalInventoryBin.BinName
                }).Distinct().Select(d => d.BinName).ToList();

            return lstInventory;
        }

        public int InventoryDetails(int inmateId)
        {
            //sub grid for inventory 
            int lstInventory = _context.PersonalInventory
                .Where(s => s.InmateId == inmateId && s.InventoryDispositionCode == 4 && s.DeleteFlag == 0)
                .Select(s => new
                {
                    s.PersonalInventoryBin.BinName
                }).Distinct().Select(d => d.BinName).Count();

            return lstInventory;
        }

        public List<InventoryQueueIntakeDetails> GetIntakeInprogress(int facilityId)
        {
            List<InventoryQueueIntakeDetails> lstInventoryInprogress = _context.Incarceration
                .Where(s => s.Inmate.InmateActive == 1
                            && s.Inmate.FacilityId == facilityId)
                .Select(s => new InventoryQueueIntakeDetails
                {
                    InmateId = s.Inmate.InmateId,
                    IncarcerationId = s.IncarcerationId,
                    DateIn = s.DateIn,
                    InmateNumber = s.Inmate.InmateNumber,
                    InmateCurrentTrakId = s.Inmate.InmateCurrentTrackId,
                    IntakeCompleteFlagId = s.IntakeCompleteFlag,
                    BookAndReleaseFlag = s.BookAndReleaseFlag,
                    BookCompleteFlag = s.BookCompleteFlag,
                    OverallFinalReleaseDate = s.OverallFinalReleaseDate,
                    ReleaseClearFlag = s.ReleaseClearFlag,
                    HousingUnitId = s.Inmate.HousingUnitId,
                    ReleaseOut = s.ReleaseOut,
                    PersonInfoDetails = new PersonInfoVm
                    {
                        PersonLastName = s.Inmate.Person.PersonLastName,
                        PersonMiddleName = s.Inmate.Person.PersonMiddleName,
                        PersonFirstName = s.Inmate.Person.PersonFirstName
                    },
                    HousingDetails = new HousingDetail
                    {
                        HousingUnitLocation = s.Inmate.HousingUnit.HousingUnitLocation,
                        HousingUnitNumber = s.Inmate.HousingUnit.HousingUnitNumber,
                        HousingUnitBedNumber = s.Inmate.HousingUnit.HousingUnitBedNumber
                    }
                }).OrderBy(i => i.DateIn).ToList();
            return lstInventoryInprogress;
        }

        //  Receiving transfer bins
        public List<InventoryQueueIntakeDetails> GetIntakeInmateRecevingBin()
        {
            List<InventoryQueueIntakeDetails> lstInventoryReceiving =
            _context.PersonalInventory.Where(i=>i.DeleteFlag == 0 && i.InmateId.HasValue
                                                  && i.InventoryDispositionCode == (int?)Disposition.Storage).Select(s => new InventoryQueueIntakeDetails
             {
                 PersonalInventoryBinId = s.PersonalInventoryBinId,
                 InmateId = s.InmateId,
                 DeleteFlag = s.DeleteFlag,
                HousingUnitId = s.Inmate.HousingUnitId,
                InmateCurrentTrakId = s.Inmate.InmateCurrentTrackId,
                InmateNumber = s.Inmate.InmateNumber,
                FacilityId = s.PersonalInventoryBin.FacilityId,
                InventoryReturnDate=s.InventoryReturnDate,
                BinName=s.PersonalInventoryBin.BinName
                                                  }).Distinct().ToList();

            //select new
            //{
            //    i.InmateId,
            //    i.DeleteFlag,
            //    i.PersonalInventoryBinId,
            //    i.Inmate.HousingUnitId,
            //    InmateCurrentTrakId = i.Inmate.InmateCurrentTrackId,
            //    i.Inmate.InmateCurrentTrack,
            //    i.Inmate.InmateNumber,
            //    Facility = i.PersonalInventoryBin.FacilityId

            //}).Distinct().


            return lstInventoryReceiving;
        }

        public List<InventoryFormsDetails> GetInventoryProperyFormsDetails()
        {

            List<InventoryFormsDetails> InventoryFormsDetails = _context.FormRecord.Where
                                                                (w => w.FormTemplates.FormCategoryFilterId == 1
                                                                    && w.FormTemplates.FormCategoryId == 10 
                                                                    && w.FormTemplates.RouteToHousingFlag == 1
                                                                    && w.FormTemplates.RouteToHousingReturn
                                                                    && w.InmateId == w.Inmate.InmateId
                                                                    && w.Inmate.InmateActive == 1 && w.DeleteFlag == 0   
                                                                    && w.FormHousingRoute == 1                                                                 
                                                                ).OrderByDescending(o => o.FormHousingClear)
                                                                 .ThenBy(t => t.FormHousingClearDate).ThenBy(tt => tt.CreateDate)
                                                                
            .Select( s => new InventoryFormsDetails
            {
                TemplateId = s.FormTemplates.FormTemplatesId,
                FormRecordId = s.FormRecordId,
                FormNote = s.FormNotes,
                FormName = s.FormTemplates.DisplayName,
                InmateName = s.Inmate.Person.PersonLastName + ", " + s.Inmate.Person.PersonFirstName,
                InmateId = s.Inmate.InmateId, 
                PersonId = s.Inmate.Person.PersonId,
                InmateNumber = s.Inmate.InmateNumber,
                CreatedBy = s.CreateByNavigation.PersonNavigation.PersonLastName + ", " 
                            + s.CreateByNavigation.PersonNavigation.PersonFirstName,
                CreatedDate = s.CreateDate,
                UpdatedBy = s.UpdateByNavigation.PersonNavigation.PersonLastName + ", " 
                            + s.UpdateByNavigation.PersonNavigation.PersonFirstName,
                UpdatedDate = s.UpdateDate,
                ClearedBy = s.FormHousingClearByNavigation.PersonNavigation.PersonLastName + ", " +
                            s.FormHousingClearByNavigation.PersonNavigation.PersonFirstName,
                ClearedDate = s.FormHousingClearDate,
                IncarcerationId= _context.Incarceration.FirstOrDefault(i => i.InmateId == s.Inmate.InmateId).IncarcerationId,
                ArrestId = _context.Arrest.FirstOrDefault(a => a.InmateId == s.Inmate.InmateId).ArrestId
            }).ToList(); 
             return InventoryFormsDetails;
        }

        public async Task<int> ClearInventoryQueue(int recordId)
        {          
            FormRecord formRecord = _context.FormRecord.Single(s => s.FormRecordId == recordId);
            formRecord.FormHousingReturnClear = true;
            formRecord.FormHousingReturnClearBy = _personnelId;
            formRecord.FormHousingReturnDate = DateTime.Now;
            return await _context.SaveChangesAsync();
        }
    }
}