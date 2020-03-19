using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GenerateTables.Models;
using ServerAPI.ViewModels;
using ServerAPI.Utilities;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using ServerAPI.Policies;

namespace ServerAPI.Services
{
    public class AlertService : IAlertService
    {
        private readonly AAtims _context;
        private readonly int _personnelId;

        private readonly ICommonService _commonService;
        private readonly IPersonService _personService;
        private readonly IInterfaceEngineService _interfaceEngine;

        // flag alert object is used in GetFlagAlertHistoryByPerson methods
        List<LookupVm> _staticFlagLst = new List<LookupVm>();
        private readonly IUserPermissionPolicy _userPermissionPolicy;

        public AlertService(AAtims context, IHttpContextAccessor httpContextAccessor, ICommonService commonService,
            IPersonService personService, IInterfaceEngineService interfaceEngineService,
            IUserPermissionPolicy userPermissionPolicy)
        {
            _context = context;
            _personnelId =
                Convert.ToInt32(httpContextAccessor.HttpContext.User.FindFirst(AuthDetailConstants.PERSONNELID)?.Value);
            _commonService = commonService;
            _personService = personService;
            _interfaceEngine = interfaceEngineService;
            _userPermissionPolicy = userPermissionPolicy;
        }

        #region Message Alert

        // Insert or Update Person Message Alert Details
        public async Task<bool> InsertUpdateMessageAlertDetails(PersonAlertVm personAlertDetails)
        {
            if (_personService.IsPersonSealed(personAlertDetails.PersonId)) //Validate person is sealed or not
            {
                return false;
            }

            bool isExist = false;
            PersonAlert dbPersonAlertDetails = _context.PersonAlert
                .SingleOrDefault(pa => pa.PersonAlertId == personAlertDetails.AlertId);

            if (dbPersonAlertDetails?.PersonAlertId > 0)
            {
                isExist = true;
            }
            else
            {
                dbPersonAlertDetails = new PersonAlert
                {
                    CreateDate = DateTime.Now,
                    CreatedBy = _personnelId
                };
            }

            dbPersonAlertDetails.PersonId = personAlertDetails.PersonId;
            dbPersonAlertDetails.Alert = personAlertDetails.AlertMessage;
            dbPersonAlertDetails.ActiveAlertFlag = personAlertDetails.ActiveAlertFlag ? 1 : 0;
            dbPersonAlertDetails.ExpireDate = personAlertDetails.ExpireDate;
            dbPersonAlertDetails.UpdateDate = DateTime.Now;
            dbPersonAlertDetails.UpdateBy = _personnelId;

            if (!isExist)
            {
                _context.PersonAlert.Add(dbPersonAlertDetails);
            }

            await _context.SaveChangesAsync();

            //Insert Person Alert History details
            personAlertDetails.AlertId = dbPersonAlertDetails.PersonAlertId;
            await InsertMessageAlertHistory(personAlertDetails);
            return true;
        }

        // Insert Message Alert History Details
        private async Task InsertMessageAlertHistory(PersonAlertVm personAlertDetails)
        {

            PersonAlertHistory dbPersonAlertHistory = new PersonAlertHistory
            {
                PersonnelId = _personnelId,
                PersonAlertId = personAlertDetails.AlertId,
                PersonAlertHistoryList = personAlertDetails.HistoryValue,
                CreateDate = DateTime.Now
            };
            _context.PersonAlertHistory.Add(dbPersonAlertHistory);

            await _context.SaveChangesAsync();
        }

        // Get Message Alert Details List
        public List<PersonAlertVm> GetMessageAlertDetailLst(int personId)
        {
            return _context.PersonAlert
                .Where(pa => pa.PersonId == personId)
                .Select(pad => new PersonAlertVm
                {
                    AlertId = pad.PersonAlertId,
                    AlertMessage = pad.Alert,
                    ActiveAlertFlag = pad.ActiveAlertFlag == 1 &&
                        (!pad.ExpireDate.HasValue || pad.ExpireDate.Value.Date >= DateTime.Now.Date),
                    ExpireDate = pad.ExpireDate
                }).ToList();
        }

        // Get Message Alert History List
        public List<HistoryVm> GetMessageAlertHistoryLst(int alertId)
        {
            List<HistoryVm> alertHistoryLst =
                _context.PersonAlertHistory
                    .Where(pa => pa.PersonAlertId == alertId)
                    .Select(pad => new HistoryVm
                    {
                        PersonId = pad.Personnel.PersonId,
                        HistoryId = pad.PersonAlertHistoryId,
                        OfficerBadgeNumber = pad.Personnel.OfficerBadgeNum,
                        CreateDate = pad.CreateDate,
                        HistoryList = pad.PersonAlertHistoryList
                    }).OrderByDescending(de => de.HistoryId).ToList();

            if (alertHistoryLst.Any())
            {
                //Get person details list
                int[] personIds = alertHistoryLst.Select(p => p.PersonId).ToArray();

                List<Person> lstPersonDet = (from per in _context.Person
                                             where personIds.Contains(per.PersonId)
                                             select new Person
                                             {
                                                 PersonId = per.PersonId,
                                                 PersonLastName = per.PersonLastName
                                             }).ToList();

                alertHistoryLst.ForEach(item =>
                {
                    item.PersonLastName =
                        lstPersonDet.SingleOrDefault(p => p.PersonId == item.PersonId)?.PersonLastName;
                    //To GetJson Result Into Dictionary
                    if (!string.IsNullOrEmpty(item.HistoryList))
                    {
                        Dictionary<string, string> personHistoryList =
                            JsonConvert.DeserializeObject<Dictionary<string, string>>(item.HistoryList);
                        item.Header =
                            personHistoryList.Select(ph => new PersonHeader { Header = ph.Key, Detail = ph.Value })
                                .ToList();
                    }
                });
            }

            return alertHistoryLst;
        }

        #endregion

        #region Flag Alert

        // Insert/Update/Delete Flag Alert Details
        public async Task<int> InsertUpdateFlagAlert(FlagAlertVm flagAlertDetails)
        {
            bool isExist = false, isNew = false;
            string EventName = "";

            PersonFlag dbPersonFlag =
                _context.PersonFlag.SingleOrDefault(pfg => pfg.PersonId == flagAlertDetails.PersonId
                && (flagAlertDetails.PersonFlagIndex > 0 && pfg.PersonFlagIndex == flagAlertDetails.PersonFlagIndex
                || flagAlertDetails.InmateFlagIndex > 0 && pfg.InmateFlagIndex == flagAlertDetails.InmateFlagIndex
                || flagAlertDetails.DietFlagIndex > 0 && pfg.DietFlagIndex == flagAlertDetails.DietFlagIndex
                || flagAlertDetails.MedicalFlagIndex > 0 && pfg.MedicalFlagIndex == flagAlertDetails.MedicalFlagIndex
                || flagAlertDetails.PREAFlagIndex > 0 && pfg.PREAFlagIndex == flagAlertDetails.PREAFlagIndex));
            if (dbPersonFlag != null)
            {
                isExist = true;
                if (dbPersonFlag.DeleteFlag > 0)
                {
                    isNew = true;
                }

                dbPersonFlag.DeleteBy = flagAlertDetails.DeleteFlag ? _personnelId : (int?)null;
                dbPersonFlag.DeleteFlag = flagAlertDetails.DeleteFlag ? 1 : 0;
                dbPersonFlag.DeleteDate = flagAlertDetails.DeleteFlag ? DateTime.Now : new DateTime?();
            }
            else
            {
                isNew = true;
                dbPersonFlag = new PersonFlag
                {
                    CreateDate = DateTime.Now,
                    CreateBy = _personnelId
                };
            }

            dbPersonFlag.PersonId = flagAlertDetails.PersonId;
            dbPersonFlag.PersonFlagIndex = flagAlertDetails.PersonFlagIndex;
            dbPersonFlag.InmateFlagIndex = flagAlertDetails.InmateFlagIndex;
            dbPersonFlag.DietFlagIndex = flagAlertDetails.DietFlagIndex;
            dbPersonFlag.MedicalFlagIndex = flagAlertDetails.MedicalFlagIndex;
            dbPersonFlag.PREAFlagIndex = flagAlertDetails.PREAFlagIndex;
            dbPersonFlag.UpdateBy = _personnelId;
            dbPersonFlag.UpdateDate = DateTime.Now;
            dbPersonFlag.FlagNote = flagAlertDetails.FlagNote;
            dbPersonFlag.FlagExpire = flagAlertDetails.FlagExpire;
            dbPersonFlag.LookupSubListId = flagAlertDetails.LookupSubListId > 0 ? flagAlertDetails.LookupSubListId : default;
            if (!isExist)
            {
                _context.PersonFlag.Add(dbPersonFlag);
                _context.SaveChanges();
            }

            IQueryable<WebServiceEventAssign> eventAssigns = _context.WebServiceEventAssign
                .Where(w => w.WebServiceEventInactive != 1
                    && w.WebServiceEventType.WebServiceEventName == (flagAlertDetails.DeleteFlag
                        ? EventNameConstants.REMOVEALERTFLAG : EventNameConstants.NEWALERTFLAG))
                .Select(s => s);

            foreach (WebServiceEventAssign f in eventAssigns.ToList())
            {
                // eventAssigns.ToList().ForEach(async (f) => {
                int ExportEventId = 0;
                if (f.PersonFlagLookupIndex > 0)
                {
                    if (flagAlertDetails.LookupType == f.PersonFlagLookupType && flagAlertDetails.LookupIndex == f.PersonFlagLookupIndex)
                    {
                        EventName = (flagAlertDetails.DeleteFlag ? EventNameConstants.REMOVEALERTFLAG : EventNameConstants.NEWALERTFLAG);
                        ExportEventId = f.WebServiceEventAssignId;
                    }
                }
                else
                {
                    EventName = (flagAlertDetails.DeleteFlag ? EventNameConstants.REMOVEALERTFLAG : EventNameConstants.NEWALERTFLAG);
                    ExportEventId = f.WebServiceEventAssignId;
                }

                if (ExportEventId > 0)
                {
                    _interfaceEngine.Export(new ExportRequestVm
                    {
                        EventName = EventName,
                        PersonnelId = _personnelId,
                        Param1 = flagAlertDetails.PersonId.ToString(),
                        Param2 = dbPersonFlag.PersonFlagId.ToString(),
                        WebServiceEventAssignId = ExportEventId
                    });
                }
            }


            await _context.SaveChangesAsync();
            await InsertFlagAlertHistory(flagAlertDetails);

            //For PREAFlag we need to insert and update PREAReview table
            if (flagAlertDetails.PREAFlagIndex > 0)
            {
                if (isNew)
                {
                    Lookup lookup = _context.Lookup.SingleOrDefault(l => l.LookupType == LookupConstants.PREAFLAGS
                        && l.LookupIndex == flagAlertDetails.PREAFlagIndex && !string.IsNullOrEmpty(l.LookupCategory));
                    if (lookup == null) return dbPersonFlag.PersonFlagId;
                    if (Convert.ToInt32(lookup.LookupCategory) <= 0) return dbPersonFlag.PersonFlagId;
                    PREAReview pREAReview = new PREAReview
                    {
                        InmateId = _context.Inmate.Single(i => i.PersonId == flagAlertDetails.PersonId).InmateId,
                        PREAReviewDate = DateTime.Now,
                        PREAReviewFlagId = flagAlertDetails.PREAFlagIndex,
                        CreateDate = DateTime.Now,
                        CreateBy = _personnelId
                    };
                    _context.PREAReview.Add(pREAReview);
                    _context.SaveChanges();
                }
                else
                {
                    PREAReview pREAReview = _context.PREAReview
                        .SingleOrDefault(p => p.PREAReviewFlagId == flagAlertDetails.PREAFlagIndex && !p.DeleteFlag);
                    if (pREAReview == null) return dbPersonFlag.PersonFlagId;
                    if (flagAlertDetails.DeleteFlag)
                    {
                        pREAReview.DeleteFlag = true;
                        pREAReview.DeleteDate = DateTime.Now;
                        pREAReview.DeleteBy = _personnelId;
                    }
                    else
                    {
                        pREAReview.UpadateDate = DateTime.Now;
                        pREAReview.UpdateBy = _personnelId;
                    }
                    _context.SaveChanges();
                }
            }

            return dbPersonFlag.PersonFlagId;
        }

        // Insert Flag Alert History details
        private async Task InsertFlagAlertHistory(FlagAlertVm flagAlertDetails)
        {

            PersonFlagHistory personFlagHistory = new PersonFlagHistory
            {
                PersonId = flagAlertDetails.PersonId,
                FlagNote = flagAlertDetails.FlagNote,
                FlagExpire = flagAlertDetails.FlagExpire,
                PersonFlagIndex = flagAlertDetails.PersonFlagIndex,
                InmateFlagIndex = flagAlertDetails.InmateFlagIndex,
                DietFlagIndex = flagAlertDetails.DietFlagIndex,
                MedicalFlagIndex = flagAlertDetails.MedicalFlagIndex,
                PREAFlagIndex = flagAlertDetails.PREAFlagIndex,
                DeleteFlag = flagAlertDetails.DeleteFlag ? 1 : 0,
                CreateBy = _personnelId,
                CreateDate = DateTime.Now,
                LookupSubListId = flagAlertDetails.LookupSubListId > 0 ? flagAlertDetails.LookupSubListId : null
            };

            if (personFlagHistory.DeleteFlag == 1)
            {
                personFlagHistory.DeleteBy = _personnelId;
                personFlagHistory.DeleteDate = DateTime.Now;
            }

            _context.PersonFlagHistory.Add(personFlagHistory);
            await _context.SaveChangesAsync();
        }

        //  Get Flag Alert Details
        public List<FlagAlertVm> GetFlagAlertDetails(int personId, bool isPermission)
        {
            List<FlagAlertVm> dbFlagAlertLst = _context.PersonFlag
                .Where(pf => pf.PersonId == personId)
                .Select(prf => new FlagAlertVm
                {
                    // PersonFlagId will be shown only for active records
                    PersonFlagId = prf.DeleteFlag == 0 ? prf.PersonFlagId : 0,
                    PersonFlagIndex = prf.PersonFlagIndex ?? 0,
                    InmateFlagIndex = prf.InmateFlagIndex ?? 0,
                    DietFlagIndex = prf.DietFlagIndex ?? 0,
                    MedicalFlagIndex = prf.MedicalFlagIndex ?? 0,
                    FlagNote = prf.FlagNote,
                    FlagExpire = prf.FlagExpire,
                    DeleteFlag = prf.DeleteFlag == 1,
                    LookupSubListId = prf.LookupSubListId,
                    LookupSubListName = prf.LookupSubList.SubListValue,
                    PREAFlagIndex = prf.PREAFlagIndex ?? 0,
                    CreateDate = prf.CreateDate
                }).ToList();

            List<LookupVm> staticFlagLst = _commonService.GetLookups(new[]
            {
                LookupConstants.TRANSCAUTION,
                LookupConstants.PERSONCAUTION,
                LookupConstants.DIET,
                LookupConstants.MEDFLAG,
                LookupConstants.PREAFLAGS
            }).ToList();

            if (isPermission)
            {
                staticFlagLst = _userPermissionPolicy.GetFlagPermission(staticFlagLst);
            }

            //Convert LookupVm model into FlagAlertVm model using Json Serialize
            List<FlagAlertVm> personFlagAlertLst =
                JsonConvert.DeserializeObject<List<FlagAlertVm>>(JsonConvert.SerializeObject(staticFlagLst));

            // Set PersonFlagIndex from Lookup index
            List<FlagAlertVm> personCautionLst =
                personFlagAlertLst.Where(pfa => pfa.LookupType == LookupConstants.PERSONCAUTION).ToList();

            personCautionLst.ForEach(per => per.PersonFlagIndex = per.LookupIndex);

            // Set InmateFlagIndex from Lookup index
            List<FlagAlertVm> transCautionLst =
                personFlagAlertLst.Where(pfa => pfa.LookupType == LookupConstants.TRANSCAUTION).ToList();

            transCautionLst.ForEach(tra => tra.InmateFlagIndex = tra.LookupIndex);

            // Set DietFlagIndex from Lookup index
            List<FlagAlertVm> dietCautionLst =
                personFlagAlertLst.Where(pfa => pfa.LookupType == LookupConstants.DIET).ToList();

            dietCautionLst.ForEach(det => det.DietFlagIndex = det.LookupIndex);

            // Set MedFlagIndex from Lookup index
            List<FlagAlertVm> medCautionLst =
                personFlagAlertLst.Where(pfa => pfa.LookupType == LookupConstants.MEDFLAG).ToList();

            medCautionLst.ForEach(med => med.MedicalFlagIndex = med.LookupIndex);

            // Set PREAFlagIndex from Lookup index
            List<FlagAlertVm> pREACautionLst =
                personFlagAlertLst.Where(pfa => pfa.LookupType == LookupConstants.PREAFLAGS).ToList();

            pREACautionLst.ForEach(med => med.PREAFlagIndex = med.LookupIndex);

            if (dbFlagAlertLst.Any())
            {
                List<PersonFlagHistory> personFlagHistories =
                    _context.PersonFlagHistory.Where(pfh =>
                        pfh.PersonId == personId).ToList();
                dbFlagAlertLst.ForEach(flagAlert =>
                {
                    if (flagAlert.PersonFlagIndex > 0)
                    {
                        FlagAlertVm personFlagAlert = personFlagAlertLst.SingleOrDefault(pfa =>
                            pfa.LookupType == LookupConstants.PERSONCAUTION &&
                            pfa.LookupIndex == flagAlert.PersonFlagIndex);
                        if (personFlagAlert != null)
                        {
                            personFlagAlert.PersonId = personId;
                            personFlagAlert.PersonFlagId = flagAlert.PersonFlagId;
                            personFlagAlert.PersonFlagIndex = flagAlert.PersonFlagIndex;
                            personFlagAlert.FlagNote = flagAlert.FlagNote;
                            personFlagAlert.FlagExpire = flagAlert.FlagExpire;
                            personFlagAlert.HistoryCount =
                                personFlagHistories.Count(pfh =>
                                    pfh.PersonFlagIndex == flagAlert.PersonFlagIndex);
                            personFlagAlert.LookupSubListId = flagAlert.LookupSubListId;
                            personFlagAlert.LookupSubListName = flagAlert.LookupSubListName;
                            personFlagAlert.CreateDate = flagAlert.CreateDate;
                        }
                    }
                    else if (flagAlert.InmateFlagIndex > 0)
                    {
                        FlagAlertVm inmateFlagAlert = personFlagAlertLst.SingleOrDefault(pfa =>
                            pfa.LookupType == LookupConstants.TRANSCAUTION &&
                            pfa.LookupIndex == flagAlert.InmateFlagIndex);
                        if (inmateFlagAlert != null)
                        {
                            inmateFlagAlert.PersonId = personId;
                            inmateFlagAlert.PersonFlagId = flagAlert.PersonFlagId;
                            inmateFlagAlert.InmateFlagIndex = flagAlert.InmateFlagIndex;
                            inmateFlagAlert.FlagNote = flagAlert.FlagNote;
                            inmateFlagAlert.FlagExpire = flagAlert.FlagExpire;
                            inmateFlagAlert.HistoryCount =
                                personFlagHistories.Count(pfh =>
                                    pfh.InmateFlagIndex == flagAlert.InmateFlagIndex);
                            inmateFlagAlert.LookupSubListId = flagAlert.LookupSubListId;
                            inmateFlagAlert.LookupSubListName = flagAlert.LookupSubListName;
                            inmateFlagAlert.CreateDate = flagAlert.CreateDate;
                        }
                    }
                    else if (flagAlert.DietFlagIndex > 0)
                    {
                        FlagAlertVm dietFlagAlert = personFlagAlertLst.SingleOrDefault(pfa =>
                            pfa.LookupType == LookupConstants.DIET && pfa.LookupIndex == flagAlert.DietFlagIndex);
                        if (dietFlagAlert != null)
                        {
                            dietFlagAlert.PersonId = personId;
                            dietFlagAlert.PersonFlagId = flagAlert.PersonFlagId;
                            dietFlagAlert.DietFlagIndex = flagAlert.DietFlagIndex;
                            dietFlagAlert.FlagNote = flagAlert.FlagNote;
                            dietFlagAlert.FlagExpire = flagAlert.FlagExpire;
                            dietFlagAlert.HistoryCount =
                                personFlagHistories.Count(pfh =>
                                    pfh.DietFlagIndex == flagAlert.DietFlagIndex);
                            dietFlagAlert.LookupSubListId = flagAlert.LookupSubListId;
                            dietFlagAlert.LookupSubListName = flagAlert.LookupSubListName;
                            dietFlagAlert.CreateDate = flagAlert.CreateDate;
                        }
                    }
                    else if (flagAlert.MedicalFlagIndex > 0)
                    {
                        FlagAlertVm medFlagAlert = personFlagAlertLst.SingleOrDefault(med =>
                            med.LookupType == LookupConstants.MEDFLAG && med.LookupIndex == flagAlert.MedicalFlagIndex);
                        if (medFlagAlert != null)
                        {
                            medFlagAlert.PersonId = personId;
                            medFlagAlert.PersonFlagId = flagAlert.PersonFlagId;
                            medFlagAlert.MedicalFlagIndex = flagAlert.MedicalFlagIndex;
                            medFlagAlert.FlagNote = flagAlert.FlagNote;
                            medFlagAlert.FlagExpire = flagAlert.FlagExpire;
                            medFlagAlert.HistoryCount =
                                personFlagHistories.Count(med =>
                                    med.MedicalFlagIndex == flagAlert.MedicalFlagIndex);
                            medFlagAlert.LookupSubListId = flagAlert.LookupSubListId;
                            medFlagAlert.LookupSubListName = flagAlert.LookupSubListName;
                            medFlagAlert.CreateDate = flagAlert.CreateDate;
                        }
                    }
                    else if (flagAlert.PREAFlagIndex > 0)
                    {
                        FlagAlertVm pREAFlagAlert = personFlagAlertLst.SingleOrDefault(prea =>
                            prea.LookupType == LookupConstants.PREAFLAGS && prea.LookupIndex == flagAlert.PREAFlagIndex);
                        if (pREAFlagAlert != null)
                        {
                            pREAFlagAlert.PersonId = personId;
                            pREAFlagAlert.PersonFlagId = flagAlert.PersonFlagId;
                            pREAFlagAlert.PREAFlagIndex = flagAlert.PREAFlagIndex;
                            pREAFlagAlert.FlagNote = flagAlert.FlagNote;
                            pREAFlagAlert.FlagExpire = flagAlert.FlagExpire;
                            pREAFlagAlert.HistoryCount =
                                personFlagHistories.Count(prea =>
                                    prea.PREAFlagIndex == flagAlert.PREAFlagIndex);
                            pREAFlagAlert.LookupSubListId = flagAlert.LookupSubListId;
                            pREAFlagAlert.LookupSubListName = flagAlert.LookupSubListName;
                            pREAFlagAlert.CreateDate = flagAlert.CreateDate;
                        }
                    }
                });
            }
            if (isPermission)
            {
                personFlagAlertLst.ForEach(f => f.LookupSubList = GetLookupSubList(f.LookupIdentity));
            }
            return personFlagAlertLst;
        }

        // Get Flag Alert History Details
        public List<FlagAlertHistoryVm> GetFlagAlertHistoryDetails(int personId, int flagIndex, string type)
        {
            IQueryable<PersonFlagHistory> personFlagHistory =
                _context.PersonFlagHistory.Where(pfh => pfh.PersonId == personId);

            List<FlagAlertHistoryVm> personFlagHistoryLst = new List<FlagAlertHistoryVm>();
            switch (type)
            {
                case LookupConstants.PERSONCAUTION:
                    personFlagHistoryLst = personFlagHistory.Where(pfh => pfh.PersonFlagIndex == flagIndex)
                        .Select(flag => new FlagAlertHistoryVm
                        {
                            FlagNote = flag.FlagNote,
                            FlagExpire = flag.FlagExpire,
                            CreateDate = flag.CreateDate,
                            DeleteFlag = flag.DeleteFlag == 1,
                            CreateBy = flag.CreateBy,
                            AssignmentReason = flag.LookupSubList.SubListValue
                        }).ToList();
                    break;
                case LookupConstants.TRANSCAUTION:
                    personFlagHistoryLst = personFlagHistory.Where(pfh => pfh.InmateFlagIndex == flagIndex)
                        .Select(flag => new FlagAlertHistoryVm
                        {
                            FlagNote = flag.FlagNote,
                            FlagExpire = flag.FlagExpire,
                            CreateDate = flag.CreateDate,
                            DeleteFlag = flag.DeleteFlag == 1,
                            CreateBy = flag.CreateBy,
                            AssignmentReason = flag.LookupSubList.SubListValue
                        }).ToList();
                    break;
                case LookupConstants.DIET:
                    personFlagHistoryLst = personFlagHistory.Where(pfh => pfh.DietFlagIndex == flagIndex).Select(flag =>
                        new FlagAlertHistoryVm
                        {
                            FlagNote = flag.FlagNote,
                            FlagExpire = flag.FlagExpire,
                            CreateDate = flag.CreateDate,
                            DeleteFlag = flag.DeleteFlag == 1,
                            CreateBy = flag.CreateBy,
                            AssignmentReason = flag.LookupSubList.SubListValue
                        }).ToList();
                    break;
                case LookupConstants.MEDFLAG:
                    personFlagHistoryLst = personFlagHistory.Where(med => med.MedicalFlagIndex == flagIndex).Select(flag =>
                        new FlagAlertHistoryVm
                        {
                            FlagNote = flag.FlagNote,
                            FlagExpire = flag.FlagExpire,
                            CreateDate = flag.CreateDate,
                            DeleteFlag = flag.DeleteFlag == 1,
                            CreateBy = flag.CreateBy,
                            AssignmentReason = flag.LookupSubList.SubListValue
                        }).ToList();
                    break;
                case LookupConstants.PREAFLAGS:
                    personFlagHistoryLst = personFlagHistory.Where(prea => prea.PREAFlagIndex == flagIndex).Select(flag =>
                        new FlagAlertHistoryVm
                        {
                            FlagNote = flag.FlagNote,
                            FlagExpire = flag.FlagExpire,
                            CreateDate = flag.CreateDate,
                            DeleteFlag = flag.DeleteFlag == 1,
                            CreateBy = flag.CreateBy,
                            AssignmentReason = flag.LookupSubList.SubListValue
                        }).ToList();
                    break;
            }

            if (personFlagHistoryLst.Any())
            {
                //Get personnel details list
                int[] personnelIds = personFlagHistoryLst.Select(p => p.CreateBy).Distinct().ToArray();

                List<Personnel> lstPersonnelDet = (from per in _context.Personnel
                                                   where personnelIds.Contains(per.PersonnelId)
                                                   select new Personnel
                                                   {
                                                       PersonId = per.PersonId,
                                                       PersonnelId = per.PersonnelId,
                                                       OfficerBadgeNumber = per.OfficerBadgeNum
                                                   }).ToList();

                //Get personnel details list
                int[] personIds = lstPersonnelDet.Select(p => p.PersonId).ToArray();
                List<Person> lstPersonDet = (from per in _context.Person
                                             where personIds.Contains(per.PersonId)
                                             select new Person
                                             {
                                                 PersonId = per.PersonId,
                                                 PersonLastName = per.PersonLastName,
                                                 PersonFirstName = per.PersonFirstName
                                             }).ToList();

                personFlagHistoryLst.ForEach(hist =>
                {
                    Personnel personnelDet = lstPersonnelDet.SingleOrDefault(det => det.PersonnelId == hist.CreateBy);
                    if (personnelDet is null) return;
                    hist.OfficerBadgeNumber = personnelDet.OfficerBadgeNumber;
                    hist.PersonFirstName = lstPersonDet.SingleOrDefault(det => det.PersonId == personnelDet.PersonId)
                        ?.PersonFirstName;
                    hist.PersonLastName = lstPersonDet.SingleOrDefault(det => det.PersonId == personnelDet.PersonId)
                        ?.PersonLastName;
                });
            }

            return personFlagHistoryLst.OrderByDescending(p => p.CreateDate).ToList();
        }

        public FlagAlertHistoryByPersonVm GetFlagAlertHistoryByPerson(int personId)
        {
            FlagAlertHistoryByPersonVm flagAlertHistoryByPersonVm = new FlagAlertHistoryByPersonVm();

            _staticFlagLst = _commonService.GetLookups(new[]
                {LookupConstants.TRANSCAUTION, LookupConstants.PERSONCAUTION, LookupConstants.DIET, LookupConstants.MEDFLAG});

            // person flag index
            flagAlertHistoryByPersonVm.LstPersonFlag = GetFlagAlertHistoryByPerson(personId, FlagAlert.PersonFlagIndex);

            // inmate flag index
            flagAlertHistoryByPersonVm.LstInmateFlag = GetFlagAlertHistoryByPerson(personId, FlagAlert.InmateFlagIndex);

            // diet flag index
            flagAlertHistoryByPersonVm.LstDietFlag = GetFlagAlertHistoryByPerson(personId, FlagAlert.DietFlagIndex);

            // med flag index
            flagAlertHistoryByPersonVm.LstMedFlag = GetFlagAlertHistoryByPerson(personId, FlagAlert.MedicalFlagIndex);

            // prea flag index
            flagAlertHistoryByPersonVm.LstPreaFlag = GetFlagAlertHistoryByPerson(personId, FlagAlert.PreaFlagIndex);

            return flagAlertHistoryByPersonVm;
        }

        public List<FlagAlertHistoryVm> GetFlagAlertHistoryByPerson(int personId, FlagAlert flagAlert)
        {
            List<FlagAlertHistoryVm> lstFlagAlertHistoryVm = new List<FlagAlertHistoryVm>();

            if (!_staticFlagLst.Any())
            {
                _staticFlagLst = _commonService.GetLookups(new[]
                {
                    LookupConstants.TRANSCAUTION,
                    LookupConstants.PERSONCAUTION,
                    LookupConstants.DIET,
                    LookupConstants.MEDFLAG,
                    LookupConstants.PREAFLAGS
                });
            }

            switch (flagAlert)
            {
                case FlagAlert.PersonFlagIndex:
                    lstFlagAlertHistoryVm = _context.PersonFlagHistory
                        .Where(pfh => pfh.PersonId == personId && pfh.PersonFlagIndex > 0)
                        .Select(flag =>
                            new FlagAlertHistoryVm
                            {
                                FlagNote = flag.FlagNote,
                                FlagExpire = flag.FlagExpire,
                                CreateDate = flag.CreateDate,
                                DeleteFlag = flag.DeleteFlag == 1,
                                CreateBy = flag.CreateBy,
                                PersonFlagIndex = flag.PersonFlagIndex ?? 0,
                                PersonId = personId,
                                AssignmentReason = flag.LookupSubList.SubListValue
                            }).OrderByDescending(pfi => pfi.CreateDate).ToList();

                    if (lstFlagAlertHistoryVm.Any())
                    {
                        lstFlagAlertHistoryVm.ForEach(fa =>
                        {
                            fa.LookupDescription = _staticFlagLst.SingleOrDefault(pfa =>
                                pfa.LookupType == LookupConstants.PERSONCAUTION &&
                                pfa.LookupIndex == fa.PersonFlagIndex)?.LookupDescription;
                            fa.LookupType = LookupConstants.PERSONCAUTION;
                        });
                    }
                    break;
                case FlagAlert.InmateFlagIndex:
                    lstFlagAlertHistoryVm = _context.PersonFlagHistory
                        .Where(pfh => pfh.PersonId == personId && pfh.InmateFlagIndex > 0)
                        .Select(flag =>
                            new FlagAlertHistoryVm
                            {
                                FlagNote = flag.FlagNote,
                                FlagExpire = flag.FlagExpire,
                                CreateDate = flag.CreateDate,
                                DeleteFlag = flag.DeleteFlag == 1,
                                CreateBy = flag.CreateBy,
                                InmateFlagIndex = flag.InmateFlagIndex ?? 0,
                                PersonId = personId,
                                AssignmentReason = flag.LookupSubList.SubListValue
                            }).OrderByDescending(pfi => pfi.CreateDate).ToList();

                    if (lstFlagAlertHistoryVm.Any())
                    {
                        lstFlagAlertHistoryVm.ForEach(fa =>
                        {
                            fa.LookupDescription =
                                _staticFlagLst.SingleOrDefault(pfa =>
                                        pfa.LookupType == LookupConstants.TRANSCAUTION &&
                                        pfa.LookupIndex == fa.InmateFlagIndex)?
                                    .LookupDescription;
                            fa.LookupType = LookupConstants.TRANSCAUTION;
                        });
                    }
                    break;
                case FlagAlert.DietFlagIndex:
                    lstFlagAlertHistoryVm = _context.PersonFlagHistory
                        .Where(pfh => pfh.PersonId == personId && pfh.DietFlagIndex > 0)
                        .Select(flag =>
                            new FlagAlertHistoryVm
                            {
                                FlagNote = flag.FlagNote,
                                FlagExpire = flag.FlagExpire,
                                CreateDate = flag.CreateDate,
                                DeleteFlag = flag.DeleteFlag == 1,
                                CreateBy = flag.CreateBy,
                                DietFlagIndex = flag.DietFlagIndex ?? 0,
                                PersonId = personId,
                                AssignmentReason = flag.LookupSubList.SubListValue
                            }).OrderByDescending(pfi => pfi.CreateDate).ToList();

                    if (lstFlagAlertHistoryVm.Any())
                    {
                        lstFlagAlertHistoryVm.ForEach(fa =>
                        {
                            fa.LookupDescription = _staticFlagLst.SingleOrDefault(pfa =>
                                    pfa.LookupType == LookupConstants.DIET &&
                                    pfa.LookupIndex == fa.DietFlagIndex)?
                                .LookupDescription;
                            fa.LookupType = LookupConstants.DIET;
                        });
                    }
                    break;
                case FlagAlert.MedicalFlagIndex:
                    lstFlagAlertHistoryVm = _context.PersonFlagHistory
                        .Where(med => med.PersonId == personId && med.MedicalFlagIndex > 0)
                        .Select(flag =>
                            new FlagAlertHistoryVm
                            {
                                FlagNote = flag.FlagNote,
                                FlagExpire = flag.FlagExpire,
                                CreateDate = flag.CreateDate,
                                DeleteFlag = flag.DeleteFlag == 1,
                                CreateBy = flag.CreateBy,
                                MedicalFlagIndex = flag.MedicalFlagIndex ?? 0,
                                PersonId = personId,
                                AssignmentReason = flag.LookupSubList.SubListValue
                            }).OrderByDescending(pfi => pfi.CreateDate).ToList();

                    if (lstFlagAlertHistoryVm.Any())
                    {
                        lstFlagAlertHistoryVm.ForEach(fa =>
                        {
                            fa.LookupDescription = _staticFlagLst.SingleOrDefault(pfa =>
                                    pfa.LookupType == LookupConstants.MEDFLAG &&
                                    pfa.LookupIndex == fa.MedicalFlagIndex)?
                                .LookupDescription;
                            fa.LookupType = LookupConstants.MEDFLAG;
                        });
                    }
                    break;
                case FlagAlert.PreaFlagIndex:
                    lstFlagAlertHistoryVm = _context.PersonFlagHistory
                        .Where(med => med.PersonId == personId && med.PREAFlagIndex > 0)
                        .Select(flag =>
                            new FlagAlertHistoryVm
                            {
                                FlagNote = flag.FlagNote,
                                FlagExpire = flag.FlagExpire,
                                CreateDate = flag.CreateDate,
                                DeleteFlag = flag.DeleteFlag == 1,
                                CreateBy = flag.CreateBy,
                                PreaFlagIndex = flag.PREAFlagIndex ?? 0,
                                PersonId = personId,
                                AssignmentReason = flag.LookupSubList.SubListValue
                            }).OrderByDescending(pfi => pfi.CreateDate).ToList();

                    if (lstFlagAlertHistoryVm.Any())
                    {
                        lstFlagAlertHistoryVm.ForEach(fa =>
                        {
                            fa.LookupDescription = _staticFlagLst.SingleOrDefault(pfa =>
                                    pfa.LookupType == LookupConstants.PREAFLAGS &&
                                    pfa.LookupIndex == fa.PreaFlagIndex)?
                                .LookupDescription;
                            fa.LookupType = LookupConstants.PREAFLAGS;
                        });
                    }
                    break;
            }

            if (lstFlagAlertHistoryVm.Any())
            {
                //Get personnel details list
                int[] personnelIds = lstFlagAlertHistoryVm.Select(p => p.CreateBy).Distinct().ToArray();

                List<Personnel> lstPersonnelDet = (from per in _context.Personnel
                                                   where personnelIds.Contains(per.PersonnelId)
                                                   select new Personnel
                                                   {
                                                       PersonId = per.PersonId,
                                                       PersonnelId = per.PersonnelId,
                                                       OfficerBadgeNumber = per.OfficerBadgeNum
                                                   }).ToList();

                //Get personnel details list
                int[] personIds = lstPersonnelDet.Select(p => p.PersonId).ToArray();
                List<Person> lstPersonDet = (from per in _context.Person
                                             where personIds.Contains(per.PersonId)
                                             select new Person
                                             {
                                                 PersonId = per.PersonId,
                                                 PersonLastName = per.PersonLastName,
                                                 PersonFirstName = per.PersonFirstName
                                             }).ToList();

                lstFlagAlertHistoryVm.ForEach(hist =>
                {
                    Personnel personnelDet = lstPersonnelDet.SingleOrDefault(det => det.PersonnelId == hist.CreateBy);
                    if (personnelDet is null) return;
                    hist.OfficerBadgeNumber = personnelDet.OfficerBadgeNumber;
                    hist.PersonFirstName = lstPersonDet.SingleOrDefault(det => det.PersonId == personnelDet.PersonId)
                        ?.PersonFirstName;
                    hist.PersonLastName = lstPersonDet.SingleOrDefault(det => det.PersonId == personnelDet.PersonId)
                        ?.PersonLastName;
                });
            }

            return lstFlagAlertHistoryVm;
        }

        public List<KeyValuePair<int, string>> GetLookupSubList(int lookupIdentity) => _context.LookupSubList
                .Where(w => w.LookupIdentity == lookupIdentity && !w.DeleteFlag)
                .Select(s => new KeyValuePair<int, string>(s.LookupSubListId, s.SubListValue)).ToList();


        #endregion

        #region Assoc Alert

        public List<PersonClassificationDetails> GetAssocAlertDetails(int personId)
        {
            List<Lookup> lookupsSublist = _context.Lookup.Where(w => w.LookupType == LookupConstants.CLASSGROUPSUB).ToList();
            List<LookupVm> lookupVms = _commonService.GetLookups(new[] { LookupConstants.CLASSGROUP });
            List<int> lookupDescLst = lookupVms.Where(x => x.LookupFlag9 == 1 || x.LookupFlag10 == 1)
                 .Select(x => Convert.ToInt32(x.LookupIndex)).ToList();

            List<PersonClassificationDetails> perClassify = _context.PersonClassification.Where(perClass =>
                    perClass.PersonId == personId && lookupDescLst.Contains(perClass.PersonClassificationTypeId ?? 0))
                .Select(perClass =>
                    new PersonClassificationDetails
                    {
                        PersonClassificationId = perClass.PersonClassificationId,
                        ClassificationType = lookupVms
                            .SingleOrDefault(s => s.LookupIndex == perClass.PersonClassificationTypeId)
                            .LookupDescription,
                        ClassificationSubset = lookupsSublist
                            .SingleOrDefault(s => s.LookupIndex == perClass.PersonClassificationSubsetId)
                            .LookupDescription,
                        ClassificationNotes = perClass.PersonClassificationNotes,
                        ClassificationStatus = perClass.PersonClassificationStatus,
                        PersonId = perClass.PersonId,
                        DateFrom = perClass.PersonClassificationDateFrom,
                        DateTo = perClass.PersonClassificationDateThru,
                        CreateDate = perClass.CreateDate,
                        PersonnelId = perClass.PersonnelId,
                        UpdateDate = perClass.UpdateDate,
                        ClassificationCompleteBy = perClass.PersonClassificationCompleteBy,
                        InactiveFlag = perClass.InactiveFlag == 1,
                        ClassificationFlag = perClass.PersonClassificationFlag
                            .Where(pcf =>
                                pcf.PersonClassificationId == perClass.PersonClassificationId &&
                                !pcf.DeleteFlag.HasValue &&
                                pcf.AssocFlagIndex.HasValue)
                            .Select(pcf => pcf.AssocFlagIndex.Value).ToArray(),
                        CreateById = perClass.CreatedByPersonnelId,
                        UpdateById = perClass.UpdatedByOfficerId,
                        ClassificationTypeId = perClass.PersonClassificationTypeId,
                        ClassificationSubsetId = perClass.PersonClassificationSubsetId,
                    }).ToList();

            List<int> createdIds =
                perClassify.Select(i => new[] { i.UpdateById, i.CreateById })
                    .SelectMany(i => i)
                    .Where(i => i.HasValue)
                    .Select(i => i.Value)
                    .ToList();

            List<PersonnelVm> createOfficer = _personService.GetPersonNameList(createdIds.ToList());

            perClassify.ForEach(perClass =>
            {
                perClass.CreatedBy = createOfficer.Single(ao => ao.PersonnelId == perClass.CreateById);
                if (perClass.UpdateById.HasValue)
                {
                    perClass.UpdatedBy = createOfficer.Single(ao => ao.PersonnelId == perClass.UpdateById.Value);
                }
            });

            return perClassify;
        }

        public List<AssocAlertHistoryVm> GetAssocHistoryDetails(int personClassificationId)
        {
            List<AssocAlertHistoryVm> assAlertHis = _context.PersonClassificationHistory
                .Where(pch => pch.PersonClassificationId == personClassificationId).Select(pch =>
                    new AssocAlertHistoryVm
                    {
                        PersonClassificationHistoryId = pch.PersonClassificationHistoryId,
                        CreateDate = pch.CreateDate,
                        CreateBy = pch.PersonnelId,
                        HistoryList = pch.PersonClassificationHistoryList,
                        PersonId = pch.Personnel.PersonId,
                        OfficerBadgeNumber = pch.Personnel.OfficerBadgeNum
                    }).ToList();

            if (assAlertHis.Any())
            {
                //Get personnel details list
                int[] personIds = assAlertHis.Select(p => p.PersonId).ToArray();
                List<Person> lstPersonDet = (from per in _context.Person
                                             where personIds.Contains(per.PersonId)
                                             select new Person
                                             {
                                                 PersonId = per.PersonId,
                                                 PersonLastName = per.PersonLastName,
                                                 PersonFirstName = per.PersonFirstName
                                             }).ToList();

                assAlertHis.ForEach(hist =>
                {
                    hist.PersonFirstName = lstPersonDet.SingleOrDefault(det => det.PersonId == hist.PersonId)
                        ?.PersonFirstName;
                    hist.PersonLastName = lstPersonDet.SingleOrDefault(det => det.PersonId == hist.PersonId)
                        ?.PersonLastName;

                    if (hist.HistoryList == null) return;
                    Dictionary<string, string> classifyHistoryList =
                        JsonConvert.DeserializeObject<Dictionary<string, string>>(hist.HistoryList);
                    hist.Header =
                        classifyHistoryList.Select(ph => new PersonHeader { Header = ph.Key, Detail = ph.Value })
                            .ToList();
                });
            }

            return assAlertHis;
        }

        public async Task<AssocAlertValidation> InsertUpdateAssocDetails(PersonClassificationDetails personAssocDetails)
        {
            AssocAlertValidation assValidate = new AssocAlertValidation();
            if (_personService.IsPersonSealed(personAssocDetails.PersonId)) //Validate person is sealed or not
            {
                assValidate.ErrorMessage = AssocErrorMessage.SealedInmate;
                return assValidate;
            }


            assValidate = ValidateAssoc(personAssocDetails);
            if (AssocErrorMessage.None != assValidate.ErrorMessage)
            {
                return assValidate;
            }

            bool isExist = false;
            PersonClassification dbPersonClassification = _context.PersonClassification
                .SingleOrDefault(pa => pa.PersonClassificationId == personAssocDetails.PersonClassificationId);

            if (dbPersonClassification?.PersonClassificationId > 0)
            {
                isExist = true;
                dbPersonClassification.UpdateDate = DateTime.Now;
                dbPersonClassification.UpdatedByOfficerId = _personnelId;
                dbPersonClassification.UpdatedByDate = DateTime.Now;
                dbPersonClassification.PersonClassificationTypeId = personAssocDetails.ClassificationTypeId;
                dbPersonClassification.PersonClassificationSubsetId = personAssocDetails.ClassificationSubsetId;

                int[] perClassFlag = _context.PersonClassificationFlag.Where(pcf =>
                        pcf.PersonClassificationId == dbPersonClassification.PersonClassificationId &&
                        (!pcf.DeleteFlag.HasValue || pcf.DeleteFlag == 0) && pcf.AssocFlagIndex.HasValue)
                    .Select(pcf => pcf.AssocFlagIndex.Value).ToArray();

                foreach (int classFlagId in perClassFlag)
                {
                    if (!personAssocDetails.ClassificationFlag.Contains(classFlagId))
                    {
                        PersonClassificationFlag upPerClassFlag = _context.PersonClassificationFlag.Single(pcf =>
                            pcf.PersonClassificationId == dbPersonClassification.PersonClassificationId &&
                            pcf.AssocFlagIndex == classFlagId &&
                            (!pcf.DeleteFlag.HasValue || pcf.DeleteFlag == 0));

                        upPerClassFlag.DeleteFlag = 1;
                        upPerClassFlag.DeleteBy = _personnelId;
                        upPerClassFlag.DeleteDate = DateTime.Now;
                    }
                }
            }
            else
            {
                dbPersonClassification = new PersonClassification
                {
                    CreateDate = DateTime.Now,
                    CreatedByPersonnelId = _personnelId,
                    PersonnelId = _personnelId,
                    PersonClassificationDateFrom = DateTime.Now
                };
            }

            dbPersonClassification.PersonId = personAssocDetails.PersonId;
            dbPersonClassification.PersonClassificationType = personAssocDetails.ClassificationType;
            dbPersonClassification.PersonClassificationDateThru = personAssocDetails.DateTo;
            dbPersonClassification.PersonClassificationNotes = personAssocDetails.ClassificationNotes;
            dbPersonClassification.PersonClassificationSubset = personAssocDetails.ClassificationSubset;
            dbPersonClassification.PersonClassificationStatus = personAssocDetails.ClassificationStatus;
            dbPersonClassification.SuspectId = 0;
            dbPersonClassification.PersonClassificationTypeId = personAssocDetails.ClassificationTypeId;
            dbPersonClassification.PersonClassificationSubsetId = personAssocDetails.ClassificationSubsetId;

            if (!isExist)
            {
                _context.PersonClassification.Add(dbPersonClassification);
            }

            await _context.SaveChangesAsync();

            foreach (int assocFlag in personAssocDetails.ClassificationFlag)
            {
                PersonClassificationFlag perClassFlag = _context.PersonClassificationFlag.SingleOrDefault(pcf =>
                    pcf.PersonClassificationId == dbPersonClassification.PersonClassificationId &&
                    pcf.AssocFlagIndex == assocFlag &&
                    (!pcf.DeleteFlag.HasValue || pcf.DeleteFlag == 0));

                if (perClassFlag is null)
                {
                    PersonClassificationFlag pcf = new PersonClassificationFlag
                    {
                        PersonClassificationId = dbPersonClassification.PersonClassificationId,
                        AssocFlagIndex = assocFlag,
                        CreateBy = _personnelId,
                        CreateDate = DateTime.Now
                    };
                    _context.PersonClassificationFlag.Add(pcf);
                }
            }

            PersonClassificationHistory dbPersonClassificationHistory = new PersonClassificationHistory
            {
                PersonClassificationId = dbPersonClassification.PersonClassificationId,
                PersonnelId = _personnelId,
                CreateDate = DateTime.Now,
                PersonClassificationHistoryList = personAssocDetails.ClassifyHistoryList
            };

            _context.PersonClassificationHistory.Add(dbPersonClassificationHistory);
            await _context.SaveChangesAsync();
            _interfaceEngine.Export(new ExportRequestVm
            {
                EventName = EventNameConstants.ASSOCIATIONSAVE,
                PersonnelId = _personnelId,
                Param1 = personAssocDetails.PersonId.ToString(),
                Param2 = dbPersonClassification.PersonClassificationId.ToString()
            });
            return assValidate;
        }

        public async Task<AssocAlertValidation> DeleteAssocDetails(PersonClassificationDetails personAssocDetails)
        {
            AssocAlertValidation alValidate = new AssocAlertValidation();
            if (_personService.IsPersonSealed(personAssocDetails.PersonId)) //Validate person is sealed or not
            {
                alValidate.ErrorMessage = AssocErrorMessage.SealedInmate;
                return alValidate;
            }
            if (!personAssocDetails.InactiveFlag)
            {
                alValidate = ValidateAssoc(personAssocDetails);
                if (AssocErrorMessage.None != alValidate.ErrorMessage)
                {
                    return alValidate;
                }
            }

            PersonClassification dbPersonClassification = _context.PersonClassification
                .Single(pa => pa.PersonClassificationId == personAssocDetails.PersonClassificationId);
            dbPersonClassification.UpdatedByOfficerId = _personnelId;
            dbPersonClassification.UpdatedByDate = DateTime.Now;

            if (personAssocDetails.InactiveFlag)
            {
                dbPersonClassification.InactiveFlag = 1;
                dbPersonClassification.PersonClassificationDateThru = DateTime.Now;
                _interfaceEngine.Export(new ExportRequestVm
                {
                    EventName = EventNameConstants.ASSOCIATIONREMOVE,
                    PersonnelId = _personnelId,
                    Param1 = dbPersonClassification.PersonId.ToString(),
                    Param2 = dbPersonClassification.PersonClassificationId.ToString()
                });
            }
            else
            {
                dbPersonClassification.InactiveFlag = 0;
                dbPersonClassification.PersonClassificationDateThru = null;
            }
            await _context.SaveChangesAsync();
            return alValidate;
        }

        private AssocAlertValidation ValidateAssoc(PersonClassificationDetails perAssioDetail)
        {
            AssocAlertValidation assValidate = new AssocAlertValidation();
            int inmateId = _context.Inmate.Single(inm => inm.PersonId == perAssioDetail.PersonId).InmateId;

            IQueryable<PersonClassification> personClassify = _context.PersonClassification.Where(pClass =>
                pClass.PersonId == perAssioDetail.PersonId
                && (perAssioDetail.ClassificationSubsetId == null ||
                    pClass.PersonClassificationSubsetId == perAssioDetail.ClassificationSubsetId)
                && (pClass.PersonClassificationDateThru == null ||
                    pClass.PersonClassificationDateThru.Value.Date > DateTime.Now.Date)
                && pClass.InactiveFlag == 0
                && pClass.PersonClassificationTypeId == perAssioDetail.ClassificationTypeId
                && (perAssioDetail.PersonClassificationId == 0 ||
                    pClass.PersonClassificationId != perAssioDetail.PersonClassificationId)
                && (perAssioDetail.ClassificationStatus == null ||
                    pClass.PersonClassificationStatus == perAssioDetail.ClassificationStatus));                   

            if (personClassify.Any())
            {
                assValidate.ErrorMessage = AssocErrorMessage.AssocAssigned;
                return assValidate;
            }

            if (perAssioDetail.ClassificationSubsetId > 0)
            {
                IQueryable<KeepSepSubsetInmate> subsetInmate = _context.KeepSepSubsetInmate.Where(pClass =>
                    pClass.KeepSepInmate2Id == inmateId 
                    && pClass.KeepSepAssoc1Id == perAssioDetail.ClassificationTypeId
                    && (perAssioDetail.ClassificationSubsetId != null ||
                        pClass.KeepSepAssoc1SubsetId == perAssioDetail.ClassificationSubsetId));
                if (subsetInmate.Any())
                {
                    assValidate.ErrorMessage = AssocErrorMessage.KeepSepAssigned;
                    return assValidate;
                }
            }
            else
            {
                IQueryable<KeepSepAssocInmate> assocInmate = _context.KeepSepAssocInmate.Where(pClass =>
                    pClass.KeepSepInmate2Id == inmateId
                    && pClass.KeepSepAssoc1Id == perAssioDetail.ClassificationTypeId);

                if (assocInmate.Any())
                {
                    assValidate.ErrorMessage = AssocErrorMessage.KeepSepAssigned;
                    return assValidate;
                }
            }

            IQueryable<PersonClassification> personClassification = _context.PersonClassification.Where(pClass =>
                pClass.PersonId == perAssioDetail.PersonId &&
                (pClass.PersonClassificationDateThru == null || pClass.PersonClassificationDateThru > DateTime.Now) &&
                pClass.InactiveFlag == 0);

            if (!personClassification.Any()) return assValidate;
            IQueryable<KeepSepAssocAssoc> keepSepAssosAssoc1 = personClassification.SelectMany(
                p => _context.KeepSepAssocAssoc.Where(ks =>
                    ks.KeepSepAssoc1Id == p.PersonClassificationTypeId &&
                    ks.KeepSepAssoc2Id == perAssioDetail.ClassificationTypeId &&
                    ks.DeleteFlag == 0));

            IQueryable<KeepSepAssocAssoc> keepSepAssosAssoc2 = personClassification.SelectMany(
                p => _context.KeepSepAssocAssoc.Where(ks =>
                    ks.KeepSepAssoc2Id == p.PersonClassificationTypeId &&
                    ks.KeepSepAssoc1Id == perAssioDetail.ClassificationTypeId &&
                    ks.DeleteFlag == 0));
            List<Lookup> lookupslist = _context.Lookup.Where(w => w.LookupType == LookupConstants.CLASSGROUP).ToList();
            List<Lookup> lookupsSublist = _context.Lookup.Where(w => w.LookupType == LookupConstants.CLASSGROUPSUB).ToList();
            if (keepSepAssosAssoc2.Any() || keepSepAssosAssoc1.Any())
            {
                if (keepSepAssosAssoc1.Any())
                {

                    int? keepAssocId =  keepSepAssosAssoc1.Select(kee => kee.KeepSepAssoc1Id).First();
                    assValidate.Association = lookupslist.SingleOrDefault(s => s.LookupIndex == keepAssocId)?.LookupDescription;              
                }
                else if (keepSepAssosAssoc2.Any())
                {
                     int? keepAssocId =  keepSepAssosAssoc2.Select(kee => kee.KeepSepAssoc2Id).First();
                    assValidate.Association = lookupslist.SingleOrDefault(s => s.LookupIndex == keepAssocId)?.LookupDescription;             
                }

                assValidate.ErrorMessage = AssocErrorMessage.KeepSepAssocAssigned;
                return assValidate;
            }

            IQueryable<KeepSepAssocSubset> keepSepAssosSubset = personClassification.SelectMany(
                p => _context.KeepSepAssocSubset.Where(ks =>
                    ks.KeepSepAssoc1Id == p.PersonClassificationTypeId &&
                    ks.KeepSepAssoc2SubsetId == perAssioDetail.ClassificationSubsetId &&
                    ks.DeleteFlag == 0));

            if (keepSepAssosSubset.Any())
            {
                int? keepAssocId =  keepSepAssosSubset.Select(kee => kee.KeepSepAssoc1Id).First();
                assValidate.Association = lookupslist.SingleOrDefault(s => s.LookupIndex == keepAssocId)?.LookupDescription;               
                assValidate.ErrorMessage = AssocErrorMessage.SubsetAssigned;
                return assValidate;
            }

            IQueryable<KeepSepSubsetSubset> keepSepSubsetSubset1 = personClassification.SelectMany(
                p => _context.KeepSepSubsetSubset.Where(ks =>
                    ks.KeepSepAssoc1SubsetId == p.PersonClassificationSubsetId &&
                    ks.KeepSepAssoc2SubsetId == perAssioDetail.ClassificationSubsetId &&
                    ks.DeleteFlag == 0));

            IQueryable<KeepSepSubsetSubset> keepSepSubsetSubset2 = personClassification.SelectMany(
                p => _context.KeepSepSubsetSubset.Where(ks =>
                    ks.KeepSepAssoc2SubsetId == p.PersonClassificationSubsetId &&
                    ks.KeepSepAssoc1SubsetId == perAssioDetail.ClassificationSubsetId &&
                    ks.DeleteFlag == 0));

            if (!keepSepSubsetSubset1.Any() && !keepSepSubsetSubset2.Any()) return assValidate;
            if (keepSepSubsetSubset1.Any())
            {
                 int? keepAssocId =  keepSepSubsetSubset1.Select(kee => kee.KeepSepAssoc2SubsetId).First();
                assValidate.Subset = lookupsSublist.SingleOrDefault(s => s.LookupIndex == keepAssocId)?.LookupDescription;               
            }
            else if (keepSepSubsetSubset2.Any())
            {
                 int? keepAssocId =  keepSepSubsetSubset2.Select(kee => kee.KeepSepAssoc2SubsetId).First();
                assValidate.Subset = lookupsSublist.SingleOrDefault(s => s.LookupIndex == keepAssocId)?.LookupDescription;                
            }

            assValidate.ErrorMessage = AssocErrorMessage.KeepSepSubsetAssigned;
            return assValidate;
        }

        #endregion

        #region Privilege Alert

        // To get inmate privilege details
        public List<PrivilegeAlertVm> GetPrivilegeDetails(int inmateId, int incidentId)
        {
            List<PrivilegeAlertVm> lstPrivilegeAlert =
                _context.InmatePrivilegeXref.Where(pv => pv.InmateId == inmateId
                                                         && pv.Privilege.InactiveFlag == 0
                                                         && (incidentId == 0 || pv.PrivilegeDiscLinkId == incidentId))
                                                         .Select(pv =>
                    new PrivilegeAlertVm
                    {
                        PrivilegeId = pv.PrivilegeId,
                        InmateId = pv.InmateId,
                        PrivilegeDiscLinkId = pv.PrivilegeDiscLinkId,
                        PrivilegeRemoveDateTime = pv.PrivilegeRemoveDatetime,
                        InmatePrivilegeXrefId = pv.InmatePrivilegeXrefId,
                        PrivilegeDate = pv.PrivilegeDate,
                        PrivilegeExpires = pv.PrivilegeExpires,
                        PrivilegeNote = pv.PrivilegeNote,
                        PrivilegeType = pv.Privilege.PrivilegeType,
                        PrivilegeDescription = pv.Privilege.PrivilegeDescription,
                        PrivilegeReviewFlag = pv.ReviewFlag,
                        PrivilegeReviewInterval = pv.ReviewInterval,
                        PrivilegeNextReview = pv.ReviewNext,
                        PrivilegeFlagList = pv.InmatePrivilegeXrefFlag.Select(fn => new PrivilegeFlagVm
                        {
                            PrivilegeFlagLookupId = fn.PrivilegeFlagLookupId,
                            IsFlag = fn.FlagValue == 1
                        }).ToList(),
                        PrivilegeRemoveOfficerId = pv.PrivilegeRemoveOfficerId,
                        PrivilegeRemoveNote = pv.PrivilegeRemoveNote
                    }).ToList();

            List<int> discLinkIdLst = lstPrivilegeAlert.Where(ds => ds.PrivilegeDiscLinkId > 0)
                .Select(ds => ds.PrivilegeDiscLinkId ?? 0).Distinct().ToList();

            List<int> lstPersonnelId = lstPrivilegeAlert.Select(s => s.PrivilegeRemoveOfficerId ?? 0).ToList();
            List<PersonnelVm> lstPersonDet = _personService.GetPersonNameList(lstPersonnelId);

            // To get Incident Number         
            List<PrivilegeAlertVm> dbIncidentNumber = _context.DisciplinaryIncident.Where(di =>
                discLinkIdLst.Contains(di.DisciplinaryIncidentId)).Select(di =>
                new PrivilegeAlertVm
                {
                    DisciplinaryNumber = di.DisciplinaryNumber,
                    PrivilegeDiscLinkId = di.DisciplinaryIncidentId
                }).ToList();

            lstPrivilegeAlert.ForEach(item =>
            {
                item.DisciplinaryNumber = dbIncidentNumber.SingleOrDefault(d =>
                    d.PrivilegeDiscLinkId == item.PrivilegeDiscLinkId)?.DisciplinaryNumber;
                item.PersonnelDetails = lstPersonDet.SingleOrDefault(s => s.PersonnelId == item.PrivilegeRemoveOfficerId);

            });

            return lstPrivilegeAlert.OrderByDescending(i => i.InmatePrivilegeXrefId).ToList();
        }

        // Insert or update and Remove privilege details
        public async Task<int> InsertOrUpdatePrivilegeInfo(PrivilegeAlertVm privilege)
        {
            InmatePrivilegeXref dbPrivilegeXref = null;
            if (privilege.InmatePrivilegeXrefId > 0)
            {
                dbPrivilegeXref = _context.InmatePrivilegeXref
                   .Single(p => p.InmatePrivilegeXrefId == privilege.InmatePrivilegeXrefId);
            }

            // Remove Privilege Details based on privilegeXrefId
            if (privilege.IsRemove && dbPrivilegeXref != null)
            {
                dbPrivilegeXref.PrivilegeRemoveOfficerId = _personnelId;
                dbPrivilegeXref.PrivilegeRemoveDatetime = DateTime.Now;
                dbPrivilegeXref.PrivilegeRemoveNote = privilege.PrivilegeRemoveNote;
            }
            // Update Privilege Details based on privilegeXrefId
            else
            {
                if (dbPrivilegeXref is null)
                {
                    // Insert Privilege Details
                    dbPrivilegeXref = new InmatePrivilegeXref
                    {
                        InmateId = privilege.InmateId,
                        PrivilegeOfficerId = _personnelId,
                        CreateDate = DateTime.Now
                    };
                }
                dbPrivilegeXref.PrivilegeId = privilege.PrivilegeId;
                dbPrivilegeXref.PrivilegeDate = privilege.PrivilegeDate;
                dbPrivilegeXref.UpdateBy = _personnelId;
                dbPrivilegeXref.PrivilegeExpires = privilege.PrivilegeExpires;
                dbPrivilegeXref.PrivilegeNote = privilege.PrivilegeNote;
                dbPrivilegeXref.PrivilegeDiscLinkId = privilege.PrivilegeDiscLinkId;
                dbPrivilegeXref.UpdateDate2 = DateTime.Now;
                dbPrivilegeXref.ReviewFlag = privilege.PrivilegeReviewFlag;
                dbPrivilegeXref.ReviewInterval = privilege.PrivilegeReviewInterval;
                dbPrivilegeXref.ReviewNext = privilege.PrivilegeNextReview;

                if (dbPrivilegeXref.InmatePrivilegeXrefId <= 0)
                {
                    _context.InmatePrivilegeXref.Add(dbPrivilegeXref);
                }
            }
            _context.SaveChanges();
            privilege.InmatePrivilegeXrefId = dbPrivilegeXref.InmatePrivilegeXrefId;

            //Insert InmatePrivilegeXrefFlag table
            if (privilege.PrivilegeFlagList != null && privilege.PrivilegeFlagList.Any())
            {
                InsertInmatePrivilegeXrefFlag(privilege);
            }

            // Insert PrivilegeXrefHistory table
            InmatePrivilegeXrefHistory privilegeHistory = new InmatePrivilegeXrefHistory
            {
                InmatePrivilegeXrefId = dbPrivilegeXref.InmatePrivilegeXrefId,
                PersonnelId = _personnelId,
                CreateDate = DateTime.Now,
                InmatePrivilegeXrefHistoryList = privilege.PrivilegeXrefHistoryList
            };
            _context.InmatePrivilegeXrefHistory.Add(privilegeHistory);

            return await _context.SaveChangesAsync();
        }

        private void InsertInmatePrivilegeXrefFlag(PrivilegeAlertVm privilege)
        {
            privilege.PrivilegeFlagList.ForEach(item =>
            {
                InmatePrivilegeXrefFlag dbPrivilegeXrefFlag = _context.InmatePrivilegeXrefFlag
                    .SingleOrDefault(p => p.PrivilegeFlagLookupId == item.PrivilegeFlagLookupId
                                          && p.InmatePrivilegeXrefId == privilege.InmatePrivilegeXrefId);

                if (dbPrivilegeXrefFlag is null && item.IsFlag)
                {
                    dbPrivilegeXrefFlag = new InmatePrivilegeXrefFlag
                    {
                        InmatePrivilegeXrefId = privilege.InmatePrivilegeXrefId,
                        PrivilegeFlagLookupId = item.PrivilegeFlagLookupId,
                        FlagValue = item.IsFlag ? 1 : 0,
                        CreateDate = DateTime.Now,
                        CreateBy = _personnelId
                    };
                    _context.InmatePrivilegeXrefFlag.Add(dbPrivilegeXrefFlag);
                }
                else if (!(dbPrivilegeXrefFlag is null))
                {
                    dbPrivilegeXrefFlag.FlagValue = item.IsFlag ? 1 : 0;
                    dbPrivilegeXrefFlag.UpdateDate = DateTime.Now;
                    dbPrivilegeXrefFlag.UpdateBy = _personnelId;
                }
                _context.SaveChanges();
            });
        }

        public PrivilegeIncidentDetailsVm GetPrivilegeIncidentDetails(int inmateId, int facilityId, bool isMailComponent)
        {
            PrivilegeIncidentDetailsVm privilegeVm = new PrivilegeIncidentDetailsVm
            {
                //To get Privilege List
                PrivilegeList = GetPrivilegeList(facilityId),

                //To get Disciplinary Incident List
                DisciplinaryIncidentList = GetDisciplinaryIncidentList(inmateId),

                // To Get Privilege Lookup List
                PrivilegeLookupList = GetPrivilegeLookupList()
            };
            if (isMailComponent)
            {
                privilegeVm.PrivilegeList = privilegeVm.PrivilegeList.Where(e => e.MailCoverFlag || e.MailPrivilegeFlag).ToList();
            }



            return privilegeVm;
        }

        //Get Link to Incident details
        public List<DisciplinaryIncidentDetails> GetDisciplinaryIncidentList(int inmateId)
        {
            List<DisciplinaryIncidentDetails> disciplinaryIncident =
                _context.DisciplinaryInmate.Where(fnx => fnx.InmateId == inmateId)
                    .Select(fnx => new DisciplinaryIncidentDetails
                    {
                        IncidentId = fnx.DisciplinaryIncident.DisciplinaryIncidentId,
                        DisciplinaryNumber = fnx.DisciplinaryIncident.DisciplinaryNumber,
                        DisciplinaryDate = fnx.DisciplinaryIncident.DisciplinaryIncidentDate,
                        DisciplinaryType = fnx.DisciplinaryIncident.DisciplinaryType
                    }).ToList();

            //List<LookupVm> lookUp =  _commonService.GetLookups(new[] {LookupConstants.DISCTYPE});
            List<LookupVm> lookUp = (from lu in _context.Lookup
                                     where lu.LookupType == LookupConstants.DISCTYPE
                                     select new LookupVm
                                     {
                                         LookupIndex = lu.LookupIndex,
                                         LookupDescription = lu.LookupDescription
                                     }).ToList();
            disciplinaryIncident.ForEach(a => a.DisciplinaryDescription =
                lookUp.SingleOrDefault(lk => lk.LookupIndex == a.DisciplinaryType)?.LookupDescription);

            return disciplinaryIncident;
        }

        // Get Privilege List Details
        private List<PrivilegeDetailsVm> GetPrivilegeList(int facilityId) =>
            _context.Privileges.Where(p => p.InactiveFlag == 0 && p.RemoveFromTrackingFlag == 1
                                           && (facilityId <= 0 || p.FacilityId == facilityId || p.FacilityId == null)
                                           && p.RemoveFromPrivilegeFlag == 0)
                .Select(p => new PrivilegeDetailsVm
                {
                    PrivilegeId = p.PrivilegeId,
                    PrivilegeDescription = p.PrivilegeDescription,
                    PrivilegeType = p.PrivilegeType,
                    FacilityId = p.FacilityId,
                    MailPrivilegeFlag = p.MailPrivilegeFlag,
                    MailCoverFlag = p.MailPrivilegeCoverFlag
                }).ToList();

        //Get PrivilegeXref History Details
        public List<HistoryVm> GetPrivilegeHistoryDetails(int privilegeXrefId)
        {
            List<HistoryVm> privilegeHistory = _context.InmatePrivilegeXrefHistory
                .Where(a => a.InmatePrivilegeXrefId == privilegeXrefId)
                .Select(
                    a => new HistoryVm
                    {
                        HistoryId = a.InmatePrivilegeXrefHistoryId,
                        CreateDate = a.CreateDate,
                        OfficerBadgeNumber = a.Personnel.OfficerBadgeNum,
                        PersonId = a.Personnel.PersonId,
                        HistoryList = a.InmatePrivilegeXrefHistoryList
                    }).OrderByDescending(i => i.CreateDate).ToList();

            int[] personIds = privilegeHistory.Select(p => p.PersonId).ToArray();

            List<Person> lstPersonDetails = _context.Person.Where(p => personIds.Contains(p.PersonId)).Select(p =>
                new Person
                {
                    PersonId = p.PersonId,
                    PersonLastName = p.PersonLastName,
                    PersonFirstName = p.PersonFirstName,
                    PersonMiddleName = p.PersonMiddleName
                }).ToList();

            privilegeHistory.ForEach(item =>
            {
                Person personDet = lstPersonDetails.Single(p => p.PersonId == item.PersonId);
                item.PersonLastName = personDet.PersonLastName;
                item.PersonFirstName = personDet.PersonFirstName;
                item.PersonMiddleName = personDet.PersonMiddleName;
                item.OfficerBadgeNumber = personDet.PersonNumber;
                if (item.HistoryList == null) return;
                Dictionary<string, string> personHistoryList =
                    JsonConvert.DeserializeObject<Dictionary<string, string>>(item.HistoryList);
                item.Header = personHistoryList.Select(ph => new PersonHeader { Header = ph.Key, Detail = ph.Value })
                    .ToList();
            });
            return privilegeHistory;
        }

        //Get Privilege Lookup Flag Details
        private List<PrivilegeFlagVm> GetPrivilegeLookupList()
        {
            List<PrivilegeFlagVm> privilegeLookup = _context.PrivilegeFlagLookup
                .Where(fn => fn.InactiveFlag == 0 || !fn.InactiveFlag.HasValue)
                .Select(fn => new PrivilegeFlagVm
                {
                    FlagName = fn.FlagName,
                    PrivilegeFlagLookupId = fn.PrivilegeFlagLookupId,
                    PrivilegeId = fn.PrivilegeId
                }).ToList();

            return privilegeLookup;
        }

        #endregion

        #region Observation Log

        public ObservationVm GetObservationLog(int inmateId, int deleteFlag)
        {
            ObservationVm observationVm = new ObservationVm();

            List<Lookup> lstLookup = _context.Lookup
                .Where(lk => lk.LookupType == LookupConstants.OBSTYPE && lk.LookupInactive == 0).ToList();
            IQueryable<ObservationScheduleAction> observationScheduleActions =
                _context.ObservationScheduleAction.Where(w => w.DeleteFlag == 0);


            observationVm.LstActiveSchedule = _context.ObservationSchedule.Where(w =>
                w.DeleteFlag == 0 && w.InmateId == inmateId
                  && (w.EndDate > DateTime.Now || !w.EndDate.HasValue)

                )
                .Select(s => new ActiveScheduleVm
                {
                    ObservationScheduleId = s.ObservationScheduleId,
                    StartDate = s.StartDate,
                    EndDate = s.EndDate,
                    Note = s.Note,
                    TypeName = lstLookup.Single(l => l.LookupIndex == s.ObservationType).LookupDescription,
                    ObservationType = s.ObservationType,
                    LstScheduleAction = observationScheduleActions
                    .Where(w => w.ObservationScheduleId == s.ObservationScheduleId)
                    .Select(d => new ScheduleActionVm
                    {
                        ObservationScheduleActionId = d.ObservationScheduleActionId,
                        ActionName = d.ObservationAction.ActionName,
                        ObservationActionId = d.ObservationActionId,
                        ObservationScheduleInterval = d.ObservationScheduleInterval,
                        LastReminderEntry = d.LastReminderEntry,
                        ObservationScheduleNote = d.ObservationScheduleNote,
                        ObservationLateEntryMax = d.ObservationLateEntryMax,
                        HousingUnitId = d.ObservationSchedule.Inmate.HousingUnitId
                    }).ToList()
                }).ToList();

            observationVm.LstActiveSchedule = observationVm.LstActiveSchedule.Where(w => w.StartDate <= DateTime.Now).ToList();
            observationVm.AlertObservationLogs = _context.ObservationLog.Where(w =>
                    w.ObservationScheduleAction.ObservationSchedule.InmateId == inmateId)
                .Select(s => new AlertObservationLog
                {
                    ObservationLogId = s.ObservationLogId,
                    ObservationScheduleActionId = s.ObservationScheduleActionId,
                    ObservationDateTime = s.ObservationDateTime,
                    ObservationLateEntryFlag = s.ObservationLateEntryFlag,
                    ActionAbbr = s.ObservationScheduleAction.ObservationAction.ActionAbbr,
                    CreateDate = s.CreateDate,
                    LastReminderEntryBy = s.ObservationScheduleAction.LastReminderEntryBy,
                    OfficerDetails = new PersonnelVm
                    {
                        OfficerBadgeNumber = s.CreateByNavigation.OfficerBadgeNum,
                        PersonLastName = s.CreateByNavigation.PersonNavigation.PersonLastName
                    }
                }).ToList();


            observationVm.LstInactiveSchedule = _context.ObservationScheduleAction.Where(w =>
                    w.ObservationSchedule.InmateId == inmateId
                    && (deleteFlag == 1
                        ? w.ObservationSchedule.DeleteFlag == deleteFlag ||
                        w.ObservationSchedule.DeleteFlag == 0 && w.ObservationSchedule.EndDate.HasValue &&
                        w.ObservationSchedule.EndDate < DateTime.Now
                        : w.ObservationSchedule.DeleteFlag == 0 && w.ObservationSchedule.EndDate.HasValue &&
                        w.ObservationSchedule.EndDate < DateTime.Now))
                .Select(s => new ActiveScheduleVm
                {
                    ObservationScheduleId = s.ObservationSchedule.ObservationScheduleId,
                    StartDate = s.ObservationSchedule.StartDate,
                    EndDate = s.ObservationSchedule.EndDate,
                    Note = s.ObservationSchedule.Note,
                    TypeName = lstLookup
                        .Single(l => l.LookupIndex == s.ObservationSchedule.ObservationType)
                        .LookupDescription,
                    ObservationType = s.ObservationSchedule.ObservationType,
                    ObservationActionId = s.ObservationScheduleActionId,
                    DeleteFlag = s.ObservationSchedule.DeleteFlag
                }).ToList();

            return observationVm;
        }

        //get observation schedule history details
        public List<HistoryVm> GetObservationHistory(int observationScheduleId)
        {
            List<HistoryVm> lstObservation = _context.ObservationScheduleHistory
                .Where(w => w.ObservationScheduleId == observationScheduleId)
                .OrderByDescending(ph => ph.CreateDate)
                .Select(ph => new HistoryVm
                {
                    HistoryId = ph.ObservationScheduleHistoryId,
                    CreateDate = ph.CreateDate,
                    PersonId = ph.Personnel.PersonId,
                    OfficerBadgeNumber = ph.Personnel.OfficerBadgeNum,
                    HistoryList = ph.ObservationScheduleHistoryList
                }).ToList();
            if (lstObservation.Count <= 0) return lstObservation;
            //To Improve Performence All Person Details Loaded By Single Hit Before Looping
            int[] personIds = lstObservation.Select(x => x.PersonId).ToArray();
            //get person list
            List<Person> lstPersonDet = _context.Person.Where(per => personIds.Contains(per.PersonId)).ToList();

            lstObservation.ForEach(item =>
            {
                item.PersonLastName = lstPersonDet.SingleOrDefault(p => p.PersonId == item.PersonId)?.PersonLastName;
                //To GetJson Result Into Dictionary
                if (item.HistoryList == null) return;
                Dictionary<string, string> personHistoryList =
                    JsonConvert.DeserializeObject<Dictionary<string, string>>(item.HistoryList);
                item.Header =
                    personHistoryList.Select(ph => new PersonHeader { Header = ph.Key, Detail = ph.Value })
                        .ToList();
            });
            return lstObservation;
        }

        public async Task<int> InsertObservationScheduleEntryDetails(ObservationScheduleVm scheduleDetails)
        {
            //insert observation schedule details
            ObservationSchedule dbObservationSchedule = new ObservationSchedule
            {
                ObservationType = scheduleDetails.ObservationType,
                InmateId = scheduleDetails.InmateId,
                StartDate = scheduleDetails.StartDate,
                EndDate = scheduleDetails.EndDate,
                StartBy = _personnelId,
                Note = scheduleDetails.Note,
                CreateDate = DateTime.Now,
                CreateBy = _personnelId
            };
            _context.ObservationSchedule.Add(dbObservationSchedule);

            //insert observation schedule details into history table
            ObservationScheduleHistory dbScheduledHistory = new ObservationScheduleHistory
            {
                ObservationScheduleId = dbObservationSchedule.ObservationScheduleId,
                PersonnelId = _personnelId,
                CreateDate = DateTime.Now,
                ObservationScheduleHistoryList = scheduleDetails.HistoryList
            };
            _context.ObservationScheduleHistory.Add(dbScheduledHistory);

            List<ScheduleActionVm> listObservation = _context.ObservationPolicy
                .Where(w => (w.DeleteFlag == NumericConstants.ZERO || !w.DeleteFlag.HasValue) &&
                            w.ObservationType == scheduleDetails.ObservationType)
                .Select(s => new ScheduleActionVm
                {
                    ObservationActionId = s.ObservationActionId,
                    ObservationScheduleInterval = s.ObservationPolicyInterval,
                    ObservationLateEntryMax = s.ObservationLateEntryMax,
                    ObservationScheduleNote = s.ObservationPolicyNote,
                    ObservationType = s.ObservationType
                }).ToList();
            if (listObservation.Count > 0)
            {
                List<ObservationScheduleAction> listDbScheduleAction = listObservation.Select(s =>
                    new ObservationScheduleAction
                    {
                        ObservationScheduleId = dbObservationSchedule.ObservationScheduleId,
                        ObservationActionId = s.ObservationActionId,
                        ObservationScheduleInterval = s.ObservationScheduleInterval,
                        ObservationScheduleNote = s.ObservationScheduleNote,
                        ObservationLateEntryMax = s.ObservationLateEntryMax,
                        CreateDate = DateTime.Now,
                        CreateBy = _personnelId,
                        LastReminderEntry = scheduleDetails.LastReminderEntry,
                        LastReminderEntryBy = _personnelId
                    }).ToList();
                _context.ObservationScheduleAction.AddRange(listDbScheduleAction);

                //get housing unit id from inmate table
                int? houingUnitId = _context.Inmate
                                       .Where(w => w.InmateId == scheduleDetails.InmateId)
                                       .Select(s => s.HousingUnitId).SingleOrDefault();

                List<ObservationLog> listObservationLog = listDbScheduleAction.Select(s =>
                    new ObservationLog
                    {
                        ObservationScheduleActionId = s.ObservationScheduleActionId,
                        HousingUnitId = houingUnitId,
                        ObservationDateTime = DateTime.Now,
                        ObservationLateEntryFlag = NumericConstants.ZERO,
                        ObservationNote = scheduleDetails.Note,
                        CreateDate = DateTime.Now,
                        CreateBy = _personnelId
                    }).ToList();
                _context.ObservationLog.AddRange(listObservationLog);
            }
            _interfaceEngine.Export(new ExportRequestVm
            {
                EventName = AlertsConstants.OBSERVATIONLOG,
                PersonnelId = _personnelId,
                Param1 = scheduleDetails.PersonId.ToString(),
                Param2 = dbObservationSchedule.ObservationScheduleId.ToString()
            });

            return await _context.SaveChangesAsync();
        }

        public async Task<int> UpdateObservationScheduleEntryDetails(ObservationScheduleVm scheduleDetails)
        {
            ObservationSchedule dbObservation = _context.ObservationSchedule.SingleOrDefault(w =>
                w.ObservationScheduleId == scheduleDetails.ObservationScheduleId);
            if (dbObservation is null) return await _context.SaveChangesAsync();
            dbObservation.EndDate = scheduleDetails.EndDate;
            dbObservation.Note = scheduleDetails.Note;
            dbObservation.UpdateDate = DateTime.Now;
            dbObservation.UpdateBy = _personnelId;

            InsertObservationScheduleHistory(dbObservation.ObservationScheduleId, scheduleDetails.HistoryList);
            return await _context.SaveChangesAsync();
        }

        private void InsertObservationScheduleHistory(int observationScheduleId, string historyList)
        {
            //insert observation schedule details into history table
            ObservationScheduleHistory dbScheduledHistory = new ObservationScheduleHistory
            {
                ObservationScheduleId = observationScheduleId,
                PersonnelId = _personnelId,
                CreateDate = DateTime.Now,
                ObservationScheduleHistoryList = historyList
            };
            _context.ObservationScheduleHistory.Add(dbScheduledHistory);
        }

        public async Task<int> DeleteUndoObservationScheduleEntry(ObservationScheduleVm scheduleDetails)
        {
            ObservationSchedule dbObservation =
                _context.ObservationSchedule.SingleOrDefault(w =>
                    w.ObservationScheduleId == scheduleDetails.ObservationScheduleId);
            if (dbObservation is null) return await _context.SaveChangesAsync();
            dbObservation.DeleteFlag = dbObservation.DeleteFlag == NumericConstants.ZERO
                ? NumericConstants.ONE
                : NumericConstants.ZERO;
            dbObservation.DeleteDate =
                dbObservation.DeleteFlag == NumericConstants.ZERO ? (DateTime?)null : DateTime.Now;
            dbObservation.DeleteBy = dbObservation.DeleteFlag == NumericConstants.ZERO ? (int?)null : _personnelId;

            InsertObservationScheduleHistory(dbObservation.ObservationScheduleId, scheduleDetails.HistoryList);
            return await _context.SaveChangesAsync();
        }

        public ObservationLookupDetails GetObservationLookupDetails()
        {
            ObservationLookupDetails observationLookupDetails = new ObservationLookupDetails
            {
                ListLookup = _commonService.GetLookups(new[] { LookupConstants.OBSTYPE })
            };

            if (observationLookupDetails.ListLookup.Count <= 0) return observationLookupDetails;
            int[] lstLookupIndex = observationLookupDetails.ListLookup.Select(s => s.LookupIndex)
                .ToArray();

            observationLookupDetails.ListObservationPolicy = _context.ObservationPolicy
                .Where(w => (w.DeleteFlag == NumericConstants.ZERO || !w.DeleteFlag.HasValue) &&
                            lstLookupIndex.Contains(w.ObservationType ?? 0))
                .Select(s => s.ObservationType ?? 0).ToList();
            return observationLookupDetails;
        }

        public async Task<int> UpdateScheduleActionNote(ObservationScheduleActionVm observationScheduleAction)
        {
            ObservationScheduleAction dbScheduleAction = _context.ObservationScheduleAction
                .SingleOrDefault(w =>
                    w.ObservationScheduleActionId == observationScheduleAction.ObservationScheduleActionId);
            if (dbScheduleAction is null) return await _context.SaveChangesAsync();
            dbScheduleAction.ObservationScheduleNote = observationScheduleAction.ObservationScheduleNote;
            dbScheduleAction.UpdateDate = DateTime.Now;
            dbScheduleAction.UpdateBy = _personnelId;
            return await _context.SaveChangesAsync();
        }

        public ObservationLogItemDetails LoadObservationLogDetail(int observationLogId)
        {
            List<Lookup> lstLookup = _context.Lookup
                .Where(lk => lk.LookupType == LookupConstants.OBSTYPE && lk.LookupInactive == 0).ToList();

            ObservationLogItemDetails listObservationLogs = _context.ObservationLog
                .Where(w => w.ObservationLogId == observationLogId)
                .Select(s => new ObservationLogItemDetails
                {
                    ObservationDate = s.ObservationDateTime,
                    ObservationNotes = s.ObservationNote,
                    ObservationActionName = s.ObservationScheduleAction.ObservationAction.ActionName,
                    ObservationActionNote = s.ObservationScheduleAction.ObservationScheduleNote,
                    ObservationScheduleInterval = s.ObservationScheduleAction.ObservationScheduleInterval,
                    LastReminderEntry = s.ObservationScheduleAction.LastReminderEntry,
                    ObservationScheduleId = s.ObservationScheduleAction.ObservationScheduleId,
                    ObservationLateEntryFlag = s.ObservationLateEntryFlag == 1,
                    ObservationType = lstLookup.Single(l =>
                        l.LookupIndex ==
                        s.ObservationScheduleAction.ObservationSchedule.ObservationType).LookupDescription,
                    ObservationScheduleNote = s.ObservationScheduleAction.ObservationSchedule.Note
                }).SingleOrDefault();

            return listObservationLogs;
        }

        //Get ObservationLog Alert Details
        public List<ObservationLogVm> GetObservationLog(int inmateId)
        {
            List<Lookup> lstLookup = (from lk in _context.Lookup
                                      where lk.LookupType == LookupConstants.OBSTYPE
                                      select lk).ToList();
            List<ObservationLogVm> lstObservationLogs = _context.ObservationSchedule
            .Where(w => w.DeleteFlag == 0 && w.InmateId == inmateId && w.StartDate <= DateTime.Now &&
            (w.EndDate > DateTime.Now || !w.EndDate.HasValue))
            .Select(os => new ObservationLogVm
            {
                ObservationScheduleId = os.ObservationScheduleId,
                Description = lstLookup.Find(l => l.LookupIndex == os.ObservationType).LookupDescription,
                StartDate = os.StartDate.Value,
                EndDate = os.EndDate,
                ColorCode = lstLookup.Find(l => l.LookupIndex == os.ObservationType).LookupColor
            }).OrderByDescending(o => o.ObservationScheduleId).ToList();
            return lstObservationLogs;
        }

        #endregion

        public List<AlertFlagsVm> GetAlerts(int personId, int inmateId)
        {
            //Flag
            List<Lookup> lstLookup =
                (from l in _context.Lookup
                 where !l.LookupNoAlert.HasValue && l.LookupInactive == 0
                     && (l.LookupType == LookupConstants.PERSONCAUTION
                         || l.LookupType == LookupConstants.TRANSCAUTION
                         || l.LookupType == LookupConstants.DIET)
                 select l).ToList();

            //PersonFlag
            List<PersonFlag> lstPersonFlag = _context.PersonFlag.Where(p => p.PersonId == personId && p.DeleteFlag == 0).ToList();
            //.OrderByDescending(p => p.PersonFlagId)

            //Message
            //Output: Want to show only FlagNote and Description = "MESSAGE" Color="16777215"
            List<AlertFlagsVm> lstInmateAlerts = (from pa in _context.PersonAlert
                                                  where pa.PersonId == personId && pa.ActiveAlertFlag == 1
                                                  && (!pa.ExpireDate.HasValue || pa.ExpireDate.Value.Date >= DateTime.Now.Date)
                                                  orderby pa.PersonAlertId descending
                                                  select new AlertFlagsVm
                                                  {
                                                      FlagNote = pa.Alert,
                                                      AlertType = AlertType.Message,
                                                      AlertColor = "#FFFFFF"
                                                  }).ToList();

            if (lstPersonFlag.Count > 0)
            {
                //FLAG ALWAYS
                List<AlertFlagsVm> lstPersonAlert = (from pf in lstPersonFlag
                                                     where pf.PersonFlagIndex > 0
                                                     select new AlertFlagsVm
                                                     {
                                                         FlagNote = pf.FlagNote,
                                                         AlertColor = lstLookup.SingleOrDefault(w => w.LookupType == LookupConstants.PERSONCAUTION
                                                                 && w.LookupIndex == pf.PersonFlagIndex)?.LookupColor,
                                                         AlertOrder = lstLookup.SingleOrDefault(w => w.LookupType == LookupConstants.PERSONCAUTION
                                                                 && w.LookupIndex == pf.PersonFlagIndex)?.LookupAlertOrder,
                                                         Description = lstLookup.SingleOrDefault(w => w.LookupType == LookupConstants.PERSONCAUTION
                                                                 && w.LookupIndex == pf.PersonFlagIndex)?.LookupDescription,
                                                         AssignmentReason = pf.LookupSubList?.SubListValue
                                                     }).ToList();
                lstInmateAlerts.AddRange(lstPersonAlert);

                //FLAG ACTIVE
                List<AlertFlagsVm> lstTransAlert = (from pf in lstPersonFlag
                                                    where pf.InmateFlagIndex > 0
                                                    select new AlertFlagsVm
                                                    {
                                                        FlagNote = pf.FlagNote,
                                                        AlertColor = lstLookup.Where(w => w.LookupType == LookupConstants.TRANSCAUTION &&
                                                                w.LookupIndex == pf.InmateFlagIndex).Select(s => s.LookupColor).FirstOrDefault(),
                                                        AlertOrder = lstLookup.Where(w => w.LookupType == LookupConstants.TRANSCAUTION &&
                                                                w.LookupIndex == pf.InmateFlagIndex).Select(s => s.LookupAlertOrder).FirstOrDefault(),
                                                        Description = lstLookup.Where(w => w.LookupType == LookupConstants.TRANSCAUTION &&
                                                                w.LookupIndex == pf.InmateFlagIndex).Select(s => s.LookupDescription).FirstOrDefault(),
                                                        AssignmentReason = pf.LookupSubList?.SubListValue
                                                    }).ToList();
                lstInmateAlerts.AddRange(lstTransAlert);

                //FLAG DIET
                List<AlertFlagsVm> lstDietAlert = (from pf in lstPersonFlag
                                                   where pf.DietFlagIndex > 0
                                                   select new AlertFlagsVm
                                                   {
                                                       FlagNote = pf.FlagNote,
                                                       AlertColor = lstLookup.Where(w => w.LookupType == LookupConstants.DIET &&
                                                               w.LookupIndex == pf.DietFlagIndex).Select(s => s.LookupColor).SingleOrDefault(),
                                                       AlertOrder = lstLookup.Where(w => w.LookupType == LookupConstants.DIET &&
                                                               w.LookupIndex == pf.DietFlagIndex).Select(s => s.LookupAlertOrder).SingleOrDefault(),
                                                       Description = lstLookup.Where(w => w.LookupType == LookupConstants.DIET &&
                                                               w.LookupIndex == pf.DietFlagIndex).Select(s => s.LookupDescription).SingleOrDefault(),
                                                       AssignmentReason = pf.LookupSubList?.SubListValue
                                                   }).ToList();
                lstInmateAlerts.AddRange(lstDietAlert);
            }

            //TODO Move colors to the client!
            //LOCAL HOLD
            //Output: Want to show Color="#80C0FF"/8438015 and Description="LOCAL HOLD"
            List<AlertFlagsVm> lstLocalHold = (from wh in _context.WarrantHold
                                               where wh.PersonId == personId && !wh.WarrantHoldRemoved.HasValue
                                               orderby wh.WarrantHoldId descending
                                               select new AlertFlagsVm
                                               {
                                                   AlertType = AlertType.LocalHold,
                                                   AlertColor = "#80C0FF"
                                               }).ToList();

            lstInmateAlerts.AddRange(lstLocalHold);

            //ILLEGAL ALIEN
            //Output: Want to show Color="#000018"/-2147483624 and Description="ILLEGAL ALIEN"
            AlertFlagsVm lstIllegalAlien =
                (from p in _context.Person
                 where p.PersonId == personId && p.IllegalAlienFlag
                 select new AlertFlagsVm
                 {
                     AlertType = AlertType.IllegalAlien,
                     AlertColor = "#000018"
                 }).SingleOrDefault();
            if (lstIllegalAlien != null)
            {
                lstInmateAlerts.Add(lstIllegalAlien);
            }

            //Registrant Details Color="white"
            List<AlertFlagsVm> lstRegistrantInfo =
                (from rp in _context.RegistrantPerson
                 where rp.PersonId == personId
                 orderby rp.RegistrantPersonId descending
                 select new AlertFlagsVm
                 {
                     Description = rp.RegistrantLookup.RegistrantName,
                     AlertColor = "#FFFFFF"
                 }).ToList();

            lstInmateAlerts.AddRange(lstRegistrantInfo);

            //WEEKENDER
            //Output: Want to show Color="#C0C0FF"/12632319 and Description="WEEKENDER"
            int[] incarcerationId = _context.Incarceration
                .OrderByDescending(inc => inc.IncarcerationId)
                .Where(inc => inc.InmateId == inmateId)
                .Select(inc => inc.IncarcerationId).ToArray();
            for (int i = 0; i < incarcerationId.Length; i++)
            {
                List<AlertFlagsVm> lstWeekender =
                    (from ia in _context.IncarcerationArrestXref
                     where ia.IncarcerationId == incarcerationId[i]
                           && !ia.ReleaseDate.HasValue
                           && ia.Arrest.ArrestSentenceWeekender == 1
                     select new AlertFlagsVm
                     {
                         AlertType = AlertType.Weekender,
                         AlertColor = "#C0C0FF"
                     }).ToList();

                lstInmateAlerts.AddRange(lstWeekender);
            }

            // Merge Record Alert Color="12632319"
            List<AlertFlagsVm> lstMergeAlerts = GetMergeAlerts(personId);
            lstInmateAlerts.AddRange(lstMergeAlerts);

            if (inmateId > 0)
            {
                //In Custody ALERTS
                bool inmateActive = _context.Inmate.Single(i => i.InmateId == inmateId).InmateActive == 1;
                if (inmateActive)
                {
                    List<AlertFlagsVm> lstInCustody = new List<AlertFlagsVm>
                    {
                        new AlertFlagsVm
                        {
                            AlertType = AlertType.InCustody,
                            AlertColor = "#C0C0FF"
                        }
                    };
                    lstInmateAlerts.AddRange(lstInCustody);
                }
            }

            return lstInmateAlerts.OrderByDescending(o => o.AlertOrder)
                .ThenBy(o => o.Description).ToList();
        }

        private List<AlertFlagsVm> GetMergeAlerts(int personId)
        {
            List<AlertFlagsVm> lstMergeAlerts = new List<AlertFlagsVm>();
            // Merged Record Alert
            int mergedCnt = _context.DataAoHistory.Count(d => d.HistoryType == CommonConstants.Merge.ToString()
                                                              && (d.KeepPersonId == personId ||
                                                                  d.DataPersonId == personId));
            if (mergedCnt <= 0) return lstMergeAlerts;
            var lstMerge = _context.DataAoHistory
                .Where(d => d.HistoryType == CommonConstants.Merge.ToString()
                            && (d.KeepPersonId == personId || d.DataPersonId == personId))
                .Select(d => new
                {
                    AlertType = AlertType.Merge,
                    Date = d.DataDate,
                    d.DataBy,
                    d.KeepPersonId,
                    d.DataPersonId,
                    Reason = d.DataReason,
                    FlagNote = d.DataNote
                }).ToList();

            int[] keepPersonIds = lstMerge.Select(p => p.KeepPersonId).ToArray();

            int?[] dataPersonIds = lstMerge.Select(p => p.DataPersonId).ToArray();

            var lstKeepPersons = _context.Inmate
                .Where(p => keepPersonIds.Contains(p.Person.PersonId))
                .Select(p => new
                {
                    p.Person.PersonLastName,
                    p.Person.PersonFirstName,
                    p.Person.PersonMiddleName,
                    p.Person.PersonDob,
                    p.InmateNumber,
                    p.PersonId
                }).ToList();

            var lstDataPersons = _context.Person
                .Where(p => dataPersonIds.Contains(p.PersonId))
                .Select(p => new
                {
                    p.PersonLastName,
                    p.PersonFirstName,
                    p.PersonMiddleName,
                    p.PersonDob,
                    p.PersonId
                }).ToList();

            int[] personnelIds = lstMerge.Select(p => p.DataBy).ToArray();

            List<PersonnelVm> lstPersonnel = _personService.GetPersonNameList(personnelIds.ToList());

            List<AlertFlagsVm> alerts = lstMergeAlerts;
            lstMerge.ForEach(item =>
            {
                PersonnelVm mergeBy = lstPersonnel.Where(p => p.PersonId == item.DataBy)
                    .Select(p => new PersonnelVm
                    {
                        PersonLastName = p.PersonLastName,
                        PersonFirstName = p.PersonFirstName,
                        PersonMiddleName = p.PersonMiddleName,
                        OfficerBadgeNumber = p.OfficerBadgeNumber
                    }).SingleOrDefault();

                PersonInfoVm keepPerson = lstKeepPersons.Where(p => p.PersonId == item.KeepPersonId)
                    .Select(p => new PersonInfoVm
                    {
                        PersonLastName = p.PersonLastName,
                        PersonFirstName = p.PersonFirstName,
                        PersonMiddleName = p.PersonMiddleName,
                        PersonDob = p.PersonDob,
                        InmateNumber = p.InmateNumber
                    }).SingleOrDefault();

                PersonInfoVm mergePerson = lstDataPersons.Where(p => p.PersonId == item.DataPersonId)
                    .Select(p => new PersonInfoVm
                    {
                        PersonLastName = p.PersonLastName,
                        PersonFirstName = p.PersonFirstName,
                        PersonMiddleName = p.PersonMiddleName,
                        PersonDob = p.PersonDob
                    }).SingleOrDefault();

                AlertFlagsVm alertFlagsVm = new AlertFlagsVm
                {
                    AlertType = item.AlertType,
                    Date = item.Date,
                    MergeBy = mergeBy,
                    KeepPerson = keepPerson,
                    MergePerson = mergePerson,
                    Reason = item.Reason,
                    FlagNote = item.FlagNote,
                    AlertColor = "#C0C0FF"
                };
                alerts.Add(alertFlagsVm);
            });

            lstMergeAlerts = lstMergeAlerts.GroupBy(i => new
            {
                i.Date,
                i.MergeBy,
                i.KeepPerson,
                i.MergePerson,
                i.Reason,
                i.FlagNote
            }).Select(i => i.First()).ToList();
            return lstMergeAlerts;
        }

        //Get Medical Alert Details
        public List<AlertFlagsVm> GetMedicalAlerts(int personId)
        {
            //Unrestricted PHI Medical Flag
            List<Lookup> lstLookup = (from l in _context.Lookup
                                      where !l.LookupNoAlert.HasValue && l.LookupInactive == 0
                                      && l.LookupType == LookupConstants.MEDFLAG && l.LookupFlag6 == 1
                                      select l).ToList();

            //Medical Flag
            List<AlertFlagsVm> lstMedicalAlerts = new List<AlertFlagsVm>();
            if (lstLookup.Count > 0)
            {
                lstMedicalAlerts =
                (from pf in _context.PersonFlag
                 where pf.DeleteFlag == 0 && (pf.MedicalFlagIndex ?? 0) > 0
                                          && pf.PersonId == personId
                                          && lstLookup.Any(w => w.LookupIndex == pf.MedicalFlagIndex)
                 orderby pf.PersonFlagId descending
                 select new AlertFlagsVm
                 {
                     FlagNote = pf.FlagNote,
                     AlertColor = lstLookup.Single(w => w.LookupIndex == pf.MedicalFlagIndex).LookupColor,
                     AlertOrder = lstLookup.Single(w => w.LookupIndex == pf.MedicalFlagIndex).LookupAlertOrder,
                     Description = lstLookup.Single(w => w.LookupIndex == pf.MedicalFlagIndex).LookupDescription
                 }).ToList();
            }

            return lstMedicalAlerts;
        }

        //Get Privileges Alert Details
        public List<PrivilegeDetailsVm> GetPrivilegesAlert(int inmateId) =>
            //Below query is taking 3ms and Privilege table is joined as inner join
            (from ipx in _context.InmatePrivilegeXref
             where ipx.InmateId == inmateId && ipx.PrivilegeDate < DateTime.Now
                                            && (!ipx.PrivilegeExpires.HasValue || ipx.PrivilegeExpires >= DateTime.Now)
                                            && (ipx.PrivilegeRemoveDatetime ?? DateTime.Now.AddDays(1)) > DateTime.Now
                                            && !ipx.PrivilegeRemoveOfficerId.HasValue
             orderby ipx.InmatePrivilegeXrefId descending
             select new PrivilegeDetailsVm
             {
                 PrivilegeId = ipx.PrivilegeId,
                 ExpireDate = ipx.PrivilegeExpires,
                 PrivilegeType = ipx.Privilege.PrivilegeType,
                 PrivilegeDescription = ipx.Privilege.PrivilegeDescription
             }).ToList();

        public bool CheckScheduleOverlap(DateTime dateTime, int inmateId) => _context.ObservationSchedule.Any(a =>
            a.InmateId == inmateId && a.StartDate.HasValue && a.StartDate.Value == dateTime);
    }
}
