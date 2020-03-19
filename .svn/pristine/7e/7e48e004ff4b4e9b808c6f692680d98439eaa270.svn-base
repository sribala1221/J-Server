using GenerateTables.Models;
using Microsoft.AspNetCore.Http;
using System;
using ServerAPI.Utilities;
using ServerAPI.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ServerAPI.Services
{
    public class IntakeCurrencyService : IIntakeCurrencyService
    {
        private readonly AAtims _context;
        private readonly ICommonService _commonService;
        
        private readonly int _personnelId;
        private readonly IPersonService _personService;
        private readonly IInterfaceEngineService _interfaceEngineService;

        public IntakeCurrencyService(AAtims context, ICommonService commonService,
            IHttpContextAccessor httpContextAccessor,IPersonService personService,
            IInterfaceEngineService interfaceEngineService)
        {
            _context = context;
            _personService = personService;
            _commonService = commonService;
            _personnelId = Convert.ToInt32(httpContextAccessor.HttpContext.User
                .FindFirst("personnelId")?.Value);
            _interfaceEngineService = interfaceEngineService;
        }

        public IntakeCurrencyViewerVm GetIntakeCurrencyViewer(int incarcerationId)
        {
            IntakeCurrencyViewerVm intakeCurrencyViewer = new IntakeCurrencyViewerVm
            {
                IntakeCurrency = GetIntakeCurrencyDetails(incarcerationId),

                CurrencyModifyReason = _commonService.GetLookupKeyValuePairs(LookupConstants.INTAKECURMODREAS)
            };
            return intakeCurrencyViewer;
        }

        private List<IntakeCurrencyVm> GetIntakeCurrencyDetails(int incarcerationId)
        {
            List<IntakeCurrencyVm> intakeCurrency =
                _context.IncarcerationIntakeCurrency.Where(i => i.IncarcerationId == incarcerationId).Select(i =>
                    new IntakeCurrencyVm
                    {
                        IncarcerationIntakeCurrencyId = i.IncarcerationIntakeCurrencyId,
                        IncarcerationId = i.IncarcerationId,
                        Currency01Count = i.Currency01Count,
                        Currency05Count = i.Currency05Count,
                        Currency10Count = i.Currency10Count,
                        Currency25Count = i.Currency25Count,
                        Currency50Count = i.Currency50Count,
                        Currency100Count = i.Currency100Count,
                        Currency200Count = i.Currency200Count,
                        Currency500Count = i.Currency500Count,
                        Currency1000Count = i.Currency1000Count,
                        Currency2000Count = i.Currency2000Count,
                        Currency5000Count = i.Currency5000Count,
                        Currency10000Count = i.Currency10000Count,
                        OtherAmount = i.OtherAmount,
                        OtherDescription = i.OtherDescription,
                        TotalAmount = i.TotalAmount,
                        ModifyFlag = i.ModifyFlag,
                        ModifyReason = i.ModifyReason,
                        ModifyNote = i.ModifyNote,
                        CreateBy = i.CreateBy,
                        CreateDate = i.CreateDate
                    }).OrderByDescending(o => o.IncarcerationIntakeCurrencyId).ToList();
            intakeCurrency.ForEach(cur =>
            {
                Personnel personnel =  _context.Personnel.Find(cur.CreateBy);
                Person person = _context.Person.Find(personnel.PersonId);
                cur.Personnel = new PersonnelVm
                {
                    PersonLastName = person.PersonLastName,
                    PersonFirstName = person.PersonFirstName,
                    PersonnelNumber = personnel.PersonnelNumber
                };
            });

            return intakeCurrency;
        }

        public async Task<int> InsertIntakeCurrency(IntakeCurrencyVm value)
        {
            IncarcerationIntakeCurrency incarcerationIntakeCurrency = new IncarcerationIntakeCurrency
            {
                IncarcerationId = value.IncarcerationId,
                CreateBy = _personnelId,
                CreateDate = DateTime.Now,
                Currency01Count = value.Currency01Count,
                Currency05Count = value.Currency05Count,
                Currency10Count = value.Currency10Count,
                Currency25Count = value.Currency25Count,
                Currency50Count = value.Currency50Count,
                Currency100Count = value.Currency100Count,
                Currency200Count = value.Currency200Count,
                Currency500Count = value.Currency500Count,
                Currency1000Count = value.Currency1000Count,
                Currency2000Count = value.Currency2000Count,
                Currency5000Count = value.Currency5000Count,
                Currency10000Count = value.Currency10000Count,
                OtherAmount = value.OtherAmount,
                OtherDescription = value.OtherDescription,
                TotalAmount = value.TotalAmount,
                ModifyFlag = value.ModifyFlag,
                ModifyReason = value.ModifyReason,
                ModifyNote = value.ModifyNote
            };
            _context.IncarcerationIntakeCurrency.Add(incarcerationIntakeCurrency);
            _context.SaveChanges();
            
            _interfaceEngineService.Export(new ExportRequestVm
            {
                EventName = EventNameConstants.INTAKECURRENCY,
                PersonnelId = _personnelId,
                Param1 = value.PersonId.ToString(),
                Param2 = incarcerationIntakeCurrency.IncarcerationIntakeCurrencyId.ToString()
            });

            return await _context.SaveChangesAsync();
        }

        public IntakeCurrencyPdfViewerVm GetIntakeCurrencyPdfViewer(int incarcerationId)
        {
            IntakeCurrencyPdfViewerVm intakeCurrencyPdfViewer = new IntakeCurrencyPdfViewerVm
            {
                IntakeCurrencyList = _context.IncarcerationIntakeCurrency
                    .Where(i => i.IncarcerationId == incarcerationId).Select(i =>
                        new IntakeCurrencyVm
                        {
                            IncarcerationIntakeCurrencyId = i.IncarcerationIntakeCurrencyId,
                            IncarcerationId = i.IncarcerationId,
                            Currency01Count = i.Currency01Count,
                            Currency05Count = i.Currency05Count,
                            Currency10Count = i.Currency10Count,
                            Currency25Count = i.Currency25Count,
                            Currency50Count = i.Currency50Count,
                            Currency100Count = i.Currency100Count,
                            Currency200Count = i.Currency200Count,
                            Currency500Count = i.Currency500Count,
                            Currency1000Count = i.Currency1000Count,
                            Currency2000Count = i.Currency2000Count,
                            Currency5000Count = i.Currency5000Count,
                            Currency10000Count = i.Currency10000Count,
                            OtherAmount = i.OtherAmount,
                            OtherDescription = i.OtherDescription,
                            TotalAmount = i.TotalAmount,
                            ModifyFlag = i.ModifyFlag,
                            ModifyReason = i.ModifyReason,
                            ModifyNote = i.ModifyNote,
                            CreateBy = i.CreateBy,
                            CreateDate = i.CreateDate,
                            Personnel = _context.Personnel.Where(d => d.PersonnelId == i.CreateBy).Select(
                                p => new PersonnelVm
                                {
                                    PersonLastName = p.PersonNavigation.PersonLastName,
                                    PersonFirstName = p.PersonNavigation.PersonFirstName,
                                    PersonnelNumber = p.PersonnelNumber
                                }).SingleOrDefault()
                        }).OrderByDescending(o => o.IncarcerationIntakeCurrencyId).ToList()
            };


            intakeCurrencyPdfViewer.IntakeCurrency = intakeCurrencyPdfViewer.IntakeCurrencyList.FirstOrDefault();

            intakeCurrencyPdfViewer.SentencePdfDetails = GetSentenceSummaryPdf(incarcerationId);

            return intakeCurrencyPdfViewer;
        }

        private SentencePdfDetailsVm GetSentenceSummaryPdf(int incarcerationId)
        {
            IPersonService personService = _personService;
            SentencePdfDetailsVm sentencePdfDetails = new SentencePdfDetailsVm();

            int inmateId = _context.Incarceration.Single(inm => inm.IncarcerationId == incarcerationId)
                               .InmateId ?? 0;
            Inmate dbInmateDetails = _context.Inmate.SingleOrDefault(inm => inm.InmateId == inmateId);

            if (dbInmateDetails == null) return sentencePdfDetails;

            //Get PDF Header Details
            sentencePdfDetails = new SentencePdfDetailsVm {
                AgencyName = _context.Agency.FirstOrDefault(ag => ag.AgencyJailFlag)?.AgencyName,
                StampDate = DateTime.Now,
                InmateNumber = dbInmateDetails.InmateNumber,
                PersonnelNumber = _context.Personnel.SingleOrDefault(per =>
                    per.PersonnelId == _personnelId)?.PersonnelNumber,
                OfficerName = _context.Person.FirstOrDefault(p =>
                    p.Personnel.FirstOrDefault().PersonnelId == _personnelId)?.PersonLastName,
                PersonDetails = personService.GetPersonDetails(dbInmateDetails.PersonId)
            };

            //Get Person Details
            sentencePdfDetails.PersonDetails.InmateNumber = dbInmateDetails.InmateNumber;

            return sentencePdfDetails;
        }

    
    }
}