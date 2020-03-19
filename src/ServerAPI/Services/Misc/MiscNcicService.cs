using GenerateTables.Models;
using Microsoft.AspNetCore.Http;
using ServerAPI.Interfaces;
using ServerAPI.Utilities;
using ServerAPI.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
namespace ServerAPI.Services
{
    public class MiscNcicService : IMiscNcicService
    {
        private readonly AAtims _context;
        private readonly ICommonService _commonService;
        private readonly IPersonIdentityService _personIdentityService;
        private readonly IHttpContextAccessor _iHttpContextAccessor;
        private readonly IBookingService _iBookingService;
        private readonly IPersonCharService _personChar;

        public MiscNcicService(AAtims context, ICommonService commonService,
            IPersonIdentityService personIdentityService, IHttpContextAccessor httpContextAccessor,
            IBookingService iBookingService, IPersonCharService personChar)
        {
            _context = context;
            _commonService = commonService;
            _personIdentityService = personIdentityService;
            _iHttpContextAccessor = httpContextAccessor;
            _iBookingService = iBookingService;
            _personChar = personChar;
        }

        public NcicVm GetNcicDetails(int inmateId, int personId)
        {
            NcicVm nvm = new NcicVm
            {
                SiteOption = _commonService.GetSiteOptionValue(SiteOptionsConstants.NCIC_RUN),
                LstNcicType =
                    _commonService.GetLookupKeyValuePairs(LookupConstants.NCICREQTRANS),
                PersonDetails = _personIdentityService.GetPersonIdentity(personId),
                LstState = _commonService.GetLookupList(LookupConstants.STATE)
                    .Select(t => new KeyValuePair<int, string>(t.LookupIndex,
                        t.LookupName)).ToList(),
                LstGender = _commonService.GetLookupKeyValuePairs(LookupConstants.SEX),
                LstIncarcerationAndBooking = _iBookingService.GetIncarcerationAndBookings(inmateId, true),
                PersonChar = _personChar.GetCharDetails(personId),
                InmateActive = inmateId > 0 ? _context.Inmate.Single(s => s.InmateId == inmateId).InmateActive : 0
            };

            return nvm;
        }

        public Task<int> DeleteExternalAttachment(int appletSavedId)
        {
            int personnelId = Convert.ToInt32(_iHttpContextAccessor.HttpContext.User.FindFirst("personnelId")?.Value);

            AppletsSaved updateRecordsCheck =
                _context.AppletsSaved.Single(r => r.AppletsSavedId == appletSavedId);
            updateRecordsCheck.AppletsDeleteFlag = 1;
            updateRecordsCheck.DeletedBy = personnelId;
            updateRecordsCheck.DeleteDate = DateTime.Now;
            updateRecordsCheck.DeletedByNavigation = _context.Personnel.Single(per => per.PersonnelId == personnelId);

            return _context.SaveChangesAsync();
        }
    }

}