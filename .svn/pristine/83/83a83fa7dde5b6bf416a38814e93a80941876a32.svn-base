using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GenerateTables.Models;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using ServerAPI.Utilities;
using ServerAPI.ViewModels;

// ReSharper disable once CheckNamespace
namespace ServerAPI.Services
{
    [UsedImplicitly]
    public class InmateMailService : IInmateMailService
    {

        private readonly AAtims _context;
        private readonly int _personnelId;
        private readonly IPersonService _personService;
        public InmateMailService(AAtims context,
        IHttpContextAccessor httpContextAccessor, IPersonService personService)
        {
            _context = context;
            _personnelId = Convert.ToInt32(httpContextAccessor.HttpContext.User.FindFirst("personnelId")?.Value);
            _personService = personService;
        }

        public InmateMailHousingDetailVm GetHousingDetails(int inmateId)
        {
            IQueryable<InmatePrivilegeXref> privilegeDetailsVm = _context.InmatePrivilegeXref.Where(f =>
f.Privilege.RemoveFromPrivilegeFlag == 0 && (f.Privilege.MailPrivilegeCoverFlag
|| f.Privilege.MailPrivilegeFlag) && f.Privilege.InactiveFlag == 0 && f.InmateId == inmateId);


            InmateMailHousingDetailVm inmateMailHousingDetailVm =
             _context.Inmate.Where(i => i.InmateId == inmateId)
                  .Select(s => new InmateMailHousingDetailVm
                  {
                      FacilityAbbr = s.Facility.FacilityAbbr,
                      HousingUnitListId = s.HousingUnit.HousingUnitListId,
                      HousingUnitLocation = s.HousingUnit.HousingUnitLocation,
                      HousingUnitNumber = s.HousingUnit.HousingUnitNumber,
                      HousingUnitBedLocation = s.HousingUnit.HousingUnitBedLocation ?? "",
                      HousingUnitBedNumber = s.HousingUnit.HousingUnitBedNumber ?? "",
                      HousingUnitId = s.HousingUnitId ?? 0,
                      FacilityId = s.Facility.FacilityId,
                      LocationId = s.InmateCurrentTrackId,
                      MailBinid = s.HousingUnit.HousingUnitList.MailBinid ?? 0,
                      PersonInfo = new PersonInfo
                      {
                          PersonId = s.PersonId,
                          PersonLastName = s.Person.PersonLastName,
                          PersonFirstName = s.Person.PersonFirstName,
                          PersonMiddleName = s.Person.PersonMiddleName,
                          PersonDob = s.Person.PersonDob,
                          InmateId = s.InmateId,
                          InmateNumber = s.InmateNumber
                      },
                      MailPrivilegeFlag = privilegeDetailsVm.Count(a => a.Privilege.MailPrivilegeFlag == true) > 0 ? true : false,
                      MailCoverFlag = privilegeDetailsVm.Count(a => a.Privilege.MailPrivilegeCoverFlag == true) > 0 ? true : false,
                  }).SingleOrDefault();

            return inmateMailHousingDetailVm;
        }
        public InmateMailVm GetMailRecordDefaultList(int facilityId)
        {
            InmateMailVm inmateMail = new InmateMailVm
            {
                MailBinList = GetBinDetailsList(facilityId),
                MailVendorList = LoadMailVendor(facilityId),
                MailSenderList = LoadMailSender(),
                MailRecipientList = LoadMailRecipient(),
                MailRecordList = GetMailRecordList(facilityId),
                MailVendorSubscribeList = LoadMailVendorSubscribe()
            };
            return inmateMail;

        }

        private List<MailSenderVm> LoadMailSender()
        {
            return _context.MailSender.Where(b => !b.DeleteFlag).Select(s => new MailSenderVm
            {
                MailSenderid = s.MailSenderid,
                SenderName = s.SenderName,
                SenderAddress = s.SenderAddress,
                SenderCity = s.SenderCity,
                SenderState = s.SenderState,
                SenderZip = s.SenderZip,
                SenderAlertNote = s.SenderAlertNote,
                NotAllowedFlag = s.NotAllowedFlag,
                DeleteFlag = s.DeleteFlag,
            }).OrderByDescending(s => s.MailSenderid).ToList();
        }

        private List<MailRecipientVm> LoadMailRecipient() =>
            _context.MailRecipient.Where(b => !b.DeleteFlag).Select(s => new MailRecipientVm
            {
                MailRecipientid = s.MailRecipientid,
                RecipientName = s.RecipientName,
                RecipientAddress = s.RecipientAddress,
                RecipientCity = s.RecipientCity,
                RecipientState = s.RecipientState,
                RecipientZip = s.RecipientZip,
                RecipientAlertNote = s.RecipientAlertNote,
                NotAllowedFlag = s.NotAllowedFlag,
                DeleteFlag = s.DeleteFlag,
            }).OrderByDescending(s => s.MailRecipientid).ToList();

        public List<MailBinVm> GetBinDetailsList(int facilityId) =>
            _context.MailBin.Where(b => !b.DeleteFlag && (facilityId==0 || b.FacilityId == facilityId)).Select(s => new MailBinVm
            {
                MailBinid = s.MailBinid,
                MailBinName = s.MailBinName,
                AssignedHousingString = s.AssignedHousingString,
                InmateFlag = s.InmateFlag,
                HoldBinFlag = s.HoldBinFlag,
                StaffFlag = s.StaffFlag,
                OutgoingBinFlag = s.OutgoingBinFlag,
                RefusalBinFlag = s.RefusalBinFlag,
                FacilityId = s.FacilityId,
                DeleteFlag = s.DeleteFlag,
            }).ToList();
        private List<MailVendorVm> LoadMailVendor(int facilityId)
        {

            List<Lookup> lstLookup = _context.Lookup.Where(l => new[]{
                LookupConstants.MAILTYPE}.Contains(l.LookupType)).ToList();
            List<MailVendorVm> mailVendorList = _context.MailVendor.Where(v => !v.DeleteFlag && v.FacilityId == facilityId)
                .Select(v => new MailVendorVm
                {
                    MailVendorid = v.MailVendorid,
                    FacilityId = v.FacilityId,
                    VendorName = v.VendorName,
                    VendorAddress = v.VendorAddress,
                    VendorCity = v.VendorCity,
                    VendorState = v.VendorState,
                    VendorZip = v.VendorZip,
                    NoSubscribeRefusalReason = v.NoSubscribeRefusalReason,
                    NotAllowedRefusalReason = v.NotAllowedRefusalReason,
                    AllowFlag = v.AllowFlag,
                    AllowFlagSubscribe = v.AllowFlagSubscribe,
                    AllowFlagBatch = v.AllowFlagBatch,
                    NotAllowedFlag = v.NotAllowedFlag,
                    MailType = v.MailType,
                    DeleteFlag = v.DeleteFlag,
                    MailTypeName = lstLookup.FirstOrDefault(e => e.LookupIndex == v.MailType).LookupName,
                }
            ).ToList();

            return mailVendorList;
        }

        public int InsertMailRecord(MailRecordVm value)
        {
            List<Lookup> lstLookup = _context.Lookup.Where(l => new[]{
               LookupConstants.MAILSENDERTYPE}.Contains(l.LookupType)).ToList();
            List<Lookup> sendmail = lstLookup.Where(e => e.LookupType == LookupConstants.MAILSENDERTYPE).ToList();
            MailRecord record = new MailRecord();
            MailSender sender = new MailSender();
            MailRecipient recipient = new MailRecipient();
            if (value.IncomingFlag)
            {
                if (sendmail.FirstOrDefault(e => e.LookupIndex == value.MailSenderType)?.LookupFlag7 > 0)
                {
                    if (value.MailVendor != null)
                    {
                        if (value.MailVendor.MailVendorid > 0)
                        {
                            record.MailVendorid = value.MailVendor.MailVendorid;
                        }

                    }
                }
                else
                {
                    if (value.MailSender?.SenderName != null)
                    {
                        if (value.MailSender.MailSenderid > 0)
                        {
                            sender = _context.MailSender.Find(value.MailSender.MailSenderid);
                            sender.UpdateBy = _personnelId;
                            sender.UpdateDate = DateTime.Now;
                            sender.MailSenderid = value.MailSender.MailSenderid;
                        }
                        else
                        {
                            sender.CreateBy = _personnelId;
                            sender.CreateDate = DateTime.Now;
                            sender.DeleteFlag = false;
                            sender.SenderName = value.MailSender.SenderName;
                            sender.SenderAddress = value.MailSender.SenderAddress;
                            sender.SenderCity = value.MailSender.SenderCity;
                            sender.SenderState = value.MailSender.SenderState;
                            sender.SenderZip = value.MailSender.SenderZip;
                            sender.SenderAlertNote = value.MailSender.SenderAlertNote;
                            sender.NotAllowedFlag = value.MailSender.NotAllowedFlag;
                            _context.Add(sender);

                        }
                        _context.SaveChanges();
                        record.MailSenderid = sender.MailSenderid;
                    }

                }
                record.DestinationOther = value.DestinationOther;
            }
            else
            {
                if (value.MailRecipient?.RecipientName != null)
                {
                    if (value.MailRecipient.MailRecipientid > 0)
                    {
                        recipient = _context.MailRecipient.Find(value.MailRecipient.MailRecipientid);
                        recipient.UpdateBy = _personnelId;
                        recipient.UpdateDate = DateTime.Now;
                        recipient.MailRecipientid = value.MailRecipient.MailRecipientid;
                    }
                    else
                    {
                        recipient.CreateBy = _personnelId;
                        recipient.CreateDate = DateTime.Now;
                        recipient.DeleteFlag = false;
                        recipient.RecipientName = value.MailRecipient.RecipientName;
                        recipient.RecipientAddress = value.MailRecipient.RecipientAddress;
                        recipient.RecipientCity = value.MailRecipient.RecipientCity;
                        recipient.RecipientState = value.MailRecipient.RecipientState;
                        recipient.RecipientZip = value.MailRecipient.RecipientZip;
                        recipient.RecipientAlertNote = value.MailRecipient.RecipientAlertNote;
                        recipient.NotAllowedFlag = value.MailSender.NotAllowedFlag;
                        _context.Add(recipient);

                    }
                    _context.SaveChanges();
                    record.MailRecipientid = recipient.MailRecipientid;
                }
                record.SenderOther = value.SenderOther;
            }

            record.FacilityId = value.FacilityId;
            record.OutgoingFlag = value.OutgoingFlag;
            record.MailDestination = value.MailDestination;
            record.MailType = value.MailType;
            record.MailSenderType = value.MailSenderType;
            record.InmateId = value.InmateId > 0 ? value.InmateId : null;
            record.HousingUnitId = value.HousingUnitId > 0 ? value.HousingUnitId : null;
            record.PersonnelId = value.PersonnelId > 0 ? value.PersonnelId : null;
            record.MailSearchNotAllowed = value.MailSearchNotAllowed;
            record.MailCoverFlag = value.MailCoverFlag;
            record.HoldMailFlag = value.HoldMailFlag;
            if (value.RefusalFlag)
            {
                record.RefusalFlag = value.RefusalFlag;
                record.RefusalReason = value.RefusalReason;
                record.RefusalDate = DateTime.Now;
                record.RefusalBy = _personnelId;
            }
            record.MailNote = value.MailNote;
            record.MailBinid = value.MailBinid;
            record.RefusalPrintFlag = value.RefusalPrintFlag;
            record.CreateBy = _personnelId;
            record.CreateDate = DateTime.Now;
            _context.Add(record);
            _context.SaveChanges();
            record.IncomingFlag = value.IncomingFlag;
            _context.SaveChanges();
            return record.MailRecordid;
        }

        public int UpdateMailRecord(MailRecordVm value)
        {
            List<Lookup> lstLookup = _context.Lookup.Where(l => new[]{
               LookupConstants.MAILSENDERTYPE}.Contains(l.LookupType)).ToList();
            List<Lookup> sendmail = lstLookup.Where(e => e.LookupType == LookupConstants.MAILSENDERTYPE).ToList();
            MailRecord record = _context.MailRecord.Find(value.MailRecordid);
            MailSender sender = new MailSender();
            MailRecipient recipient = new MailRecipient();

            if (value.IncomingFlag)
            {
                if (sendmail.FirstOrDefault(e => e.LookupIndex == value.MailSenderType)?.LookupFlag7 > 0)
                {
                    if (value.MailVendor != null)
                    {
                        record.MailVendorid = value.MailVendor.MailVendorid;
                        if (record.MailVendorid > 0)
                        {
                            MailVendor vendor = _context.MailVendor.Find(record.MailVendorid);
                            vendor.NotAllowedFlag = value.MailVendor.NotAllowedFlag;
                        }


                    }

                }
                else
                {
                    if (value.MailSender != null)
                    {
                        if (value.MailSender.SenderName != null)
                        {
                            if (value.MailSender.MailSenderid > 0)
                            {
                                sender = _context.MailSender.Find(value.MailSender.MailSenderid);
                                sender.UpdateBy = _personnelId;
                                sender.UpdateDate = DateTime.Now;
                                sender.MailSenderid = value.MailSender.MailSenderid;
                                sender.SenderName = value.MailSender.SenderName;
                                sender.SenderAddress = value.MailSender.SenderAddress;
                                sender.SenderCity = value.MailSender.SenderCity;
                                sender.SenderState = value.MailSender.SenderState;
                                sender.SenderZip = value.MailSender.SenderZip;
                                sender.SenderAlertNote = value.MailSender.SenderAlertNote;
                                sender.NotAllowedFlag = value.MailSender.NotAllowedFlag;
                            }
                            else
                            {
                                sender.CreateBy = _personnelId;
                                sender.CreateDate = DateTime.Now;
                                sender.DeleteFlag = false;
                                sender.SenderName = value.MailSender.SenderName;
                                sender.SenderAddress = value.MailSender.SenderAddress;
                                sender.SenderCity = value.MailSender.SenderCity;
                                sender.SenderState = value.MailSender.SenderState;
                                sender.SenderZip = value.MailSender.SenderZip;
                                sender.SenderAlertNote = value.MailSender.SenderAlertNote;
                                sender.NotAllowedFlag = value.MailSender.NotAllowedFlag;
                                _context.Add(sender);

                            }
                            _context.SaveChanges();

                        }
                        record.MailSenderid = sender.MailSenderid;
                    }

                }
                record.DestinationOther = value.DestinationOther;
            }
            else
            {
                if (value.MailRecipient != null)
                {
                    if (value.MailRecipient.RecipientName != null)
                    {
                        if (value.MailRecipient.MailRecipientid > 0)
                        {
                            recipient = _context.MailRecipient.Find(value.MailRecipient.MailRecipientid);
                            recipient.UpdateBy = _personnelId;
                            recipient.UpdateDate = DateTime.Now;
                            recipient.MailRecipientid = value.MailRecipient.MailRecipientid;
                            recipient.RecipientName = value.MailRecipient.RecipientName;
                            recipient.RecipientAddress = value.MailRecipient.RecipientAddress;
                            recipient.RecipientCity = value.MailRecipient.RecipientCity;
                            recipient.RecipientState = value.MailRecipient.RecipientState;
                            recipient.RecipientZip = value.MailRecipient.RecipientZip;
                            recipient.RecipientAlertNote = value.MailRecipient.RecipientAlertNote;
                            recipient.NotAllowedFlag = value.MailRecipient.NotAllowedFlag;
                        }
                        else
                        {
                            recipient.CreateBy = _personnelId;
                            recipient.CreateDate = DateTime.Now;
                            recipient.DeleteFlag = false;
                            recipient.RecipientName = value.MailRecipient.RecipientName;
                            recipient.RecipientAddress = value.MailRecipient.RecipientAddress;
                            recipient.RecipientCity = value.MailRecipient.RecipientCity;
                            recipient.RecipientState = value.MailRecipient.RecipientState;
                            recipient.RecipientZip = value.MailRecipient.RecipientZip;
                            recipient.RecipientAlertNote = value.MailRecipient.RecipientAlertNote;
                            recipient.NotAllowedFlag = value.MailRecipient.NotAllowedFlag;
                            _context.Add(recipient);

                        }
                        _context.SaveChanges();

                    }
                    record.MailRecipientid = recipient.MailRecipientid;

                }
                record.SenderOther = value.SenderOther;
            }
            record.MailRecordid = value.MailRecordid;
            record.FacilityId = value.FacilityId;
            record.OutgoingFlag = value.OutgoingFlag;
            record.MailDestination = value.MailDestination;
            record.MailType = value.MailType;
            record.MailSenderType = value.MailSenderType;
            record.InmateId = value.InmateId > 0 ? value.InmateId : null;
            record.HousingUnitId = value.HousingUnitId > 0 ? value.HousingUnitId : null;
            record.PersonnelId = value.PersonnelId > 0 ? value.PersonnelId : null;
            record.MailSearchNotAllowed = value.MailSearchNotAllowed;
            record.MailCoverFlag = value.MailCoverFlag;
            record.HoldMailFlag = value.HoldMailFlag;
            record.MailVendorid = record.MailVendorid > 0 ? record.MailVendorid : null;
            record.MailSenderid = record.MailSenderid > 0 ? record.MailSenderid : null;
            record.MailRecipientid = record.MailRecipientid > 0 ? record.MailRecipientid : null;
            if (value.RefusalFlag)
            {
                record.RefusalFlag = value.RefusalFlag;
                record.RefusalReason = value.RefusalReason;
                record.RefusalDate = DateTime.Now;
                record.RefusalBy = _personnelId;
            }
            else
            {
                record.RefusalFlag = false;
                record.RefusalReason = null;
            }
            if (value.SearchListId != null)
            {
                record.MailFlagString = value.SearchListId.Length > 0 ? string.Join(",", value.SearchListId) : null;
            }


            record.MailNote = value.MailNote;

            record.MailMoneyAmount = value.MailMoneyAmount;
            record.MailMoneyType = value.MailMoneyType;
            record.MailMoneyNumber = value.MailMoneyNumber;
            record.MailSearchNote = value.MailSearchNote;
            record.MailMoneyFlag = value.MailMoneyFlag;
            record.MailSearchFlag = value.MailSearchFlag;

            record.MailBinid = value.MailBinid;
            record.RefusalPrintFlag = value.RefusalPrintFlag;
            record.UpdateBy = _personnelId;
            record.UpdateDate = DateTime.Now;
            record.MailMoneyPrintFlag = value.MailMoneyPrintFlag;
            if (value.MailSearchFlag)
            {
                record.MailSearchDate = DateTime.Now;
                record.MailSearchBy = _personnelId;
            }
            _context.SaveChanges();
            record.IncomingFlag = value.IncomingFlag;
            _context.SaveChanges();
            return record.MailRecordid;
        }
        public int InsertMailVendorSubscribe(MailVendorSubscribeVm subscribeVm)
        {
            MailVendorSubscribe subscribe = new MailVendorSubscribe();

            if (subscribeVm.MailVendorSubscribeid > 0)
            {
                subscribe = _context.MailVendorSubscribe.Find(subscribeVm.MailVendorSubscribeid);
                subscribe.UpdateBy = _personnelId;
                subscribe.UpdateDate = DateTime.Now;
                subscribe.InmateId = subscribeVm.InmateId;
                subscribe.MailVendorid = subscribeVm.MailVendorid;
                subscribe.SubscribeStart = subscribeVm.SubscribeStart;
                subscribe.SubscribeEnd = subscribeVm.SubscribeEnd;
                subscribe.SubscribeNote = subscribeVm.SubscribeNote;

            }
            else
            {
                subscribe.CreateDate = DateTime.Now;
                subscribe.CreateBy = _personnelId;
                subscribe.DeleteFlag = false;
                subscribe.InmateId = subscribeVm.InmateId;
                subscribe.MailVendorid = subscribeVm.MailVendorid;
                subscribe.SubscribeNote = subscribeVm.SubscribeNote;
                subscribe.SubscribeStart = subscribeVm.SubscribeStart;
                subscribe.SubscribeEnd = subscribeVm.SubscribeEnd;
                _context.Add(subscribe);
            }
            return _context.SaveChanges();
        }


        public List<MailVendorSubscribeVm> LoadMailVendorSubscribe()
        {
            List<MailVendorSubscribeVm> subscribeList = _context.MailVendorSubscribe
            .Select(s => new MailVendorSubscribeVm
            {
                MailVendorid = s.MailVendorid,
                MailVendorSubscribeid = s.MailVendorSubscribeid,
                InmateId = s.InmateId,
                SubscribeStart = s.SubscribeStart,
                SubscribeEnd = s.SubscribeEnd,
                SubscribeNote = s.SubscribeNote,
                DeleteFlag = s.DeleteFlag,

            }).ToList();

            int[] subscribeInmateIds = subscribeList.Select(a => a.InmateId).ToArray();
            int[] vendorIds = subscribeList.Select(a => a.MailVendorid).ToArray();

            List<Inmate> inmateList = _context.Inmate.Where(i => subscribeInmateIds.Any(a => a == i.InmateId)
                                                                 && i.InmateActive == 1)
           .Select(s => new Inmate
           {
               InmateId = s.InmateId,
               PersonId = s.PersonId,
               InmateNumber = s.InmateNumber,
               HousingUnitId = s.HousingUnitId
           }).ToList();





            List<MailVendorVm> vendorList = _context.MailVendor.Where(v => vendorIds.Any(a => a == v.MailVendorid)).
            Select(s => new MailVendorVm
            {
                MailVendorid = s.MailVendorid,
                VendorName = s.VendorName,
                VendorAddress = s.VendorAddress,
                VendorCity = s.VendorCity,
                VendorState = s.VendorState,
                VendorZip = s.VendorZip,
                MailType = s.MailType,
                FacilityId = s.FacilityId,
                AllowFlagSubscribe = s.AllowFlagSubscribe,
                AllowFlagBatch = s.AllowFlagBatch,
                NotAllowedFlag = s.NotAllowedFlag
            }).ToList();

            List<Lookup> lstLookup = _context.Lookup.Where(l => new[]{
                LookupConstants.MAILTYPE}.Contains(l.LookupType)).ToList();
            List<Lookup> mailType = lstLookup.Where(l => l.LookupType == LookupConstants.MAILTYPE).ToList();

            subscribeList.ForEach(sub =>
                       {
                           Inmate inmate = inmateList.FirstOrDefault(a => a.InmateId == sub.InmateId);
                           MailVendorVm vendor = vendorList.FirstOrDefault(v => v.MailVendorid == sub.MailVendorid);
                           sub.MailTypeName = mailType.FirstOrDefault(e => vendor != null && e.LookupIndex == vendor.MailType)?.LookupName;
                           if (vendor != null)
                           {
                               sub.VendorName = vendor.VendorName;
                               sub.SubscribeVendorAddress = vendor.VendorAddress;
                               sub.SubscribeVendorCity = vendor.VendorCity;
                               sub.SubscribeVendorState = vendor.VendorState;
                               sub.SubscribeVendorZip = vendor.VendorZip;
                               sub.FacilityId = vendor.FacilityId;
                               sub.AllowFlagSubscribe = vendor.AllowFlagSubscribe;
                               sub.AllowFlagBatch = vendor.AllowFlagBatch;
                               sub.NotAllowedFlag = vendor.NotAllowedFlag;
                               sub.MailType = vendor.MailType;
                           }
                           if (inmate != null) sub.HousingUnitId = inmate.HousingUnitId;
                       });




            return subscribeList;

        }
        public MailVendorSubscribeFacilityVm GetHousingList(int facilityId)
        {
            MailVendorSubscribeFacilityVm mailVendorSubscribe = new MailVendorSubscribeFacilityVm();

            //    List<MailVendorSubscribeVm> subscribeList = LoadMailVendorSubscribe();

            List<Lookup> mailType = _context.Lookup.Where(e => e.LookupType == LookupConstants.MAILTYPE).ToList();

            int[] inmateIdLists = _context.Inmate.Where(i =>
                    i.InmateActive == 1 && i.FacilityId == facilityId).Select(a => a.InmateId).ToArray();



            List<MailVendorSubscribeHousingLocationVm> housingLocationVm = _context.MailVendorSubscribe.Where(i =>
                    inmateIdLists.Any(a => a == i.InmateId))
                   .Select(s => new MailVendorSubscribeHousingLocationVm
                   {

                       InmateId = s.InmateId,
                       LocationId = s.Inmate.InmateCurrentTrackId ?? 0,
                       Location = s.Inmate.InmateCurrentTrack,
                       HousingDetail = new HousingCapacityVm
                       {
                           HousingLocation = s.Inmate.HousingUnit.HousingUnitLocation,
                           HousingNumber = s.Inmate.HousingUnit.HousingUnitNumber,
                           HousingBedLocation = s.Inmate.HousingUnit.HousingUnitBedLocation,
                           HousingBedNumber = s.Inmate.HousingUnit.HousingUnitBedNumber,
                           HousingUnitListId = s.Inmate.HousingUnitId.HasValue ? s.Inmate.HousingUnit.HousingUnitListId : 0
                       },
                       FacilityAbbr = s.Inmate.Facility.FacilityAbbr,
                       FacilityId = s.Inmate.Facility.FacilityId,
                       Person = new PersonInfoVm
                       {
                           PersonId = s.Inmate.Person.PersonId,
                           PersonFirstName = s.Inmate.Person.PersonFirstName,
                           PersonLastName = s.Inmate.Person.PersonLastName,
                           PersonMiddleName = s.Inmate.Person.PersonMiddleName,
                           PersonSuffix = s.Inmate.Person.PersonSuffix
                       },
                       InmateNumber = s.Inmate.InmateNumber,
                       MailTypeName = mailType.First(e => e.LookupIndex == s.MailVendor.MailType).LookupName,
                       VendorName = s.MailVendor.VendorName,
                       SubscribeVendorAddress = s.MailVendor.VendorAddress,
                       SubscribeVendorCity = s.MailVendor.VendorCity,
                       SubscribeVendorState = s.MailVendor.VendorState,
                       SubscribeVendorZip = s.MailVendor.VendorZip,
                       SubscribeStart = s.SubscribeStart,
                       SubscribeEnd = s.SubscribeEnd,
                       SubscribeNote = s.SubscribeNote,
                       MailVendorSubscribeid = s.MailVendorSubscribeid,
                       MailVendorid = s.MailVendorid,
                       DeleteFlag = s.DeleteFlag,

                   }).ToList();

            mailVendorSubscribe.LstVendorSubscribeFacility = housingLocationVm;


            mailVendorSubscribe.LstMailVendorSubscribeFacilityCount = mailVendorSubscribe.LstVendorSubscribeFacility.GroupBy(g => new
            {
                g.HousingDetail.HousingLocation,
                g.HousingDetail.HousingNumber,
                g.HousingDetail.HousingUnitListId
            }).OrderBy(o => o.Key.HousingLocation).ThenBy(t => t.Key.HousingNumber).Select(s =>
                new MailVendorSubscribeHousingCountVm
                {
                    HousingUnitLocation = s.Key.HousingLocation,
                    HousingUnitNumber = s.Key.HousingNumber,
                    HousingUnitListId = s.Key.HousingUnitListId,
                    Count = s.Count(),

                }).ToList();

            return mailVendorSubscribe;
        }



        public List<MailVendorVm> MailVendorInsert(MailVendorVm value)
        {
            MailVendor mailVendor = new MailVendor
            {
                CreateBy = _personnelId,
                CreateDate = DateTime.Now,
                FacilityId = value.FacilityId,
                VendorName = value.VendorName,
                MailType = value.MailType,
                VendorAddress = value.VendorAddress,
                VendorCity = value.VendorCity,
                VendorState = value.VendorState,
                VendorZip = value.VendorZip,
                AllowFlag = value.AllowFlag,
                NotAllowedFlag = value.NotAllowedFlag,
                AllowFlagSubscribe = value.AllowFlagSubscribe,
                NoSubscribeRefusalReason = value.NoSubscribeRefusalReason,
                AllowFlagBatch = value.AllowFlagBatch,
                NotAllowedRefusalReason = value.NotAllowedRefusalReason
            };
            _context.Add(mailVendor);
            _context.SaveChanges();
            mailVendor.AllowFlag = value.AllowFlag;
            mailVendor.NotAllowedFlag = value.NotAllowedFlag;
            _context.SaveChanges();
            return LoadMailVendor(value.FacilityId);
        }


        public List<MailRecordVm> GetMailRecordList(int facilityId)
        {
            List<Lookup> lstLookup = _context.Lookup.Where(l => new[]{
                LookupConstants.MAILTYPE,LookupConstants.MAILDEST,LookupConstants.MAILSENDERTYPE,
                LookupConstants.MAILDESTOUT,LookupConstants.MAILSENDERTYPEOUT,LookupConstants.COLLECTTYPE,
                LookupConstants.MAILREFUSALREAS}.Contains(l.LookupType)).ToList();
            List<Lookup> mailDest = lstLookup.Where(e => e.LookupType == LookupConstants.MAILDEST).ToList();
            List<Lookup> mailType = lstLookup.Where(e => e.LookupType == LookupConstants.MAILTYPE).ToList();
            List<Lookup> mailSender = lstLookup.Where(e => e.LookupType == LookupConstants.MAILSENDERTYPE).ToList();
            List<Lookup> refusal = lstLookup.Where(e => e.LookupType == LookupConstants.MAILREFUSALREAS).ToList();
            List<Lookup> outgoingMailDest = lstLookup.Where(e => e.LookupType == LookupConstants.MAILDESTOUT).ToList();
            List<Lookup> outgoingMailSender = lstLookup.Where(e => e.LookupType == LookupConstants.MAILSENDERTYPEOUT).ToList();
            List<Lookup> money = lstLookup.Where(e => e.LookupType == LookupConstants.COLLECTTYPE).ToList();
            List<MailRecordVm> mailRecordList = _context.MailRecord.Where(e => facilityId == 0||e.FacilityId == facilityId).Select(v => new MailRecordVm
            {
                MailRecordid = v.MailRecordid,
                FacilityId = v.FacilityId,
                IncomingFlag = v.IncomingFlag,
                OutgoingFlag = v.OutgoingFlag,
                MailDestination = v.MailDestination,
                MailType = v.MailType,
                MailSenderType = v.MailSenderType,
                InmateId = v.InmateId ?? v.InmateId,
                HousingUnitId = v.HousingUnitId ?? v.HousingUnitId,
                PersonnelId = v.PersonnelId ?? v.PersonnelId,
                DestinationOther = v.DestinationOther,
                MailSearchNotAllowed = v.MailSearchNotAllowed,
                RefusalFlag = v.RefusalFlag,
                RefusalReason = v.RefusalReason,
                MailNote = v.MailNote,
                MailBinid = v.MailBinid,
                RefusalPrintFlag = v.RefusalPrintFlag,
                SenderOther = v.SenderOther,
                MailTypeName = mailType.First(e => e.LookupIndex == v.MailType).LookupName,
                RefusalName = v.RefusalFlag ? refusal.First(e => e.LookupIndex == v.RefusalReason).LookupName : null,
                MailSenderid = v.MailSenderid,
                MailRecipientid = v.MailRecipientid,
                MailVendorid = v.MailVendorid,
                DeleteFlag = v.DeleteFlag,
                CreateDate = v.CreateDate,
                MailCoverFlag = v.MailCoverFlag,
                HoldMailFlag = v.HoldMailFlag,
                MailFlagString = v.MailFlagString ?? "",
                MailMoneyAmount = v.MailMoneyAmount,
                MailMoneyType = v.MailMoneyType,
                MailMoneyNumber = v.MailMoneyNumber,
                MailSearchNote = v.MailSearchNote,
                MailMoneyFlag = v.MailMoneyFlag,
                MailSearchFlag = v.MailSearchFlag,
                MailSearchBy = v.MailSearchBy,
                MailDeliveredFlag = v.MailDeliveredFlag,
                RefusalBy = v.RefusalBy,
                UpdateBy = v.UpdateBy,
                CreateBy = v.CreateBy,
                MailMoneyPrintFlag = v.MailMoneyPrintFlag,
            }).ToList();

            int[] senderListIds = mailRecordList.Select(a => a.MailSenderid ?? 0).ToArray();
            int[] mailBinListIds = mailRecordList.Select(a => a.MailBinid).ToArray();
            int[] venderListIds = mailRecordList.Select(a => a.MailVendorid ?? 0).ToArray();
            int[] recipientListIds = mailRecordList.Select(a => a.MailRecipientid ?? 0).ToArray();
            int[] inmateIdLists = mailRecordList.Select(a => a.InmateId ?? 0).ToArray();
            int[] personnelLstIds = mailRecordList.Select(a => a.PersonnelId ?? 0).ToArray();

            List<MailBinVm> lstMailBn = _context.MailBin.Where(b => mailBinListIds.Any(a => a == b.MailBinid)).Select(s => new MailBinVm
            {
                MailBinid = s.MailBinid,
                MailBinName = s.MailBinName,
            }).ToList();
            List<MailSenderVm> lstMailSender = _context.MailSender
                       .Where(a => senderListIds.Any(x => x == a.MailSenderid)).Select(s =>
                     new MailSenderVm
                     {
                         MailSenderid = s.MailSenderid,
                         SenderName = s.SenderName,
                         SenderAddress = s.SenderAddress,
                         SenderCity = s.SenderCity,
                         SenderState = s.SenderState,
                         SenderZip = s.SenderZip,
                         SenderAlertNote = s.SenderAlertNote,
                         NotAllowedFlag = s.NotAllowedFlag,
                         DeleteFlag = s.DeleteFlag,
                     }).ToList();
            List<MailVendorVm> lstMailVendor = _context.MailVendor
          .Where(a => venderListIds.Any(x => x == a.MailVendorid)).Select(s =>
                    new MailVendorVm
                    {
                        MailVendorid = s.MailVendorid,
                        FacilityId = s.FacilityId,
                        VendorName = s.VendorName,
                        VendorAddress = s.VendorAddress,
                        VendorCity = s.VendorCity,
                        VendorState = s.VendorState,
                        VendorZip = s.VendorZip,
                        NoSubscribeRefusalReason = s.NoSubscribeRefusalReason,
                        NotAllowedRefusalReason = s.NotAllowedRefusalReason,
                        AllowFlag = s.AllowFlag,
                        AllowFlagSubscribe = s.AllowFlagSubscribe,
                        AllowFlagBatch = s.AllowFlagBatch,
                        NotAllowedFlag = s.NotAllowedFlag,
                    }).ToList();
            List<MailRecipientVm> lstMailRecipient = _context.MailRecipient
          .Where(i => recipientListIds.Any(a => a == i.MailRecipientid)).Select(s => new MailRecipientVm
          {
              MailRecipientid = s.MailRecipientid,
              RecipientName = s.RecipientName,
              RecipientAddress = s.RecipientAddress,
              RecipientCity = s.RecipientCity,
              RecipientState = s.RecipientState,
              RecipientZip = s.RecipientZip,
              RecipientAlertNote = s.RecipientAlertNote,
              NotAllowedFlag = s.NotAllowedFlag,
              DeleteFlag = s.DeleteFlag,
          }).ToList();
            List<Inmate> lstInmates = _context.Inmate.Where(i =>
                    inmateIdLists.Any(a => a == i.InmateId) && i.InmateActive == 1)
                 .Select(a => new Inmate
                 {
                     InmateId = a.InmateId,
                     InmateNumber = a.InmateNumber,
                     PersonId = a.PersonId,
                     InmateActive = a.InmateActive
                 }).ToList();
            int[] personIds = lstInmates.Select(a => a.PersonId).ToArray();
            List<PersonInfoVm> lstPersonInfoVms = _context.Person.Where(i =>
                    personIds.Any(a => a == i.PersonId))
                .Select(s => new PersonInfoVm
                {
                    PersonId = s.PersonId,
                    PersonLastName = s.PersonLastName,
                    PersonFirstName = s.PersonFirstName,
                    PersonMiddleName = s.PersonMiddleName,
                    PersonDob = s.PersonDob
                }).ToList();

            lstPersonInfoVms.ForEach(item =>
            {
                Inmate inmate = lstInmates.FirstOrDefault(a => a.PersonId == item.PersonId);
                if (inmate != null)
                {
                    item.InmateId = inmate.InmateId;
                    item.InmateNumber = inmate.InmateNumber;
                    item.InmateActive = inmate.InmateActive == 1;
                }
            });
            List<PersonnelVm> lstPersonnelVm = _context.Personnel.Where(i => personnelLstIds.Any(a => a == i.PersonnelId))
                .Select(a => new PersonnelVm
                {
                    PersonLastName = a.PersonNavigation.PersonLastName,
                    PersonFirstName = a.PersonNavigation.PersonFirstName,
                    PersonMiddleName = a.PersonNavigation.PersonMiddleName,
                    PersonnelId = a.PersonnelId,
                    PersonnelNumber = a.OfficerBadgeNum
                }).ToList();

            mailRecordList.ForEach(item =>
                      {
                          if (item.IncomingFlag)
                          {
                              item.DestinationName = mailDest.FirstOrDefault(e => e.LookupIndex == item.MailDestination)?.LookupName;
                              item.SenderTypeName = mailSender.FirstOrDefault(e => e.LookupIndex == item.MailSenderType)?.LookupName;
                          }
                          else
                          {
                              item.SenderTypeName = outgoingMailSender.FirstOrDefault(e => e.LookupIndex == item.MailSenderType)?.LookupName;
                              item.DestinationName = outgoingMailDest.FirstOrDefault(e => e.LookupIndex == item.MailDestination)?.LookupName;
                          }
                          item.PersonInfo = lstPersonInfoVms.SingleOrDefault(p => p.InmateId == item.InmateId);
                          item.PersonnelVm = lstPersonnelVm.SingleOrDefault(p => p.PersonnelId == item.PersonnelId);
                          item.MailSender = lstMailSender.SingleOrDefault(p => p.MailSenderid == item.MailSenderid);
                          item.MailVendor = lstMailVendor.SingleOrDefault(p => p.MailVendorid == item.MailVendorid);
                          item.MailRecipient = lstMailRecipient.SingleOrDefault(p => p.MailRecipientid == item.MailRecipientid);
                          if (lstMailBn.SingleOrDefault(p => p.MailBinid == item.MailBinid) != null)
                          {
                              item.MailBinName = lstMailBn.SingleOrDefault(p => p.MailBinid == item.MailBinid)?.MailBinName;
                          }
                          if (money.SingleOrDefault(e => e.LookupIndex == item.MailMoneyType) != null)
                          {
                              item.MailMoneyAmountTypeName = money.SingleOrDefault(e => e.LookupIndex == item.MailMoneyType)?.LookupName;
                          }
                      });

            return mailRecordList.OrderByDescending(e => e.MailRecordid).ToList();
        }
        public int DeleteMailRecord(MailRecordVm value)
        {
            MailRecord record = _context.MailRecord.Find(value.MailRecordid);
            if (value.DeleteFlag)
            {
                record.DeleteBy = _personnelId;
                record.DeleteFlag = value.DeleteFlag;
                record.DeleteDate = DateTime.Now;
                record.DeleteReason = value.DeleteReason;
            }
            else
            {
                record.DeleteFlag = value.DeleteFlag;
            }

            return _context.SaveChanges();
        }

        public int DeleteVendorSubscribe(int subscribeId)
        {
            MailVendorSubscribe subscribe = _context.MailVendorSubscribe.Find(subscribeId);
            subscribe.DeleteFlag = true;
            subscribe.DeleteDate = DateTime.Now;
            subscribe.DeleteBy = _personnelId;
            return _context.SaveChanges();
        }

        public int UndoVendorSubscribe(int subscribeId)
        {
            MailVendorSubscribe subscribe = _context.MailVendorSubscribe.Find(subscribeId);
            subscribe.DeleteFlag = false;
            subscribe.DeleteDate = DateTime.Now;
            subscribe.DeleteBy = _personnelId;
            return _context.SaveChanges();
        }
        public List<MailVendorVm> VendorForSelect()
        {
            List<MailVendorVm> mailVendorList = _context.MailVendor.Where(v => !v.DeleteFlag).
            Select(v => new MailVendorVm
            {
                MailVendorid = v.MailVendorid,
                VendorName = v.VendorName,
                AllowFlagSubscribe = v.AllowFlagSubscribe,
                DeleteFlag = v.DeleteFlag,
                MailType = v.MailType,
                FacilityId = v.FacilityId
            }).ToList();

            return mailVendorList;
        }
        public InmateMailPrivilegeVm GetPrivilegeByOfficer(int facilityId)
        {
            InmateMailPrivilegeVm inmateMailPrivilegeVm = new InmateMailPrivilegeVm();
            IQueryable<InmatePrivilegeXref> list = _context.InmatePrivilegeXref.Where(f =>
                f.Privilege.RemoveFromPrivilegeFlag == 0 && (f.Privilege.MailPrivilegeCoverFlag
                || f.Privilege.MailPrivilegeFlag) && f.Privilege.InactiveFlag == 0);

            list = list.Where(f =>
                !f.PrivilegeExpires.HasValue ||
                f.PrivilegeExpires.HasValue &&
                f.PrivilegeExpires.Value >= DateTime.Now);

            if (facilityId > 0)
            {
                list = list.Where(f => f.Inmate.FacilityId == facilityId);
            }

            List<FacilityPrivilegeVm> privilege = list.Select(s => new FacilityPrivilegeVm
            {
                //privilege inmate xref
                InmatePrivilegeXrefId = s.InmatePrivilegeXrefId,
                PrivilegeDate = s.PrivilegeDate, //Elapsed
                PrivilegeExpires = s.PrivilegeExpires, // status
                PrivilegeRemoveDateTime = s.PrivilegeRemoveDatetime,
                PrivilegeReviewFlag = s.ReviewFlag,
                PrivilegeReviewInterval = s.ReviewInterval,
                PrivilegeNextReview = s.ReviewNext,
                PrivilegeNote = s.PrivilegeNote,
                PrivilegeRemoveNote = s.PrivilegeRemoveNote,
                PrivilegeRemoveOfficerId = s.PrivilegeRemoveOfficerId,
                Incident = new FacilityPrivilegeIncidentLinkVm
                {
                    PrivilegeDiscLinkId = s.PrivilegeDiscLinkId.Value
                },
                //privilege
                PrivilegeId = s.Privilege.PrivilegeId,
                PrivilegeType = s.Privilege.PrivilegeType,
                PrivilegeDescription = s.Privilege.PrivilegeDescription,
                //Inmate
                InmateId = s.InmateId,
                PersonId = s.Inmate.Person.PersonId,
                LastName = s.Inmate.Person.PersonLastName,
                FirstName = s.Inmate.Person.PersonFirstName,
                Number = s.Inmate.InmateNumber,
                MailPrivilegeFlag = s.Privilege.MailPrivilegeFlag,
                MailPrivilegeCoverFlag = s.Privilege.MailPrivilegeCoverFlag,
            }).ToList();

            //To get incident ID
            List<int> discLinkIdLst = privilege.Where(ds => ds.Incident.PrivilegeDiscLinkId > 0)
                .Select(ds => ds.Incident.PrivilegeDiscLinkId ?? 0).Distinct().ToList();

            // To get Incident         
            List<FacilityPrivilegeIncidentLinkVm> incidentValue = _context.DisciplinaryIncident.Where(di =>
                discLinkIdLst.Contains(di.DisciplinaryIncidentId)).Select(di =>
                new FacilityPrivilegeIncidentLinkVm
                {
                    PrivilegeDiscLinkId = di.DisciplinaryIncidentId,
                    IncidentNumber = di.DisciplinaryNumber,
                    IncidentTypeId = di.DisciplinaryType,
                    IncidentDate = di.DisciplinaryIncidentDate,
                    ShortSnopsis = di.DisciplinarySynopsis,
                    ViolationNote = di.DisciplinaryInmate.Select(s => s.DisciplinaryViolationDescription)
                        .FirstOrDefault(),
                    SanctionNote = di.DisciplinaryInmate.Select(s => s.DisciplinarySanction).FirstOrDefault()
                }).ToList();
            List<int> lstPersonnelId = privilege.Where(ds => ds.PrivilegeRemoveOfficerId > 0)
                .Select(ds => ds.PrivilegeRemoveOfficerId ?? 0).ToList();
            List<PersonnelVm> lstPersonDetail = new List<PersonnelVm>();
            if (lstPersonnelId.Count > 0)
            {
                lstPersonDetail = _personService.GetPersonNameList(lstPersonnelId);
            }


            //Assigning to privilege object
            privilege.Where(f => f.Incident.PrivilegeDiscLinkId > 0 || f.PrivilegeRemoveOfficerId > 0).ToList().ForEach(i =>
            {
                i.Incident =
                    incidentValue.SingleOrDefault(f => f.PrivilegeDiscLinkId == i.Incident.PrivilegeDiscLinkId);
                if (i.Incident != null)
                    i.Incident.IncidentType = _context.Lookup.FirstOrDefault(f =>
                        f.LookupType == LookupConstants.DISCTYPE &&
                        (int?)f.LookupIndex == i.Incident.IncidentTypeId)?.LookupDescription;
                i.PersonnelDetails = lstPersonDetail.SingleOrDefault(s => s.PersonnelId == i.PrivilegeRemoveOfficerId);
            });
            inmateMailPrivilegeVm.FacilityPrivilegeList = privilege;

            inmateMailPrivilegeVm.PriviligeCounts = new List<KeyValuePair<string, int>>
            {

               new KeyValuePair<string,int>(LookupConstants.MailRevoke
               ,inmateMailPrivilegeVm.FacilityPrivilegeList.Count(w => w.MailPrivilegeFlag)),
               new KeyValuePair<string,int>(LookupConstants.MailCover,
                   inmateMailPrivilegeVm.FacilityPrivilegeList.Count(w => w.MailPrivilegeCoverFlag)),
            }.ToList();


            return inmateMailPrivilegeVm;
        }
        public List<PrebookAttachment> LoadMailAttachment(string type, int id)
        {
            List<PrebookAttachment> attachment = _context.AppletsSaved.Where(e => e.MailRecordid == id).OrderBy(a => a.CreateDate).Select(y =>
                      new PrebookAttachment
                      {
                          AttachmentId = y.AppletsSavedId,
                          AttachmentDate = y.CreateDate,
                          AttachmentDeleted = y.AppletsDeleteFlag == 1,
                          AttachmentType = y.AppletsSavedType,
                          AttachmentTitle = y.AppletsSavedTitle,
                          AttachmentDescription = y.AppletsSavedDescription,
                          AttachmentKeyword1 = y.AppletsSavedKeyword1,
                          AttachmentKeyword2 = y.AppletsSavedKeyword2,
                          AttachmentKeyword3 = y.AppletsSavedKeyword3,
                          AttachmentKeyword4 = y.AppletsSavedKeyword4,
                          AttachmentKeyword5 = y.AppletsSavedKeyword5,
                          InmatePrebookId = y.InmatePrebookId,
                          AltSentRequestId = y.AltSentRequestId,
                          InmateId = y.InmateId,
                          DisciplinaryIncidentId = y.DisciplinaryIncidentId,
                          MedicalInmateId = y.MedicalInmateId,
                          ArrestId = y.ArrestId,
                          IncarcerationId = y.IncarcerationId,
                          ProgramCaseInmateId = y.ProgramCaseInmateId,
                          GrievanceId = y.GrievanceId,
                          RegistrantRecordId = y.RegistrantRecordId,
                          FacilityId = y.FacilityId,
                          HousingUnitLocation = y.HousingUnitLocation,
                          HousingUnitNumber = y.HousingUnitNumber,
                          AltSentId = y.AltSentId,
                          ExternalInmateId = y.ExternalInmateId,
                          AttachmentFile = Path.GetFileName(y.AppletsSavedPath),
                          CreatedBy = new PersonnelVm
                          {
                              PersonLastName = y.CreatedByNavigation.PersonNavigation.PersonLastName,
                              PersonFirstName = y.CreatedByNavigation.PersonNavigation.PersonFirstName,
                              PersonMiddleName = y.CreatedByNavigation.PersonNavigation.PersonMiddleName
                          },
                          UpdateDate = y.UpdateDate,
                          UpdatedBy = new PersonnelVm
                          {
                              PersonLastName = y.UpdatedByNavigation.PersonNavigation.PersonLastName,
                              PersonFirstName = y.UpdatedByNavigation.PersonNavigation.PersonFirstName,
                              PersonMiddleName = y.UpdatedByNavigation.PersonNavigation.PersonMiddleName
                          },

                      }).ToList();
            return attachment;
        }


        public int UpdateDeliveryData(List<MailRecordVm> list)
        {
            list.ForEach(itm =>
            {
                MailRecord record = _context.MailRecord.Find(itm.MailRecordid);
                record.MailDeliveredBy = _personnelId;
                record.MailDeliveredDate = DateTime.Now;
                record.MailDeliveredFlag = true;
            });
            return _context.SaveChanges();
        }
        public List<MailRecordVm> MailSearchRecord(MailSearchRecordVm record)
        {

            List<MailRecordVm> mailRecordList = GetMailRecordList(record.FacilityId);
            mailRecordList = mailRecordList.Where(m => (!record.OutgoingFlag || m.IncomingFlag == record.IncomingFlag)
                && (!record.IncomingFlag || m.OutgoingFlag == record.OutgoingFlag)
                && (record.MailDestination == 0 || m.MailDestination == record.MailDestination)
                && (record.MailType == 0 || m.MailType == record.MailType)
                && (record.MailSenderType == 0 || m.MailSenderType == record.MailSenderType)
                && (record.MailBinid == 0 || m.MailBinid == record.MailBinid)
                 && (!record.HoldFlagHistoryFlag || m.HoldMailFlag == record.HoldFlagHistoryFlag)
                 && (!record.RefusedHistoryFlag || m.RefusalFlag == record.RefusedHistoryFlag)
                  && (!record.NotSearchedHistoryFlag || m.MailSearchNotAllowed == record.NotSearchedHistoryFlag)
                  && (!record.SearchedHistoryFlag || m.MailSearchFlag == record.SearchedHistoryFlag)
                   && (!record.MoneyFlagHistoryFlag || m.MailMoneyFlag == record.MoneyFlagHistoryFlag)
                   && (!record.DeliveredHistoryFlag || m.MailDeliveredFlag == record.DeliveredHistoryFlag)
                   && (!record.CreatedStartDate.HasValue || !record.CreatedEndDate.HasValue ||
                       record.CreatedStartDate.Value.Date <= m.CreateDate.Date
                       && record.CreatedEndDate.Value.Date >= m.CreateDate.Date)
                    && (!record.ShowDelete || m.DeleteFlag == record.ShowDelete)
                    && (record.MailIdFrom == 0 || record.MailIdTo == 0 ||
                       record.MailIdFrom <= m.MailRecordid
                       && record.MailIdTo >= m.MailRecordid)
                    && (record.MailVendorid == 0 || m.MailVendorid == record.MailVendorid)
                     && (record.RefusalReason == 0 || m.RefusalReason == record.RefusalReason)
                     && (record.CreateBy == 0 || m.CreateBy == record.CreateBy)
                    && (record.UpdateBy == 0 || m.UpdateBy == record.UpdateBy)
                     && (record.MailSearchBy == 0 || m.MailSearchBy == record.MailSearchBy)
                     && (record.RefusalBy == 0 || m.RefusalBy == record.RefusalBy)
                    && (record.HousingUnitId == 0 || m.HousingUnitId == record.HousingUnitId)
                     && (record.InmateId == 0 || m.InmateId == record.InmateId)
                     && (record.PersonnelId == 0 || m.PersonnelId == record.PersonnelId)
                      && (!record.MailMoneyFlag || m.MailMoneyFlag == record.MailMoneyFlag)
                      && (record.AmountFrom == 0 || record.AmountTo == 0 ||
                       record.AmountFrom <= m.MailMoneyAmount
                       && record.AmountTo >= m.MailMoneyAmount)
                    && (string.IsNullOrEmpty(record.MailMoneyNumber) || m.MailMoneyNumber.StartsWith(record.MailMoneyNumber))
                  && (record.MailMoneyType == null || m.MailMoneyType == record.MailMoneyType)
                   && (string.IsNullOrEmpty(record.SenderNameContains) || m.MailSender == null
                   || m.MailSender.SenderName.ToLower().StartsWith(record.SenderNameContains.ToLower()))
            && (string.IsNullOrEmpty(record.ReciepientNameContains) || m.MailRecipient == null
                   || m.MailRecipient.RecipientName.ToLower().StartsWith(record.ReciepientNameContains.ToLower()))
                ).ToList();
            if (!record.IsDate)
            {

                mailRecordList = mailRecordList.Where(e => !e.MailDeliveredFlag).ToList();

            }

            if (!string.IsNullOrEmpty(record.MailFlagString))
            {

                mailRecordList = mailRecordList.Where(e =>
                    e.MailFlagString.Split(",").Any(a => a == record.MailFlagString)
                    ).ToList();
            }
            return mailRecordList;
        }

        public List<HousingUnitVm> GetHousingNumber()
        {
            List<MailRecordVm> mailRecordList = _context.MailRecord.Select(s =>
            new MailRecordVm
            {
                MailRecordid = s.MailRecordid,
                HousingUnitId = s.HousingUnitId
            }).ToList();

            int?[] housingUnitIds = mailRecordList.Select(a => a.HousingUnitId).Distinct().ToArray();

            List<HousingUnitVm> housingList = _context.HousingUnit.Where(h => housingUnitIds.Any(a => a == h.HousingUnitId)).
               Select(s => new HousingUnitVm
               {
                   HousingUnitId = s.HousingUnitId,
                   HousingUnitLocation = s.HousingUnitLocation,
                   HousingUnitNumber = s.HousingUnitNumber
               }).ToList();

            return housingList;
        }

        public InmateMailVm LoadMailRecordByHousingId(int housingUnitListId = 0, int facilityId = 0, int housingGroupId = 0)
        {
            List<MailRecordVm> mailRecordList = GetMailRecordList(facilityId);
            List<HousingUnitList> housingUnitList = _context.HousingUnitList.Where(h => h.FacilityId == facilityId).ToList();

            if (housingUnitListId > 0)
            {
                mailRecordList = mailRecordList.Where(m => m.MailBinid == housingUnitList.FirstOrDefault(h => h.HousingUnitListId == housingUnitListId)?.MailBinid).ToList();
            }
            else
            {
                int[] housingUnitListIds = _context.HousingGroupAssign.Where(a => a.HousingGroupId == housingGroupId)
                     .Select(a => a.HousingUnitListId ?? 0)
                     .Where(a => a > 0).ToArray();
                int[] mailBinIds = housingUnitList.Where(h => housingUnitListIds.Any(a => a == h.HousingUnitListId))
                .Select(b => b.MailBinid ?? 0).ToArray();

                mailRecordList = mailRecordList.Where(m => mailBinIds.Any(a => a == m.MailBinid)).ToList();

            }
            InmateMailVm inmateMail = new InmateMailVm
            {
                MailBinList = GetBinDetailsList(facilityId),
                MailVendorList = LoadMailVendor(facilityId),
                MailSenderList = LoadMailSender(),
                MailRecipientList = LoadMailRecipient(),
                MailRecordList = mailRecordList,
                MailVendorSubscribeList = LoadMailVendorSubscribe()
            };
            return inmateMail;

        }

        public MailRecordVm GetMailRecordById(int id, int facilityId)
        {
            List<MailRecordVm> mailRecordList = GetMailRecordList(facilityId);
            MailRecordVm mailRecord = mailRecordList.First(e => e.MailRecordid == id);
            return mailRecord;
        }



    }

}