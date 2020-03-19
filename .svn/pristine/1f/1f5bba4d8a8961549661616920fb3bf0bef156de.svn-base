using System;
using System.Collections.Generic;
using System.Linq;
using GenerateTables.Models;
using ServerAPI.ViewModels;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using ServerAPI.Utilities;

namespace ServerAPI.Services
{
    public class InmateBookingCaseService : IInmateBookingCaseService
    {
        private readonly AAtims _context;
        private readonly int _personnelId;
        private readonly IBookingReleaseService _bookingReleaseService;
        private readonly ICommonService _commonService;

        public InmateBookingCaseService(AAtims context,
            IHttpContextAccessor httpContextAccessor, IBookingReleaseService bookingReleaseService,
            ICommonService commonService)
        {
            _context = context;
            _bookingReleaseService = bookingReleaseService;
            _personnelId = Convert.ToInt32(httpContextAccessor.HttpContext.User.FindFirst("personnelId")?.Value);
            _commonService = commonService;
        }

        #region Inmate Booking Bail

        //Get Inmate Booking Bail Details For Page Load
        public BookingBailDetails GetInmateBookingBailDetails(int arrestId)
        {
            //Booking Bail Info Details
            BookingBailDetails bailDetails = new BookingBailDetails
            {
                BailDetails = _context.Arrest.Where(a => a.ArrestId == arrestId).Select(s => new BailDetails
                {
                    BailAmount = s.BailAmount,
                    BailFlag = s.BailNoBailFlag == 1,
                    BailType = s.BailType,
                    BailNote = s.BailNote,
                    ArrestId = s.ArrestId
                }).SingleOrDefault(),
                BailTransactionDetails = GetBookingBailTransactionDetails(arrestId)
            };
            return bailDetails;
        }

        //Get Method For Booking Bail Transaction Details
        public List<BailTransactionDetails> GetBookingBailTransactionDetails(int arrestId)
        {
            List<PersonnelVm> lstPersonnel = _context.Personnel.Select(s => new PersonnelVm
            {
                PersonnelId = s.PersonnelId,
                PersonLastName = s.PersonNavigation.PersonLastName,
                PersonFirstName = s.PersonNavigation.PersonFirstName,
                OfficerBadgeNumber = s.OfficerBadgeNum
            }).ToList();

            //Booking Bail Transaction Details
            List<BailTransactionDetails> bailTransactionDetails = _context.BailTransaction
                .Where(b => b.ArrestId == arrestId)
                .Select(b => new BailTransactionDetails
                {
                    BailTransactionId = b.BailTransactionId,
                    VoidFlag = b.VoidFlag == 1,
                    BailReceiptNumber = b.BailReceiptNumber,
                    BailCompanyName = b.BailCompany.BailCompanyName,
                    BailCompanyId = b.BailCompanyId ?? 0,
                    PaymentTypeLookup = b.PaymentTypeLookup,
                    BailAgentId = b.BailAgentId,
                    BailPaymentNumber = b.BailPaymentNumber,
                    BailPostedBy = b.BailPostedBy,
                    BailTransactionNotes = b.BailTransactionNotes,
                    AmountPosted = b.AmountPosted,
                    CreateDate = b.CreateDate,
                    CreatedPerson = lstPersonnel.SingleOrDefault(s => s.PersonnelId == b.CreateBy)
                }).ToList();

            List<BailAgentVm> bailAgent = _context.BailAgent
                .Where(b => bailTransactionDetails.Select(w => w.BailAgentId).Contains(b.BailAgentId))
                .Select(s => new BailAgentVm
                {
                    BailAgentId = s.BailAgentId,
                    BailAgentFirstName = s.BailAgentFirstName,
                    BailAgentLastName = s.BailAgentLastName,
                    BailAgentMiddleName = s.BailAgentMiddleName,
                    BailAgentLicenseNum = s.BailAgentLicenseNum
                }).ToList();

            List<Form> formDetails = _context.FormRecord
                .Where(f => bailTransactionDetails.Select(s => s.BailTransactionId)
                    .Contains(f.BailTransactionId ?? 0))
                .Select(f => new Form
                {
                    FormRecordId = f.FormRecordId,
                    FormTemplateId = f.BailTransactionId ?? 0
                }).ToList();

            bailTransactionDetails.ForEach(item =>
            {
                item.BailAgent = bailAgent.SingleOrDefault(s => s.BailAgentId == item.BailAgentId);
                item.FormDetails = formDetails.SingleOrDefault(s => s.FormTemplateId == item.BailTransactionId);
            });
            return bailTransactionDetails;
        }

        //Delete Or Undo For Bail Transaction Table 
        public async Task<int> DeleteUndoBailTransaction(int bailTransactionId)
        {
            BailTransaction bail = _context.BailTransaction.Single(i => i.BailTransactionId == bailTransactionId);
            bail.VoidFlag = bail.VoidFlag == 1 ? 0 : 1;
            //if we put default instead of (int?)null it return 0
            bail.VoidBy = bail.VoidFlag == 1 ? _personnelId : (int?)null;
            //if we put default instead of (DateTime?)null it return default datetime
            bail.VoidDate = bail.VoidFlag == 1 ? DateTime.Now : (DateTime?)null;
            return await _context.SaveChangesAsync();
        }

        //To Get Inmate Booking Bail Save History Details
        public List<BailSaveHistory2Vm> GetBailSaveHistory(int arrestId)
        {
            List<BailSaveHistory2Vm> historyDetails = _context.BailSaveHistory2.Where(w => w.ArrestId == arrestId)
                .Select(s => new BailSaveHistory2Vm
                {
                    BailSaveHistory2Id = s.BailSaveHistory2Id,
                    BailType = s.BailType,
                    BailNote = s.BailNote,
                    BailAmount = s.BailAmount,
                    CreateBy = s.CreateBy,
                    CreateDate = s.CreateDate
                }).ToList();

            List<PersonnelVm> personnelList = _context.Personnel
                .Where(p => historyDetails.Select(h => h.CreateBy).Contains(p.PersonnelId))
                .Select(p => new PersonnelVm
                {
                    PersonnelId = p.PersonnelId,
                    PersonFirstName = p.PersonNavigation.PersonFirstName,
                    PersonLastName = p.PersonNavigation.PersonLastName,
                    OfficerBadgeNumber = p.OfficerBadgeNum
                }).ToList();

            historyDetails.ForEach(h =>
            {
                h.OfficerDetails = personnelList.Single(s => s.PersonnelId == h.CreateBy);
            });

            return historyDetails;
        }

        //Method For Bail Update
        public async Task<int> UpdateBail(BailDetails bailDetails)
        {

            //When change unsentence to sentence for the first time only           
            if (bailDetails.IsforSentencing)
            {
                List<ArrestSentenceHistoryList> arrestSentenceHistoryLt = _context.ArrestSentenceHistory
                    .Where(w => w.ArrestId == bailDetails.ArrestId)
                    .OrderBy(o => o.ArrestSentenceHistoryId)
                    .Select(s => new ArrestSentenceHistoryList
                        {
                            ArrestSentenceHistoryId = s.ArrestSentenceHistoryId,
                            ArrestSentenceHistory = s.ArrestSentenceHistoryList
                        }).ToList();
                int i = 0;
                foreach (ArrestSentenceHistoryList item in arrestSentenceHistoryLt)
                {
                    // TODO something is very strange going on here!
                    item.ArrestSentenceHistory = item.ArrestSentenceHistory.Replace("Sentence Code", "SentenceCode");

                    GetSentenceCode json = JsonConvert.DeserializeObject<GetSentenceCode>(item.ArrestSentenceHistory);

                    if (string.IsNullOrEmpty(json.SentenceCode)) continue;
                    if (json.SentenceCode.ToUpper() != "SENTENCED (SENT)") continue;
                    i++;
                    break;
                }
                if (i == arrestSentenceHistoryLt.Count)
                {
                    Arrest arrestBail = _context.Arrest.Single(s => s.ArrestId == bailDetails.ArrestId);
                    bailDetails.BailAmount = 0;
                    bailDetails.BailFlag = true;
                    bailDetails.BailType = arrestBail.BailType;
                    bailDetails.BailNote = arrestBail.BailNote;
                }
                else
                {
                    return 0;
                }
            }                      

            //Update Bail Details In Arrest Table
            Arrest arrest = _context.Arrest.Single(s => s.ArrestId == bailDetails.ArrestId);
            {
                arrest.BailAmount = bailDetails.BailAmount;
                arrest.BailType = bailDetails.BailType;
                arrest.BailNoBailFlag = bailDetails.BailFlag ? 1 : 0;
                arrest.UpdateDate = DateTime.Now;
                arrest.BailNote = bailDetails.BailNote;
            }
            await _context.SaveChangesAsync();
            //Insert For BailSaveHistory2 Table
            BailSaveHistory2 bailSaveHistory = new BailSaveHistory2
            {
                ArrestId = bailDetails.ArrestId,
                BailAmount = bailDetails.BailAmount,
                BailNoBailFlag = bailDetails.BailFlag.ToString(),
                BailType = bailDetails.BailType,
                CreateBy = _personnelId,
                BailNote = bailDetails.BailNote,
                CreateDate = DateTime.Now
            };
            int personId = _context.Arrest.Where(w => w.ArrestId == bailDetails.ArrestId).Select(i => i.Inmate.PersonId)
                .SingleOrDefault();
            _bookingReleaseService.CalculateBailTotalAmount(bailDetails.ArrestId, personId, false, true);
            _context.Add(bailSaveHistory);
            return await _context.SaveChangesAsync();
        }

        //To get Bail Company and Bail Agent Details
        public BailCompanyDetails GetBailCompanyDetails()
        {
            BailCompanyDetails bail = new BailCompanyDetails
            {
                BailCompanies = _context.BailCompany
                    .Where(w => !w.DeleteFlag.HasValue || w.DeleteFlag == 0).Select(b => new BailCompanyVm
                    {
                        BailCompanyId = b.BailCompanyId,
                        BailCompanyName = b.BailCompanyName,
                        BailCompanyBondLimit = b.BailCompanyBondLimit,
                        BailCompanyAddressNum = b.BailCompanyAddressNum,
                        BailCompanyStreetName = b.BailCompanyStreetName,
                        BailCompanyAddress = b.BailCompanyAddress,
                        BailCompanyCity = b.BailCompanyCity,
                        BailCompanyState = b.BailCompanyState,
                        BailCompanyPhone = b.BailCompanyPhone,
                        BailCompanyFax = b.BailCompanyFax
                    }).ToList()
            };

            List<KeyValuePair<int, int>> listCompanyAgent = _context.BailCompanyAgentXref
                .Where(b => b.BailAgent.BailAgentDeleteFlag != 1)
                .Select(b => new KeyValuePair<int, int>(b.BailAgentId, b.BailCompanyId)).ToList();

            bail.BailAgencies = _context.BailAgent.Where(w => w.BailAgentDeleteFlag != 1).Select(b =>
                new BailAgentVm
                {
                    BailAgentId = b.BailAgentId,
                    BailAgentLastName = b.BailAgentLastName,
                    BailAgentFirstName = b.BailAgentFirstName,
                    BailAgentMiddleName = b.BailAgentMiddleName,
                    BailAgentLicenseNum = b.BailAgentLicenseNum,
                    BailAgentLicenseExpire = b.BailAgentLicenseExpire,
                    BailCompanyIds = listCompanyAgent.Where(i => i.Key == b.BailAgentId)
                        .Select(i => new KeyValuePair<int, bool>(i.Value, true)).ToList()
                }).ToList();

            return bail;
        }

        //To Get BailAgentHistory Details
        public List<HistoryVm> GetBailAgentHistoryDetails(int bailAgentId)
        {
            List<HistoryVm> bailAgentHistory = _context.BailAgentHistory.Where(a => a.BailAgentId == bailAgentId)
                .Select(a => new HistoryVm
                {
                    HistoryId = a.BailAgentHistoryId,
                    CreateDate = a.CreateDate,
                    OfficerBadgeNumber = a.Personnel.OfficerBadgeNum,
                    PersonId = a.Personnel.PersonId,
                    HistoryList = a.BailAgentHistoryList
                }).OrderByDescending(i => i.CreateDate).ToList();

            int[] personIds = bailAgentHistory.Select(p => p.PersonId).ToArray();

            List<Person> lstPersonDetails = _context.Person.Where(p => personIds.Contains(p.PersonId)).Select(p =>
                new Person
                {
                    PersonId = p.PersonId,
                    PersonLastName = p.PersonLastName,
                    PersonFirstName = p.PersonFirstName,
                    PersonMiddleName = p.PersonMiddleName,
                    PersonNumber = p.PersonNumber
                }).ToList();

            bailAgentHistory.ForEach(item =>
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
            return bailAgentHistory;
        }

        //Insert and Update For Bail Agent
        public async Task<int> InsertUpdateBailAgent(BailAgentVm agent)
        {
            BailAgent bailAgent = _context.BailAgent.SingleOrDefault(s => s.BailAgentId == agent.BailAgentId);
            if (bailAgent == null)
            {
                //Insert For Bail Agent
                bailAgent = new BailAgent
                {
                    BailAgentCreateBy = _personnelId,
                    BailAgentCreateDate = DateTime.Now
                };
            }
            else
            {
                //Update For Bail Agent
                bailAgent.BailAgentUpdateBy = _personnelId;
                bailAgent.BailAgentUpdateDate = DateTime.Now;
            }
            //Common Fields For Insert and Update
            bailAgent.BailAgentLastName = agent.BailAgentLastName;
            bailAgent.BailAgentFirstName = agent.BailAgentFirstName;
            bailAgent.BailAgentMiddleName = agent.BailAgentMiddleName;
            bailAgent.BailAgentLicenseNum = agent.BailAgentLicenseNum;
            bailAgent.BailAgentLicenseExpire = agent.BailAgentLicenseExpire;
            if (bailAgent.BailAgentId == 0)
            {
                _context.BailAgent.Add(bailAgent);
                agent.BailCompanyIds.ForEach(id =>
                {
                    (int key, bool value) = id;
                    int count = _context.BailCompanyAgentXref.Count(b =>
                        b.BailAgentId == bailAgent.BailAgentId && b.BailCompanyId == key);
                    if (count != 0 || !value) return;
                    //Insert For BailCompanyAgentXref Table
                    BailCompanyAgentXref xref = new BailCompanyAgentXref
                    {
                        BailCompanyId = key,
                        BailAgentId = bailAgent.BailAgentId,
                        CreateBy = _personnelId,
                        CreateDate = DateTime.Now
                    };
                    _context.BailCompanyAgentXref.Add(xref);
                });
            }
            else
            {
                agent.BailCompanyIds.ForEach(id =>
                {
                    (int key, bool value) = id;
                    int count = _context.BailCompanyAgentXref.Count(b =>
                        b.BailAgentId == bailAgent.BailAgentId && b.BailCompanyId == key);
                    if (count == 0 && value)
                    {
                        //Insert For BailCompanyAgentXref Table
                        BailCompanyAgentXref xref = new BailCompanyAgentXref
                        {
                            BailCompanyId = key,
                            BailAgentId = bailAgent.BailAgentId,
                            CreateBy = _personnelId,
                            CreateDate = DateTime.Now
                        };
                        _context.BailCompanyAgentXref.Add(xref);
                    }
                    else
                    {
                        if (count == 0 || value) return;
                        BailCompanyAgentXref xref = _context.BailCompanyAgentXref.Single(i =>
                            i.BailCompanyId == key && i.BailAgentId == bailAgent.BailAgentId);
                        _context.BailCompanyAgentXref.Remove(xref);
                    }
                });
            }

            //Insert data to BailAgentHistory table
            BailAgentHistory agentHistory = new BailAgentHistory
            {
                BailAgentId = bailAgent.BailAgentId,
                PersonnelId = _personnelId,
                CreateDate = DateTime.Now,
                BailAgentHistoryList = agent.BailAgentHistoryList
            };
            _context.BailAgentHistory.Add(agentHistory);

            return await _context.SaveChangesAsync();
        }

        //Delete For Bail Agent Table
        public async Task<int> DeleteBailAgent(int bailAgentId)
        {
            BailAgent agent = _context.BailAgent.Single(a => a.BailAgentId == bailAgentId);
            agent.BailAgentDeleteBy = _personnelId;
            agent.BailAgentDeleteFlag = 1;
            agent.BailAgentDeleteDate = DateTime.Now;
            return await _context.SaveChangesAsync();
        }

        //Insert For Bail Transaction Table
        public async Task<KeyValuePair<int, BailTransaction>> InsertBailTransaction(BailTransactionDetails bailTransaction)
        {
            BailTransaction bail = new BailTransaction
            {
                BailAgentId = bailTransaction.BailAgentId,
                BailCompanyId = bailTransaction.BailCompanyId,
                BailPaymentNumber = bailTransaction.BailPaymentNumber,
                BailTransactionNotes = bailTransaction.BailTransactionNotes,
                PaymentTypeLookup = bailTransaction.PaymentTypeLookup,
                BailReceiptNumber = _commonService.GetGlobalNumber(NumericConstants.SIX),
                BailPostedBy = bailTransaction.BailPostedBy,
                AmountPosted = bailTransaction.AmountPosted,
                ArrestId = bailTransaction.ArrestId,
                CreateBy = _personnelId,
                CreateDate = DateTime.Now
            };
            _context.BailTransaction.Add(bail);
            await _context.SaveChangesAsync();
            int templateId = _context.FormTemplates.Where(f =>
                f.FormCategoryId == (int?)FormCategories.BailReceipt &&
                (!f.Inactive.HasValue || f.Inactive == 0)).Select(f => f.FormTemplatesId).SingleOrDefault();
            return new KeyValuePair<int, BailTransaction>(templateId, bail);
        }

        //For Getting Booking Bail Amount Details
        public BailDetails GetBookingBailAmount(int arrestId, int personId)
        {
            //SiteOptions value for CrimeId
            int siteOptionId = _context.SiteOptions.Where(s =>
                s.SiteOptionsName == SiteOptionsConstants.HIGHESTBAILPERBOOKING &&
                s.SiteOptionsValue == SiteOptionsConstants.ON && s.SiteOptionsStatus == "1")
                .Select(s => s.SiteOptionsId).FirstOrDefault();

            BailDetails bail = _bookingReleaseService.BailAmount(arrestId, personId, siteOptionId);
            //Calculated bail Amount
            bail.CalculatedBailAmount = bail.BailAmount;
            //bail amount from bail table
            bail.BailAmount = _context.Arrest.Single(a => a.ArrestId == arrestId).BailAmount;
            //for no bail checking
            int count = _context.Crime.Count(c =>
                c.CrimeDeleteFlag == 0 && c.ArrestId == arrestId && c.BailType == BailType.NOBAIL) +
                _context.CrimeForce.Count(c => c.DeleteFlag == 0 && c.ArrestId == arrestId &&
                    (!c.ForceSupervisorReviewFlag.HasValue || c.ForceSupervisorReviewFlag == 0) &&
                    c.WarrantId == null && c.BailNoBailFlag == 1);
            if (count > 0)
            {
                bail.BailFlag = true;
            }
            return bail;
        }

        #endregion

    }
}
