using System;
using System.Collections.Generic;
using System.Linq;
using GenerateTables.Models;
using ServerAPI.ViewModels;
using ServerAPI.Utilities;
using System.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;
using jsreport.Client;
using jsreport.Types;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace ServerAPI.Services
{
    public class FacilityOperationsService : IFacilityOperationsService
    {
        private readonly AAtims _context;
        private readonly ICommonService _commonService;
        private List<RosterVm> _inmateHousing;
        private List<KeyValuePair<int?, int?>> _housingGroups;
        private readonly IPhotosService _photos;
        private readonly IMiscLabelService _miscLabelService;
        private readonly Uri _jsReportUrl;

        public FacilityOperationsService(AAtims context, ICommonService commonService, IPhotosService photosService, IMiscLabelService miscLabelService, IConfiguration configuration)
        {
            _context = context;
            _commonService = commonService;
            _photos = photosService;
            _miscLabelService = miscLabelService;
            _jsReportUrl = new Uri(configuration.GetSection("SiteVariables")["ReportUrl"]);
        }

        public RosterMaster GetRosterMasterDetails(RosterFilters rosterFilters)
        {
            RosterMaster rosterMaster = new RosterMaster
            {
                RosterDetails = SearchHousing(rosterFilters),
                LocationDetails = GetLocationDetails(rosterFilters)
            };
            if (!rosterFilters.IsPageInitialize) return rosterMaster;
            rosterMaster.LookupCombos = _commonService.GetLookups(new[]
            {
                LookupConstants.SEX, LookupConstants.RACE, LookupConstants.CLASSGROUP,
                LookupConstants.CLASREAS, LookupConstants.PERSONCAUTION, LookupConstants.TRANSCAUTION,
                LookupConstants.DIET
            });
            return rosterMaster;
        }

        public List<RosterInmateInfo> GetFilterRoster(RosterFilters rosterFilters)
        {
            List<RosterDetails> rosterDetails = SearchHousing(rosterFilters);
            if (rosterDetails.Count == 0) return new List<RosterInmateInfo>();
            List<RosterInmateInfo> lstRosterInmateInfos = rosterDetails[rosterDetails.Count - 1].InmateList;
            return lstRosterInmateInfos;
        }
        public List<RosterInmateInfo> getAllRosterInmate(RosterFilters rosterFilters)
        {
            List<RosterDetails> rosterDetails = SearchHousing(rosterFilters);
            List<RosterInmateInfo> lstRosterInmateInfos = new List<RosterInmateInfo>();
            if (rosterDetails.Count == 0) return lstRosterInmateInfos;
            rosterDetails.ForEach(item =>
            {
                if (item.InmateList != null)
                {
                    lstRosterInmateInfos.AddRange(item.InmateList);
                }
            });
            return lstRosterInmateInfos.OrderBy(a => a.PersonLastName).ThenBy(a => a.PersonFirstName)
                .ThenBy(a => a.PersonMiddleName).ToList();
        }

        private List<RosterDetails> SearchHousing(RosterFilters rosterFilters)
        {
            _inmateHousing = _context.Inmate.Where(a =>
                    a.FacilityId == rosterFilters.FacilityId
                    && a.InmateActive == 1 && (!a.HousingUnit.HousingUnitInactive.HasValue || a.HousingUnit.HousingUnitInactive==0)
                    && (!rosterFilters.HousingUnitListId.HasValue
                        || a.HousingUnit.HousingUnitListId == rosterFilters.HousingUnitListId)
                    && (!rosterFilters.Gender.HasValue || a.Person.PersonSexLast == rosterFilters.Gender)
                    && (!rosterFilters.Race.HasValue || a.Person.PersonRaceLast == rosterFilters.Race)
                    && (string.IsNullOrEmpty(rosterFilters.Classification)
                        || a.InmateClassification.InmateClassificationReason == rosterFilters.Classification)
                    && (!rosterFilters.IllegalAlien ||
                        a.Person.IllegalAlienFlag == rosterFilters.IllegalAlien))
                .Select(a => new RosterVm
                {
                    FacilityId = a.HousingUnitId > 0 ? a.HousingUnit.FacilityId : default,
                    InmateId = a.InmateId,
                    InmateNumber = a.InmateNumber,
                    InmateCurrentTrackId = a.InmateCurrentTrackId,
                    InmateCurrentTrack = a.InmateCurrentTrack,
                    PersonId = a.Person.PersonId,
                    PersonFirstName = a.Person.PersonFirstName,
                    PersonMiddleName = a.Person.PersonMiddleName,
                    PersonLastName = a.Person.PersonLastName,
                    PersonPhoto = _photos.GetPhotoByCollectionIdentifier(a.Person.Identifiers),
                    HousingUnitListId = a.HousingUnitId > 0 ? a.HousingUnit.HousingUnitListId : default,
                    HousingUnitLocation = a.HousingUnitId > 0 ? a.HousingUnit.HousingUnitLocation : default,
                    HousingUnitNumber = a.HousingUnitId > 0 ? a.HousingUnit.HousingUnitNumber : default,
                    HousingUnitBedNumber = a.HousingUnitId > 0 ? a.HousingUnit.HousingUnitBedNumber : default
                }).ToList();
            if (rosterFilters.ToLoadInmate)
            {
                if (!string.IsNullOrEmpty(rosterFilters.Location))
                {
                    if (rosterFilters.Housing == RosterConstants.NOHOUSING)
                    {
                        _inmateHousing = _inmateHousing.Where(a => a.FacilityId == 0 &&
                                                                   ((rosterFilters.Location == FloorNote.ALL && !string.IsNullOrEmpty(a.InmateCurrentTrack))
                                                                   || a.InmateCurrentTrack == rosterFilters.Location)).ToList();
                    }
                    else
                    {
                        _inmateHousing = _inmateHousing.Where(a =>
                                ((rosterFilters.Location == FloorNote.ALL && rosterFilters.Housing == FloorNote.ALL &&
                                  !string.IsNullOrEmpty(a.InmateCurrentTrack)) ||
                                 (((rosterFilters.Location == FloorNote.ALL &&
                                    !string.IsNullOrEmpty(a.InmateCurrentTrack)) ||
                                   a.InmateCurrentTrack == rosterFilters.Location) &&
                                  ((string.IsNullOrEmpty(rosterFilters.HousingLocation) && string.IsNullOrEmpty(rosterFilters.HousingNumber) && a.FacilityId == 0) ||
                                   ((string.IsNullOrEmpty(rosterFilters.HousingLocation) || a.HousingUnitLocation == rosterFilters.HousingLocation) &&
                                    (string.IsNullOrEmpty(rosterFilters.HousingNumber) || a.HousingUnitNumber == rosterFilters.HousingNumber))))))
                            .ToList();
                    }
                }
                else if (!string.IsNullOrEmpty(rosterFilters.HousingNumber))
                {
                    _inmateHousing = _inmateHousing.Where(a => a.HousingUnitLocation == rosterFilters.HousingLocation &&
                                                               a.HousingUnitNumber == rosterFilters.HousingNumber)
                        .ToList();
                }
                else if (!string.IsNullOrEmpty(rosterFilters.HousingLocation))
                {
                    _inmateHousing = _inmateHousing.Where(a => a.HousingUnitLocation == rosterFilters.HousingLocation)
                        .ToList();
                }
                else if (rosterFilters.Housing == RosterConstants.NOHOUSING)
                {
                    _inmateHousing = _inmateHousing.Where(a => a.FacilityId == 0).ToList();
                }

                if (rosterFilters.IsCheckOut)
                {
                    _inmateHousing = _inmateHousing.Where(a => !string.IsNullOrEmpty(a.InmateCurrentTrack))
                        .ToList();
                }
                else if (rosterFilters.IsActual)
                {
                    _inmateHousing = _inmateHousing.Where(a => string.IsNullOrEmpty(a.InmateCurrentTrack))
                        .ToList();
                }

                if (rosterFilters.IsFilterByName)
                {
                    _inmateHousing = _inmateHousing.OrderBy(a => a.PersonLastName).ThenBy(a => a.PersonFirstName)
                        .ThenBy(a => a.PersonMiddleName)
                        .ToList();
                }
            }
            if (rosterFilters.HousingGroupId.HasValue)
            {
                _housingGroups = _context.HousingGroupAssign
                    .Where(a => a.HousingGroupId == rosterFilters.HousingGroupId)
                    .Select(a => new KeyValuePair<int?, int?>(a.HousingUnitListId, a.LocationId)).ToList();
                _inmateHousing = _inmateHousing.Where(a =>
                        _housingGroups.Where(x => x.Key.HasValue).Select(x => x.Key).Contains(a.HousingUnitListId))
                    .ToList();
            }
            if (!string.IsNullOrEmpty(rosterFilters.Association))
            {
                List<int> personIds = _context.PersonClassification
                    .Where(a => a.PersonClassificationTypeId == rosterFilters.AssociationId &&
                                (!a.PersonClassificationDateThru.HasValue ||
                                 a.PersonClassificationDateThru >= DateTime.Now))
                    .Select(a => a.PersonId).ToList();
                _inmateHousing = _inmateHousing.Where(a => personIds.Contains(a.PersonId)).ToList();
            }
            if (!string.IsNullOrEmpty(rosterFilters.AlertType))
            {
                List<int> listPersonId = _context.PersonFlag.Where(a =>
                        rosterFilters.AlertType == LookupConstants.PERSONCAUTION &&
                        Convert.ToDouble(a.PersonFlagIndex ?? 0).Equals(rosterFilters.AlertIndex)
                        || rosterFilters.AlertType == LookupConstants.TRANSCAUTION &&
                        Convert.ToDouble(a.InmateFlagIndex ?? 0).Equals(rosterFilters.AlertIndex)
                        || rosterFilters.AlertType == LookupConstants.DIET &&
                        Convert.ToDouble(a.DietFlagIndex ?? 0).Equals(rosterFilters.AlertIndex))
                    .Select(a => a.PersonId).ToList();
                _inmateHousing =
                    _inmateHousing.SelectMany(a => listPersonId.Where(x => x == a.PersonId), (a, x) => a)
                        .ToList();
            }
            if (!string.IsNullOrEmpty(rosterFilters.Balance))
            {
                List<AccountAoInmateVm> accountAoList = _context.Inmate.Where(a => a.InmateActive == 1).Select(a =>
                    new AccountAoInmateVm
                    {
                        InmateId = a.InmateId
                    }).ToList();
                _context.AccountAoInmate.ToList().ForEach(a =>
                {
                    accountAoList.Where(x => x.InmateId == a.InmateId).ToList().ForEach(x =>
                    {
                        x.BalanceInmate = a.BalanceInmate;
                    });
                });
                accountAoList = accountAoList.Where(a =>
                    (!rosterFilters.Balance.Equals(RosterConstants.POSITIVE) || a.BalanceInmate > 0) &&
                    (!rosterFilters.Balance.Equals(RosterConstants.ZERO) ||
                     (!a.BalanceInmate.HasValue || a.BalanceInmate == 0))
                ).ToList();
                _inmateHousing = _inmateHousing.Where(a => accountAoList.Select(x => x.InmateId)
                    .Contains(a.InmateId)).ToList();
            }
            if (!string.IsNullOrEmpty(rosterFilters.Status))
            {
                List<Incarceration> incarcerations = _context.Incarceration.Where(a => (!rosterFilters.Status.Equals(RosterConstants.Sent)
                                                              || (!a.ReleaseOut.HasValue &&
                                                           a.OverallFinalReleaseDate.HasValue))
                                                          && (!rosterFilters.Status.Equals(RosterConstants.UnSent)
                                                              || (!a.ReleaseOut.HasValue &&
                                                               !a.OverallFinalReleaseDate.HasValue))).ToList();
                _inmateHousing = _inmateHousing.SelectMany(a => incarcerations.Where(x => x.InmateId == a.InmateId), (a, x) => a).ToList();
            }
            List<RosterDetails> searchHousingDetails = new List<RosterDetails>();
            if (!rosterFilters.ToLoadInmate || !string.IsNullOrEmpty(rosterFilters.Location))
            {
                searchHousingDetails = new List<RosterDetails>
                {
                    new RosterDetails
                    {
                        Housing = FloorNote.ALL,
                        Assigned = _inmateHousing.Count,
                        Checkout = _inmateHousing.Count(a => !string.IsNullOrEmpty(a.InmateCurrentTrack))
                    }
                };
            }
            if (_inmateHousing.Any(a => a.FacilityId == 0))
            {
                searchHousingDetails.Add(new RosterDetails
                {
                    Housing = RosterConstants.NOHOUSING,
                    Assigned = _inmateHousing.Count(a => a.FacilityId == 0),
                    Checkout = _inmateHousing.Count(a => a.FacilityId == 0
                                                         && !string.IsNullOrEmpty(a.InmateCurrentTrack)),
                    InmateList = rosterFilters.ToLoadInmate ? _inmateHousing.Where(a => a.FacilityId == 0).Select(a => new RosterInmateInfo
                    {
                        InmateId = a.InmateId,
                        InmateNumber = a.InmateNumber,
                        InmateCurrentTrack = a.InmateCurrentTrack,
                        PersonId = a.PersonId,
                        PersonFirstName = a.PersonFirstName,
                        PersonMiddleName = a.PersonMiddleName,
                        PersonLastName = a.PersonLastName,
                        PersonPhoto = a.PersonPhoto,
                        HousingUnitLocation = a.HousingUnitLocation,
                        HousingUnitNumber = a.HousingUnitNumber,
                        HousingUnitBedNumber = a.HousingUnitBedNumber
                    }).ToList() : null
                });
            }
            List<HousingDetail> housingNumberAll = _context.HousingUnit.Where(a =>
                    a.FacilityId == rosterFilters.FacilityId &&
                    (!a.HousingUnitInactive.HasValue || a.HousingUnitInactive==0))
                .GroupBy(a => new
                {
                    a.FacilityId,
                    a.Facility.FacilityAbbr,
                    a.HousingUnitLocation,
                    a.HousingUnitNumber,
                    a.HousingUnitInactive,
                    a.HousingUnitClassConflictCheck,
                    a.HousingUnitClassifyRecString
                }).Select(a => new HousingDetail
                {
                    FacilityId = a.Key.FacilityId,
                    HousingUnitLocation = a.Key.HousingUnitLocation,
                    HousingUnitNumber = a.Key.HousingUnitNumber
                }).ToList();
            searchHousingDetails.AddRange(_inmateHousing.SelectMany(a => housingNumberAll
                    .Where(x => x.FacilityId == a.FacilityId &&
                                x.HousingUnitLocation == a.HousingUnitLocation &&
                                x.HousingUnitNumber == a.HousingUnitNumber), (a, x) => a)
                .Select(a => new
                {
                    a.FacilityId,
                    a.InmateId,
                    a.InmateNumber,
                    a.InmateCurrentTrack,
                    a.PersonId,
                    a.PersonFirstName,
                    a.PersonMiddleName,
                    a.PersonLastName,
                    a.PersonPhoto,
                    a.HousingUnitLocation,
                    a.HousingUnitNumber,
                    a.HousingUnitBedNumber
                }).GroupBy(a => new
                {
                    a.FacilityId,
                    a.HousingUnitLocation,
                    a.HousingUnitNumber
                }).Select(a => new RosterDetails
                {
                    FacilityId = a.Key.FacilityId ?? 0,
                    Assigned = a.Count(),
                    Checkout = a.Count(x => !string.IsNullOrEmpty(x.InmateCurrentTrack)),
                    HousingLocation = a.Key.HousingUnitLocation,
                    HousingNumber = a.Key.HousingUnitNumber,
                    InmateList = rosterFilters.ToLoadInmate ? a.Select(x => new RosterInmateInfo
                    {
                        InmateId = x.InmateId,
                        InmateNumber = x.InmateNumber,
                        InmateCurrentTrack = x.InmateCurrentTrack,
                        PersonId = x.PersonId,
                        PersonFirstName = x.PersonFirstName,
                        PersonMiddleName = x.PersonMiddleName,
                        PersonLastName = x.PersonLastName,
                        PersonPhoto = x.PersonPhoto,
                        HousingUnitLocation = x.HousingUnitLocation,
                        HousingUnitNumber = x.HousingUnitNumber,
                        HousingUnitBedNumber = x.HousingUnitBedNumber
                    }).ToList() : null
                }).Where(a => a.FacilityId == rosterFilters.FacilityId));
            if (!rosterFilters.HousingUnitListId.HasValue && !rosterFilters.HousingGroupId.HasValue)
            {
                searchHousingDetails.AddRange(searchHousingDetails.Where(a => a.FacilityId > 0)
                    .GroupBy(a => a.HousingLocation)
                    .Select(a => new RosterDetails
                    {
                        HousingLocation = a.Key,
                        Assigned = a.Sum(x => x.Assigned),
                        Checkout = a.Sum(x => x.Checkout)
                    }));
            }
            return searchHousingDetails.OrderBy(a => a.HousingLocation).ThenBy(a => a.HousingNumber).ToList();
        }

        private List<LocationDetails> GetLocationDetails(RosterFilters rosterFilters)
        {
            if (rosterFilters.HousingGroupId > 0)
            {
                _inmateHousing = _inmateHousing.Where(a => _housingGroups.Where(x => x.Value.HasValue)
                    .Select(x => x.Value)
                    .Contains(a.InmateCurrentTrackId)).ToList();
            }
            List<LocationDetails> locationDetails = new List<LocationDetails>
            {
                new LocationDetails
                {
                    Location = FloorNote.ALL,
                    Assigned = _inmateHousing.Count(a => !string.IsNullOrEmpty(a.InmateCurrentTrack)),
                    Checkout = _inmateHousing.Count(a => !string.IsNullOrEmpty(a.InmateCurrentTrack))
                }
            };
            locationDetails.AddRange(_inmateHousing.Where(a => !string.IsNullOrEmpty(a.InmateCurrentTrack))
                .GroupBy(a => a.InmateCurrentTrack)
                .Select(a => new LocationDetails
                {
                    Location = a.Key,
                    Assigned = a.Count(),
                    Checkout = a.Count()
                }).OrderBy(a => a.Location));
            return locationDetails;
        }

        public List<FormTemplate> GetPersonFormTemplates() //to show booking 
        {
            List<FormTemplate> lstPersonFormTemplate = _context.PersonFormTemplate.Where(a => a.ShowInRosterOverlay == true).Select(a => new FormTemplate
            {
                TemplateName = a.TemplateName,
                RequireBookingSelect = a.RequireBookingSelect,
                PersonFormTemplateId = a.PersonFormTemplateId,

            }).ToList();
            return lstPersonFormTemplate;
        }


        // Get print overlay details
        public PrintOverLay GetRosterOverlay(PrintOverLay printOverLay)
        {
            printOverLay.TemplateControls = GetTemplateControls(printOverLay.PersonFormTemplateId);
            //if (!printOverLay.TemplateControls.Any()) return printOverLay;
            if (printOverLay.RequireBookingSelect == true)
            {
                printOverLay.TemplateValueJsonString = _miscLabelService.GetTemplateSqlDataJsonString(
                    printOverLay.TemplateSql, printOverLay.ArrestId);
            }
            else if (printOverLay.Flag == CustomConstants.Personnel)
            {
                printOverLay.TemplateValueJsonString = _miscLabelService.GetTemplateSqlDataJsonString(printOverLay.TemplateSql,
                    printOverLay.PersonnelId, 0, true);
            }
            else
            {
                printOverLay.TemplateValueJsonString =
                    _miscLabelService.GetTemplateSqlDataJsonString(printOverLay.TemplateSql, printOverLay.PersonId, printOverLay.PersonId);
            }
            return printOverLay;
        }

        // Get list of arrest Id's and Booking numbers based on Inmate Id's
        public List<InmateBookings> GetInmateBookings(int[] inmateId)
        {
            return _context.IncarcerationArrestXref.Where(a =>
                    a.Incarceration.InmateId.HasValue && inmateId.Contains(a.Incarceration.InmateId.Value))
                .OrderByDescending(a => a.Incarceration.IncarcerationId)
                .ThenBy(a => a.Arrest.ArrestId).Select(a => new InmateBookings
                {
                    InmateId = a.Incarceration.InmateId,
                    ArrestId = a.Arrest.ArrestId,
                    ArrestBookingNo = a.Arrest.ArrestBookingNo
                }).ToList();
        }

        // Get template control details based on form template Id.
        private List<TemplateControl> GetTemplateControls(int personFormTemplateId)
        {
            return _context.PersonFormTemplateCtl
                .Where(a => a.PersonFormTemplateId == personFormTemplateId)
                .Select(a => new TemplateControl
                {
                    Type = a.CtlType,
                    FieldName = a.CtlFieldName,
                    Value = a.CtlValue,
                    XPos = a.CtlCordx,
                    YPos = a.CtlCordy,
                    Width = a.CtlCordw,
                    Height = a.CtlCordh,
                    Font = a.CtlFont,
                    FontSize = a.CtlFontSize,
                    ForeColor = a.CtlColor1,
                    BackColor = a.CtlColor2
                }).ToList();
        }
        public async Task<IActionResult> PrintOverlayReport(string ids,int templateId)
        {
            PrintAllOverLay obj = _context.PersonFormTemplate.Where(a => a.PersonFormTemplateId == templateId).Select(a => new PrintAllOverLay()
            {
                TemplateSql = a.TemplateSql,
                ShortId = a.ShortId
            }).First();
            string TemplateValueJsonString = _miscLabelService.GetTemplateSqlDataJsonString(obj.TemplateSql, ids);
            ReportingService rs = new ReportingService(_jsReportUrl.ToString());
            _commonService.atimsReportsContentLog(obj.ShortId, TemplateValueJsonString);
            Report report = await rs.RenderAsync(obj.ShortId, TemplateValueJsonString);
            FileContentResult LabelPdf = new FileContentResult(_commonService.ConvertStreamToByte(report.Content), "application/pdf");
            return LabelPdf;
        }
    }
}

