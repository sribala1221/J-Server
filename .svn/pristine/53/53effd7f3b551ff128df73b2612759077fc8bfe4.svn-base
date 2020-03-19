using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using GenerateTables.Models;
using ServerAPI.ViewModels;
using System.Threading.Tasks;
using ServerAPI.Utilities;
using System.Text.RegularExpressions;

namespace ServerAPI.Services
{
	public class SearchService : ISearchService
	{

		private readonly AAtims _context;
	    private readonly IPhotosService _photos;

        public SearchService(AAtims context, IPhotosService photosService)
		{
			_context = context;
		    _photos = photosService;
		}

		private List<FilteredInmateDetails> _filteredInmateDetailLst = new List<FilteredInmateDetails>();
		private List<SearchResult> _personDetailsLst = new List<SearchResult>();
		private SearchRequestVm _searchDetails = new SearchRequestVm();
		private List<int> _personFilteredIdLst = new List<int>();
		private List<int> _arrestIds = new List<int>();
		private Dictionary<int, int> _inmateLstValue = new Dictionary<int, int>();
		private List<Lookup> _lstLookUp;

		// Get Intake Search Details based on requested information
		public List<SearchResult> GetBookingSearchList(SearchRequestVm searchDetails)
		{
			_searchDetails = searchDetails;

			if (_searchDetails == null) return _personDetailsLst;
			if (searchDetails.IsArrestSearch || !searchDetails.activeCasesOnly)
			{
				LoadFilteredArrestIds();

				//Load IncarcerationArrestXref Search Details
				LoadArrestIdFromArrestXrefSearch();
			}
			

			if (searchDetails.IsInmateSearch || searchDetails.activeBookingOnly)
			{
				LoadInmateSearch();
			}

			if (searchDetails.IsPersonSearch)
			{
				LoadPersonSearch();
			}
			
			if (searchDetails.IsCharSearch)
			{
				LoadPersonByCharSearch();
			}
			
			//Person Descriptor Search
			if (!string.IsNullOrEmpty(searchDetails.Descriptor) || !string.IsNullOrEmpty(searchDetails.Location) ||
				!string.IsNullOrEmpty(searchDetails.ItemCode) || !string.IsNullOrEmpty(searchDetails.Category))
			{
				LoadPersonDescriptorSearch();
			}

			//Charges Search based on Crime and CrimeLookUp table
			if (!string.IsNullOrEmpty(searchDetails.ChargeType) ||
				!string.IsNullOrEmpty(searchDetails.ChargeSection)
				|| !string.IsNullOrEmpty(searchDetails.ChargeDescription) || searchDetails.ChargeGroup > 0)
			{
				LoadChargesSearchDetails();
			}

			_arrestIds = _arrestIds.Count > 20000 ? _arrestIds.Take(20000).ToList() : _arrestIds;
			_inmateLstValue = _inmateLstValue.Count > 20000 ? _inmateLstValue.Take(20000)
				.ToDictionary(de=>de.Key,de=>de.Value) : _inmateLstValue;
			_personFilteredIdLst = _personFilteredIdLst.Count > 20000 ? 
				_personFilteredIdLst.Take(20000).ToList() : _personFilteredIdLst;

			// Load Filtered Inmate Details
			
			LoadFilteredInmateDetails();
			
			if (searchDetails.IsAddressSearch)
			{
				LoadAddressSearchDetails();
			}
			if(searchDetails.isInmateProfileSearch)
			{
				LoadProfileSearchDetails();
			}
			if(searchDetails.isInmateRelatedSearch)
			{
				LoadInmateRelatedDetails();
			}
			if (_filteredInmateDetailLst.Any())
			{
				LoadFilteredPersonDetails();
			}

			if (_personDetailsLst.Any())
			{
				LoadInmateAndArrestDetails();
				LoadInmateClassificationDetails();
			}
			return _personDetailsLst;
		}

		// Filter Inmate and Arrest based on requested Arrest search details

		private void LoadFilteredArrestIds()
		{
			IQueryable<Arrest> arrestList = _context.Arrest.Where(arre => arre.InmateId > 0 &&
				 arre.ArrestActive == 1);
				 //!_searchDetails.ActiveOnly || !_searchDetails.IsBookingSearch ||
			if(_searchDetails.activeCasesOnly)
			{
				List<int?> incArrrestIds=_context.IncarcerationArrestXref.Where(q=>q.ReleaseDate ==null).Select(q=>q.ArrestId).ToList();
				arrestList=arrestList.Where(arr=> incArrrestIds != null && incArrrestIds.Contains(arr.ArrestId));
			}
			else if(_searchDetails.isCaseSearch)
			{
				List<int?> incArrrestIds=_context.IncarcerationArrestXref.Where(q=>q.ReleaseDate !=null).Select(q=>q.ArrestId).ToList();
				arrestList=arrestList.Where(arr=> incArrrestIds.Contains(arr.ArrestId));
			}

			if (!string.IsNullOrEmpty(_searchDetails.BookingNumber))
			{
				arrestList = arrestList.Where(ar => ar.ArrestBookingNo.Contains(_searchDetails.BookingNumber));
			}
			if (!string.IsNullOrEmpty(_searchDetails.CaseNumber))
			{
				arrestList = arrestList.Where(ar => ar.ArrestCaseNumber.Contains(_searchDetails.CaseNumber));
			}
			if (!string.IsNullOrEmpty(_searchDetails.CourtDocket))
			{
				arrestList = arrestList.Where(ar => ar.ArrestCourtDocket.Contains(_searchDetails.CourtDocket));
			}
			if (_searchDetails.BookingType > 0)
			{
				arrestList = arrestList.Where(arl => arl.ArrestType == _searchDetails.BookingType.ToString());
			}
			if (!string.IsNullOrEmpty(_searchDetails.SiteBooking) && arrestList.Any())
			{
				arrestList = arrestList.Where(arl => arl.ArrestSiteBookingNo.Contains(_searchDetails.SiteBooking));
			}

			if (_searchDetails.BillingAgency > 0)
			{
				arrestList = arrestList.Where(arl => arl.ArrestBillingAgencyId == _searchDetails.BillingAgency);
			}

			if (_searchDetails.Court > 0)
			{
				arrestList = arrestList.Where(arl => arl.ArrestCourtJurisdictionId == _searchDetails.Court);
			}

			if (_searchDetails.ArrestingAgency > 0)
			{
				arrestList = arrestList.Where(arl => arl.ArrestingAgencyId == _searchDetails.ArrestingAgency);
			}

			if (_searchDetails.ArrestDateFrom.HasValue) {
                arrestList = _searchDetails.ArrestDateTo.HasValue
                    ? arrestList.Where(arl => arl.ArrestDate.HasValue &&
                                              arl.ArrestDate.Value.Date >= _searchDetails.ArrestDateFrom.Value.Date &&
                                              arl.ArrestDate.Value.Date <= _searchDetails.ArrestDateTo.Value.Date)
                    : arrestList.Where(arl => arl.ArrestDate.HasValue &&
                                              arl.ArrestDate.Value.Date == _searchDetails.ArrestDateFrom.Value.Date);
            }

			//Filter based on Arresting and Booking Officer
			if (_searchDetails.ArrestingOfficer > 0)
			{
				arrestList = arrestList.Where(arl => arl.ArrestOfficerId == _searchDetails.ArrestingOfficer);
			}
			if(_searchDetails.arrestingOfficerName>0)
			{
				arrestList = arrestList.Where(arl => arl.ArrestOfficerId == _searchDetails.arrestingOfficerName);
			}

			if (_searchDetails.BookingOfficer > 0)
			{
				arrestList = arrestList.Where(arl => arl.ArrestBookingOfficerId == _searchDetails.BookingOfficer);
			}
			if(_searchDetails.caseBookingFrom.HasValue && _searchDetails.caseBookingTo.HasValue)
            {
                arrestList =
                    arrestList.Where(inc => inc.ArrestBookingDate >= _searchDetails.caseBookingFrom && inc.ArrestBookingDate<=_searchDetails.caseBookingTo);
            }
            if(_searchDetails.caseClearFrom.HasValue && _searchDetails.caseClearTo.HasValue)
            {
                arrestList =
                    arrestList.Where(inc => inc.ArrestReleaseClearedDate >= _searchDetails.caseClearFrom && inc.ArrestReleaseClearedDate<=_searchDetails.caseClearTo);
            }
            if(_searchDetails.casesentStartFrom.HasValue && _searchDetails.casesentStartTo.HasValue)
            {
                arrestList =
                    arrestList.Where(inc => inc.ArrestSentenceStartDate.Value.Date >= _searchDetails.casesentStartFrom && inc.ArrestSentenceStartDate<=_searchDetails.casesentStartTo);
            }
            if(_searchDetails.caseSchStartFrom.HasValue && _searchDetails.caseSchStartTo.HasValue)
            {
                arrestList =
                    arrestList.Where(inc => inc.ArrestSentenceReleaseDate.Value.Date >= _searchDetails.caseSchStartFrom && inc.ArrestSentenceReleaseDate<=_searchDetails.caseSchStartTo);
            }
            if(!string.IsNullOrEmpty(_searchDetails.bailType))
            {
                arrestList = arrestList.Where(arl => arl.BailType == _searchDetails.bailType);
            }
            if(_searchDetails.noBail)
            {
                arrestList = arrestList.Where(arl => arl.BailNoBailFlag == 0);
            }
            if(_searchDetails.arrestConvictionStatus>0)
            {
                arrestList = arrestList.Where(arl => arl.ArrestBookingStatus == _searchDetails.arrestConvictionStatus);
            }
            if(!string.IsNullOrEmpty(_searchDetails.lawEnforcementDispositionId))
            {
                arrestList = arrestList.Where(arl => arl.ArrestLawEnforcementDisposition == _searchDetails.lawEnforcementDispositionId);
            }
            if(_searchDetails.sentCode!=0)
            {
                arrestList = arrestList.Where(arl => arl.ArrestSentenceCode == _searchDetails.sentCode);
            }
            if(_searchDetails.sentMethod>0)
            {
                arrestList = arrestList.Where(arl => arl.ArrestSentenceMethodId == _searchDetails.sentMethod);
            }
            if(_searchDetails.bailFrom>0)
            {
                arrestList = arrestList.Where(arl => arl.BailAmount >= _searchDetails.bailFrom && arl.BailAmount<=_searchDetails.bailTo);
            }
            if(_searchDetails.sentDaysFrom>0)
            {
                arrestList = arrestList.Where(arl => arl.ArrestSentenceActualDaysToServe >= _searchDetails.sentDaysFrom && arl.ArrestSentenceActualDaysToServe<=_searchDetails.sentDaysTo);
            }
			_arrestIds = arrestList.Select(de => de.ArrestId).ToList();

			// Filter by Warrant Number
			if (!string.IsNullOrEmpty(_searchDetails.WarrantNumber))
			{
				_arrestIds =
					_context.Warrant.Where(war => _arrestIds.Contains(war.ArrestId ?? 0) &&
								war.WarrantNumber.StartsWith(_searchDetails.WarrantNumber))
						.Select(war => war.ArrestId ?? 0).ToList();
			}
			if(!string.IsNullOrEmpty(_searchDetails.warrentType))
			{
				_arrestIds=_context.Warrant.Where(war=>_arrestIds.Contains(war.ArrestId ?? 0)&&war.WarrantChargeType==_searchDetails.warrentType)
				.Select(war=>war.ArrestId ?? 0).ToList();
			}
			if(_searchDetails.jurisdictionId>0)
			{
				_arrestIds=_context.Warrant.Where(war=>_arrestIds.Contains(war.ArrestId ?? 0)&&war.WarrantAgencyId==_searchDetails.jurisdictionId)
				.Select(war=>war.ArrestId ?? 0).ToList();	
			}
			if(_searchDetails.freeFormJus)
			{
				_arrestIds=_context.Warrant.Where(war=>_arrestIds.Contains(war.ArrestId ?? 0)&&war.WarrantCounty!="")
				.Select(war=>war.ArrestId ?? 0).ToList();
			}
		}

		//  Load Arrest Id from IncarcerationArrestXref Search
		private void LoadArrestIdFromArrestXrefSearch()
		{
            if (!_searchDetails.BookingDateFrm.HasValue && !_searchDetails.ReleaseDateFrom.HasValue &&
                string.IsNullOrEmpty(_searchDetails.ReleaseReason) || !_arrestIds.Any()) return;

            IQueryable<IncarcerationArrestXref> incarcerationArrXf = from iax in _context.IncarcerationArrestXref
                where iax.ArrestId > 0 && _arrestIds.Contains(iax.ArrestId ?? 0)
                select new IncarcerationArrestXref
                {
                    ReleaseReason = iax.ReleaseReason,
                    BookingDate = iax.BookingDate,
                    ReleaseDate = iax.ReleaseDate,
                    ArrestId = iax.ArrestId
                };

            if (!string.IsNullOrEmpty(_searchDetails.ReleaseReason))
            {
                incarcerationArrXf = incarcerationArrXf
                    .Where(iax => !string.IsNullOrEmpty(iax.ReleaseReason) && iax.ReleaseReason.ToUpper()
                                      .StartsWith(_searchDetails.ReleaseReason.ToUpper()));
            }

            if (_searchDetails.BookingDateFrm.HasValue) {
                incarcerationArrXf = _searchDetails.BookingDateTo.HasValue
                    ? incarcerationArrXf.Where(iax => iax.BookingDate.HasValue &&
                          iax.BookingDate.Value.Date >= _searchDetails.BookingDateFrm.Value.Date &&
                          iax.BookingDate.Value.Date <= _searchDetails.BookingDateTo.Value.Date)
                    : incarcerationArrXf.Where(iax => iax.BookingDate.HasValue &&
                          iax.BookingDate.Value.Date == _searchDetails.BookingDateFrm.Value.Date);
            }

            if (_searchDetails.caseClearFrom.HasValue) {
                incarcerationArrXf = _searchDetails.caseClearTo.HasValue
                    ? incarcerationArrXf.Where(iax => iax.ReleaseDate.HasValue &&
                          iax.ReleaseDate.Value.Date >= _searchDetails.caseClearFrom.Value.Date &&
                          iax.ReleaseDate.Value.Date <= _searchDetails.caseClearTo.Value.Date)
                    : incarcerationArrXf.Where(iax => iax.ReleaseDate.HasValue &&
                          iax.ReleaseDate.Value.Date == _searchDetails.caseClearFrom.Value.Date);
            }
			if(!string.IsNullOrEmpty(_searchDetails.clearReason))
			{
				incarcerationArrXf=incarcerationArrXf.Where(iax=>iax.ReleaseReason==_searchDetails.clearReason);
			}
            _arrestIds = incarcerationArrXf.Select(incArrXf => incArrXf.ArrestId ?? 0).ToList();
        }

		//  Filter Inmate based on Incarceration and Inmate details 
		private void LoadInmateSearch()
		{
			if (!_searchDetails.IsArrestSearch || _arrestIds.Count > 0)
            {
                List<int> arrInmateIds = _context.Arrest
                    .Where(arr => arr.InmateId > 0 && (!_searchDetails.IsArrestSearch ||
                        _arrestIds.Contains(arr.ArrestId))).Select(de => de.InmateId ?? 0).ToList();

                IQueryable<Inmate> dbInmateDetails = _context.Inmate.Where(inm =>
                        (!_searchDetails.IsArrestSearch || arrInmateIds.Contains(inm.InmateId))
                        && (!_searchDetails.ActiveOnly || inm.InmateActive == 1) &&
                        (_searchDetails.FacilityId == 0 || inm.FacilityId == _searchDetails.FacilityId));

			// Load Inmate List 
			IQueryable<Inmate> inmateList =
				dbInmateDetails.Where(inm => (!_searchDetails.ActiveOnly || inm.InmateActive == 1) 
											  &&(_searchDetails.FacilityId == 0 ||
											   inm.FacilityId == _searchDetails.FacilityId));

			IQueryable<Aka> dbAkaList = _context.Aka.Where(aka => !aka.DeleteFlag.HasValue || aka.DeleteFlag == 0);
            if (!string.IsNullOrWhiteSpace(_searchDetails.InmateNumber))
			{
				List<int> akaFilteredPersonId = dbAkaList
					.Where(aka => !string.IsNullOrEmpty(aka.AkaInmateNumber) && aka.AkaInmateNumber.StartsWith(_searchDetails.InmateNumber))
					.Select(aka => aka.PersonId ?? 0).ToList();

                inmateList = inmateList.Where(inl =>
                    inl.InmateNumber.StartsWith(_searchDetails.InmateNumber) ||
                    akaFilteredPersonId.Contains(inl.PersonId));
            }

			if (!string.IsNullOrWhiteSpace(_searchDetails.InmateSiteNumber))
            {
                List<int> akaFilteredPersonId = dbAkaList.Where(aka =>
                        !string.IsNullOrEmpty(aka.AkaSiteInmateNumber) &&
                        aka.AkaSiteInmateNumber.Contains(_searchDetails.InmateSiteNumber))
                    .Select(aka => aka.PersonId ?? 0).ToList();
				inmateList = inmateList.Where(inl => inl.InmateSiteNumber
				.Contains(_searchDetails.InmateSiteNumber) || akaFilteredPersonId.Contains(inl.PersonId));
			}

			if (!string.IsNullOrEmpty(_searchDetails.PrebookNumber))
			{
				List<int> preBookPersonIdLst =
					_context.InmatePrebook.Where(inp => inp.PreBookNumber.StartsWith(_searchDetails.PrebookNumber))
						.Select(inp => inp.PersonId ?? 0).ToList();

				inmateList = inmateList.Where(inl => preBookPersonIdLst.Contains(inl.PersonId));
			}

			// Load Incarceration Search
			LoadIncarcerationSearch(inmateList);
			}

            if (!_searchDetails.activeBookingOnly) return;
            IQueryable<Incarceration> incarcerationLst = from inc in _context.Incarceration
                where inc.InmateId > 0  && (inc.ReleaseOut==null)
                select inc;
            if (_searchDetails.keepNoKeeper >0)
			{
                incarcerationLst =
                    incarcerationLst.Where(inc => inc.NoKeeper == (_searchDetails.keepNoKeeper==1));
            }
            if(!string.IsNullOrWhiteSpace(_searchDetails.receiveMethod))
            {
                incarcerationLst =
                    incarcerationLst.Where(inc => inc.ReceiveMethod == _searchDetails.receiveMethod);
            }
            if(_searchDetails.dateSearchFrom.HasValue && _searchDetails.dateSearchTo.HasValue)
            {

                incarcerationLst =
                    incarcerationLst.Where(inc => inc.DateIn >= _searchDetails.dateSearchFrom && inc.DateIn<=_searchDetails.dateSearchTo);
            }
            if(_searchDetails.dateReleaseFrom.HasValue && _searchDetails.dateReleaseTo.HasValue)
            {
                incarcerationLst =
                    incarcerationLst.Where(inc => inc.ReleaseOut >= _searchDetails.dateReleaseFrom && inc.ReleaseOut<=_searchDetails.dateReleaseTo);
            }
            if(_searchDetails.dateschReleaseFrom.HasValue && _searchDetails.dateschReleaseTo.HasValue)
            {
                incarcerationLst =
                    incarcerationLst.Where(inc => inc.OverallFinalReleaseDate >= _searchDetails.dateschReleaseFrom && inc.OverallFinalReleaseDate<=_searchDetails.dateschReleaseTo);
            }
            if(_searchDetails.datesentStartFrom.HasValue && _searchDetails.datesentStartTo.HasValue)
            {
                incarcerationLst =
                    incarcerationLst.Where(inc => inc.OverallSentStartDate >= _searchDetails.datesentStartFrom && inc.OverallSentStartDate<=_searchDetails.datesentStartTo);
            }
            if(_searchDetails.dateAfterReleaseFrom.HasValue && _searchDetails.dateAfterReleaseTo.HasValue)
            {
                incarcerationLst =
                    incarcerationLst.Where(inc => inc.TransportScheduleDate >= _searchDetails.dateAfterReleaseFrom && inc.TransportScheduleDate<=_searchDetails.dateAfterReleaseTo);
            }
            if(_searchDetails.dateInCustodyFrom.HasValue && _searchDetails.dateInCustodyTo.HasValue)
            {
                incarcerationLst =
                    incarcerationLst.Where(inc => inc.DateIn >= _searchDetails.dateInCustodyTo && inc.ReleaseOut<=_searchDetails.dateInCustodyFrom);
            }
            List<int> filteredIds = incarcerationLst.Select(inc => inc.InmateId ?? 0).ToList();
			
			
            IQueryable<Inmate> inmateFilteredList =
                _context.Inmate.Where(inm => (filteredIds.Contains(inm.InmateId)));
            var list = incarcerationLst.Select(s=>new {
               s.InmateId,
               Days=GetDifferentDays(s.DateIn,s.ReleaseOut)
            }).ToList();
            if(_searchDetails.daysCustomdyFrom>0 || _searchDetails.daysCustomdyTo>0)
            {
                list=list.Where(de=>de.Days>=_searchDetails.daysCustomdyFrom && de.Days<=_searchDetails.daysCustomdyTo).ToList();
                List<int?> filter = list.Select(x => x.InmateId).ToList();
                inmateFilteredList=inmateFilteredList.Where(inl=>filter.Contains(inl.InmateId));
            }
			
            if(!string.IsNullOrWhiteSpace(_searchDetails.InmateNumber))
            {
                inmateFilteredList=inmateFilteredList.Where(s=>s.InmateNumber.StartsWith(_searchDetails.InmateNumber));
            }
            if(!string.IsNullOrWhiteSpace(_searchDetails.InmateSiteNumber))
            {
                inmateFilteredList=inmateFilteredList.Where(s=>s.InmateSiteNumber.StartsWith(_searchDetails.InmateSiteNumber));
            }
            // Newly added for Prebook Related Search
            List<int> incarcerationIds = incarcerationLst.Select(inc => inc.IncarcerationId).ToList();
            List<int?> personIds=new List<int?>();
			
            if(_searchDetails.isInmateprebookrelatedsearch)
            {
                List<InmatePrebook> preBookLst=_context.InmatePrebook.Where(w=>incarcerationIds.Contains(w.IncarcerationId??0)).ToList();
                if(!string.IsNullOrWhiteSpace(_searchDetails.prebookNo))
                {
                    preBookLst = preBookLst.Where(w=>w.PreBookNumber==_searchDetails.prebookNo).ToList();
                }
                if(!string.IsNullOrWhiteSpace(_searchDetails.prebookCaseNumber))
                {
                    preBookLst = preBookLst.Where(w=>w.CaseNumber==_searchDetails.prebookCaseNumber).ToList();
                }
                if(_searchDetails.PrebookArrestAgency>0)
                {
                    preBookLst = preBookLst.Where(w=>w.ArrestAgencyId==_searchDetails.PrebookArrestAgency).ToList();
                }
                if(!string.IsNullOrWhiteSpace(_searchDetails.prebookCaseNumber))
                {
                    preBookLst = preBookLst.Where(w=>w.CaseNumber==_searchDetails.prebookCaseNumber).ToList();
                }
                if(_searchDetails.prebookarrestingOfficerName>0)
                {
                    preBookLst = preBookLst.Where(w=>w.ArrestingOfficerId==_searchDetails.prebookarrestingOfficerName).ToList();
                }
                personIds.AddRange(preBookLst.Select(w=>w.PersonId).ToList());
                inmateFilteredList=inmateFilteredList.Where(w=> personIds.Contains(w.PersonId));
            }
            inmateFilteredList = inmateFilteredList.Where(inl => filteredIds.Contains(inl.InmateId));
		
            foreach (Inmate details in inmateFilteredList)
            {
                if (_inmateLstValue.All(de => de.Key != details.PersonId))
                    _inmateLstValue.Add(details.PersonId, details.InmateId);
            }
        }

		private void LoadIncarcerationSearch(IQueryable<Inmate> inmateList)
		{
			if (_searchDetails.IncarcerationSearch && inmateList.Any())
			{
				var tempInmateDetails = inmateList.Select(de => new
				{
					de.InmateId,
					de.PersonId
				});

				List<int> tempInmateIdLst = tempInmateDetails.Select(det => det.InmateId).ToList();

                IQueryable<Incarceration> incarcerationLst = from inc in _context.Incarceration
                    where inc.InmateId > 0 && tempInmateIdLst.Contains(inc.InmateId ?? 0)
                                           /* To be have a values when inmate is released or cleared*/
                                           && (!_searchDetails.ActiveOnly || !inc.ReleaseOut.HasValue)
                    select inc;
				
				if(!string.IsNullOrWhiteSpace(_searchDetails.incarcerabookingNumber))
				{
					incarcerationLst=incarcerationLst.Where(inc=>inc.BookingNo==_searchDetails.incarcerabookingNumber);
				}
				
				if (_searchDetails.keepNoKeeper >0)
				{
					incarcerationLst = incarcerationLst.Where(inc => inc.NoKeeper == (_searchDetails.keepNoKeeper==1));
				}
				if(!string.IsNullOrWhiteSpace(_searchDetails.receiveMethod))
				{
					incarcerationLst =
						incarcerationLst.Where(inc => inc.ReceiveMethod == _searchDetails.receiveMethod);
				}
				if(_searchDetails.dateSearchFrom.HasValue && _searchDetails.dateSearchTo.HasValue)
				{
                    incarcerationLst =
                        incarcerationLst.Where(inc => inc.DateIn >= _searchDetails.dateSearchFrom && inc.DateIn<=_searchDetails.dateSearchTo);
				}
				if(_searchDetails.dateReleaseFrom.HasValue && _searchDetails.dateReleaseTo.HasValue)
				{
					incarcerationLst =
						incarcerationLst.Where(inc => inc.ReleaseOut >= _searchDetails.dateReleaseFrom && inc.ReleaseOut<=_searchDetails.dateReleaseTo);
				}
				if(_searchDetails.dateschReleaseFrom.HasValue && _searchDetails.dateschReleaseTo.HasValue)
				{
					incarcerationLst =
						incarcerationLst.Where(inc => inc.OverallFinalReleaseDate >= _searchDetails.dateschReleaseFrom && inc.OverallFinalReleaseDate<=_searchDetails.dateschReleaseTo);
				}
				if(_searchDetails.datesentStartFrom.HasValue && _searchDetails.datesentStartTo.HasValue)
				{
					incarcerationLst =
						incarcerationLst.Where(inc => inc.OverallSentStartDate >= _searchDetails.datesentStartFrom && inc.OverallSentStartDate<=_searchDetails.datesentStartTo);
				}
				if(_searchDetails.dateAfterReleaseFrom.HasValue && _searchDetails.dateAfterReleaseTo.HasValue)
				{
					incarcerationLst =
						incarcerationLst.Where(inc => inc.TransportScheduleDate >= _searchDetails.dateAfterReleaseFrom && inc.TransportScheduleDate<=_searchDetails.dateAfterReleaseTo);
				}
				if(_searchDetails.dateInCustodyFrom.HasValue && _searchDetails.dateInCustodyTo.HasValue)
				{
					incarcerationLst =
						incarcerationLst.Where(inc => inc.DateIn <= _searchDetails.dateInCustodyTo && inc.ReleaseOut<=_searchDetails.dateInCustodyFrom);
				}
				
                if(_searchDetails.sentenceStatus==2)
                {
                    incarcerationLst =
                        incarcerationLst.Where(inc => inc.OverallSentStartDate == null);
                }
                if(_searchDetails.sentenceStatus==3)
                {
						
                    incarcerationLst =
                        incarcerationLst.Where(inc => inc.OverallSentStartDate != null && inc.OverallFinalReleaseDate==null);
                }
                if(_searchDetails.sentenceStatus==4)
                {
						
                    incarcerationLst =
                        incarcerationLst.Where(inc => inc.OverallSentStartDate != null&& inc.OverallFinalReleaseDate!=null);
                }
                if(_searchDetails.transportAfterRelease)
                {
                    incarcerationLst =
                        incarcerationLst.Where(inc => inc.TransportFlag==1);
                }
                if(_searchDetails.overallSentDaysFrom>0 || _searchDetails.overallSentDaysTo>0)
                {
                    incarcerationLst =
                        incarcerationLst.Where(inc => inc.TotSentDays<=_searchDetails.overallSentDaysFrom && inc.TotSentDays>=_searchDetails.overallSentDaysTo);
                }
				if (_searchDetails.ClearByOfficer > 0)
				{
					incarcerationLst =
						incarcerationLst.Where(inc => inc.ReleaseClearBy == _searchDetails.ClearByOfficer);
				}

				if (_searchDetails.ReleaseOfficer > 0)
				{
					incarcerationLst =
						incarcerationLst.Where(inc => inc.ReleaseCompleteBy == _searchDetails.ReleaseOfficer);
				}

				if (_searchDetails.IntakeOfficer > 0)
				{
					incarcerationLst = incarcerationLst.Where(inc => inc.InOfficerId == _searchDetails.IntakeOfficer);
				}
				List<int> filteredIds = incarcerationLst.Select(inc => inc.InmateId??0).ToList();
			    var list = incarcerationLst.Select(s=>new {
                    s.InmateId,
				    Days=GetDifferentDays(s.DateIn,s.ReleaseOut)
				}).ToList();
			if(_searchDetails.daysCustomdyFrom>0 || _searchDetails.daysCustomdyTo>0)
				{
					list=list.Where(de=>de.Days>=_searchDetails.daysCustomdyFrom && de.Days<=_searchDetails.daysCustomdyTo).ToList();
					List<int?> filter = list.Select(x => x.InmateId).ToList();
				inmateList=inmateList.Where(inl=>filter.Contains(inl.InmateId));
				}
            
				List<int> incarcerationIds = incarcerationLst.Select(inc => inc.IncarcerationId).ToList();
			    List<int?> personIds=new List<int?>();
				if(_searchDetails.isInmateprebookrelatedsearch)
                {
                    List<InmatePrebook> preBookLst=_context.InmatePrebook.Where(w=> incarcerationIds.Contains(w.IncarcerationId??0)).ToList();
                    if(!string.IsNullOrWhiteSpace(_searchDetails.prebookNo))
                    {
                        preBookLst = preBookLst.Where(w=>w.PreBookNumber==_searchDetails.prebookNo).ToList();
                    }
                    if(!string.IsNullOrWhiteSpace(_searchDetails.prebookCaseNumber))
                    {
                        preBookLst = preBookLst.Where(w=>w.CaseNumber==_searchDetails.prebookCaseNumber).ToList();
                    }
                    if(_searchDetails.PrebookArrestAgency>0)
                    {
                        preBookLst = preBookLst.Where(w=>w.ArrestAgencyId==_searchDetails.PrebookArrestAgency).ToList();
                    }
                    if(!string.IsNullOrWhiteSpace(_searchDetails.prebookCaseNumber))
                    {
                        preBookLst = preBookLst.Where(w=>w.CaseNumber==_searchDetails.prebookCaseNumber).ToList();
                    }
                    if(_searchDetails.prebookarrestingOfficerName>0)
                    {
                        preBookLst = preBookLst.Where(w=>w.ArrestingOfficerId==_searchDetails.prebookarrestingOfficerName).ToList();
                    }
                    personIds.AddRange(preBookLst.Select(w=>w.PersonId).ToList());
                    inmateList=inmateList.Where(w=>personIds.Contains(w.PersonId));
                }
			
                inmateList = inmateList.Where(inl => filteredIds.Contains(inl.InmateId));
			
				foreach (Inmate details in inmateList)
				{
					if (_inmateLstValue.All(de => de.Key != details.PersonId))
						_inmateLstValue.Add(details.PersonId, details.InmateId);
				}
			}
			else
			{
				foreach (Inmate details in inmateList)
				{
					if (_inmateLstValue.All(de => de.Key != details.PersonId))
						_inmateLstValue.Add(details.PersonId, details.InmateId);
				}
			}
		}
		private int GetDifferentDays(DateTime? dateIn,DateTime? releaseOut)
		{
            Debug.Assert(dateIn != null, nameof(dateIn) + " != null");
		    if (releaseOut==null) 
		    
		    {
			    releaseOut=DateTime.Now;
                int days = (releaseOut.Value.Date - dateIn.Value.Date).Days;
			    return days;
		    }
		    else
		    {
                int days = (releaseOut.Value.Date - dateIn.Value.Date).Days;
			    
                return days;
		    }
			
			
		}
		//  Filter Person and Aka details based on person details
		private void LoadPersonSearch()
		{
			List<int> filteredPerIds = new List<int>();
			bool isFilteredPerson = false;
			bool isPersonFieldSearch = false;

			if (_searchDetails.IsInmateSearch)
			{
				isFilteredPerson = true;
				isPersonFieldSearch = true;
				filteredPerIds = _inmateLstValue.Keys.ToList();
			}
			else if (_searchDetails.IsArrestSearch)
			{
				isFilteredPerson = true;
				isPersonFieldSearch = true;
				filteredPerIds = _context.Arrest.Where(arr => arr.InmateId > 0 &&
										(!_searchDetails.IsArrestSearch || _arrestIds.Contains(arr.ArrestId)))
				   .Select(de => de.Inmate.PersonId).ToList();
			}

			// FN, LN, MN, MONK, DOB, AFIS
			IQueryable<Aka> akaList = _context.Aka.Where(aka =>
				(!aka.DeleteFlag.HasValue || aka.DeleteFlag == 0) &&
						(string.IsNullOrEmpty(_searchDetails.Moniker) ||
						aka.PersonGangName.StartsWith(_searchDetails.Moniker)));

			List<int> personIdLst = !string.IsNullOrEmpty(_searchDetails.Moniker) ? 
				akaList.Select(ak => ak.PersonId ?? 0).Distinct().ToList() : new List<int>();

			IQueryable<Person> dbPersonLst = _context.Person
				.Where(per => (!isFilteredPerson || filteredPerIds.Contains(per.PersonId)) &&
				(string.IsNullOrEmpty(_searchDetails.Moniker) || personIdLst.Contains(per.PersonId)));


			if (!string.IsNullOrEmpty(_searchDetails.FirstName))
			{
				akaList = akaList.Where(aka => aka.AkaFirstName.StartsWith(_searchDetails.FirstName) ||
											   aka.Person.PersonFirstName.StartsWith(_searchDetails.FirstName));

				dbPersonLst = dbPersonLst.Where(per => per.PersonFirstName.StartsWith(_searchDetails.FirstName));
				isPersonFieldSearch = true;
			}

			if (!string.IsNullOrEmpty(_searchDetails.LastName))
			{
				akaList = akaList.Where(aka => aka.AkaLastName.StartsWith(_searchDetails.LastName) ||
											   aka.Person.PersonLastName.StartsWith(_searchDetails.LastName));

				dbPersonLst = dbPersonLst.Where(per => per.PersonLastName.StartsWith(_searchDetails.LastName));
				isPersonFieldSearch = true;
			}

			if (!string.IsNullOrEmpty(_searchDetails.MiddleName))
			{
				isPersonFieldSearch = true;
				akaList = akaList.Where(aka => aka.AkaMiddleName.StartsWith(_searchDetails.MiddleName) ||
											   aka.Person.PersonMiddleName.StartsWith(_searchDetails.MiddleName));

				dbPersonLst = dbPersonLst.Where(per => per.PersonMiddleName.StartsWith(_searchDetails.MiddleName));
			}
			if (!string.IsNullOrEmpty(_searchDetails.cityofBirth))
			{
				isPersonFieldSearch = true;
				akaList = akaList.Where(aka => aka.Person.PersonPlaceOfBirth.StartsWith(_searchDetails.cityofBirth));
				dbPersonLst = dbPersonLst.Where(per => per.PersonPlaceOfBirth.StartsWith(_searchDetails.cityofBirth));
			}
			if (!string.IsNullOrEmpty(_searchDetails.stateorCountryOfBirth))
			{
				isPersonFieldSearch = true;
				akaList = akaList.Where(aka => aka.Person.PersonPlaceOfBirthList.StartsWith(_searchDetails.stateorCountryOfBirth));
				dbPersonLst = dbPersonLst.Where(per => per.PersonPlaceOfBirthList.StartsWith(_searchDetails.stateorCountryOfBirth));
			}

			

            if (!string.IsNullOrEmpty(_searchDetails.OtherId))
            {
                isPersonFieldSearch = true;
                if(_searchDetails.exactonly)
				{
                    akaList = akaList.Where(aka => aka.AkaOtherIdNumber.StartsWith(_searchDetails.OtherId) ||
                                                   aka.Person.PersonOtherIdNumber==_searchDetails.OtherId);

                    dbPersonLst = dbPersonLst.Where(per => per.PersonOtherIdNumber==_searchDetails.OtherId);
				}
				else
				{
					akaList = akaList.Where(aka => aka.AkaOtherIdNumber.StartsWith(_searchDetails.OtherId) ||
                                               aka.Person.PersonOtherIdNumber.StartsWith(_searchDetails.OtherId));

                dbPersonLst = dbPersonLst.Where(per => per.PersonOtherIdNumber.StartsWith(_searchDetails.OtherId));
				}
            }

            var akaPersonList = (from aka in akaList
								 where string.IsNullOrEmpty(_searchDetails.AfisNumber) ||
									   aka.AkaAfisNumber.StartsWith(_searchDetails.AfisNumber) ||
									   aka.Person.AfisNumber.StartsWith(_searchDetails.AfisNumber)
								 select new
								 {
									 aka.PersonId,
									 aka.AkaDob,
									 aka.Person.PersonDob,
									 PersonCii = aka.Person.PersonCii ?? aka.AkaCii,
									 PersonAlienNo = aka.Person.PersonAlienNo ?? aka.AkaAlienNo,
									 PersonDlNumber = aka.Person.PersonDlNumber ?? aka.AkaDl,
									 PersonFbiNo = aka.Person.PersonFbiNo ?? aka.AkaFbi,
									 PersonDoc = aka.Person.PersonDoc ?? aka.AkaDoc
								 }).ToList();

			List<Person> personList = (from per in dbPersonLst
									   where string.IsNullOrEmpty(_searchDetails.AfisNumber) || per.AfisNumber.StartsWith(_searchDetails.AfisNumber)
									   select new Person
									   {
										   PersonId = per.PersonId,
										   PersonDob = per.PersonDob,
										   PersonCii = per.PersonCii,
										   PersonAlienNo = per.PersonAlienNo,
										   PersonDlNumber = per.PersonDlNumber,
										   PersonFbiNo = per.PersonFbiNo,
										   PersonDoc = per.PersonDoc
									   }).ToList();

			if (!string.IsNullOrEmpty(_searchDetails.AfisNumber))
			{
				isPersonFieldSearch = true;
			}

			if (_searchDetails.DateOfBirth.HasValue && _searchDetails.DateOfBirth > DateTime.MinValue)
			{
				isPersonFieldSearch = true;
				akaPersonList = (from aka in akaPersonList
								 where _searchDetails.DateOfBirth != null && (aka.AkaDob.HasValue && aka.AkaDob.Value.Date == _searchDetails.DateOfBirth.Value.Date ||
                                                                              aka.PersonDob.HasValue && aka.PersonDob.Value.Date == _searchDetails.DateOfBirth.Value.Date)
								 select aka).ToList();

				personList = (from per in personList
							  where _searchDetails.DateOfBirth != null && per.PersonDob.HasValue && per.PersonDob.Value.Date == _searchDetails.DateOfBirth.Value.Date
							  select per).ToList();
			}

            if (isFilteredPerson)
			{
				akaPersonList = akaPersonList.Where(
						aka => filteredPerIds.Contains(aka.PersonId ?? 0)).ToList();
			}

			// CII, Alien, DLNumber, FBINumber, DocNumber
			if (!string.IsNullOrEmpty(_searchDetails.CiiNumber) || !string.IsNullOrEmpty(_searchDetails.FbiNumber) ||
				!string.IsNullOrEmpty(_searchDetails.DocNumber)
				|| !string.IsNullOrEmpty(_searchDetails.DlNumber) || !string.IsNullOrEmpty(_searchDetails.AlienNumber))
			{

				personList.AddRange((from aka in akaPersonList
									 select new Person
									 {
										 PersonId = aka.PersonId ?? 0,
										 PersonDob = aka.PersonDob,
										 PersonCii = aka.PersonCii,
										 PersonAlienNo = aka.PersonAlienNo,
										 PersonDlNumber = aka.PersonDlNumber,
										 PersonFbiNo = aka.PersonFbiNo,
										 PersonDoc = aka.PersonDoc
									 }).ToList());
				// Newly added condition for Exact match in Number field search
				if(_searchDetails.exactonly)
				{
					if (!string.IsNullOrEmpty(_searchDetails.CiiNumber))
                    {
                        _personFilteredIdLst.AddRange((from prd in personList
                            where !string.IsNullOrEmpty(prd.PersonCii) &&
                                  prd.PersonCii.ToUpper()==_searchDetails.CiiNumber.ToUpper()
                            select prd.PersonId).ToList());
                    }

                    if (!string.IsNullOrEmpty(_searchDetails.FbiNumber))
                    {
                        _personFilteredIdLst.AddRange((from prd in personList
                            where !string.IsNullOrEmpty(prd.PersonFbiNo) &&
                                  prd.PersonFbiNo.ToUpper()==_searchDetails.FbiNumber.ToUpper()
                            select prd.PersonId).ToList());
                    }

                    if (!string.IsNullOrEmpty(_searchDetails.DocNumber))
                    {
                        _personFilteredIdLst.AddRange((from prl in personList
                            where !string.IsNullOrEmpty(prl.PersonDoc) &&
                                  prl.PersonDoc.ToUpper()==_searchDetails.DocNumber.ToUpper()
                            select prl.PersonId).ToList());
                    }

                    if (!string.IsNullOrEmpty(_searchDetails.DlNumber))
                    {
                        _personFilteredIdLst.AddRange((from prl in personList
                            where !string.IsNullOrEmpty(prl.PersonDlNumber) &&
                                  prl.PersonDlNumber.ToUpper()==_searchDetails.DlNumber.ToUpper()
                            select prl.PersonId).ToList());
                    }

                    if (!string.IsNullOrEmpty(_searchDetails.AlienNumber))
                    {
                        _personFilteredIdLst.AddRange((from prl in personList
                            where !string.IsNullOrEmpty(prl.PersonAlienNo) &&
                                  prl.PersonAlienNo.ToUpper()==_searchDetails.AlienNumber.ToUpper()
                            select prl.PersonId).ToList());
                    }
				}
				else
				{
					if (!string.IsNullOrEmpty(_searchDetails.CiiNumber))
                    {
                        _personFilteredIdLst.AddRange((from prd in personList
                            where !string.IsNullOrEmpty(prd.PersonCii) &&
                                  prd.PersonCii.ToUpper().StartsWith(_searchDetails.CiiNumber.ToUpper())
                            select prd.PersonId).ToList());
                    }

                    if (!string.IsNullOrEmpty(_searchDetails.FbiNumber))
                    {
                        _personFilteredIdLst.AddRange((from prd in personList
                            where !string.IsNullOrEmpty(prd.PersonFbiNo) &&
                                  prd.PersonFbiNo.ToUpper().StartsWith(_searchDetails.FbiNumber.ToUpper())
                            select prd.PersonId).ToList());
                    }

                    if (!string.IsNullOrEmpty(_searchDetails.DocNumber))
                    {
                        _personFilteredIdLst.AddRange((from prl in personList
                            where !string.IsNullOrEmpty(prl.PersonDoc) &&
                                  prl.PersonDoc.ToUpper().StartsWith(_searchDetails.DocNumber.ToUpper())
                            select prl.PersonId).ToList());
                    }

                    if (!string.IsNullOrEmpty(_searchDetails.DlNumber))
                    {
                        _personFilteredIdLst.AddRange((from prl in personList
                            where !string.IsNullOrEmpty(prl.PersonDlNumber) &&
                                  prl.PersonDlNumber.ToUpper().StartsWith(_searchDetails.DlNumber.ToUpper())
                            select prl.PersonId).ToList());
                    }

                    if (!string.IsNullOrEmpty(_searchDetails.AlienNumber))
                    {
                        _personFilteredIdLst.AddRange((from prl in personList
                            where !string.IsNullOrEmpty(prl.PersonAlienNo) &&
                                  prl.PersonAlienNo.ToUpper().StartsWith(_searchDetails.AlienNumber.ToUpper())
                            select prl.PersonId).ToList());
                    }
				}
			}
			else
			{
				if (isPersonFieldSearch)
				{
					_personFilteredIdLst.AddRange(personList.Select(pr => pr.PersonId).ToList());

				}
				if (akaPersonList.Any())
				{
					_personFilteredIdLst.AddRange(akaPersonList.Where(aka => aka.PersonId.HasValue).Select(aka => aka.PersonId.Value));
				}
			}

			// Person (Character) age search changed to person identity search
					if (_searchDetails.Agefrom != 0) 
					{
                        List<KeyValuePair<int, int>> personAgeDetails = (from prd in _context.Person
                            where _personFilteredIdLst.Contains(prd.PersonId)
                            select new
                            {
                                prd.PersonId,
                                PersonAge = GetPersonAge(prd.PersonDob)
                            }).ToDictionary(de => de.PersonId, de => de.PersonAge).ToList();
                        if (personAgeDetails.Any())
                        {
                            _personFilteredIdLst = (from personAge in personAgeDetails
                                where _searchDetails.Ageto > 0
                                    ? personAge.Value >= _searchDetails.Agefrom && personAge.Value <= _searchDetails.Ageto
                                    : personAge.Value == _searchDetails.Agefrom
                                select personAge.Key).ToList();
                        }
			}

			// Phone Number based on Person/Inmate tables
            if (string.IsNullOrEmpty(_searchDetails.PhoneCode)) return;
            {
                List<int> personPhFilteredLst = new List<int>();
                if (_searchDetails.PhoneCode.Length <= 14)
                {
					
                    IQueryable<PersonVm> dbInmateDetails = from inm in _context.Person
                        where !string.IsNullOrEmpty(inm.PersonPhone) || !string.IsNullOrEmpty(inm.PersonCellPhone)
                                                                     || !string.IsNullOrEmpty(inm.PersonPhone2) || !string.IsNullOrEmpty(inm.PersonBusinessPhone)
                                                                     || !string.IsNullOrEmpty(inm.PersonBusinessFax)
                        select new PersonVm
                        {
                            PersonId = inm.PersonId,
                            PhoneCode = inm.PersonPhone,
                            PersonCellPhone=inm.PersonCellPhone,
                            PersonPhone2=inm.PersonPhone2,
                            PersonBusinessPhone=inm.PersonBusinessPhone,
                            PersonBusinessFax=inm.PersonBusinessFax
                        };
                    string phoneNo = Regex.Replace(_searchDetails.PhoneCode, 
                        @"[\[\]\^\$\.\|\?\*\+\(\)\\~`\!@#%&\-_+={}'""<>:;, ]{1,}", string.Empty);
                    // Phone number also exact match search changed
                    if(_searchDetails.exactonly)
                    {
                        personPhFilteredLst = (from pl in dbInmateDetails
                            where !string.IsNullOrEmpty(pl.PhoneCode) && pl.PhoneCode==phoneNo
                                  ||!string.IsNullOrEmpty(pl.PersonCellPhone) && pl.PersonCellPhone==phoneNo
                                  ||!string.IsNullOrEmpty(pl.PersonPhone2) && pl.PersonPhone2==phoneNo
                                  ||!string.IsNullOrEmpty(pl.PersonBusinessPhone) && pl.PersonBusinessPhone==phoneNo
                                  ||!string.IsNullOrEmpty(pl.PersonBusinessFax) && pl.PersonBusinessFax==phoneNo
                            select pl.PersonId).ToList();

                        dbPersonLst =
                            dbPersonLst.Where(pr => dbInmateDetails.Any(dinm => dinm.PersonId == pr.PersonId)).Select(pr => pr);

                        personPhFilteredLst.AddRange((from pl in dbPersonLst
                            where !string.IsNullOrEmpty(pl.PersonCellPhone) && pl.PersonCellPhone==_searchDetails.PhoneCode ||
                                  !string.IsNullOrEmpty(pl.PersonPhone) && pl.PersonPhone==_searchDetails.PhoneCode ||
                                  !string.IsNullOrEmpty(pl.PersonPhone2) && pl.PersonPhone2==_searchDetails.PhoneCode ||
                                  !string.IsNullOrEmpty(pl.PersonBusinessPhone) && pl.PersonBusinessPhone==_searchDetails.PhoneCode
                                  || !string.IsNullOrEmpty(pl.PersonBusinessFax) && pl.PersonBusinessFax==_searchDetails.PhoneCode
                                  || pl.Aka.Any(aka => !string.IsNullOrEmpty(aka.AkaOtherPhoneNumber)
                                                       && aka.AkaOtherPhoneNumber==_searchDetails.PhoneCode)
                            select pl.PersonId).ToList());
                    }
                    else
                    {
                        personPhFilteredLst = (from pl in dbInmateDetails
                            where !string.IsNullOrEmpty(pl.PhoneCode) && pl.PhoneCode.StartsWith(phoneNo)
                                  ||!string.IsNullOrEmpty(pl.PersonCellPhone) && pl.PersonCellPhone.StartsWith(phoneNo)
                                  ||!string.IsNullOrEmpty(pl.PersonPhone2) && pl.PersonPhone2.StartsWith(phoneNo)
                                  ||!string.IsNullOrEmpty(pl.PersonBusinessPhone) && pl.PersonBusinessPhone.StartsWith(phoneNo)
                                  ||!string.IsNullOrEmpty(pl.PersonBusinessFax) && pl.PersonBusinessFax.StartsWith(phoneNo)
                            select pl.PersonId).ToList();

                        dbPersonLst =
                            dbPersonLst.Where(pr => dbInmateDetails.Any(dinm => dinm.PersonId == pr.PersonId)).Select(pr => pr);

                        personPhFilteredLst.AddRange((from pl in dbPersonLst
                            where !string.IsNullOrEmpty(pl.PersonCellPhone) && pl.PersonCellPhone.StartsWith(_searchDetails.PhoneCode) ||
                                  !string.IsNullOrEmpty(pl.PersonPhone) && pl.PersonPhone.StartsWith(_searchDetails.PhoneCode) ||
                                  !string.IsNullOrEmpty(pl.PersonPhone2) && pl.PersonPhone2.StartsWith(_searchDetails.PhoneCode) ||
                                  !string.IsNullOrEmpty(pl.PersonBusinessPhone) && pl.PersonBusinessPhone.StartsWith(_searchDetails.PhoneCode)
                                  || !string.IsNullOrEmpty(pl.PersonBusinessFax) && pl.PersonBusinessFax.StartsWith(_searchDetails.PhoneCode)
                                  || pl.Aka.Any(aka => !string.IsNullOrEmpty(aka.AkaOtherPhoneNumber)
                                                       && aka.AkaOtherPhoneNumber.StartsWith(_searchDetails.PhoneCode))
                            select pl.PersonId).ToList());
                    }
                }

                _personFilteredIdLst = (from pid in personPhFilteredLst
                                        where _personFilteredIdLst.Contains(pid)
                    select pid).ToList();
            }
        }

		// Filter Person Details based on requested Person Character details 
		private void LoadPersonByCharSearch()
		{
			IQueryable<Person> dbPersonList = from pr in _context.Person
                where !_searchDetails.IsPersonSearch || _personFilteredIdLst.Contains(pr.PersonId)
                select pr;
			if (_searchDetails.Eyecolor > 0)
			{
                dbPersonList = from spl in dbPersonList
                               where spl.PersonEyeColorLast == _searchDetails.Eyecolor
                    select spl;
			}
			if (_searchDetails.Haircolor > 0)
			{
                dbPersonList = from spl in dbPersonList
                               where spl.PersonHairColorLast == _searchDetails.Haircolor
                    select spl;
			}
			if (_searchDetails.Race > 0)
			{
                dbPersonList = from spl in dbPersonList
                               where spl.PersonRaceLast == _searchDetails.Race
                    select spl;
			}

			if (_searchDetails.gender > 0)
			{
                dbPersonList = from spl in dbPersonList
                               where spl.PersonSexLast == _searchDetails.gender
                    select spl;
			}

			if (_searchDetails.Weightfrom > 0)
			{
                dbPersonList = from spl in dbPersonList
                               where _searchDetails.Weightto > 0
                        ? spl.PersonWeightLast >= _searchDetails.Weightfrom
                          && spl.PersonWeightLast <= _searchDetails.Weightto
                        : spl.PersonWeightLast == _searchDetails.Weightfrom
                    select spl;
			}

			if (_searchDetails.HPfrom > 0)
			{
				int heightFrom = _searchDetails.HSfrom + _searchDetails.HPfrom * 12;
				int heightTo = _searchDetails.HSto + _searchDetails.HPto * 12;

                dbPersonList = from spl in dbPersonList
                               where heightTo > 0
                        ? spl.PersonHeightPrimaryLast * 12 + spl.PersonHeightSecondaryLast <= heightTo &&
                          spl.PersonHeightPrimaryLast * 12 + spl.PersonHeightSecondaryLast >= heightFrom
                        : spl.PersonHeightPrimaryLast * 12 + spl.PersonHeightSecondaryLast == heightFrom
                    select spl;
			}

			_personFilteredIdLst = dbPersonList.Select(det => det.PersonId).ToList(); // Assign filtered person Ids

			if (!string.IsNullOrEmpty(_searchDetails.Occupation))
			{
				_personFilteredIdLst = (from pdn in _context.PersonDescription
										where pdn.PersonId > 0 && _personFilteredIdLst.Contains(pdn.PersonId ?? 0)
											  && pdn.PersonOccupation.Contains(_searchDetails.Occupation)
										select pdn.PersonId ?? 0).ToList();
			}

            
        }

		// Person Descriptor Search
		private void LoadPersonDescriptorSearch()
		{
            if (_searchDetails.IsPersonSearch && !_personFilteredIdLst.Any()) return;
            IQueryable<PersonDescriptor> personDescriptorLst = from pd in _context.PersonDescriptor
                where !_searchDetails.IsPersonSearch || _personFilteredIdLst.Contains(pd.PersonId.Value)
                select pd;

            if (!string.IsNullOrEmpty(_searchDetails.Descriptor))
            {
                personDescriptorLst = from pdl in personDescriptorLst
                                      where pdl.DescriptorText.Contains(_searchDetails.Descriptor)
                                      select pdl;
            }

            if (!string.IsNullOrEmpty(_searchDetails.Location))
            {
                personDescriptorLst = from pdl in personDescriptorLst
                                      where pdl.ItemLocation == _searchDetails.Location
                    select pdl;
            }

            if (!string.IsNullOrEmpty(_searchDetails.ItemCode))
            {
                personDescriptorLst = from pdl in personDescriptorLst
                                      where pdl.Code == _searchDetails.ItemCode
                    select pdl;
            }

            if (!string.IsNullOrEmpty(_searchDetails.Category))
            {
                personDescriptorLst = from pdl in personDescriptorLst
                                      where pdl.Category == _searchDetails.Category
                    select pdl;
            }
            _personFilteredIdLst = personDescriptorLst.Select(det => det.PersonId.Value).ToList();
        }

		// Load Charges Search Details
		private void LoadChargesSearchDetails()
		{
			if (!_searchDetails.IsArrestSearch)
			{
				_arrestIds = (from ar in _context.Arrest
							  where ar.InmateId > 0 &&
									(!_searchDetails.ActiveOnly || !_searchDetails.IsBookingSearch || ar.ArrestActive == 1)
							  select ar.ArrestId).ToList();
			}

			List<CrimeLookupVm> crimeLkpLst = (from crm in _context.Crime
											   where crm.ArrestId > 0 && _arrestIds.Contains(crm.ArrestId ?? 0)
											   select new CrimeLookupVm
											   {
												   ArrestId = crm.ArrestId,
												   CrimeLookupId = crm.CrimeLookupId,
												   CrimeGroupId = crm.CrimeLookup.CrimeGroupId,
												   CrimeCodeType = crm.CrimeLookup.CrimeCodeType,
												   CrimeSection = crm.CrimeLookup.CrimeSection,
												   CrimeDescription = crm.CrimeLookup.CrimeDescription,
												   CrimeStatuteCode=crm.CrimeLookup.CrimeStatuteCode,
												   CrimeType=crm.CrimeType
											   }).ToList();

			if (crimeLkpLst.Any())
			{
				if (!string.IsNullOrEmpty(_searchDetails.ChargeType))
				{
					crimeLkpLst = (from crimeLt in crimeLkpLst
								   where !string.IsNullOrEmpty(crimeLt.CrimeCodeType) &&
                                         crimeLt.CrimeCodeType.ToUpper() == _searchDetails.ChargeType.ToUpper()
								   select crimeLt).ToList();
				}

				if (_searchDetails.ChargeGroup > 0)
				{
					crimeLkpLst = (from crimeLt in crimeLkpLst
								   where crimeLt.CrimeGroupId == _searchDetails.ChargeGroup
								   select crimeLt).ToList();
				}

				if (!string.IsNullOrEmpty(_searchDetails.ChargeSection))
				{
					crimeLkpLst = (from crimeLt in crimeLkpLst
								   where !string.IsNullOrEmpty(crimeLt.CrimeSection) &&
                                         crimeLt.CrimeSection.ToUpper().Contains(_searchDetails.ChargeSection.ToUpper())
								   select crimeLt).ToList();
				}

				if (!string.IsNullOrEmpty(_searchDetails.ChargeDescription))
				{
					crimeLkpLst = (from crimeLt in crimeLkpLst
								   where !string.IsNullOrEmpty(crimeLt.CrimeDescription) &&
                                         crimeLt.CrimeDescription.ToUpper().Contains(_searchDetails.ChargeDescription.ToUpper())
								   select crimeLt).ToList();
				}
				if(!string.IsNullOrEmpty(_searchDetails.bookingCaseType))
				{
					crimeLkpLst=(from crimeLt in crimeLkpLst
					where crimeLt.CrimeStatuteCode==_searchDetails.bookingCaseType select crimeLt).ToList();
				}
				if(!string.IsNullOrEmpty(_searchDetails.CrimeType))
				{
					crimeLkpLst=(from crimeLt in crimeLkpLst
					where crimeLt.CrimeType==_searchDetails.CrimeType select crimeLt).ToList();
				}
			}
			_searchDetails.IsArrestSearch = true;
			_arrestIds = crimeLkpLst.Select(cld => cld.ArrestId ?? 0).ToList();

		}

		// Filter Inmate based on Classification details
		private void LoadInmateClassificationDetails()
		{
			if (!string.IsNullOrEmpty(_searchDetails.Classify))
			{
				_personDetailsLst = _personDetailsLst.Where(per => !string.IsNullOrEmpty(per.Classify)
				&& per.Classify.ToUpper() == _searchDetails.Classify.ToUpper()).ToList();
			}
		}

		// Load Address Search Details
		private void LoadAddressSearchDetails()
		{
			if (!_filteredInmateDetailLst.Any()) return;
			IQueryable<Address> addressDetails = _context.Address;

			if (!string.IsNullOrEmpty(_searchDetails.AddressNumber))
			{
                addressDetails = addressDetails.Where(address => address.AddressNumber == _searchDetails.AddressNumber);
			}

			if (!string.IsNullOrEmpty(_searchDetails.AddressDirection))
			{
                addressDetails = addressDetails.Where(address => address.AddressDirection == _searchDetails.AddressDirection);
			}

			if (!string.IsNullOrEmpty(_searchDetails.DirectionSuffix))
			{
                addressDetails = addressDetails.Where(address => address.AddressDirectionSuffix == _searchDetails.DirectionSuffix);
			}

			if (!string.IsNullOrEmpty(_searchDetails.AddressStreet)) // need to implement soundex
			{
                addressDetails = addressDetails.Where(address => address.AddressStreet == _searchDetails.AddressStreet);
			}

			if (!string.IsNullOrEmpty(_searchDetails.AddressCity))// need to implement soundex
			{
                addressDetails = addressDetails.Where(address => address.AddressCity == _searchDetails.AddressCity);
			}

			if (!string.IsNullOrEmpty(_searchDetails.AddressSuffix))
			{
                addressDetails = addressDetails.Where(address => address.AddressSuffix == _searchDetails.AddressSuffix);
			}

			if (!string.IsNullOrEmpty(_searchDetails.AddressUnitType))
			{
                addressDetails = addressDetails.Where(address => address.AddressUnitType == _searchDetails.AddressUnitType);
			}

			if (!string.IsNullOrEmpty(_searchDetails.AddressUnitNumber))
			{
                addressDetails = addressDetails.Where(address => address.AddressUnitNumber == _searchDetails.AddressUnitNumber);
			}

			if (!string.IsNullOrEmpty(_searchDetails.AddressState))
			{
                addressDetails = addressDetails.Where(address => address.AddressState == _searchDetails.AddressState);
			}

			if (!string.IsNullOrEmpty(_searchDetails.AddressZip))
			{
                addressDetails = addressDetails.Where(address => address.AddressZip == _searchDetails.AddressZip);
			}
			if (!string.IsNullOrEmpty(_searchDetails.personOccupation)) 
			{
                addressDetails = addressDetails.Where(address => address.PersonOccupation == _searchDetails.personOccupation);
			}
			if (!string.IsNullOrEmpty(_searchDetails.employer))
			{
                addressDetails = addressDetails.Where(address => address.PersonEmployer == _searchDetails.employer);
			}
			if (!string.IsNullOrEmpty(_searchDetails.AddressType))
			{
                addressDetails = addressDetails.Where(address => address.AddressType == _searchDetails.AddressType);
			}

			if (!string.IsNullOrEmpty(_searchDetails.AddressLine2))
            {
                addressDetails = addressDetails.Where(address => !string.IsNullOrEmpty(address.AddressLine2)
                    && address.AddressLine2.ToLower().Contains(_searchDetails.AddressLine2.ToLower()));
            }
			if (_searchDetails.Homeless || _searchDetails.Transient || _searchDetails.Refused)
			{
				List<Address> tempDetails = new List<Address>();
				if (_searchDetails.Homeless)
				{
					tempDetails.AddRange(addressDetails.Where(address => address.AddressHomeless));
				}

				if (_searchDetails.Transient)
				{
					tempDetails.AddRange(addressDetails.Where(address => address.AddressTransient));
				}

				if (_searchDetails.Refused)
				{
					tempDetails.AddRange(addressDetails.Where(address => address.AddressRefused));
				}
                addressDetails = tempDetails.AsQueryable();
			}

			List<int> personIds = addressDetails.Select(ad => ad.PersonId ?? 0).ToList();
			_filteredInmateDetailLst = _filteredInmateDetailLst.Where(fil => personIds.Contains(fil.PersonId)).Take(10000).ToList();
		}
		private void LoadProfileSearchDetails()
		{
    		if (!_filteredInmateDetailLst.Any()) return;
			IQueryable<PersonDescription> profileDetails = _context.PersonDescription;
			List<int> personIds =new List<int>();
			if (_searchDetails.maritalStatus>0)
			{
				profileDetails = profileDetails.Where(pro => pro.PersonMaritalStatus == _searchDetails.maritalStatus);
				personIds.AddRange( profileDetails.Select(pro => pro.PersonId ?? 0).ToList());
			}
			if(_searchDetails.ethnicity>0)
			{
				profileDetails = profileDetails.Where(pro => pro.PersonEthnicity == _searchDetails.ethnicity);
				personIds.AddRange( profileDetails.Select(pro => pro.PersonId ?? 0).ToList());
			}
			if(_searchDetails.primLang>0)
			{
				profileDetails = profileDetails.Where(pro => pro.PersonPrimaryLanguage == _searchDetails.primLang);
				personIds.AddRange( profileDetails.Select(pro => pro.PersonId ?? 0).ToList());
			}
			if(personIds.Count>0)
			{
				_filteredInmateDetailLst = _filteredInmateDetailLst.Where(fil => personIds.Contains(fil.PersonId)).Take(10000).ToList();
			}
			
			IQueryable<Person> personProfileDetails = _context.Person;
			List<int> personIds1 = new  List<int>();

			if(!string.IsNullOrEmpty(_searchDetails.religion))
			{
				personProfileDetails = personProfileDetails.Where(pro => pro.PersonReligion == _searchDetails.religion);
				personIds1.AddRange(personProfileDetails.Select(pro => pro.PersonId).ToList());
			}
			if(!string.IsNullOrEmpty(_searchDetails.genderIdentity))
			{
				personProfileDetails = personProfileDetails.Where(pro => pro.PersonGenderIdentity == _searchDetails.genderIdentity);
				personIds1.AddRange(personProfileDetails.Select(pro => pro.PersonId).ToList());
			}
			if(!string.IsNullOrEmpty(_searchDetails.eduGrade))
			{
				personProfileDetails = personProfileDetails.Where(pro => pro.PersonEduGrade == _searchDetails.eduGrade);
				personIds1.AddRange(personProfileDetails.Select(pro => pro.PersonId).ToList());
			}
			if(!string.IsNullOrEmpty(_searchDetails.eduDegree))
			{
				personProfileDetails = personProfileDetails.Where(pro => pro.PersonEduDegree == _searchDetails.eduDegree);
				personIds1.AddRange(personProfileDetails.Select(pro => pro.PersonId).ToList());
			}
			if(!string.IsNullOrEmpty(_searchDetails.medInsuranceProvider))
			{
				personProfileDetails = personProfileDetails.Where(pro => pro.PersonMedInsuranceProvider == _searchDetails.medInsuranceProvider);
				personIds1.AddRange(personProfileDetails.Select(pro => pro.PersonId).ToList());
			}
			if(!string.IsNullOrEmpty(_searchDetails.eduDiscipline))
			{
				personProfileDetails = personProfileDetails.Where(pro => pro.PersonEduDiscipline == _searchDetails.eduDiscipline);
				personIds1.AddRange(personProfileDetails.Select(pro => pro.PersonId).ToList());
			}
			if(!string.IsNullOrEmpty(_searchDetails.medInsurancePolicyNo))
			{
				personProfileDetails = personProfileDetails.Where(pro => pro.PersonMedInsurancePolicyNo == _searchDetails.medInsurancePolicyNo);
				personIds1.AddRange(personProfileDetails.Select(pro => pro.PersonId).ToList());
			}
			if(personIds1.Count>0)
			{
				_filteredInmateDetailLst = _filteredInmateDetailLst.Where(fil => personIds1.Contains(fil.PersonId)).Take(10000).ToList();
			}
			
			IQueryable<PersonSkillAndTrade> personSkillandTradeDetails = _context.PersonSkillAndTrade;
			List<int> personIds2 = new List<int>();
			if(!string.IsNullOrEmpty(_searchDetails.skillTrade))
			{
                personSkillandTradeDetails = personSkillandTradeDetails.Where(pro => pro.PersonSkillTrade == _searchDetails.skillTrade);
				personIds2.AddRange(personSkillandTradeDetails.Select(pro => pro.PersonId).ToList());
			}
			if(personIds2.Count>0)
			{
				_filteredInmateDetailLst = _filteredInmateDetailLst.Where(fil => personIds2.Contains(fil.PersonId)).Take(10000).ToList();
			}
			
        }
			private void LoadInmateRelatedDetails()
			{
			if (!_filteredInmateDetailLst.Any()) return;
			IQueryable<Inmate> inmateDetails=_context.Inmate.Where(inmate=> inmate.InmateActive==1);
			IQueryable<PersonFlag> personFlagDetails=_context.PersonFlag.Where(inmate => inmate.DeleteFlag==0);
			
			if(_searchDetails.InmateSearchFacilityId>0)
			{
				List<int> personIds =new List<int>();
				inmateDetails = inmateDetails.Where(pro => pro.FacilityId == _searchDetails.InmateSearchFacilityId);
				personIds.AddRange(inmateDetails.Select(pro => pro.PersonId).ToList());
				_filteredInmateDetailLst = _filteredInmateDetailLst.Where(fil => personIds.Contains(fil.PersonId)).Take(10000).ToList();
			}
			
			if(!string.IsNullOrEmpty(_searchDetails.BuildingId))
			{
				List<int> personIds =new List<int>();
                List<int> housingDetail = _context.HousingUnit.Where(housing =>
                        housing.HousingUnitLocation != "" && housing.HousingUnitLocation == _searchDetails.BuildingId &&
                        housing.FacilityId == _searchDetails.InmateSearchFacilityId &&
                        (!housing.HousingUnitInactive.HasValue || housing.HousingUnitInactive == 0))
                    .Select(hou => hou.HousingUnitId).ToList();
                inmateDetails = inmateDetails.Where(pro=>housingDetail.Contains(pro.HousingUnitId??0) );
				personIds.AddRange(inmateDetails.Select(pro=>pro.PersonId).ToList());
				_filteredInmateDetailLst = _filteredInmateDetailLst.Where(fil => personIds.Contains(fil.PersonId)).Take(10000).ToList();
			}
			if(_searchDetails.podId>0)
			{
				List<int> personIds =new List<int>();
                List<int> housingDetail = _context.HousingUnit.Where(housing =>
                        housing.HousingUnitLocation != "" && housing.HousingUnitLocation == _searchDetails.BuildingId &&
                        housing.HousingUnitListId == _searchDetails.podId &&
                        housing.FacilityId == _searchDetails.InmateSearchFacilityId &&
                        (!housing.HousingUnitInactive.HasValue || housing.HousingUnitInactive == 0))
                    .Select(hou => hou.HousingUnitId).ToList();
                inmateDetails = inmateDetails.Where(pro=>housingDetail.Contains(pro.HousingUnitId??0) );
				personIds.AddRange(inmateDetails.Select(pro=>pro.PersonId).ToList());
				_filteredInmateDetailLst = _filteredInmateDetailLst.Where(fil => personIds.Contains(fil.PersonId)).Take(10000).ToList();
			}
			if(string.IsNullOrEmpty(_searchDetails.cellId))
			{
				List<int> personIds =new List<int>();
				List<int> housingDetail = _context.HousingUnit.Where(hosuing=> hosuing.HousingUnitLocation!="" 
				 && hosuing.HousingUnitLocation==_searchDetails.BuildingId && hosuing.HousingUnitListId==_searchDetails.podId 
				 && hosuing.HousingUnitBedNumber==_searchDetails.cellId && hosuing.FacilityId == _searchDetails.InmateSearchFacilityId
				 && (!hosuing.HousingUnitInactive.HasValue || hosuing.HousingUnitInactive == 0)).Select(hou=>hou.HousingUnitId).ToList();
                inmateDetails = inmateDetails.Where(pro=>housingDetail.Contains(pro.HousingUnitId??0) );
				personIds.AddRange(inmateDetails.Select(pro=>pro.PersonId).ToList());
				_filteredInmateDetailLst = _filteredInmateDetailLst.Where(fil => personIds.Contains(fil.PersonId)).Take(10000).ToList();
			}
			if(!string.IsNullOrEmpty(_searchDetails.bunkId))
			{
				List<int> personIds =new List<int>();
                List<int> housingDetail = _context.HousingUnit.Where(housing => housing.HousingUnitLocation != ""
                    && housing.HousingUnitLocation == _searchDetails.BuildingId &&
                    housing.HousingUnitListId == _searchDetails.podId
                    && housing.HousingUnitBedNumber == _searchDetails.cellId &&
                    housing.HousingUnitBedLocation == _searchDetails.bunkId
                    && housing.FacilityId == _searchDetails.InmateSearchFacilityId &&
                    !housing.HousingUnitInactive.HasValue).Select(hou => hou.HousingUnitId).ToList();
                inmateDetails = inmateDetails.Where(pro=>housingDetail.Contains(pro.HousingUnitId??0) );
				personIds.AddRange(inmateDetails.Select(pro=>pro.PersonId).ToList());
				_filteredInmateDetailLst = _filteredInmateDetailLst.Where(fil => personIds.Contains(fil.PersonId)).Take(10000).ToList();
			}

			if(string.IsNullOrEmpty(_searchDetails.classificationId))
			{
				List<int> personIds =new List<int>();
                inmateDetails = inmateDetails.Where(pro=>pro.InmateClassification.InmateClassificationReason==_searchDetails.classificationId);
				personIds.AddRange(inmateDetails.Select(pro=>pro.PersonId).ToList());
				_filteredInmateDetailLst = _filteredInmateDetailLst.Where(fil => personIds.Contains(fil.PersonId)).Take(10000).ToList();
			}
			if(_searchDetails.locationId>0)
			{
				List<int> personIds =new List<int>();
                inmateDetails = inmateDetails.Where(pro=>pro.InmateCurrentTrackId==_searchDetails.locationId);
				personIds.AddRange(inmateDetails.Select(pro=>pro.PersonId).ToList());
				_filteredInmateDetailLst = _filteredInmateDetailLst.Where(fil => personIds.Contains(fil.PersonId)).Take(10000).ToList();
			}

            if (_searchDetails.personFlagId > 0)
            {

                List<int> personIds = new List<int>();
                List<Lookup> personCautionLst =
               _context.Lookup.Where(pfa => pfa.LookupType == LookupConstants.PERSONCAUTION 
                                            && pfa.LookupIndex == _searchDetails.personFlagId).ToList();

                List<int> lookupIndex = new List<int>();
                lookupIndex.AddRange(personCautionLst.Select(pre => pre.LookupIndex));

                personFlagDetails = personFlagDetails.Where(perfla => lookupIndex.Contains(perfla.PersonFlagIndex.Value));
                personIds.AddRange(personFlagDetails.Select(pro => pro.PersonId).ToList());
                _filteredInmateDetailLst = _filteredInmateDetailLst.Where(fil => personIds.Contains(fil.PersonId)).Take(10000).ToList();


            }
            if (_searchDetails.inmateFlagId > 0)
            {

                List<int> personIds = new List<int>();
                List<Lookup> personCautionLst =
               _context.Lookup.Where(pfa => pfa.LookupType == LookupConstants.TRANSCAUTION 
                                            && pfa.LookupIndex == _searchDetails.inmateFlagId).ToList();

                List<int> lookupIndex = new List<int>();
                lookupIndex.AddRange(personCautionLst.Select(pre => pre.LookupIndex));

                personFlagDetails = personFlagDetails.Where(perfla => lookupIndex.Contains(perfla.InmateFlagIndex.Value));
                personIds.AddRange(personFlagDetails.Select(pro => pro.PersonId).ToList());
                _filteredInmateDetailLst = _filteredInmateDetailLst.Where(fil => personIds.Contains(fil.PersonId)).Take(10000).ToList();


            }

            if (_searchDetails.medDietFlagId == 0) return;
            {
                List<int> personIds = new List<int>();
                List<Lookup> personCautionLst =
                    _context.Lookup.Where(pfa => (pfa.LookupType == LookupConstants.DIET || pfa.LookupType == LookupConstants.MEDFLAG) 
                                                 && pfa.LookupIndex == _searchDetails.medDietFlagId).ToList();

                List<int> lookupIndex = new List<int>();
                lookupIndex.AddRange(personCautionLst.Select(pre => pre.LookupIndex));

                personFlagDetails = personFlagDetails.Where(perfla => lookupIndex.Contains(perfla.DietFlagIndex.Value));
                personIds.AddRange(personFlagDetails.Select(pro => pro.PersonId).ToList());
                _filteredInmateDetailLst = _filteredInmateDetailLst.Where(fil => personIds.Contains(fil.PersonId)).Take(10000).ToList();
            }
        }			
		// Calculate Person Age based on person DOB
		private static int GetPersonAge(DateTime? personDob)
		{
			if (!personDob.HasValue) return 0;
			DateTime todayDate = DateTime.Now.Date;
			int personAge = todayDate.Year - personDob.Value.Year;
			if (personDob > todayDate.AddYears(-personAge))
				personAge--;
			return personAge;
		}

		// Load Filtered InmateDetails
		private void LoadFilteredInmateDetails()
		{
			IQueryable<Inmate> dbInmateDetails = from inm in _context.Inmate
                where (!_searchDetails.ActiveOnly || inm.InmateActive == 1) &&
                      (_searchDetails.FacilityId == 0 || inm.FacilityId == _searchDetails.FacilityId)
                select inm;

			if (_searchDetails.IsPersonSearch || _searchDetails.IsCharSearch || _searchDetails.IsDescriptorSearch)
			{
				_filteredInmateDetailLst = _searchDetails.IsInmateSearch
					? (from inml in _inmateLstValue
					   where _personFilteredIdLst.Contains(inml.Key)
					   select new FilteredInmateDetails { InmateId = inml.Value, PersonId = inml.Key }).ToList()
					: (from inml in dbInmateDetails
					   where _personFilteredIdLst.Contains(inml.PersonId)
					   select new FilteredInmateDetails { InmateId = inml.InmateId, PersonId = inml.PersonId })
						.ToList();
			}
			else if (_searchDetails.IsArrestSearch)
			{
				List<int> arrInmateIds = _context.Arrest
					.Where(arr => _arrestIds.Contains(arr.ArrestId)).Select(arr => arr.InmateId ?? 0).ToList();

				_filteredInmateDetailLst = _searchDetails.IsInmateSearch
					? (from inml in _inmateLstValue
					   where arrInmateIds.Contains(inml.Value)
					   select new FilteredInmateDetails { InmateId = inml.Value, PersonId = inml.Key }).ToList()
					: (from inml in dbInmateDetails
					   where inml.PersonId > 0 && arrInmateIds.Contains(inml.InmateId)
					   select new FilteredInmateDetails { InmateId = inml.InmateId, PersonId = inml.PersonId }).ToList();
			}
			else
			{
				_filteredInmateDetailLst = _searchDetails.IsInmateSearch
					? (from inml in _inmateLstValue
					   select new FilteredInmateDetails { InmateId = inml.Value, PersonId = inml.Key }).ToList()
					: (from inml in dbInmateDetails
					   where inml.PersonId > 0
					   select new FilteredInmateDetails { InmateId = inml.InmateId, PersonId = inml.PersonId }).Take(20000).ToList();
			}
		}

		// Load Filtered Person Details
		private void LoadFilteredPersonDetails()
		{
			IQueryable<Inmate> dbInmateDetails = from inm in _context.Inmate
                where (!_searchDetails.ActiveOnly || inm.InmateActive == 1) &&
                      (_searchDetails.FacilityId == 0 || inm.FacilityId == _searchDetails.FacilityId)
                select inm;
			IQueryable<Incarceration> dbIncarcerationLst = _context.Incarceration.Where(inc => inc.InmateId > 0
				&& (!_searchDetails.ActiveOnly || !inc.ReleaseOut.HasValue));
			
					
			IQueryable<Arrest> dbArrestDetails = from arrd in _context.Arrest
                where arrd.InmateId > 0 &&
                      (!_searchDetails.ActiveOnly || !_searchDetails.IsBookingSearch || arrd.ArrestActive == 1)
                select new Arrest
                {
                    ArrestId = arrd.ArrestId,
                    InmateId = arrd.InmateId,
                    ArrestType = arrd.ArrestType,
                    ArrestBillingAgencyId = arrd.ArrestBillingAgencyId,
                    ArrestCourtJurisdictionId = arrd.ArrestCourtJurisdictionId,
					ArrestSentenceCode=arrd.ArrestSentenceCode,
					ArrestSentenceReleaseDate=arrd.ArrestSentenceReleaseDate,
					ArrestSupSeqNumber=arrd.ArrestSupSeqNumber
					
                };
			if (_searchDetails.IsBookingSearch)
			{
				List<int> selectedInmateIds =
				   _filteredInmateDetailLst.Where(de => de.InmateId > 0).Select(fid => fid.InmateId).ToList();
				List<SearchResult> dbfilteredArrestDetails = (from arr in dbArrestDetails
															  where (!_searchDetails.IsArrestSearch || _arrestIds.Contains(arr.ArrestId))
															  && selectedInmateIds.Contains(arr.InmateId ?? 0)
															  select new SearchResult
															  {
																  ArrestId = arr.ArrestId,
																  InmateId = arr.InmateId ?? 0,
																  ArrestType = arr.ArrestType,
																  ArrestBillingAgencyId = arr.ArrestBillingAgencyId,
																  ArrestCourtJurisdictionId = arr.ArrestCourtJurisdictionId,
																  ArrestSentenceCode=arr.ArrestSentenceCode,
																  ArrestSentenceReleaseDate=arr.ArrestSentenceReleaseDate,
																  CourtDocket=arr.ArrestCourtDocket,
																  CaseNo=arr.ArrestCaseNumber,
																  ArrestReleaseClearedDate=arr.ArrestReleaseClearedDate,
																  
																  
															  }).Distinct().ToList();


				_filteredInmateDetailLst = (from arr in dbfilteredArrestDetails
												//where 
											select new FilteredInmateDetails
											{
												ArrestId = arr.ArrestId,
												InmateId = arr.InmateId
											}).Distinct().ToList();

				List<FilteredInmateDetails> inmatePersonIds = (from inmt in dbInmateDetails
															   where selectedInmateIds.Contains(inmt.InmateId)
															   select new FilteredInmateDetails
															   {
																   InmateId = inmt.InmateId,
																   PersonId = inmt.PersonId
															   }).Distinct().ToList();

				_filteredInmateDetailLst.ForEach(item =>
				{
					item.PersonId = inmatePersonIds.SingleOrDefault(pr => pr.InmateId == item.InmateId)?.PersonId ?? 0;
				});
				List<int> selectedArrestIds =
					_filteredInmateDetailLst.Where(de => de.InmateId > 0).Select(fid => fid.ArrestId).ToList();

                _personDetailsLst = (from axrf in _context.IncarcerationArrestXref
                    where axrf.ArrestId > 0 && selectedArrestIds.Contains(axrf.ArrestId ?? 0)
                    select new SearchResult
                    {
                        ArrestId = axrf.ArrestId ?? 0,
                        IncarcerationId = dbIncarcerationLst
                            .SingleOrDefault(inc => inc.IncarcerationId == axrf.IncarcerationId).IncarcerationId,
                        BookingDate = axrf.BookingDate,
                        ReleaseDate = axrf.ReleaseDate,
                        ReleaseReason = axrf.ReleaseReason,
                        DateIn = dbIncarcerationLst.SingleOrDefault(inc => inc.IncarcerationId == axrf.IncarcerationId)
                            .DateIn,
                        ReleaseOut = dbIncarcerationLst
                            .SingleOrDefault(inc => inc.IncarcerationId == axrf.IncarcerationId).ReleaseOut,
                        keeper = dbIncarcerationLst.SingleOrDefault(inc => inc.IncarcerationId == axrf.IncarcerationId)
                            .NoKeeper == false ? "Keeper" : "No Keeper",
                        TransportFlag = dbIncarcerationLst
                            .SingleOrDefault(inc => inc.IncarcerationId == axrf.IncarcerationId).TransportFlag,
                        TransportScheduleDate = dbIncarcerationLst
                            .SingleOrDefault(inc => inc.IncarcerationId == axrf.IncarcerationId).TransportScheduleDate,
                        OverallSentStartDate = dbIncarcerationLst
                            .SingleOrDefault(inc => inc.IncarcerationId == axrf.IncarcerationId).OverallSentStartDate,
                        OverallFinalReleaseDate = dbIncarcerationLst
                            .SingleOrDefault(inc => inc.IncarcerationId == axrf.IncarcerationId)
                            .OverallFinalReleaseDate,
                        ArrestSentenceReleaseDate = dbfilteredArrestDetails
                            .SingleOrDefault(inc => inc.ArrestId == axrf.ArrestId).ArrestSentenceReleaseDate,

                    }).Distinct().ToList();
				Parallel.ForEach(_personDetailsLst, item =>
				{
					item.PersonId =
						_filteredInmateDetailLst.SingleOrDefault(aid => aid.ArrestId == item.ArrestId)?.PersonId ?? 0;
					item.InmateId =
						dbfilteredArrestDetails.SingleOrDefault(ar => ar.ArrestId == item.ArrestId)?.InmateId ?? 0;
					item.ArrestType =
						dbfilteredArrestDetails.SingleOrDefault(ar => ar.ArrestId == item.ArrestId)?.ArrestType;
					item.ArrestBillingAgencyId =
						dbfilteredArrestDetails.SingleOrDefault(ar => ar.ArrestId == item.ArrestId)?
							.ArrestBillingAgencyId;
					item.ArrestCourtJurisdictionId =
						dbfilteredArrestDetails.SingleOrDefault(ar => ar.ArrestId == item.ArrestId)?
							.ArrestCourtJurisdictionId;
				});
				_personDetailsLst.ForEach (item =>
				{
					item.CaseCount=GetCasesCount(item.InmateId,dbIncarcerationLst);
				});
			}
			else if(_searchDetails.isCaseSearch)
			{
				List<int> selectedInmateIds =
				   _filteredInmateDetailLst.Where(de => de.InmateId > 0).Select(fid => fid.InmateId).ToList();
				List<SearchResult> dbfilteredArrestDetails = (from arr in dbArrestDetails
															  where (!_searchDetails.IsArrestSearch || _arrestIds.Contains(arr.ArrestId))
															  && selectedInmateIds.Contains(arr.InmateId ?? 0)
															  select new SearchResult
															  {
																  ArrestId = arr.ArrestId,
																  InmateId = arr.InmateId ?? 0,
																  ArrestType = arr.ArrestType,
																  ArrestBillingAgencyId = arr.ArrestBillingAgencyId,
																  ArrestCourtJurisdictionId = arr.ArrestCourtJurisdictionId,
																  ArrestSentenceCode=arr.ArrestSentenceCode,
																  ArrestSentenceReleaseDate=arr.ArrestSentenceReleaseDate,
																  ArrestSupSeqNumber=arr.ArrestSupSeqNumber
																  
															  }).ToList();
				_filteredInmateDetailLst = (from arr in dbfilteredArrestDetails
												//where 
											select new FilteredInmateDetails
											{
												ArrestId = arr.ArrestId,
												InmateId = arr.InmateId
											}).Distinct().ToList();
				List<FilteredInmateDetails> inmatePersonIds = (from inmt in dbInmateDetails
															   where selectedInmateIds.Contains(inmt.InmateId)
															   select new FilteredInmateDetails
															   {
																   InmateId = inmt.InmateId,
																   PersonId = inmt.PersonId
															   }).Distinct().ToList();
				_filteredInmateDetailLst.ForEach(item =>
				{
					item.PersonId = inmatePersonIds.SingleOrDefault(pr => pr.InmateId == item.InmateId)?.PersonId ?? 0;
				});

				List<int> selectedArrestIds =
					_filteredInmateDetailLst.Where(de => de.InmateId > 0).Select(fid => fid.ArrestId).ToList();
					List<Crime> crimeDetails =
					_context.Crime.Where(war => selectedArrestIds.Contains(war.ArrestId ?? 0)).ToList();
List<Warrant> warrantfDetails =
					_context.Warrant.Where(war => selectedArrestIds.Contains(war.ArrestId ?? 0)).ToList();
					List<CrimeForce> crimefDetails =
					_context.CrimeForce.Where(war => selectedArrestIds.Contains(war.ArrestId ?? 0)).ToList();
                    _personDetailsLst = (from axrf in _context.IncarcerationArrestXref
                        where axrf.ArrestId > 0 && selectedArrestIds.Contains(axrf.ArrestId ?? 0)
                        select new SearchResult
                        {
                            ArrestId = axrf.ArrestId ?? 0,
                            IncarArrestXrefId = axrf.IncarcerationArrestXrefId,
                            IncarcerationId = dbIncarcerationLst
                                .SingleOrDefault(inc => inc.IncarcerationId == axrf.IncarcerationId).IncarcerationId,
                            BookingDate = axrf.BookingDate,
                            ReleaseDate = axrf.ReleaseDate,
                            ReleaseReason = axrf.ReleaseReason,
                            ArrestSentenceCode =
                                dbfilteredArrestDetails.SingleOrDefault(inc => inc.ArrestId == axrf.ArrestId)
                                    .ArrestSentenceCode != null ? dbfilteredArrestDetails
                                    .SingleOrDefault(inc => inc.ArrestId == axrf.ArrestId).ArrestSentenceCode : 3,
                            ArrestSentenceReleaseDate = dbfilteredArrestDetails
                                .SingleOrDefault(inc => inc.ArrestId == axrf.ArrestId).ArrestSentenceReleaseDate,
                            ArrestReleaseClearedDate = dbfilteredArrestDetails
                                .SingleOrDefault(ar => ar.ArrestId == axrf.ArrestId).ArrestReleaseClearedDate,
                            ArrestSupSeqNumber = dbfilteredArrestDetails
                                .SingleOrDefault(ar => ar.ArrestId == axrf.ArrestId).ArrestSupSeqNumber,
                            WarrantCount = warrantfDetails.Count(ar => ar.ArrestId == axrf.ArrestId),
                            WarrantCrimeCount =
                                crimeDetails.Count(ar => ar.ArrestId == axrf.ArrestId && ar.WarrantId != null),
                            CrimeCount = crimeDetails.Count(ar => ar.ArrestId == axrf.ArrestId && ar.WarrantId == null),
                            CrimeForceCount =
                                crimefDetails.Count(ar => ar.ArrestId == axrf.ArrestId && ar.WarrantId == null),
                            WarrantCrimeForceCount =
                                crimefDetails.Count(ar => ar.ArrestId == axrf.ArrestId && ar.WarrantId != null),
                        }).ToList();
 
				Parallel.ForEach(_personDetailsLst, item =>
				{
					item.PersonId =
						_filteredInmateDetailLst.SingleOrDefault(aid => aid.ArrestId == item.ArrestId)?.PersonId ?? 0;
					item.InmateId =
						dbfilteredArrestDetails.SingleOrDefault(ar => ar.ArrestId == item.ArrestId)?.InmateId ?? 0;
					item.ArrestType =
						dbfilteredArrestDetails.SingleOrDefault(ar => ar.ArrestId == item.ArrestId)?.ArrestType;
					item.ArrestBillingAgencyId =
						dbfilteredArrestDetails.SingleOrDefault(ar => ar.ArrestId == item.ArrestId)?
							.ArrestBillingAgencyId;
					item.ArrestCourtJurisdictionId =
						dbfilteredArrestDetails.SingleOrDefault(ar => ar.ArrestId == item.ArrestId)?
							.ArrestCourtJurisdictionId;
				});
				
			}
			else if(_searchDetails.isChargeSearch)
			{
				
				List<int> selectedInmateIds =
				   _filteredInmateDetailLst.Where(de => de.InmateId > 0).Select(fid => fid.InmateId).ToList();
				List<SearchResult> dbfilteredArrestDetails = (from arr in dbArrestDetails
															  where (!_searchDetails.IsArrestSearch || _arrestIds.Contains(arr.ArrestId))
															  && selectedInmateIds.Contains(arr.InmateId ?? 0)
															  select new SearchResult
															  {
																  ArrestId = arr.ArrestId,
																  InmateId = arr.InmateId ?? 0,
																  ArrestType = arr.ArrestType,
																  ArrestBillingAgencyId = arr.ArrestBillingAgencyId,
																  ArrestCourtJurisdictionId = arr.ArrestCourtJurisdictionId,
																  ArrestSentenceCode=arr.ArrestSentenceCode,
																  ArrestSentenceReleaseDate=arr.ArrestSentenceReleaseDate,
																  ArrestSupSeqNumber=arr.ArrestSupSeqNumber
																  
															  }).ToList();
				_filteredInmateDetailLst = (from arr in dbfilteredArrestDetails
												//where 
											select new FilteredInmateDetails
											{
												ArrestId = arr.ArrestId,
												InmateId = arr.InmateId
											}).Distinct().ToList();
											
				List<FilteredInmateDetails> inmatePersonIds = (from inmt in dbInmateDetails
															   where selectedInmateIds.Contains(inmt.InmateId)
															   select new FilteredInmateDetails
															   {
																   InmateId = inmt.InmateId,
																   PersonId = inmt.PersonId
															   }).Distinct().ToList();
				_filteredInmateDetailLst.ForEach(item =>
				{
					item.PersonId = inmatePersonIds.SingleOrDefault(pr => pr.InmateId == item.InmateId)?.PersonId ?? 0;
				});

				List<int> selectedArrestIds =
					_filteredInmateDetailLst.Where(de => de.InmateId > 0).Select(fid => fid.ArrestId).ToList();
List<Warrant> warrantDetails =
					_context.Warrant.Where(war => selectedArrestIds.Contains(war.ArrestId ?? 0)).ToList();
					List<IncarcerationArrestXref> incarcerationxrefDetails =
					_context.IncarcerationArrestXref.Where(war => selectedArrestIds.Contains(war.ArrestId ?? 0)).ToList();
                    _personDetailsLst = (from axrf in _context.Crime
                        where axrf.ArrestId > 0 && selectedArrestIds.Contains(axrf.ArrestId ?? 0)
                        select new SearchResult
                        {
                            ArrestId = axrf.ArrestId ?? 0,
                            BookingDate = incarcerationxrefDetails.SingleOrDefault(ar => ar.ArrestId == axrf.ArrestId)
                                .BookingDate,
                            ReleaseDate = incarcerationxrefDetails.SingleOrDefault(ar => ar.ArrestId == axrf.ArrestId)
                                .ReleaseDate,
                            ReleaseReason = incarcerationxrefDetails.SingleOrDefault(ar => ar.ArrestId == axrf.ArrestId)
                                .ReleaseReason,

                            ArrestReleaseClearedDate = dbfilteredArrestDetails
                                .SingleOrDefault(ar => ar.ArrestId == axrf.ArrestId).ArrestReleaseClearedDate,
                            ArrestSupSeqNumber = dbfilteredArrestDetails
                                .SingleOrDefault(ar => ar.ArrestId == axrf.ArrestId).ArrestSupSeqNumber,
                            WarrantNumber = warrantDetails.SingleOrDefault(ar => ar.ArrestId == axrf.ArrestId)
                                .WarrantNumber,
                            ChargeStatus = axrf.CrimeType,
                            Charges = axrf.CrimeLookup.CrimeCodeType + ' ' + axrf.CrimeLookup.CrimeSection + ' ' +
                                axrf.CrimeLookup.CrimeStatuteCode + ' ' + axrf.CrimeLookup.CrimeDescription,

                        }).ToList();
 
				Parallel.ForEach(_personDetailsLst, item =>
				{
					item.PersonId =
						_filteredInmateDetailLst.SingleOrDefault(aid => aid.ArrestId == item.ArrestId)?.PersonId ?? 0;
					item.InmateId =
						dbfilteredArrestDetails.SingleOrDefault(ar => ar.ArrestId == item.ArrestId)?.InmateId ?? 0;
					item.ArrestType =
						dbfilteredArrestDetails.SingleOrDefault(ar => ar.ArrestId == item.ArrestId)?.ArrestType;
					item.ArrestBillingAgencyId =
						dbfilteredArrestDetails.SingleOrDefault(ar => ar.ArrestId == item.ArrestId)?
							.ArrestBillingAgencyId;
					item.ArrestCourtJurisdictionId =
						dbfilteredArrestDetails.SingleOrDefault(ar => ar.ArrestId == item.ArrestId)?
							.ArrestCourtJurisdictionId;
				});
				
			}
			else
			{
				List<int> arrInmateIds = new List<int>();
				if (_searchDetails.IsArrestSearch)
				{
					arrInmateIds = dbArrestDetails.Where(arr => arr.InmateId > 0 &&
													 _arrestIds.Contains(arr.ArrestId))
					   .Select(de => de.InmateId ?? 0).ToList();
				}

				_personDetailsLst = (from ids in _filteredInmateDetailLst
									 where ids.InmateId > 0 &&
									 (!_searchDetails.IsArrestSearch || arrInmateIds.Contains(ids.InmateId))
									 select new SearchResult
									 {
										 InmateId = ids.InmateId,
										 PersonId = ids.PersonId,
										 
									 }).ToList();
				_personDetailsLst.ForEach (item =>
				{
					item.CaseCount=GetBookingCount(item.InmateId,dbIncarcerationLst);
				});
			}

            if (!_personDetailsLst.Any()) return;
			 IQueryable<Lookup> lookups = _context.Lookup
                .Where(w => (w.LookupType == LookupConstants.HAIRCOL || w.LookupType==LookupConstants.EYECOLOR||
				 w.LookupType==LookupConstants.SEX|| w.LookupType==LookupConstants.RACE||w.LookupType == LookupConstants.ARRTYPE
				 ||w.LookupType==LookupConstants.CRIMETYPE)) ;
            List<int> selectedPersonIds = _personDetailsLst.Select(item => item.PersonId).Distinct().ToList();
            List<PersonCharVm> dbPersonLst = (from pr in _context.Person
                where selectedPersonIds.Contains(pr.PersonId)
                select new PersonCharVm
                {
                    PersonId = pr.PersonId,
                    PersonFirstName = pr.PersonFirstName,
                    PersonLastName = pr.PersonLastName,
                    PersonMiddleName = pr.PersonMiddleName,
                    PersonDob = pr.PersonDob,
                    PersonDlNumber = pr.PersonDlNumber,
                    PersonCii = pr.PersonCii,
                    AfisNumber = pr.AfisNumber,
                    HairColorName = lookups.SingleOrDefault(look => look.LookupIndex == pr.PersonHairColorLast && 
                        look.LookupType == LookupConstants.HAIRCOL).LookupName,
                    EyeColorName = lookups.SingleOrDefault(look => look.LookupIndex == pr.PersonEyeColorLast && 
                        look.LookupType == LookupConstants.EYECOLOR).LookupName,
                    RaceName = lookups.SingleOrDefault(look => look.LookupIndex == pr.PersonRaceLast && 
                        look.LookupType == LookupConstants.RACE).LookupName,
                    SexName = lookups.SingleOrDefault(look => look.LookupIndex == pr.PersonSexLast && 
                        look.LookupType == LookupConstants.SEX).LookupName,

                    PrimaryHeight = pr.PersonHeightPrimaryLast,
                    SecondaryHeight = pr.PersonHeightSecondaryLast,
                    Weight = pr.PersonWeightLast,
                    Placeofbirth = pr.PersonPlaceOfBirth,

                }).ToList();

            List<Identifiers> identifierLst = (from idf in _context.Identifiers
                where idf.IdentifierType == "1" && idf.DeleteFlag == 0
                                                && selectedPersonIds.Contains(idf.PersonId ?? 0)
                select new Identifiers
                {
                    PersonId = idf.PersonId,
                    IdentifierId = idf.IdentifierId,
                    PhotographRelativePath = _photos.GetPhotoByIdentifier(idf)
                }).OrderByDescending(o => o.IdentifierId).ToList();

			//Booking Result
            Parallel.ForEach(_personDetailsLst, item =>
            {
                item.Photofilepath =
                    identifierLst.FirstOrDefault(idn => idn.PersonId == item.PersonId)?.PhotographRelativePath;
                item.FirstName = dbPersonLst.SingleOrDefault(pr => pr.PersonId == item.PersonId)?.PersonFirstName;
                item.MiddleName = dbPersonLst.SingleOrDefault(pr => pr.PersonId == item.PersonId)?.PersonMiddleName;
                item.LastName = dbPersonLst.SingleOrDefault(pr => pr.PersonId == item.PersonId)?.PersonLastName;
                item.Dob = dbPersonLst.SingleOrDefault(pr => pr.PersonId == item.PersonId)?.PersonDob;
				item.DlNumber=dbPersonLst.SingleOrDefault(pr=>pr.PersonId==item.PersonId)?.PersonDlNumber;
				item.Personcii=dbPersonLst.SingleOrDefault(pr=>pr.PersonId==item.PersonId)?.PersonCii;
				item.AfisNumber=dbPersonLst.SingleOrDefault(pr=>pr.PersonId==item.PersonId)?.AfisNumber;
				item.HairColorName=dbPersonLst.SingleOrDefault(pr=>pr.PersonId==item.PersonId)?.HairColorName;
				item.EyeColorName=dbPersonLst.SingleOrDefault(pr=>pr.PersonId==item.PersonId)?.EyeColorName;
				item.RaceName=dbPersonLst.SingleOrDefault(pr=>pr.PersonId==item.PersonId)?.RaceName;
				item.SexName=dbPersonLst.SingleOrDefault(pr=>pr.PersonId==item.PersonId)?.SexName;
				item.PrimaryHeight=dbPersonLst.SingleOrDefault(pr=>pr.PersonId==item.PersonId)?.PrimaryHeight;
				item.SecondaryHeight=dbPersonLst.SingleOrDefault(pr=>pr.PersonId==item.PersonId)?.SecondaryHeight;
				item.Weight=dbPersonLst.SingleOrDefault(pr=>pr.PersonId==item.PersonId)?.Weight;
				item.Placeofbirth=dbPersonLst.SingleOrDefault(pr=>pr.PersonId==item.PersonId)?.Placeofbirth;
				
            });
        }

        private int GetCasesCount(int inmateId,IQueryable<Incarceration> ss)
        {
            List<int> incarcerationLst=ss.Where(inc=>inc.InmateId== inmateId).Select(inc=>inc.IncarcerationId).ToList();
            List<int?> arrestIdLst = _context.IncarcerationArrestXref
                .Where(incx => incarcerationLst.Contains(incx.IncarcerationId ?? 0)).Select(incx => incx.ArrestId)
                .ToList();
	        return _context.Arrest.Count(arr => arrestIdLst.Contains(arr.ArrestId));
        }

        //TODO The name sound super misleading!
        private static int GetBookingCount(int inmateId, IQueryable<Incarceration> ss) =>
            ss.Count(inc => inc.InmateId == inmateId);

        // Load Inmate, Person & Arrest Details
		private void LoadInmateAndArrestDetails()
		{
			IQueryable<Inmate> dbInmateDetails = from inm in _context.Inmate
                where (!_searchDetails.ActiveOnly || inm.InmateActive == 1) &&
                      (_searchDetails.FacilityId == 0 || inm.FacilityId == _searchDetails.FacilityId)
                select inm;

			List<int> inmateIds = _personDetailsLst.Select(pdl => pdl.InmateId).Distinct().Take(1000).ToList();
			List<int> arrestIdLst = _personDetailsLst.Select(pdl => pdl.ArrestId).Take(1000).ToList();

			List<Inmate> selectedInmates = (from inmt in dbInmateDetails
											where inmateIds.Contains(inmt.InmateId)
											select new Inmate
											{
												InmateId = inmt.InmateId,
												FacilityId = inmt.FacilityId,
												InmateNumber = inmt.InmateNumber,
												InmateActive = inmt.InmateActive
											}).ToList();

			inmateIds = (from inm2 in _context.Inmate
						 where inm2.InmateClassificationId > 0 && inmateIds.Contains(inm2.InmateId)
						 select inm2.InmateClassificationId ?? 0).ToList();

			List<InmateClassification> inmateClasificatnLst = (from incl in _context.InmateClassification
															   where incl.InmateId > 0 && inmateIds.Contains(incl.InmateClassificationId)
															   select new InmateClassification
															   {
																   InmateId = incl.InmateId,
																   InmateClassificationReason = incl.InmateClassificationReason
															   }).ToList();

			List<Lookup> dbLookupDetails = (from lkp in _context.Lookup
											where lkp.LookupType == LookupConstants.ARRTYPE && !string.IsNullOrEmpty(lkp.LookupDescription)
											select new Lookup
											{
												LookupDescription = lkp.LookupDescription,
												LookupIndex = lkp.LookupIndex
											}).Distinct().ToList();
											List<Lookup> dbLookupDetailscrime = (from lkp in _context.Lookup
											where lkp.LookupType == LookupConstants.CRIMETYPE && !string.IsNullOrEmpty(lkp.LookupDescription)
											select new Lookup
											{
												LookupDescription = lkp.LookupDescription,
												LookupIndex = lkp.LookupIndex
											}).Distinct().ToList();

			IQueryable<Arrest> dbArrestDetails = from arr in _context.Arrest
                where arr.InmateId > 0 &&
                      (!_searchDetails.ActiveOnly || !_searchDetails.IsBookingSearch || arr.ArrestActive == 1)
                select new Arrest
                {
                    ArrestId = arr.ArrestId,
                    ArrestBookingNo = arr.ArrestBookingNo,
                    ArrestCaseNumber = arr.ArrestCaseNumber,
                    ArrestCourtDocket = arr.ArrestCourtDocket
                };

			List<Arrest> filteredArrestLst = (from arr in dbArrestDetails
											  where arrestIdLst.Contains(arr.ArrestId)
											  select arr).ToList();

			List<Agency> agencyDetailLst = (from ag in _context.Agency
											where !string.IsNullOrEmpty(ag.AgencyAbbreviation)
											select new Agency
											{
												AgencyAbbreviation = ag.AgencyAbbreviation,
												AgencyId = ag.AgencyId
											}).ToList();

			// Parallel.ForEach will be procced based on multiple thread functionality
			Parallel.ForEach(_personDetailsLst, details =>
			{
				details.CaseNo =
					filteredArrestLst.SingleOrDefault(far => far.ArrestId == details.ArrestId)?.ArrestCaseNumber;
				details.BookingNumber =
					filteredArrestLst.SingleOrDefault(far => far.ArrestId == details.ArrestId)?.ArrestBookingNo;
				details.CourtDocket =
					filteredArrestLst.SingleOrDefault(far => far.ArrestId == details.ArrestId)?.ArrestCourtDocket;
				details.BookingType = dbLookupDetails.SingleOrDefault(
					lkp => !string.IsNullOrEmpty(details.ArrestType) && 
						   lkp.LookupIndex.ToString() == details.ArrestType.Trim())?.LookupDescription;
				details.ChargeStatus = dbLookupDetailscrime.SingleOrDefault(
					lkp => !string.IsNullOrEmpty(details.ChargeStatus) && 
						   lkp.LookupIndex.ToString() == details.ChargeStatus.Trim())?.LookupDescription;
				details.Court =
					agencyDetailLst.SingleOrDefault(agn => agn.AgencyId == details.ArrestCourtJurisdictionId)?
						.AgencyAbbreviation;
				details.ArrestAgency =
					agencyDetailLst.SingleOrDefault(agn => agn.AgencyId == details.ArrestBillingAgencyId)?
						.AgencyAbbreviation;

				details.InmateActive =
				   selectedInmates.SingleOrDefault(pd => pd.InmateId == details.InmateId)?.InmateActive ?? 0;
				details.FacilityId =
					selectedInmates.SingleOrDefault(pd => pd.InmateId == details.InmateId)?.FacilityId ?? 0;
				details.InmateNumber =
					selectedInmates.SingleOrDefault(pd => pd.InmateId == details.InmateId)?.InmateNumber;
				details.Classify =
					inmateClasificatnLst.LastOrDefault(pd => pd.InmateId == details.InmateId)?
						.InmateClassificationReason;
			});
		}

		// Get Charges Details in Crime, Crime Force and Warrant tables based on Arrest Id
		public List<BookingSearchSubData> GetChargeDetails(int arrestId)
		{
			List<BookingSearchSubData> bookingCrimeDetailLst = new List<BookingSearchSubData>();
            if (arrestId <= 0) return bookingCrimeDetailLst;
            IQueryable<Warrant> warrantDetails = _context.Warrant.Where(wa => wa.ArrestId == arrestId);

            //Warrant Search Details
            bookingCrimeDetailLst = warrantDetails.Select(wa => new
                {
                    wa.ArrestId,
                    wa.WarrantNumber,
                    wa.WarrantDescription,
                    wa.WarrantCounty,
                    wa.WarrantType,
                    wa.WarrantBailType,
                    wa.WarrantBailAmount,
                    wa.CreateDate
                }).Distinct().Select(wa => new BookingSearchSubData
                {
                    ArrestId = wa.ArrestId ?? 0,
                    WarrantNumber = wa.WarrantNumber,
                    Description = wa.WarrantDescription,
                    WarrantCounty = wa.WarrantCounty,
                    Type = wa.WarrantType,
                    BailType = wa.WarrantBailType,
                    BailAmount = wa.WarrantBailAmount,
                    CreateDate = wa.CreateDate,
                    BailFlag = wa.WarrantBailType != null && wa.WarrantBailType == BailType.NOBAIL
                }).ToList();

            //Crime Search Details
            List<BookingSearchSubData> crimeDetailsLst =
                _context.Crime.Where(cr => cr.ArrestId == arrestId && cr.CrimeDeleteFlag == 0)
                    .Select(cr => new
                    {
                        cr.CrimeLookupId,
                        cr.ArrestId,
                        cr.WarrantId,
                        cr.Warrant,
                        cr.CrimeLookup,
                        cr.BailAmount,
                        cr.BailNoBailFlag,
                        cr.BailType,
                        cr.CrimeQualifierLookup,
                        cr.CreateDate
                    }).Distinct().Select(cr => new BookingSearchSubData
                    {
                        ArrestId = cr.ArrestId ?? 0,
                        CrimeLookupId = cr.CrimeLookupId,
                        WarrantNumber = cr.WarrantId > 0 ? cr.Warrant.WarrantNumber : string.Empty,
                        CrimeSection = cr.CrimeLookup.CrimeSection,
                        CrimeSubSection = cr.CrimeLookup.CrimeSubSection,
                        Description = cr.CrimeLookup.CrimeDescription,
                        Type = cr.CrimeLookup.CrimeCodeType,
                        BailAmount = cr.BailAmount,
                        BailFlag = cr.BailNoBailFlag == 1,
                        BailType = cr.BailType,
                        CreateDate = cr.CreateDate,
                        QualifierLookupIndex = cr.CrimeQualifierLookup ?? 0
                    }).ToList();

            if (crimeDetailsLst.Any())
            {
                IQueryable<Lookup> lookupLst = from lkp in _context.Lookup
                    where lkp.LookupType == LookupConstants.CHARGEQUALIFIER
                    select lkp;
                crimeDetailsLst.ForEach(crDetails =>
                {
                    if (crDetails.QualifierLookupIndex > 0)
                    {
                        crDetails.Qualifier = lookupLst.SingleOrDefault(lkp => 
                            lkp.LookupIndex == crDetails.QualifierLookupIndex)?.LookupDescription;
                    }
                });

                bookingCrimeDetailLst.AddRange(crimeDetailsLst);
            }

            //Crime Force Search Details
            List<BookingSearchSubData> crimeForceDetailsLst =
                _context.CrimeForce.Where(cf => cf.ArrestId == arrestId && cf.DropChargeFlag != 1 && cf.ForceCrimeLookupId != 1
                                                && cf.SearchCrimeLookupId != 1 && cf.DeleteFlag == 0)
                    .Select(cf => new
                    {
                        cf.ArrestId,
                        cf.WarrantId,
                        cf.Warrant,
                        cf.TempCrimeSection,
                        cf.TempCrimeDescription,
                        cf.TempCrimeCodeType,
                        cf.BailAmount,
                        cf.BailNoBailFlag,
                        cf.CreateDate
                    }).Distinct().Select(cf => new BookingSearchSubData
                    {
                        ArrestId = cf.ArrestId ?? 0,
                        WarrantNumber = cf.WarrantId > 0 ? cf.Warrant.WarrantNumber : string.Empty,
                        BailType = cf.WarrantId > 0 ? cf.Warrant.WarrantBailType : string.Empty,
                        CrimeSection = cf.TempCrimeSection,
                        Description = cf.TempCrimeDescription,
                        Type = cf.TempCrimeCodeType,
                        BailAmount = cf.BailAmount,
                        BailFlag = cf.BailNoBailFlag == 1,
                        CreateDate = cf.CreateDate
                    }).ToList();

            if (crimeForceDetailsLst.Any())
            {
                bookingCrimeDetailLst.AddRange(crimeForceDetailsLst);
            }
            return bookingCrimeDetailLst;
		}

		// Get Incarceration Details based on Inmate Id
		public List<InmateIncarcerationDetails> GetIncarcerationDetails(int inmateId)
		{
			//Getting TransType for Incarceration Booking
			List<InmateIncarcerationDetails> incarcerationDetailsLst = new List<InmateIncarcerationDetails>();
            if (inmateId <= 0) return incarcerationDetailsLst;

            _lstLookUp = GetLookUpList();

            incarcerationDetailsLst = _context.Incarceration.Where(inc => inc.InmateId == inmateId)
                 .OrderByDescending(inc => inc.IncarcerationId).Select(inc =>
                     new InmateIncarcerationDetails
                     {
                         InmateId = inc.InmateId ?? 0,
                         IncarcerationId = inc.IncarcerationId,
                         ReleaseOut = inc.ReleaseOut,
                         DateIn = inc.DateIn,
                         UsedPersonLast = inc.UsedPersonLast,
                         UsedPersonFirst = inc.UsedPersonFrist,
                         UsedPersonMiddle = inc.UsedPersonMiddle,
                         UsedPersonSuffix = inc.UsedPersonSuffix,
                         TransportHoldName = inc.TransportHoldName,
                         TransportScheduleDate = inc.TransportScheduleDate,
                         TransportInstructions = inc.TransportInstructions,
                         TransportCautions = inc.TransportInmateCautions,
                         TransportBail = inc.TransportInmateBail,
                         TransportHoldType = inc.TransportHoldType
                     }).ToList();
             incarcerationDetailsLst.ForEach(incdetails =>
             {
                 incdetails.TransportType = _lstLookUp.Where(l =>
                         l.LookupIndex.Equals((double?)(incdetails.TransportHoldType))
                         && l.LookupType == LookupConstants.TRANSTYPE)
                     .Select(l => l.LookupDescription).SingleOrDefault();

                 incdetails.IncarcerationArrestXrefDetailLSt = GetIncarcerationArrestXrefDetails(
                     incdetails.InmateId, incdetails.IncarcerationId);
             });
             return incarcerationDetailsLst;
		}

		// Get Incarceration Arrest Xref Details based on Inmate Id and IncarcerationId 
		private List<IncarcerationArrestXrefDetails> GetIncarcerationArrestXrefDetails(int inmateId, int incarcerationId)
		{
			List<IncarcerationArrestXrefDetails> incarceratnArrestXrefDetails =
				new List<IncarcerationArrestXrefDetails>();

			if (incarcerationId > 0 && inmateId > 0)
			{
				incarceratnArrestXrefDetails = _context.IncarcerationArrestXref.Where(inax =>
						inax.IncarcerationId.HasValue
						&& inax.IncarcerationId == incarcerationId
						&& inax.Arrest.InmateId == inmateId)
					.OrderBy(inax => inax.Arrest.ArrestBookingNo)
					.Select(inax => new IncarcerationArrestXrefDetails
					{
						ArrestId = inax.ArrestId ?? 0,
						IncarcerationId = inax.IncarcerationId ?? 0,
						IncarcerationArrestXrefId = inax.IncarcerationArrestXrefId,
						InmateId = inax.Arrest.InmateId ?? 0,
						BailAmount = inax.Arrest.BailAmount,
						ArrestSentenceStartDate = inax.Arrest.ArrestSentenceStartDate,
						ArrestSentenceReleaseDate = inax.Arrest.ArrestSentenceReleaseDate,
						BookingNumber = inax.Arrest.ArrestBookingNo,
						CourtDocket = inax.Arrest.ArrestCourtDocket,
						ArrestType = !string.IsNullOrEmpty(inax.Arrest.ArrestType) 
										? Convert.ToInt32(inax.Arrest.ArrestType) : 0,
						BookingStatus = inax.ReleaseDate.HasValue
								? BookingStatus.INACTIVE.ToString()
								: BookingStatus.ACTIVE.ToString(),
						ReleaseDate = inax.ReleaseDate,
						BookingDate = inax.BookingDate,
						ReleaseReason = inax.ReleaseReason,
						CaseNumber = inax.Arrest.ArrestCaseNumber,
						BookingActive = inax.Arrest.ArrestActive,
						ClearFlag = inax.ReleaseDate.HasValue ? 1 : 0,
						ReactivateFlag = inax.ReactivateFlag,
						Arrest = inax.Arrest.ArrestingAgency.AgencyAbbreviation,
						Court = inax.Arrest.ArrestCourtJurisdiction.AgencyAbbreviation,
						WeekEnder = inax.Arrest.ArrestSentenceWeekender
					}).ToList();

				if (incarceratnArrestXrefDetails.Any())
				{
					incarceratnArrestXrefDetails.ForEach(details =>
					{
						if (details.ArrestType > 0)
						{
							details.BookingType = _lstLookUp.Single(lkp => lkp.LookupIndex.Equals(details.ArrestType) && 
                                                       lkp.LookupType == LookupConstants.ARRTYPE).LookupDescription;
						}
					});
				}
			}
			return incarceratnArrestXrefDetails;
		}
		//Get lookup list
		private List<Lookup> GetLookUpList() => _context.Lookup.Where(
				x => x.LookupType == LookupConstants.ARRTYPE || x.LookupType == LookupConstants.TRANSTYPE).ToList();
	}
}