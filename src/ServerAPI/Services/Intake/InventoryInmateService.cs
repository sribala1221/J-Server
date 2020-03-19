﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GenerateTables.Models;
using ServerAPI.Utilities;
using ServerAPI.ViewModels;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;
using System.IO;

// ReSharper disable once CheckNamespace
namespace ServerAPI.Services
{
    public class InventoryInmateService : IInventoryInmateService
    {
        private readonly AAtims _context;
        private readonly ICommonService _commonService;
        private readonly IPersonService _personService;
        private readonly IFormsService _formsService;
        private readonly int _personnelId;
        private readonly IConfiguration _configuration;
        private readonly IPhotosService _photos;
        private readonly string _externalPath;
        private readonly string _path;
        private readonly IFacilityHousingService _facilityHousingService;
        private readonly IInterfaceEngineService _interfaceEngineService;

        public InventoryInmateService(AAtims context, ICommonService commonService,
            IHttpContextAccessor httpContextAccessor, IFormsService formsService, IConfiguration configuration,
            IPersonService personService, IPhotosService photosService,
            IFacilityHousingService facilityHousingService, IInterfaceEngineService interfaceEngineService)
        {
            _context = context;
            _commonService = commonService;
            _personnelId =
                Convert.ToInt32(httpContextAccessor.HttpContext.User.FindFirst("personnelId")?.Value);
            _formsService = formsService;
            _configuration = configuration;
            _personService = personService;
            _photos = photosService;
            _externalPath = configuration.GetValue<string>("SiteVariables:ClientExternalPhotoPath");
            _path = configuration.GetValue<string>("SiteVariables:AtimsPhotoPath");
            _facilityHousingService = facilityHousingService;
            _interfaceEngineService = interfaceEngineService;
        }

        //load the inventory grid
        public InventoryVm GetInventoryGrid(int inmateId, Disposition disposition, int showDeleteFlag)
        {
            // load the change group list details
            List<InventoryItemDetails> lstInventoryItemDetails = InventoryDetails(inmateId, disposition);

            //http://stackoverflow.com/questions/11884255/distinct-doesnt-work   
            List<InventoryDetails> lstInventoryDetails = lstInventoryItemDetails
                .Where(i => showDeleteFlag == 0 ? i.DeleteFlag == 0 : i.DeleteFlag == 0 || i.DeleteFlag == 1)
                .Select(i => new
                {
                    i.PersonalInventoryBinId,
                    i.PersonalInventoryGroupId,
                    i.PersonalBinName,
                    PersonalGroupName = i.PersonalInventoryGroupNumber,
                    i.InventoryDispositionCode,
                    i.InventoryReturnDate
                }).Distinct().Select(s => new InventoryDetails
                {
                    PersonalInventoryBinId = s.PersonalInventoryBinId,
                    PersonalInventoryGroupId = s.PersonalInventoryGroupId,
                    PersonalBinName = s.PersonalBinName,
                    PersonalGroupName = s.PersonalGroupName,
                    InventoryDispositionCode = s.InventoryDispositionCode,
                    InventoryReturnDate = s.InventoryReturnDate
                }).ToList();

            IQueryable<PersonalInventoryGroup> lstPropertyGroupNotes = _context.PersonalInventoryGroup;

            lstInventoryDetails.ForEach(item =>
            {
                item.PropertyGroupNotes = lstPropertyGroupNotes
                    .Single(x => x.PersonalInventoryGroupId == item.PersonalInventoryGroupId).GroupNote;

                List<KeyValuePair<int, DateTime?>> lstInventoryItems = lstInventoryItemDetails
                    .Where(s => s.PersonalInventoryBinId == item.PersonalInventoryBinId
                                && s.PersonalInventoryGroupId == item.PersonalInventoryGroupId)
                    .Select(s => new KeyValuePair<int, DateTime?>(s.DeleteFlag, s.InventoryReturnDate)).ToList();

                item.InventoryItemCount = Disposition.Storage == disposition && showDeleteFlag == 1
                    ? lstInventoryItems.Select(s => s.Key).Count()
                    : lstInventoryItems.Count(s => s.Value == item.InventoryReturnDate && s.Key == 0);
                    item.ReceivingFlag = lstInventoryItemDetails.First(i => i.PersonalInventoryBinId == item.PersonalInventoryBinId)
                    .ReceivingFlag;
                    item.FacilityTransferFlag=lstInventoryItemDetails.First(i=> i.PersonalInventoryBinId==item.PersonalInventoryBinId)
                    .FacilityTransferFlag;
            });

            List<int> lstAgencyIds =
                lstInventoryItemDetails.Where(a => a.InventoryEvidenceAgencyId.HasValue)
                    .Select(a => a.InventoryEvidenceAgencyId.Value)
                    .ToList();

            List<AgencyVm> lstAgencyVm = _commonService.GetAgencyNameList(lstAgencyIds);

            List<int> lstPersonnelIds =
                lstInventoryItemDetails.Where(a => a.InventoryEvidencePersonnelId.HasValue)
                    .Select(a => a.InventoryEvidencePersonnelId.Value)
                    .ToList();

            List<int> lstInventoryId = lstInventoryItemDetails.Select(s => s.PersonalInventoryId).ToList();
            List<IdentifierVm> lstIdentifierDetail = _context.Identifiers.Where(w => lstInventoryId.Contains(w.PersonalInventoryId ?? 0))
                .Select(s => new IdentifierVm
                {
                    PersonalInventoryId = s.PersonalInventoryId ?? 0,
                    IdentifierId = s.IdentifierId,
                    PhotographRelativePath = _photos.GetPhotoByIdentifier(s),
                    PhotographDate = s.PhotographDate,
                    IdentifierNarrative = s.IdentifierNarrative,
                    DeleteFlag = s.DeleteFlag,
                    DeleteDate = s.DeleteDate,
                    DeleteBy = s.DeleteBy,
                    IdentifierDescription = s.IdentifierDescription,
                    IdentifierLocation = s.IdentifierLocation,
                    PhotoTakenBy = s.PhotographTakenBy ?? 0
                }).ToList();
            //List<int> lstPropertyPersonnelId = lstIdentifierDetail.Select(s => s.DeleteBy ?? 0).ToList();
            List<int> lstPropertyPeronnelId = lstIdentifierDetail.Select(s => new[] { s.DeleteBy, s.PhotoTakenBy })
                  .SelectMany(s => s)
                  .Where(s => s.HasValue)
                  .Select(s => s.Value)
                  .ToList();

            if (lstPropertyPeronnelId.Count > 0)
            {
                lstPersonnelIds.AddRange(lstPropertyPeronnelId);
            }

            List<PersonnelVm> lstPersonnelVm = _personService.GetPersonNameList(lstPersonnelIds);

            //get article description by lookup index
            int[] listLookupIds = lstInventoryItemDetails.Select(s => s.InventoryArticles).ToArray();
            List<LookupVm> listLookupDetails = _context.Lookup.Where(w => listLookupIds.Contains(w.LookupIndex) 
                && w.LookupType == LookupConstants.INVARTCL)
                .Select(s => new LookupVm
                {
                    LookupIndex = s.LookupIndex,
                    LookupDescription = s.LookupDescription
                }).ToList();


            lstInventoryItemDetails.ForEach(item =>
            {
                item.AgencyDetails = lstAgencyVm.Where(a => a.AgencyId == item.InventoryEvidenceAgencyId).ToList();

                item.PersonalDetails =
                    lstPersonnelVm.Where(a => a.PersonnelId == item.InventoryEvidencePersonnelId).ToList();

                item.ListIdentifiers = lstIdentifierDetail.Where(s => s.PersonalInventoryId == item.PersonalInventoryId)
                .OrderByDescending(o => o.IdentifierId)
                .Select(s => new IdentifierVm
                {
                    PersonalInventoryId = s.PersonalInventoryId,
                    IdentifierId = s.IdentifierId,
                    PhotographRelativePath = s.PhotographRelativePath,
                    PhotographDate = s.PhotographDate,
                    IdentifierNarrative = s.IdentifierNarrative,
                    DeleteFlag = s.DeleteFlag,
                    DeleteDate = s.DeleteDate,
                    DeleteBy = s.DeleteBy,
                    IdentifierDescription = s.IdentifierDescription,
                    IdentifierLocation = s.IdentifierLocation,
                    Officer = lstPersonnelVm.SingleOrDefault(w => w.PersonnelId == s.DeleteBy),
                    TakenByOfficer = lstPersonnelVm.SingleOrDefault(w => w.PersonnelId == s.PhotoTakenBy)
                }).ToList();

                if (listLookupDetails.Count > 0)
                {
                    item.InventoryArticlesName = listLookupDetails.Single(w =>
                        w.LookupIndex == item.InventoryArticles).LookupDescription;
                }
            });

            //another sub grid for inventory groups
            List<InventoryPropGroupDetails> lstPropertyGroup = lstInventoryDetails
                .Where(a => showDeleteFlag == 0 ? a.DeleteFlag == 0 : a.DeleteFlag == 0 || a.DeleteFlag == 1)
                .Select(a => new InventoryPropGroupDetails
                {
                    PropGroupId = a.PersonalInventoryGroupId,
                    PropGroupName = a.PersonalGroupName,
                    PropGroupNotes = a.PropertyGroupNotes
                }).ToList();
                
            List<Incarceration> incarcerationdet=_context.Incarceration.Where(i=>i.ReleaseOut==null && i.InmateId==inmateId)
            .Select(i=>new Incarceration
            {
                ReleaseClearFlag=i.ReleaseClearFlag
            }).ToList();    

            InventoryVm inventoryVm = new InventoryVm
            {
                InventoryDetails = lstInventoryDetails,
                InventoryItemDetails = lstInventoryItemDetails,
                InventoryPropGroup = lstPropertyGroup,
                ReleaseDetail=incarcerationdet,
            };

            return inventoryVm;
        }

        //load the inventory grid child history click event
        public List<InventoryHistoryVm> GetInventoryHistory(int inventoryId, int inmateId, Disposition disposition, bool inventoryFlag)
        {
            List<InventoryHistoryVm> lstInventoryHistory = _context.PersonalInventoryHistory
                .Where(h => h.PersonalInventoryId == inventoryId &&
                            (inventoryFlag || h.InmateId == inmateId) &&
                            (Disposition.Storage != disposition || !h.InventoryReturnDate.HasValue &&
                             h.InventoryDispositionCode == (int?)Disposition.Storage))
                .Select(h => new InventoryHistoryVm
                {
                    PersonalInventoryId = h.PersonalInventoryId,
                    InventoryQuantity = h.InventoryQuantity,
                    InventoryColor = h.InventoryColor,
                    InventoryArticles = h.InventoryArticles,
                    InventoryDescription = h.InventoryDescription,
                    InventoryDispositionCode = h.InventoryDispositionCode,
                    PersonalInventoryBinNumber = h.PersonalInventoryBin.BinName,
                    InventoryDate = h.InventoryDate,
                    InventoryMailAddressId = h.InventoryMailAddressId,
                    InventoryMailPersonId = h.InventoryMailPersonId,
                    InventoryEvidencePersonnelId = h.InventoryEvidencePersonnelId,
                    DeleteDate = h.DeleteDate,
                    CreatedBy = h.CreatedBy,
                    UpdatedBy = h.UpdatedBy,
                    DeletedBy = h.DeletedBy,
                    DeleteReason = h.DeleteReason,
                    DeleteReasonNote = h.DeleteReasonNote,
                    FacilityAbbr = h.PersonalInventoryBin.Facility.FacilityAbbr,
                    PersonalInventoryUpdateDate = h.UpdateDate,
                    PersonalInventoryGroupId = h.PersonalInventoryGroupId ?? 0,
                    PersonName = h.PersonName,
                    PersonIdType = h.PersonIdType,
                    PersonAddress = h.PersonAddress,
                    CityStateZip = h.CityStateZip,
                    DeleteFlag = h.DeleteFlag,
                    InventoryEvidenceAgencyId = h.InventoryEvidenceAgencyId,
                    PersonalInventoryBinId = h.PersonalInventoryBinId,
                    InventoryEvidenceCaseNumber = h.InventoryEvidenceCaseNumber,
                    DispoNotes = h.DispoNotes
                }).ToList();

            List<InventoryItemDetails> lstInventoryGroup =
                _context.PersonalInventoryGroup.Select(a => new InventoryItemDetails
                {
                    PersonalInventoryGroupNumber = a.GroupNumber,
                    PersonalInventoryGroupId = a.PersonalInventoryGroupId
                }).ToList();

            //get inventory article name by index
            int[] listLookupIds = lstInventoryHistory.Select(s => s.InventoryArticles).ToArray();
            List<LookupVm> listLookupDetails = _context.Lookup.Where(w => listLookupIds.Contains(w.LookupIndex) 
                                                                          && w.LookupType == LookupConstants.INVARTCL && w.LookupInactive == 0)
                .Select(s => new LookupVm
                {
                    LookupIndex = s.LookupIndex,
                    LookupDescription = s.LookupDescription
                }).ToList();

            lstInventoryHistory.ForEach(item =>
            {
                if (item.PersonalInventoryGroupId != 0) // check the null condition for personal group id in database
                {
                    //adding group number in inventory history grid  
                    item.PersonalInventoryGroupNumber = lstInventoryGroup
                        .SingleOrDefault(a => a.PersonalInventoryGroupId == item.PersonalInventoryGroupId)
                        ?.PersonalInventoryGroupNumber;

                    //adding group id in inventory history grid  
                    item.PersonalInventoryGroupId = lstInventoryGroup
                        .SingleOrDefault(a => a.PersonalInventoryGroupId == item.PersonalInventoryGroupId)
                        ?.PersonalInventoryGroupId;
                    if (listLookupDetails.Count > 0)
                    {
                        item.InventoryArticlesName = listLookupDetails.SingleOrDefault(w =>
                            w.LookupIndex == item.InventoryArticles)?.LookupDescription;
                    }
                }

                // adding 5 id's column into single list to fetch the data from single table 
                List<int> personnelIds = new[]
                {
                    item.CreatedBy, item.UpdatedBy, item.DeletedBy, item.InventoryMailPersonId,
                    item.InventoryMailAddressId, item.InventoryEvidencePersonnelId
                }.Where(p => p.HasValue).Select(p => p.Value).ToList();

                // getting the person details 
                List<PersonnelVm> lstPersonDetails = _personService.GetPersonNameList(personnelIds);

                PersonnelVm personInfo = lstPersonDetails
                    .SingleOrDefault(a => a.PersonnelId == item.CreatedBy);

                if (personInfo != null)
                {
                    item.CreatedPersonFirstName = personInfo.PersonFirstName;
                    item.CreatedPersonLastName = personInfo.PersonLastName;
                    item.CreatedPersonMiddleName = personInfo.PersonMiddleName;


                    if (item.UpdatedBy.HasValue)
                    {
                        personInfo = lstPersonDetails.SingleOrDefault(a => a.PersonnelId == item.UpdatedBy);
                        if (personInfo != null)
                        {
                            item.UpdatedPersonFirstName = personInfo.PersonFirstName;
                            item.UpdatedPersonLastName = personInfo.PersonLastName;
                            item.UpdatedPersonMiddleName = personInfo.PersonMiddleName;
                        }
                    }

                    if (item.DeletedBy.HasValue)
                    {
                        personInfo = lstPersonDetails.SingleOrDefault(a => a.PersonnelId == item.DeletedBy);

                        if (personInfo != null)
                        {
                            item.DeletedPersonFirstName = personInfo.PersonFirstName;
                            item.DeletedPersonLastName = personInfo.PersonLastName;
                            item.DeletedPersonMiddleName = personInfo.PersonMiddleName;
                        }
                    }

                    if (item.InventoryMailPersonId.HasValue)
                    {
                        personInfo = lstPersonDetails.SingleOrDefault(a => a.PersonnelId == item.InventoryMailPersonId);
                        if (personInfo != null)
                        {
                            item.InventoryMailPersonFirstName = personInfo.PersonFirstName;
                            item.InventoryMailPersonLastName = personInfo.PersonLastName;
                            item.InventoryMailPersonMiddleName = personInfo.PersonMiddleName;
                        }
                    }

                    if (item.InventoryMailAddressId.HasValue)
                    {
                        personInfo =
                            lstPersonDetails.SingleOrDefault(a => a.PersonnelId == item.InventoryMailAddressId);
                        if (personInfo != null)
                        {
                            item.InventoryMailAddressPersonFirstName = personInfo.PersonFirstName;
                            item.InventoryMailAddressPersonLastName = personInfo.PersonLastName;
                            item.InventoryMailAddressPersonMiddleName = personInfo.PersonMiddleName;
                        }
                    }

                    if (item.InventoryEvidencePersonnelId.HasValue)
                    {
                        personInfo =
                            lstPersonDetails.SingleOrDefault(a => a.PersonnelId == item.InventoryEvidencePersonnelId);
                        if (personInfo != null)
                        {
                            item.InventoryEvidencePersonFirstName = personInfo.PersonFirstName;
                            item.InventoryEvidencePersonLastName = personInfo.PersonLastName;
                            item.InventoryEvidencePersonMiddleName = personInfo.PersonMiddleName;
                            item.OfficerBadgeNumber = personInfo.OfficerBadgeNumber;
                        }
                    }
                }

                if (item.InventoryEvidenceAgencyId.HasValue)
                {
                    item.AgencyName = _context.Agency.Single(a => a.AgencyId == item.InventoryEvidenceAgencyId).AgencyName;
                }

                item.LookupDescription = _commonService.GetLookupList(LookupConstants.INVDISP)
                    .SingleOrDefault(l => l.LookupIndex == item.InventoryDispositionCode).LookupDescription;

            });

            return lstInventoryHistory;
        }

        // load the property group notes sub grid history list click event
        public List<PropGroupHistoryDetails> PropertyGroupHistory(int propGroupId)
        {
            //load property history list 
            List<PropGroupHistoryDetails> lstPropGroupHistoryDetails =
                _context.PersonalInventoryGroupHistory
                    .Where(a => a.PersonalInventoryGroupId == propGroupId)
                    .Select(a => new PropGroupHistoryDetails
                    {
                        Personalgroupid = a.PersonalInventoryGroupId,
                        CreateDate = a.CreateDate,
                        InventoryNotes = a.GroupNote,
                        CreatedBy = a.CreateBy,
                        DeleteFlag = a.DeleteFlag,
                        Personalgroup = a.PersonalInventoryGroup.GroupNumber,
                        PersonalGroupHistoryId = a.PersonalInventoryGroupHistoryId,
                        LostFoundByInmateId = a.LostFoundByInmateId,
                        LostFoundByPersonnelId = a.LostFoundByPersonnelId,
                        LostFoundLocFacilityId = a.LostFoundLocFacilityId,
                        LostFoundByOther = a.LostFoundByOther,
                        LostFoundCircumstance = a.LostFoundCircumstance,
                        LostFoundDate = a.LostFoundDate,
                        LostFoundLocHousingUnitLocation = a.LostFoundLocHousingUnitLocation,
                        LostFoundLocHousingUnitNumber = a.LostFoundLocHousingUnitNumber,
                        LostFoundLocOther = a.LostFoundLocOther,
                        LostFoundLocPrivilegeId = a.LostFoundLocPrivilegeId,
                        FoundBy = new FoundBy
                        {
                            InmateNumber = a.LostFoundByInmate.InmateNumber,
                            InmateVm = new PersonnelVm
                            {
                                PersonLastName = a.LostFoundByInmate.Person.PersonLastName,
                                PersonFirstName = a.LostFoundByInmate.Person.PersonFirstName
                            },
                            PersonVm = new PersonnelVm
                            {
                                PersonLastName = a.LostFoundByPersonnel.PersonNavigation.PersonLastName,
                                PersonFirstName = a.LostFoundByPersonnel.PersonNavigation.PersonFirstName,
                                OfficerBadgeNumber = a.LostFoundByPersonnel.OfficerBadgeNum
                            },
                            LostFoundByOther = a.LostFoundByOther
                        },
                        FoundIn = new FoundIn
                        {
                            FacilityAbbr = a.LostFoundLocFacility.FacilityAbbr,
                            HousingUnitLocation = a.LostFoundLocHousingUnitLocation,
                            HousingUnitNumber = a.LostFoundLocHousingUnitNumber,
                            LostFoundLocOther = a.LostFoundLocOther,
                            PrivilegeDescription = a.LostFoundLocPrivilege.PrivilegeDescription
                        }
                    }).ToList();

            List<int> lstPersonnelIds = lstPropGroupHistoryDetails.Where(a => a.CreatedBy.HasValue)
                .Select(a => a.CreatedBy.Value)
                .ToList();

            List<PersonnelVm> lstPersonDetails = _personService.GetPersonNameList(lstPersonnelIds);

            lstPropGroupHistoryDetails.ForEach(item =>
            {
                PersonnelVm personInfo = lstPersonDetails
                    .SingleOrDefault(a => a.PersonnelId == item.CreatedBy);
                if (personInfo == null) return;
                item.PropGrpPersonFirstName = personInfo.PersonFirstName;
                item.PropGrpPersonLastName = personInfo.PersonLastName;
                item.PropGrpPersonMiddleName = personInfo.PersonMiddleName;
                item.OfficerBadgeNumber = personInfo.OfficerBadgeNumber;
            });

            return lstPropGroupHistoryDetails;
        }

        // load the change inventory group details
        public InventoryChangeGroupVm LoadChangeInventoryDetails(int inmateId, int inventoryBinId, int inventoryGroupId,
            Disposition disposition)
        {

            //dropdown list for sub grid child Move To Bin-Group 
            List<InventoryChangeDropDownDetails> lstInventoryDropDownDetails =
                _context.PersonalInventory
                    .Where(i => i.InmateId == inmateId &&
                                i.PersonalInventoryGroupId > 0 &&
                                i.InventoryDispositionCode == (int?)Disposition.Storage &&
                                !i.PersonalInventoryGroup.DeleteFlag.HasValue)
                    .Select(i => new
                    {
                        i.PersonalInventoryBinId,
                        i.PersonalInventoryGroupId,
                        PersonalGroupName = i.PersonalInventoryGroup.GroupNumber
                    }).Distinct().Select(s => new InventoryChangeDropDownDetails
                    {
                        InventoryChangeGroupBinId = s.PersonalInventoryBinId,
                        InventoryChangeGroupGroupId = s.PersonalInventoryGroupId,
                        InventoryChangeGroupGroupName = s.PersonalGroupName
                    }).ToList();

            List<InventoryItemDetails> lstInventoryItemDetails = InventoryDetails(inmateId, disposition);

            List<InventoryDetails> lstInventoryDetails =
                lstInventoryItemDetails.Select(i => new
                {
                    i.PersonalInventoryBinId,
                    i.PersonalInventoryGroupId,
                    i.PersonalBinName,
                    PersonalGroupName = i.PersonalInventoryGroupNumber,
                    i.InventoryDispositionCode,
                    i.DeleteFlag
                }).Distinct().Select(s => new InventoryDetails
                {
                    PersonalInventoryBinId = s.PersonalInventoryBinId,
                    PersonalInventoryGroupId = s.PersonalInventoryGroupId,
                    PersonalBinName = s.PersonalBinName,
                    PersonalGroupName = s.PersonalGroupName,
                    InventoryDispositionCode = s.InventoryDispositionCode,
                    DeleteFlag = s.DeleteFlag
                }).ToList();

            //get the groupdetails 
            List<InventoryDetails> lstChangeGroupDetails =
                lstInventoryDetails.Where(p => p.PersonalInventoryBinId == inventoryBinId
                                               && p.PersonalInventoryGroupId == inventoryGroupId)
                    .ToList();

            List<InventoryItemDetails> lstChangeGroupItemDetails =
                lstInventoryItemDetails.Where(p => p.PersonalInventoryBinId == inventoryBinId
                                                   && p.PersonalInventoryGroupId == inventoryGroupId)
                    .ToList();

            lstChangeGroupDetails.ForEach(item =>
            {
                item.InventoryItemCount = lstInventoryItemDetails.Count(p =>
                    p.PersonalInventoryBinId == inventoryBinId &&
                    p.PersonalInventoryGroupId == inventoryGroupId);
            });

            lstInventoryDropDownDetails.ForEach(item =>
            {
                //change the navigation is not working and its framework to be update and after its finished 
                item.InventoryChangeGroupBinName = _context.PersonalInventoryBin
                        .Single(a => a.PersonalInventoryBinId == item.InventoryChangeGroupBinId).BinName;
            });

            InventoryChangeGroupVm lstInventoryChangeGroupDetails = new InventoryChangeGroupVm
            {
                InventoryChangeGroupDetails = lstChangeGroupDetails,
                InventoryChangeGroupItemDetails = lstChangeGroupItemDetails,
                InventoryDropDownDetails = lstInventoryDropDownDetails
            };

            return lstInventoryChangeGroupDetails;
        }

        // save & update the inventory based on bin-group/splitgroup 
        public async Task<int> UpdateInventoryInmate(InventoryChangeGroupVm value)
        {

            int groupId = value.InventoryDetails.PersonalInventoryGroupId;

            // Split and New group in inventory
            if (InventoryCheckActive.SplitGroup == value.InventoryCheckActive
                || InventoryCheckActive.NewBinGroup == value.InventoryCheckActive)
            {
                PersonalInventoryGroup personalInventoryGroup = new PersonalInventoryGroup
                {
                    InmateId = value.InventoryDetails.InmateId > 0 ? value.InventoryDetails.InmateId : null,
                    GroupNumber = _commonService.GetGlobalNumber(8), // get the global numbers 
                    GroupNote = value.InventoryDetails.PropertyGroupNotes,
                    CreateDate = DateTime.Now,
                    CreateBy = _personnelId,
                    LostFoundIdentified = false
                };

                _context.PersonalInventoryGroup.Add(personalInventoryGroup);

                PersonalInventoryGroupHistory personalInventoryGroupHistory = new PersonalInventoryGroupHistory
                {
                    PersonalInventoryGroupId = personalInventoryGroup.PersonalInventoryGroupId,
                    CreateDate = DateTime.Now,
                    CreateBy = _personnelId,
                    LostFoundIdentified = false,
                    GroupNote = value.InventoryDetails.PropertyGroupNotes
                };
                _context.PersonalInventoryGroupHistory.Add(personalInventoryGroupHistory);
                _context.SaveChanges();
                groupId = personalInventoryGroup.PersonalInventoryGroupId;
            }

            // move function
            foreach (InventoryItemDetails items in value.InventoryChangeGroupItemDetails)
            {
                PersonalInventory personalInventory = _context.PersonalInventory
                    .Single(i => i.PersonalInventoryId == items.PersonalInventoryId);
                personalInventory.PersonalInventoryBinId = items.PersonalInventoryBinId;
                personalInventory.InventoryBinNumber = items.PersonalBinName;
                personalInventory.UpdateDate = DateTime.Now;
                personalInventory.UpdatedBy = _personnelId;
                personalInventory.PersonalInventoryGroupId = groupId;
                _context.SaveChanges();
                InsertInventoryHistory(items.PersonalInventoryId);
            }

            InsertInventoryInmates(value.InventoryDetails.InmateId);
            return await _context.SaveChangesAsync();
        }

        // get the available bin and assigned bin numbers
        public BinInventoryVm AvailableBinItems(int facilityId)
        {
            BinInventoryVm lstBinItems = new BinInventoryVm();
            // get the bin inmate items list
            IQueryable<InventoryDetails> personalInmateCount = _context.PersonalInventory
                .Where(p => p.DeleteFlag == 0 &&
                            p.InventoryDispositionCode == (int?)Disposition.Storage &&
                            !p.InventoryReturnDate.HasValue && p.InmateId.HasValue)
                .Select(p => new
                {
                    p.PersonalInventoryBinId,
                    p.InmateId

                }).Distinct().Select(s => new InventoryDetails
                {
                    PersonalInventoryBinId = s.PersonalInventoryBinId,
                    InmateId = s.InmateId
                });

            // get the bin lost found items list
            IQueryable<InventoryDetails> personalLostFoundCount = _context.PersonalInventory
                .Where(p => (p.InmateId == 0 || !p.InmateId.HasValue) &&
                            p.InventoryDispositionCode == (int?)Disposition.Storage &&
                            p.DeleteFlag == 0)
                .Select(p => new
                {
                    p.PersonalInventoryId,
                    p.PersonalInventoryBinId

                }).Distinct().Select(s => new InventoryDetails
                {
                    PersonalInventoryId = s.PersonalInventoryId,
                    PersonalInventoryBinId = s.PersonalInventoryBinId,

                });

            //count items list for the available & assigned
            List<BinInmateDetails> lstBinCountDetails = _context.PersonalInventoryBin
                .Where(a => !a.InActiveFlag.HasValue || a.InActiveFlag == 0)
                .Select(a => new BinInmateDetails
                {
                    PersonalInventoryBinId = a.PersonalInventoryBinId,
                    PersonalBinName = a.BinName,
                    InmateCount = personalInmateCount.Count(
                            b => b.PersonalInventoryBinId == a.PersonalInventoryBinId && b.InmateId.HasValue),
                    LostFound = personalLostFoundCount.Count(d =>
                        d.PersonalInventoryBinId == a.PersonalInventoryBinId),
                    ReceivingFlag = a.ReceivingFlag,
                    FacilityTransferFlag = a.FacilityTransferFlag,
                    FacilityId = a.FacilityId
                }).ToList();

            //getting available inmate bin items
            lstBinItems.BinAvailableDetails = lstBinCountDetails
                .Where(p => p.InmateCount == 0 && (!p.ReceivingFlag.HasValue || p.ReceivingFlag == 0)
                            && p.FacilityId == facilityId)
                .Select(p => new BinViewerDetails
                {
                    PersonalInventoryBinId = p.PersonalInventoryBinId,
                    BinName = p.PersonalBinName,
                    BinInmateCount = p.InmateCount,
                    BinLostFound = p.LostFound,
                    FacilityId = p.FacilityId,
                    ReceivingFlag=p.ReceivingFlag,
                    FacilityTransferFlag=p.FacilityTransferFlag
                }).OrderBy(a => a.BinName).ToList();

            //getting assigned inmate bin items
            lstBinItems.BinAssignedeDetails = lstBinCountDetails.Where(p => p.InmateCount > 0
                && (!p.ReceivingFlag.HasValue || p.ReceivingFlag == 0) && p.FacilityId == facilityId)
                    .Select(p => new BinViewerDetails
                    {
                        PersonalInventoryBinId = p.PersonalInventoryBinId,
                        BinName = p.PersonalBinName,
                        BinInmateCount = p.InmateCount,
                        BinLostFound = p.LostFound,
                        FacilityId = p.FacilityId,
                        ReceivingFlag=p.ReceivingFlag,
                        FacilityTransferFlag=p.FacilityTransferFlag
                    }).OrderBy(a => a.BinName).ToList();

            //getting receive inmate bin items
            lstBinItems.BinReceivingDetails = lstBinCountDetails.Where(p => p.ReceivingFlag == 1
                                && p.FacilityId == facilityId)
                    .Select(p => new BinViewerDetails
                    {
                        PersonalInventoryBinId = p.PersonalInventoryBinId,
                        BinName = p.PersonalBinName,
                        BinInmateCount = p.InmateCount,
                        BinLostFound = p.LostFound,
                        FacilityId = p.FacilityId,
                        ReceivingFlag=p.ReceivingFlag,
                        FacilityTransferFlag=p.FacilityTransferFlag
                    }).OrderBy(a => a.BinName).ToList();

            //getting facility transfer inmate bin items
            lstBinItems.BinFacilityTransferDetails = lstBinCountDetails.Where(p => p.FacilityTransferFlag == 1)
                    .Select(p => new BinViewerDetails
                    {
                        PersonalInventoryBinId = p.PersonalInventoryBinId,
                        BinName = p.PersonalBinName,
                        BinInmateCount = p.InmateCount,
                        BinLostFound = p.LostFound,
                        ReceivingFlag=p.ReceivingFlag,
                        FacilityTransferFlag= p.FacilityTransferFlag
                    }).OrderBy(a => a.BinName).ToList();

            return lstBinItems;
        }

        // get the inmate details in bin group items click event
        public List<BinInmateLoadDetails> BinInmateDetails(int personalInventoryBinId)
        {
            List<BinInmateLoadDetails> lstBindListItems = _context.PersonalInventory
                .Where(i => !i.InventoryReturnDate.HasValue
                    && i.InventoryDispositionCode == (int?)Disposition.Storage && i.DeleteFlag == 0
                    && i.PersonalInventoryBinId == personalInventoryBinId && i.InmateId.HasValue)
                .Select(i => new
                {
                    i.PersonalInventoryBinId,
                    PersonalBinName = i.PersonalInventoryBin.BinName,
                    i.InmateId
                })
                .Distinct().Select(s => new BinInmateLoadDetails
                {
                    PersonalInventoryBinId = s.PersonalInventoryBinId.Value,
                    PersonalBinName = s.PersonalBinName,
                    InmateId = s.InmateId
                }).ToList();

            lstBindListItems.ForEach(item =>
            {
                //getting person name lists 
                if (item.InmateId.HasValue)
                {
                    item.PersonInfoDetails = _personService.GetInmateDetails(item.InmateId.Value);
                }

                if (item.PersonInfoDetails is null) return;
                if (item.PersonInfoDetails.HousingUnitId.HasValue && item.PersonInfoDetails.HousingUnitId>0)
                {
                    item.HousingDetails = _facilityHousingService
                        .GetHousingDetails(item.PersonInfoDetails.HousingUnitId.Value);
                }
            });

            return lstBindListItems;
        }

        //delete function loaded the grid and drop down reason
        public BinDeleteVm DeleteInventoryDetails() => new BinDeleteVm
            {
                ListInventoryLookUpDetails = _commonService.GetLookupKeyValuePairs(LookupConstants.INVDELREAS)
            };

        //update and Save in delete reason 
        public async Task<int> DeleteandUndoInventory(InventoryDetails obj)
        {
            PersonalInventory personalInventory = _context.PersonalInventory
                .Single(p => p.PersonalInventoryId == obj.PersonalInventoryId);

            personalInventory.DeleteFlag = obj.DeleteFlag;
            personalInventory.DeleteDate = obj.DeleteFlag == 1 ? DateTime.Now : (DateTime?)null;
            personalInventory.DeletedBy = _personnelId;

            if (obj.DeleteFlag == 1)
            {
                personalInventory.DeleteReason = obj.DeleteReason;
                personalInventory.DeleteReasonNote = obj.DeleteReasonNote;
                _context.SaveChanges();
                InsertInventoryHistory(obj.PersonalInventoryId);
                InsertInventoryInmates(obj.InmateId);
            }
            return await _context.SaveChangesAsync();
        }

        //load the inventory articles and inventory color in the inventory add items grid
        public InventoryLookupVm GetLookupDetails() => new InventoryLookupVm
            {
                ListInventoryArticle = _commonService.GetLookupKeyValuePairs(LookupConstants.INVARTCL),
                ListInventoryColor = _commonService.GetLookupKeyValuePairs(LookupConstants.INVCOLOR)
            };

        //PreBook Property - Select Items To Bring In As New Property 
        public List<PersonalInventoryPreBookVm> GetPreBookInventoryItem(int inmateId, int personalInventoryId)
        {
            List<PersonalInventoryPreBookVm> lstInmateprebook = new List<PersonalInventoryPreBookVm>();
            if (inmateId > 0)
            {
                int inmateActive = _context.Inmate.Single(a => a.InmateId == inmateId).InmateActive;

                int incarcerationId = inmateActive == 0 ? _context.Incarceration
                        .OrderByDescending(o => o.IncarcerationId).First(a => a.InmateId == inmateId).IncarcerationId
                    : _context.Incarceration
                        .Single(a => a.InmateId == inmateId && !a.ReleaseOut.HasValue).IncarcerationId;

                int personId = _context.Inmate.Single(a => a.InmateId == inmateId).PersonId;
                
                int? inmatePrebookId = _context.InmatePrebook
                    .FirstOrDefault(a => a.IncarcerationId == incarcerationId && a.PersonId == personId)?.InmatePrebookId;

                if (inmatePrebookId == null) return lstInmateprebook;
                lstInmateprebook = _context.PersonalInventoryPreBook
                    .Where(s => s.InmatePrebookId == inmatePrebookId)
                    .Select(s => new PersonalInventoryPreBookVm
                    {
                        PersonalInventoryPreBookId = s.PersonalInventoryPreBookId,
                        InmatePrebookId = s.InmatePrebookId,
                        InventoryArticles = s.InventoryArticles,
                        InventoryQuantity = s.InventoryQuantity,
                        InventoryColor = s.InventoryColor,
                        InventoryDescription = s.InventoryDescription,
                        ImportFlag = s.ImportFlag,
                        // its shows the Already Imported(yellow color) in the grid in client side
                        DeleteFlag = s.DeleteFlag,
                        IncarcerationId = s.InmatePrebook.IncarcerationId,
                        FacilityId = s.InmatePrebook.FacilityId
                    }).OrderBy(s => s.DeleteFlag).ThenBy(s => s.ImportFlag)
                    .ThenBy(s => s.InventoryArticles).ToList();
                // for showing client side order by ascending, in sql query, they are use three order by   m => new { m.CategoryID, m.Name }

                //get article description by lookup index
                if (lstInmateprebook.Count <= 0) return lstInmateprebook;
                int[] listLookupIds = lstInmateprebook.Select(s => s.InventoryArticles).ToArray();
                List<LookupVm> listLookupDetails = _context.Lookup.Where(w => 
                        listLookupIds.Contains(w.LookupIndex) && w.LookupType == LookupConstants.INVARTCL && w.LookupInactive == 0)
                    .Select(s => new LookupVm
                    {
                        LookupIndex = s.LookupIndex,
                        LookupDescription = s.LookupDescription
                    }).ToList();
                lstInmateprebook.ForEach(item =>
                {
                    if (listLookupDetails.Count > 0)
                    {
                        item.InventoryArticlesName = listLookupDetails.Single(w =>
                            w.LookupIndex == item.InventoryArticles).LookupDescription;
                    }
                });
                return lstInmateprebook;
            }

            if (personalInventoryId <= 0) return lstInmateprebook;
            lstInmateprebook = _context.PersonalInventory.Where(p => p.PersonalInventoryId == personalInventoryId)
                .Select(s => new PersonalInventoryPreBookVm
            {
                InventoryArticles = s.InventoryArticles,
                InventoryQuantity = s.InventoryQuantity,
                InventoryColor = s.InventoryColor,
                InventoryDescription = s.InventoryDescription
            }).ToList();
            return lstInmateprebook;
        }

        // Save and Update the Inventory Add Items 
        public async Task<int> InsertInventoryAddItems(PersonalInventoryPreBookVm value)
        {
            int groupId = value.InventoryDetails.PersonalInventoryGroupId;

            if (InventoryAddItems.UseExistingGroup == value.InventoryAddItems)
            {
                // update personal inventory group 
                PersonalInventoryGroup personalInvGroupItems = _context.PersonalInventoryGroup
                    .Single(a => a.PersonalInventoryGroupId == value.InventoryDetails.PersonalInventoryGroupId);

                personalInvGroupItems.InmateId = value.InventoryDetails.InmateId > 0 ? value.InventoryDetails.InmateId : null;
                personalInvGroupItems.GroupNumber = value.InventoryDetails.PersonalGroupName;
                personalInvGroupItems.GroupNote = value.InventoryDetails.PropertyGroupNotes;

            }
            PersonalInventoryGroup personalInventoryGroup = new PersonalInventoryGroup();
            if (InventoryAddItems.UseNewGroup == value.InventoryAddItems)
            {
                // insert personal inventory group
                personalInventoryGroup.InmateId = value.InventoryDetails.InmateId > 0 ? value.InventoryDetails.InmateId : null;
                personalInventoryGroup.GroupNumber =
                    _commonService.GetGlobalNumber((int)AtimsGlobalNumber.PropertyGroup); // get the global numbers 
                personalInventoryGroup.GroupNote = value.InventoryDetails.PropertyGroupNotes;
                personalInventoryGroup.CreateDate = DateTime.Now;
                personalInventoryGroup.CreateBy = _personnelId;
                personalInventoryGroup.LostFoundIdentified = false;
                if (value.InventoryDetails.LostFoundDate.HasValue)// insert inventory inmate 
                {
                    personalInventoryGroup.LostFoundDate = value.InventoryDetails.LostFoundDate;
                    personalInventoryGroup.LostFoundCircumstance = value.InventoryDetails.LostFoundCircumstance;
                    personalInventoryGroup.LostFoundByPersonnelId = value.InventoryDetails.LostFoundByPersonnelId > 0
                        ? value.InventoryDetails.LostFoundByPersonnelId : null;
                    personalInventoryGroup.LostFoundByInmateId = value.InventoryDetails.LostFoundByInmateId > 0
                        ? value.InventoryDetails.LostFoundByInmateId : null;
                    personalInventoryGroup.LostFoundLocFacilityId = value.InventoryDetails.LostFoundLocFacilityId > 0
                        ? value.InventoryDetails.LostFoundLocFacilityId : null;
                    personalInventoryGroup.LostFoundLocHousingUnitLocation = value.InventoryDetails.LostFoundLocHousingUnitLocation;
                    personalInventoryGroup.LostFoundLocHousingUnitNumber = value.InventoryDetails.LostFoundLocHousingUnitNumber;
                    personalInventoryGroup.LostFoundLocPrivilegeId = value.InventoryDetails.LostFoundLocPrivilegeId;
                    personalInventoryGroup.LostFoundByOther = value.InventoryDetails.LostFoundByOther;
                    personalInventoryGroup.LostFoundLocOther = value.InventoryDetails.LostFoundLocOther;
                }
                _context.PersonalInventoryGroup.Add(personalInventoryGroup);
                groupId = personalInventoryGroup.PersonalInventoryGroupId;

            }

            // insert personal inventory history
            PersonalInventoryGroupHistory personalInventoryGroupHistory = new PersonalInventoryGroupHistory
            {
                PersonalInventoryGroupId = groupId,
                CreateDate = DateTime.Now,
                CreateBy = _personnelId,
                LostFoundIdentified = false,
                GroupNote = value.InventoryDetails.PropertyGroupNotes
            };
            if (value.InventoryDetails.LostFoundDate.HasValue)
            {
                personalInventoryGroupHistory.LostFoundDate = value.InventoryDetails.LostFoundDate;
                personalInventoryGroupHistory.LostFoundCircumstance = value.InventoryDetails.LostFoundCircumstance;
                personalInventoryGroupHistory.LostFoundByPersonnelId = value.InventoryDetails.LostFoundByPersonnelId > 0 ?
                    value.InventoryDetails.LostFoundByPersonnelId : null;
                personalInventoryGroupHistory.LostFoundByInmateId = value.InventoryDetails.LostFoundByInmateId > 0 ?
                    value.InventoryDetails.LostFoundByInmateId : null;
                personalInventoryGroupHistory.LostFoundLocFacilityId = value.InventoryDetails.LostFoundLocFacilityId > 0
                    ? value.InventoryDetails.LostFoundLocFacilityId : null;
                personalInventoryGroupHistory.LostFoundLocHousingUnitLocation = value.InventoryDetails.LostFoundLocHousingUnitLocation;
                personalInventoryGroupHistory.LostFoundLocHousingUnitNumber = value.InventoryDetails.LostFoundLocHousingUnitNumber;
                personalInventoryGroupHistory.LostFoundLocPrivilegeId = value.InventoryDetails.LostFoundLocPrivilegeId;
                personalInventoryGroupHistory.LostFoundByOther = value.InventoryDetails.LostFoundByOther;
                personalInventoryGroupHistory.LostFoundLocOther = value.InventoryDetails.LostFoundLocOther;
            }
            _context.PersonalInventoryGroupHistory.Add(personalInventoryGroupHistory);

            // PreBook Property - Select Items To Bring In As New Property grid events
            foreach (PersonalInventoryAddItems items in value.PersonalInventoryAddItemsList)
            {
                // insert personal inventory
                PersonalInventory personalInventory = new PersonalInventory
                {
                    InmateId = items.InmateId > 0 ? items.InmateId : null,
                    InventoryArticles = items.InventoryAddItemsArticles,
                    InventoryQuantity = items.InventoryAddItemsQuantity,
                    InventoryDescription = items.InventoryAddItemsDescription,
                    InventoryDispositionCode = (int?)Disposition.Storage,
                    InventoryColor = items.InventoryAddItemsColor,
                    PersonalInventoryBinId = items.PersonalInventoryBinId,
                    CreateDate = DateTime.Now,
                    CreatedBy = _personnelId,
                    InventoryDate = DateTime.Now,
                    PersonalInventoryGroupId = InventoryAddItems.UseNewGroup == value.InventoryAddItems
                        ? personalInventoryGroup.PersonalInventoryGroupId : groupId,
                    InventoryBinNumber = items.PersonalBinName,
                    InventoryOfficerId = _personnelId
                };

                if (items.PersonalInventoryPrebookId > 0)
                {
                    UpdatePreBookFlag(items.PersonalInventoryPrebookId);
                }
                _context.PersonalInventory.Add(personalInventory);
                _context.SaveChanges();

                InsertInventoryHistory(personalInventory.PersonalInventoryId);
                InsertInventoryInmates(personalInventory.InmateId);
            }
            return await _context.SaveChangesAsync();
        }

        // update flag  details in inventory add items
        private void UpdatePreBookFlag(int personalInventoryPrebookId)
        {
            PersonalInventoryPreBook personalInvPreBook = _context.PersonalInventoryPreBook
                .Single(a => a.PersonalInventoryPreBookId == personalInventoryPrebookId);
            personalInvPreBook.ImportFlag = 1;
            personalInvPreBook.ImportDate = DateTime.Now;
            personalInvPreBook.ImportBy = _personnelId;
        }

        //update the property group notes entry in inventory grid click events
        public async Task<int> UpdatePropertyGroupNotesEntry(InventoryItemDetails value)
        {

            // update the table into personal inventory group
            PersonalInventoryGroup personalInventoryGroup = _context.PersonalInventoryGroup
                .Single(a => a.PersonalInventoryGroupId == value.PersonalInventoryGroupId);
            personalInventoryGroup.GroupNote = value.PropertyGroupNotes;
            personalInventoryGroup.CreateDate = DateTime.Now;
            personalInventoryGroup.CreateBy = _personnelId;
            // update the property group inventory lost found 
            if (value.LostFoundDate.HasValue)
            {
                personalInventoryGroup.LostFoundCircumstance = value.LostFoundCircumstance;
                personalInventoryGroup.LostFoundDate = value.LostFoundDate;
                personalInventoryGroup.LostFoundByPersonnelId = value.LostFoundByPersonnelId > 0 ? value.LostFoundByPersonnelId : null;
                personalInventoryGroup.LostFoundByInmateId = value.LostFoundByInmateId > 0 ? value.LostFoundByInmateId : null;
                personalInventoryGroup.LostFoundLocFacilityId = value.LostFoundLocFacilityId;
                personalInventoryGroup.LostFoundLocHousingUnitLocation = value.LostFoundLocHousingUnitLocation;
                personalInventoryGroup.LostFoundLocHousingUnitNumber = value.LostFoundLocHousingUnitNumber;
                personalInventoryGroup.LostFoundLocPrivilegeId = value.LostFoundLocPrivilegeId;
                personalInventoryGroup.LostFoundByOther = value.LostFoundByOther;
                personalInventoryGroup.LostFoundLocOther = value.LostFoundLocOther;
            }
            // insert personal inventory history
            PersonalInventoryGroupHistory personalInventoryGroupHistory = new PersonalInventoryGroupHistory
            {
                PersonalInventoryGroupId = value.PersonalInventoryGroupId,
                CreateDate = DateTime.Now,
                CreateBy = _personnelId,
                LostFoundIdentified = false,
                GroupNote = value.PropertyGroupNotes
            };
            //insert the property group inventory lost found
            if (value.LostFoundDate.HasValue)
            {
                personalInventoryGroupHistory.LostFoundDate = value.LostFoundDate;
                personalInventoryGroupHistory.LostFoundCircumstance = value.LostFoundCircumstance;
                personalInventoryGroupHistory.LostFoundByPersonnelId = value.LostFoundByPersonnelId > 0 ? value.LostFoundByPersonnelId : null;
                personalInventoryGroupHistory.LostFoundByInmateId = value.LostFoundByInmateId > 0 ? value.LostFoundByInmateId : null;
                personalInventoryGroupHistory.LostFoundLocFacilityId = value.LostFoundLocFacilityId;
                personalInventoryGroupHistory.LostFoundLocHousingUnitLocation = value.LostFoundLocHousingUnitLocation;
                personalInventoryGroupHistory.LostFoundLocHousingUnitNumber = value.LostFoundLocHousingUnitNumber;
                personalInventoryGroupHistory.LostFoundLocPrivilegeId = value.LostFoundLocPrivilegeId;
                personalInventoryGroupHistory.LostFoundByOther = value.LostFoundByOther;
                personalInventoryGroupHistory.LostFoundLocOther = value.LostFoundLocOther;
            }
            _context.PersonalInventoryGroupHistory.Add(personalInventoryGroupHistory);
            return await _context.SaveChangesAsync();
        }

        // Move Click Events Other Inmates Inventory in Bin 
        public MoveBinVm MoveBinInmateDetails(int inmateId, int inventoryBinId)
        {
            //commonly get the values of bin inmate
            IQueryable<MoveBinDetails> lstMoveItemsDetailsItems = _context.PersonalInventory
                .Where(s => s.InmateId != inmateId && s.PersonalInventoryBinId == inventoryBinId)
                .Select(s => new MoveBinDetails
                {
                    MoveBinName = s.PersonalInventoryBin.BinName,
                    MoveBinInventoryId = s.PersonalInventoryId,
                    MoveBinInventoryBinId = s.PersonalInventoryBinId,
                    MoveBinInmateId = s.InmateId,
                    MoveBinInventoryArticles = s.InventoryArticles,
                    MoveBinInventoryColor = s.InventoryColor,
                    MoveBinInventoryQuantity = s.InventoryQuantity,
                    MoveBinInventoryDescription = s.InventoryDescription,
                    MoveBinInventoryValue = s.InventoryValue,
                    MoveBinInmateNumber = s.Inmate.InmateNumber,
                    InventoryDispositionCode = s.InventoryDispositionCode,
                    InventoryReturnDate = s.InventoryReturnDate,
                    DeleteFlag = s.DeleteFlag
                });

            List<MoveBinDetails> lstMoveItemsDetails = lstMoveItemsDetailsItems
                .Where(s => s.InventoryDispositionCode == (int?)Disposition.Storage
                            && !s.InventoryReturnDate.HasValue && s.MoveBinInmateId.HasValue)
                .Select(s => new
                {
                    s.MoveBinInventoryBinId,
                    s.MoveBinInmateNumber,
                    s.MoveBinInmateId,
                    s.MoveBinName

                }).Distinct().Select(s => new MoveBinDetails
                {
                    MoveBinInventoryBinId = s.MoveBinInventoryBinId,
                    MoveBinInmateId = s.MoveBinInmateId,
                    MoveBinInmateNumber = s.MoveBinInmateNumber,
                    MoveBinName = s.MoveBinName
                }).ToList();

            //parent grid details
            lstMoveItemsDetails.ForEach(item =>
            {

                if (item.MoveBinInmateId.HasValue)
                {
                    PersonVm lstPersonDetails = _personService.GetInmateDetails(item.MoveBinInmateId.Value);
                    item.MoveBinFirstName = lstPersonDetails.PersonFirstName;
                    item.MoveBinLastName = lstPersonDetails.PersonLastName;
                    item.MoveBinMiddleName = lstPersonDetails.PersonMiddleName;
                }

                item.MoveBinItemCount = lstMoveItemsDetails
                    .Count(s => s.MoveBinInventoryId == item.MoveBinInventoryId);

            });

            // child grid details
            List<MoveBinDetails> lstMoveDetailsitems = lstMoveItemsDetailsItems
                .Where(s => s.InventoryDispositionCode != (int?)Disposition.ReleasedToPerson
                            && s.DeleteFlag == 0)
                .Select(s => new MoveBinDetails
                {
                    MoveBinInventoryId = s.MoveBinInventoryId,
                    MoveBinInmateId = s.MoveBinInmateId,
                    MoveBinInventoryArticles = s.MoveBinInventoryArticles,
                    MoveBinInventoryQuantity = s.MoveBinInventoryQuantity,
                    MoveBinInventoryValue = s.MoveBinInventoryValue,
                    MoveBinInventoryColor = s.MoveBinInventoryColor,
                    MoveBinInventoryDescription = s.MoveBinInventoryDescription,
                    InventoryDispositionCode = s.InventoryDispositionCode,
                    InventoryDispositionName = _commonService.GetLookupList(LookupConstants.INVDISP)
                        .SingleOrDefault(l => l.LookupIndex == (int)Disposition.Storage).LookupDescription
                }).ToList();

            MoveBinVm lstMoveBinsItems = new MoveBinVm
            {
                MoveBinDetails = lstMoveItemsDetails,
                MoveBinDetailsItems = lstMoveDetailsitems
            };

            return lstMoveBinsItems;
        }

        //save function for Move,Release,Keep,Donate,Evidence
        public async Task<int> InsertInventoryMove(MoveBinVm value)
        {
            foreach (MoveBinDetails item in value.MoveBinDetails)
            {
                PersonalInventory updatePersonalInventory = _context.PersonalInventory
                    .Single(a => a.PersonalInventoryId == item.MoveBinInventoryId);

                //TODO - it doesn't make sense!
                switch (value.InmateBinEvents)
                {
                    case InmateBinEvent.Move:

                        updatePersonalInventory.InventoryBinNumber = item.MoveBinName;
                        updatePersonalInventory.PersonalInventoryBinId = item.MoveBinInventoryBinId;
                        break;

                    case InmateBinEvent.Release:

                        int lookupReleaseId = _commonService.GetLookupList(LookupConstants.INVDISP)
                            .Single(l => l.LookupIndex == (int)Disposition.ReleasedToPerson).LookupIndex;

                        updatePersonalInventory.InventoryDispositionCode = lookupReleaseId;
                        break;

                    case InmateBinEvent.Mail:

                        int lookupMailId = _commonService.GetLookupList(LookupConstants.INVDISP)
                            .Single(l => l.LookupIndex == (int)Disposition.Mail).LookupIndex;

                        updatePersonalInventory.InventoryDispositionCode = lookupMailId;
                        break;

                    case InmateBinEvent.Keep:

                        int lookupKeepId = _commonService.GetLookupList(LookupConstants.INVDISP)
                            .Single(l => l.LookupIndex == (int)Disposition.KeptInPossesion).LookupIndex;

                        updatePersonalInventory.InventoryDispositionCode = lookupKeepId;
                        break;

                    case InmateBinEvent.Donate:

                        int lookupDonateId = _commonService.GetLookupList(LookupConstants.INVDISP)
                            .Single(l => l.LookupIndex == (int)Disposition.Donated).LookupIndex;

                        updatePersonalInventory.InventoryDispositionCode = lookupDonateId;
                        break;

                    case InmateBinEvent.Evidence:

                        int lookupEvidenceId = _commonService.GetLookupList(LookupConstants.INVDISP)
                            .Single(l => l.LookupIndex == (int)Disposition.Evidence).LookupIndex;

                        updatePersonalInventory.InventoryDispositionCode = lookupEvidenceId;
                        break;

                    case InmateBinEvent.Destroy:

                        int lookupDestroyId = _commonService.GetLookupList(LookupConstants.INVDISP)
                            .Single(l => l.LookupIndex == (int)Disposition.Destroy).LookupIndex;

                        updatePersonalInventory.InventoryDispositionCode = lookupDestroyId;
                        break;
                    case InmateBinEvent.Lost:

                        int lookuplostid = _commonService.GetLookupList(LookupConstants.INVDISP).
                            Single(l => l.LookupIndex == (int)Disposition.Lost).LookupIndex;
                        updatePersonalInventory.InventoryDispositionCode = lookuplostid;
                        updatePersonalInventory.InventoryReturnDate = DateTime.Now;
                        break;
                }

                if (value.ReleaseDetails != null) // if(value.ReleaseDetails is null) return; is not code tuning here because its converted to int is required
                {
                    updatePersonalInventory.PersonName = value.ReleaseDetails.PersonName;
                    updatePersonalInventory.PersonIdType = value.ReleaseDetails.PersonIdType;
                    updatePersonalInventory.PersonAddress = value.ReleaseDetails.PersonAddress;
                    updatePersonalInventory.CityStateZip = value.ReleaseDetails.CityStateZip;
                    updatePersonalInventory.DispoNotes = value.ReleaseDetails.DispoNotes;
                    updatePersonalInventory.InventoryReturnDate = value.ReleaseDetails.InventoryReturnDate;
                    updatePersonalInventory.InventoryEvidenceCaseNumber = value.ReleaseDetails.EvidenceCaseNo;
                    updatePersonalInventory.InventoryEvidenceAgencyId = value.ReleaseDetails.EvidenceAgencyId;
                    updatePersonalInventory.InventoryEvidencePersonnelId = value.ReleaseDetails.EvidencePersonnel;
                    updatePersonalInventory.InventoryDamageFlag = value.ReleaseDetails.InventoryDamageFlag;
                    updatePersonalInventory.InventoryDamageDescription = value.ReleaseDetails.InventoryDamageDescription;
                }

                updatePersonalInventory.UpdatedBy = _personnelId;
                updatePersonalInventory.UpdateDate = DateTime.Now;
                _context.SaveChanges();
                _interfaceEngineService.Export(new ExportRequestVm
                {
                    EventName = EventNameConstants.INVENTORYDISPOSITION,
                    PersonnelId = _personnelId,
                    Param1 = (_context.Inmate
                    .SingleOrDefault(a => a.InmateId == updatePersonalInventory.InmateId)?.PersonId ?? 0).ToString(),
                    Param2 = updatePersonalInventory.PersonalInventoryId.ToString()
                });

                //common inventory history inserted
                InsertInventoryHistory(item.MoveBinInventoryId);
                InsertInventoryInmates(item.MoveBinInmateId);
            }

            return await _context.SaveChangesAsync();
        }

        // get the Inmate Release Items details
        public InventoryVm GetReleaseItems(int inmateId, Disposition disposition, int showDeleteFlag)
        {
            List<Lookup> lstLookupId = _commonService.GetLookupList(LookupConstants.INVDISP)
                .Where(a => a.LookupDescription != LookupConstants.STORAGE)
                .Select(a => new Lookup
                {
                    LookupIndex = a.LookupIndex,
                    LookupDescription = a.LookupDescription
                }).ToList();

            IQueryable<PersonalInventory> personalInventoryDate = from s in _context.PersonalInventory
                where s.InmateId == inmateId
                select new PersonalInventory
                {
                    InventoryReturnDate = s.InventoryReturnDate,
                    PersonalInventoryBin = s.PersonalInventoryBin
                };

            var inv = _context.PersonalInventory
                .Where(p => p.DeleteFlag == 0 &&
                            p.InmateId == inmateId &&
                            p.InventoryDispositionCode != (int?) Disposition.Storage)
                .AsEnumerable().GroupBy(p => new
                {
                    p.InmateId,
                    p.InventoryDispositionCode,
                    p.InventoryReturnDate,
                    p.PersonName,
                    p.PersonIdType,
                    p.CityStateZip,
                    p.PersonAddress,
                    p.DispoNotes,
                    p.UpdatedBy
                });

            List<MoveBinDetails> lstInmateReleaseDetails = inv.Select(s => new MoveBinDetails
                {
                    MoveBinInmateId = s.Key.InmateId,
                    InventoryReturnDate = s.Key.InventoryReturnDate,
                    InventoryDispositionCode = s.Key.InventoryDispositionCode,
                    PersonName = s.Key.PersonName,
                    PersonIdType = s.Key.PersonIdType,
                    CityStateZip = s.Key.CityStateZip,
                    PersonAddress = s.Key.PersonAddress,
                    DispoNotes = s.Key.DispoNotes,
                    UpdatedBy = s.Key.UpdatedBy ?? 0,
                    ListPersonalInventoryId = s.Select(w=>w.PersonalInventoryId).ToList()
                }).ToList();

            lstInmateReleaseDetails.ForEach(item =>
            {
                item.MoveBinItemCount = personalInventoryDate
                         .Count(s => s.InventoryReturnDate == item.InventoryReturnDate);
                item.InventoryDispositionName = lstLookupId.Single(s => s.LookupIndex == item.InventoryDispositionCode).LookupDescription;
                item.ReleasedByDetails = _context.Personnel.Where(per => per.PersonnelId == item.UpdatedBy)
                    .Select(per => new PersonnelVm
                    {
                        PersonLastName = per.PersonNavigation.PersonLastName,
                        OfficerBadgeNumber = per.OfficerBadgeNum,
                    }).SingleOrDefault();
            });

            InventoryVm lstitems = GetInventoryGrid(inmateId, disposition, showDeleteFlag);
            lstitems.ReleaseBinDetails = lstInmateReleaseDetails;

            return lstitems;
        }

        // COMMON METHODS IN INVENTORY START//

        // common methods used for invenotry sub- grid loaded
        private List<InventoryItemDetails> InventoryDetails(int inmateId, Disposition disposition)
        {
            //sub grid for inventory 
            List<InventoryItemDetails> lstInventory = _context.PersonalInventory.Where(s => s.InmateId == inmateId
                                && (Disposition.Storage == disposition
                                    ? !s.InventoryReturnDate.HasValue &&
                                      s.InventoryDispositionCode == (int?)Disposition.Storage
                                    : s.InventoryReturnDate.HasValue))
                    .Select(s => new InventoryItemDetails
                    {
                        InmateId = s.InmateId,
                        InventoryQuantity = s.InventoryQuantity,
                        InventoryArticles = s.InventoryArticles,
                        InventoryColor = s.InventoryColor,
                        InventoryValue = s.InventoryValue,
                        InventoryDescription = s.InventoryDescription,
                        PersonalInventoryId = s.PersonalInventoryId,
                        PersonalInventoryBinId = s.PersonalInventoryBinId,
                        PersonalInventoryGroupId = s.PersonalInventoryGroupId,
                        PersonalBinName = s.PersonalInventoryBin.BinName,
                        PersonalInventoryGroupNumber = s.PersonalInventoryGroup.GroupNumber,
                        DeleteFlag = s.DeleteFlag,
                        InventoryDispositionCode = s.InventoryDispositionCode,
                        PersonName = s.PersonName,
                        PersonAddress = s.PersonAddress,
                        PersonIdType = s.PersonIdType,
                        CityStateZip = s.CityStateZip,
                        InventoryEvidenceCaseNumber = s.InventoryEvidenceCaseNumber,
                        InventoryEvidenceAgencyId = s.InventoryEvidenceAgencyId,
                        InventoryEvidencePersonnelId = s.InventoryEvidencePersonnelId,
                        InventoryReturnDate = s.InventoryReturnDate,
                        DispoNotes = s.DispoNotes,
                        UpdateDate = s.UpdateDate,
                        CreatedDate = s.CreateDate,
                        InventoryDamageFlag = s.InventoryDamageFlag,
                        InventoryDamageDescription = s.InventoryDamageDescription,
                        InventoryMisplacedFlag = s.InventoryMisplacedFlag,
                        InventoryMisplacedNote = s.InventoryMisplacedNote,
                        ReceivingFlag=s.PersonalInventoryBin.ReceivingFlag,
                        FacilityTransferFlag=s.PersonalInventoryBin.FacilityTransferFlag,
                    }).ToList();

            return lstInventory;
        }

        //common methods for adding the personal inventory history for every save and update
        private void InsertInventoryHistory(int personalInventoryId)
        {
            // select the inventory details 
            PersonalInventoryHistory personalInventoryHistory = _context.PersonalInventory
                .Where(h => h.PersonalInventoryId == personalInventoryId)
                .Select(h => new PersonalInventoryHistory
                {
                    PersonalInventoryId = h.PersonalInventoryId,
                    InmateId = h.InmateId,
                    InventoryDate = h.InventoryDate,
                    InventoryArticles = h.InventoryArticles,
                    InventoryQuantity = h.InventoryQuantity,
                    InventoryUom = h.InventoryUom,
                    InventoryDescription = h.InventoryDescription,
                    InventoryDispositionCode = h.InventoryDispositionCode,
                    InventoryValue = h.InventoryValue,
                    InventoryDestroyed = h.InventoryDestroyed,
                    InventoryMailed = h.InventoryMailed,
                    InventoryMailPersonId = h.InventoryMailPersonId,
                    InventoryMailAddressId = h.InventoryMailAddressId,
                    InventoryOfficerId = h.InventoryOfficerId,
                    InventoryColor = h.InventoryColor,
                    InventoryBinNumber = h.InventoryBinNumber,
                    InventoryReturnDate = h.InventoryReturnDate,
                    CreateDate = h.CreateDate,
                    UpdateDate = h.UpdateDate,
                    PersonalInventoryBinId = h.PersonalInventoryBinId ?? 0,
                    PersonName = h.PersonName,
                    PersonIdType = h.PersonIdType,
                    PersonAddress = h.PersonAddress,
                    DispoNotes = h.DispoNotes,
                    CreatedBy = h.CreatedBy,
                    UpdatedBy = h.UpdatedBy,
                    DeletedBy = h.DeletedBy,
                    DeleteDate = h.DeleteDate,
                    DeleteReason = h.DeleteReason,
                    DeleteFlag = h.DeleteFlag,
                    CityStateZip = h.CityStateZip,
                    PersonalInventoryGroupId = h.PersonalInventoryGroupId,
                    DeleteReasonNote = h.DeleteReasonNote,
                    InventoryEvidencePersonnelId = h.InventoryEvidencePersonnelId,
                    InventoryEvidenceAgencyId = h.InventoryEvidenceAgencyId,
                    InventoryEvidenceCaseNumber = h.InventoryEvidenceCaseNumber,
                    InventoryDamageFlag = h.InventoryDamageFlag,
                    InventoryDamageDescription = h.InventoryDamageDescription
                }).SingleOrDefault();

            // insert the inventory details into another new table
            if (personalInventoryHistory != null) _context.PersonalInventoryHistory.Add(personalInventoryHistory);
        }

        //common methods for adding the Inmate for every save and update
        private void InsertInventoryInmates(int? inmateId)
        {
            if (!inmateId.HasValue) return;
            string[] lstBinNumber = _context.PersonalInventory.Where(n => n.InmateId == inmateId &&
                        n.InventoryDispositionCode == (int?)Disposition.Storage && n.DeleteFlag == 0)
                    .GroupBy(n => n.PersonalInventoryBin.BinName)
                    .Select(n => n.Key).ToArray();

            int countBinNumber = lstBinNumber.Length;

            Inmate updateItems = _context.Inmate.Find(inmateId);
            updateItems.InmatePersonalInventory = countBinNumber > 0 ? string.Join(",", lstBinNumber) : null;
        }

        public PersonAddressVm ReleaseItemAddressDetails(int personId) => _context.Address
            .Where(a => a.PersonId == personId && a.AddressType == PersonConstants.RESIDENCE)
            .Select(a => new PersonAddressVm
            {
                City = a.AddressCity,
                Direction = a.AddressDirection,
                DirectionSuffix = a.AddressDirectionSuffix,
                Line2 = a.AddressLine2,
                Number = a.AddressNumber,
                State = a.AddressState,
                Street = a.AddressStreet,
                Suffix = a.AddressSuffix,
                UnitNo = a.AddressUnitNumber,
                UnitType = a.AddressUnitType,
                Zip = a.AddressZip
            }).FirstOrDefault();

        public InventoryInStorage GetInventoryInStorage(InventoryVm inventoryDetails, bool isRelease)
        {
            InventoryInStorage inventoryInStorage = new InventoryInStorage();
            string bookingNo = "";
            int inmateId = inventoryDetails.InventoryDetails[0].InmateId ?? 0;

            List<int> inventoryIds = inventoryDetails.InventoryDetails.Select(s => s.PersonalInventoryId).ToList();
            DateTime? createdDate = _context.PersonalInventory
                .Where(w => inventoryIds.Contains(w.PersonalInventoryId) && w.DeleteFlag == 0)
                .OrderByDescending(o => o.CreateDate)
                .Select(s => s.CreateDate).FirstOrDefault();

            if (createdDate != null)
            {
                inventoryInStorage.IncarcerationId = _context.Incarceration
                    .Where(w => w.InmateId == inmateId && w.DateIn <= createdDate).OrderByDescending(o => o.DateIn)
                    .Select(s => s.IncarcerationId).FirstOrDefault();

                bookingNo = _context.IncarcerationArrestXref.Where(w => w.IncarcerationId == inventoryInStorage.IncarcerationId
                ).OrderBy(o => o.Arrest.ArrestBookingNo).Select(s => s.Arrest.ArrestBookingNo).FirstOrDefault();
            }

            FormRecord formRecDetails = isRelease ? _context.FormRecord.Where(fr =>
                fr.PropReleaseInmateId == inmateId && 
                fr.PropReleaseDate == inventoryDetails.ReleaseDetails.InventoryReturnDate && fr.DeleteFlag != 1)
                .OrderByDescending(de => de.FormRecordId).FirstOrDefault()
                : _context.FormRecord.Where(fr => fr.PropertyIncarcerationId == inventoryInStorage.IncarcerationId
                && fr.DeleteFlag != 1).OrderByDescending(de => de.FormRecordId).FirstOrDefault();

            if (formRecDetails != null)
            {
                inventoryInStorage = JsonConvert.DeserializeObject<InventoryInStorage>(formRecDetails.XmlData);
                inventoryInStorage.FormData = new Form
                {
                    FormTemplateId = formRecDetails.FormTemplatesId,
                    FormRecordId = formRecDetails.FormRecordId,
                    //Values = formRecordId == 0 ? spModel : xmlModel,
                    SignValues = _formsService.GetSignature(formRecDetails.FormRecordId, formRecDetails.FormTemplatesId)
                };
            }
            else
            {
                PersonnelVm personnelDetail = _context.Personnel.Where(w => w.PersonnelId == _personnelId)
                    .Select(s => new PersonnelVm
                    {
                        PersonLastName = s.PersonNavigation.PersonLastName,
                        OfficerBadgeNumber = s.OfficerBadgeNum
                    }).SingleOrDefault();

                inventoryInStorage.InmateHeaderDetails = new InmatePdfHeader
                {
                    AgencyName = _context.Agency.FirstOrDefault(w => w.AgencyBookingFlag)?.AgencyName,
                    StampDate = DateTime.Now,
                    PersonnelNumber = personnelDetail?.OfficerBadgeNumber,
                    OfficerName = personnelDetail?.PersonLastName,
                    SummaryHeader = isRelease ? InventoryQueueConstants.PROPERTYRELEASESHEET : InventoryQueueConstants.INVENTORYINSTORAGE

                };

                inventoryInStorage.PersonDetails = _context.Inmate.Where(w => w.InmateId == inmateId).Select(s =>
                    new InventoryReceiptPersonDetails
                    {
                        LastName = s.Person.PersonLastName,
                        FirstName = s.Person.PersonFirstName,
                        MiddleName = s.Person.PersonMiddleName,
                        InmateNumber = s.InmateNumber,
                        Dob = s.Person.PersonDob,
                        Age = _commonService.GetAgeFromDob(s.Person.PersonDob),
                        Gender = _context.Lookup.Where(w =>
                                w.LookupType == LookupConstants.SEX && w.LookupIndex == s.Person.PersonSexLast)
                            .Select(lk => lk.LookupDescription).SingleOrDefault(),
                        Race = _context.Lookup.Where(w =>
                                w.LookupType == LookupConstants.RACE && w.LookupIndex == s.Person.PersonRaceLast)
                            .Select(lk => lk.LookupDescription).SingleOrDefault(),
                        Balance = _context.AccountAoInmate.Where(w => w.InmateId == inmateId).Select(lk => lk.BalanceInmate).SingleOrDefault(),
                        BookingNumber = bookingNo
                    }).LastOrDefault();

                inventoryInStorage.InventoryDetails = inventoryDetails.InventoryDetails;
                inventoryInStorage.ReleaseDetails = inventoryDetails.ReleaseDetails;

				inventoryInStorage.FormData = new Form
                {
					FormTemplateId = _context.FormTemplates.FirstOrDefault(ft => 
					ft.FormCategoryId == (isRelease ? (int?)FormCategories.PropertyReleaseSheet : 
					(int?)FormCategories.PropertyIncustodySheet) && ft.Inactive != 1)?.FormTemplatesId ?? 0,

                    SignValues = new Signature()
                };
            }
            inventoryInStorage.IsReleased = isRelease;
            return inventoryInStorage;
        }

        public async Task<int> InsertPropertyPhoto(PersonPhoto personPhoto)
        {
            Identifiers dbIdentifiers = new Identifiers
            {
                PersonalInventoryId = personPhoto.PersonalInventoryId,
                IdentifierType = personPhoto.PhotoType,
                PhotographDate = personPhoto.PhotographDate,
                PhotographTakenBy = _personnelId,
                IdentifierDescription = personPhoto.IdentifierDescription,
                IdentifierNarrative = personPhoto.NarrativeText,
                IdentifierLocation = personPhoto.LocationText
            };

            _context.Add(dbIdentifiers);
            await _context.SaveChangesAsync();
            int identifierId = dbIdentifiers.IdentifierId;

            string savepath = $@"{_configuration.GetSection(PathConstants.SITEVARIABLES)[PathConstants.PHOTOPATH]}";
            string dbsavepath = $@"\{PathConstants.IDENTIFIERS}{PathConstants.PROPERTY}{PathConstants.BACKWARDSLASH}";
            string datePath = $@"{DateTime.Now.ToString(PathConstants.DATEPATH)}{PathConstants.BACKWARDSLASH}";

            string[] img = personPhoto.TempPhotoPath.Split("\\IDENTIFIERS");
            personPhoto.TempPhotoPath = savepath + $@"{PathConstants.BACKWARDSLASH}{PathConstants.IDENTIFIERS}" + img[1];

            dbsavepath += datePath;

            dbIdentifiers.PhotographRelativePath = dbsavepath + identifierId.ToString().PadLeft(5, '0') + PathConstants.JPGPATH;

            savepath += $@"{dbsavepath}";

            if (!Directory.Exists(savepath))
            {
                Directory.CreateDirectory(savepath);
            }
            savepath += identifierId.ToString().PadLeft(5, '0') + PathConstants.JPGPATH;
            File.Move(personPhoto.TempPhotoPath, savepath);
            return await _context.SaveChangesAsync();
        }

        public async Task<int> DeletePropertyPhoto(int identifiersId)
        {
            Identifiers dbIdentifiers = _context.Identifiers.SingleOrDefault(w => w.IdentifierId == identifiersId);
            if (dbIdentifiers == null) return -1;
            dbIdentifiers.DeleteBy = _personnelId;
            dbIdentifiers.DeleteDate = DateTime.Now;
            dbIdentifiers.DeleteFlag = 1;
            return await _context.SaveChangesAsync();
        }

        public InventoryInStorage GetPropertyGroupDetails(InventoryDetails inventoryDetails)
        {
            InventoryInStorage inventoryInStorage = new InventoryInStorage();
            int inmateId = inventoryDetails.InmateId ?? 0;

            PersonnelVm personnelDetail = _context.Personnel.Where(w => w.PersonnelId == _personnelId)
                .Select(s => new PersonnelVm
                {
                    PersonLastName = s.PersonNavigation.PersonLastName,
                    OfficerBadgeNumber = s.OfficerBadgeNum
                }).SingleOrDefault();

            inventoryInStorage.InmateHeaderDetails = new InmatePdfHeader
            {
                AgencyName = _context.Agency.FirstOrDefault(w => w.AgencyBookingFlag)?.AgencyName,
                PersonnelNumber = personnelDetail?.OfficerBadgeNumber,
                OfficerName = personnelDetail?.PersonLastName,
                SummaryHeader = InventoryQueueConstants.PROPERTYGROUP

            };

            inventoryInStorage.PersonDetails = _context.Inmate.Where(w => w.InmateId == inmateId).Select(s =>
                new InventoryReceiptPersonDetails
                {
                    LastName = s.Person.PersonLastName,
                    FirstName = s.Person.PersonFirstName,
                    MiddleName = s.Person.PersonMiddleName,
                    InmateNumber = s.InmateNumber,
                    PersonId = s.PersonId
                }).Single();

            IdentifierVm identifier = _context.Identifiers
                .Where(i => i.PersonId == inventoryInStorage.PersonDetails.PersonId &&
                            i.IdentifierType == "1" && i.DeleteFlag == 0)
                .Select(i => new IdentifierVm
                {
                    PhotoGraphPathAbsolute = i.PhotographPathAbsolute,
                    PhotoGraphPath = i.PhotographPath,
                    PhotographRelativePath = i.PhotographRelativePath,
                    IdentifierId = i.IdentifierId,
                    PersonId = i.PersonId ?? 0
                }).LastOrDefault();

            if (identifier != null)
            {
                inventoryInStorage.PersonDetails.PropertyGroupPhotoPath = $@"{
                        (identifier.PhotoGraphPathAbsolute ?? false
                            ? _externalPath + identifier.PhotoGraphPath
                            : _path + identifier.PhotographRelativePath)
                    }";
            }
            inventoryInStorage.PropertyGroupDetails = inventoryDetails;
            return inventoryInStorage;
        }

        public async Task<int> InsertInventoryMisplacedValues(InventoryDetails inventoryDetails)
        {  
            PersonalInventory personalInventoryMisplaced = _context.PersonalInventory.Single(s =>
            s.PersonalInventoryId == inventoryDetails.PersonalInventoryId);
            personalInventoryMisplaced.InventoryMisplacedFlag = inventoryDetails.InventoryMisplacedFlag;
            personalInventoryMisplaced.InventoryMisplacedNote = inventoryDetails.InventoryMisplacedNote;            

            // select the inventory details 
            PersonalInventoryHistory personalInventoryHistory = _context.PersonalInventory
                .Where(h => h.PersonalInventoryId == inventoryDetails.PersonalInventoryId)
                .Select(h => new PersonalInventoryHistory
                {
                    PersonalInventoryId = h.PersonalInventoryId,
                    InmateId = h.InmateId,
                    InventoryDate = h.InventoryDate,
                    InventoryArticles = h.InventoryArticles,
                    InventoryQuantity = h.InventoryQuantity,
                    InventoryUom = h.InventoryUom,
                    InventoryDescription = h.InventoryDescription,
                    InventoryDispositionCode = h.InventoryDispositionCode,
                    InventoryValue = h.InventoryValue,
                    InventoryDestroyed = h.InventoryDestroyed,
                    InventoryMailed = h.InventoryMailed,
                    InventoryMailPersonId = h.InventoryMailPersonId,
                    InventoryMailAddressId = h.InventoryMailAddressId,
                    InventoryOfficerId = h.InventoryOfficerId,
                    InventoryColor = h.InventoryColor,
                    InventoryBinNumber = h.InventoryBinNumber,
                    InventoryReturnDate = h.InventoryReturnDate,
                    CreateDate = h.CreateDate,
                    UpdateDate = h.UpdateDate,
                    PersonalInventoryBinId = h.PersonalInventoryBinId ?? 0,
                    PersonName = h.PersonName,
                    PersonIdType = h.PersonIdType,
                    PersonAddress = h.PersonAddress,
                    DispoNotes = h.DispoNotes,
                    CreatedBy = h.CreatedBy,
                    UpdatedBy = h.UpdatedBy,
                    DeletedBy = h.DeletedBy,
                    DeleteDate = h.DeleteDate,
                    DeleteReason = h.DeleteReason,
                    DeleteFlag = h.DeleteFlag,
                    CityStateZip = h.CityStateZip,
                    PersonalInventoryGroupId = h.PersonalInventoryGroupId,
                    DeleteReasonNote = h.DeleteReasonNote,
                    InventoryEvidencePersonnelId = h.InventoryEvidencePersonnelId,
                    InventoryEvidenceAgencyId = h.InventoryEvidenceAgencyId,
                    InventoryEvidenceCaseNumber = h.InventoryEvidenceCaseNumber,
                    InventoryDamageFlag = h.InventoryDamageFlag,
                    InventoryDamageDescription = h.InventoryDamageDescription,
                    InventoryMisplacedFlag = inventoryDetails.InventoryMisplacedFlag,
                    InventoryMisplacedNote = inventoryDetails.InventoryMisplacedNote
                }).SingleOrDefault();

            // insert the inventory details into another new table
            if (personalInventoryHistory != null) _context.PersonalInventoryHistory.Add(personalInventoryHistory);

            return await _context.SaveChangesAsync();
        }        
    }
}
