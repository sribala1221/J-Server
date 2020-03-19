using System;
using Microsoft.AspNetCore.Mvc;
using ServerAPI.Services;
using ServerAPI.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ServerAPI.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class RecordsDataController : ControllerBase
    {
        private readonly IDataMergeService _dataMergeService;
        private readonly IMoveService _moveService;
        private readonly IBookingService _bookingService;
        private readonly IDataSealService _sealService;

        public RecordsDataController(IDataMergeService dataMerge, IMoveService moveService, IBookingService bookingService, 
            IDataSealService dataSealService)
        {
            _dataMergeService = dataMerge;
            _moveService = moveService;
            _bookingService = bookingService;
            _sealService = dataSealService;
        }

        [HttpGet("GetNewMergePersons")]
        public IActionResult GetNewMergePersons([FromQuery]RecordsDataVm searchValue) =>
            Ok(_dataMergeService.GetNewMergePersons(searchValue));

        [HttpGet("GetPersonsToAddMerge")]
        public IActionResult GetPersonsToAddMerge([FromQuery]RecordsDataVm searchValue) => 
            Ok(_dataMergeService.GetPersonsToAddMerge(searchValue));

        [HttpGet("GetRecordsDataInmateBooking")]
        public IActionResult GetRecordsDataInmateBooking([FromQuery]int[] inmateIds) => 
            Ok(_dataMergeService.GetRecordsDataInmateBooking(inmateIds));

        [HttpGet("GetRecordsDataAkaDetails")]
        public IActionResult GetRecordsDataAkaDetails([FromQuery]int[] personIds) => 
            Ok(_dataMergeService.GetRecordsDataAkaDetails(personIds));

        /// <summary>
        /// To get reference details. lstIds contains inmateId &amp; personId. inmateId will be a key and personId will be a value
        /// Incarceration id will be getting from move module
        /// </summary>
        /// <param name="lstIds"></param>
        /// <param name="type"></param>
        /// <param name="incarcerationId"></param>
        /// <returns></returns>
        [HttpPost("GetPersonReferences")]
        public IActionResult GetPersonReferences(List<KeyValuePair<int, int>> lstIds, [FromQuery]RecordsDataType type,
            [FromQuery]int incarcerationId = 0) => Ok(_dataMergeService.GetPersonReferences(lstIds, type, incarcerationId));

        /// <summary>
        /// To get reference of each data ao lookup details
        /// Incarceration id will be getting from move module
        /// </summary>
        /// <param name="dataAoLookupId"></param>
        /// <param name="inmateId"></param>
        /// <param name="personId"></param>
        /// <param name="incarcerationId"></param>
        /// <returns></returns>
        [HttpGet("GetPersonReferenceDetails")]
        public IActionResult GetPersonReferenceDetails(int dataAoLookupId, int inmateId, int personId, int incarcerationId) => 
            Ok(_dataMergeService.GetPersonReferenceDetails(dataAoLookupId, inmateId, personId, incarcerationId));
        [HttpPost("DoMerge")]
        public async Task<IActionResult> DoMerge(DoMergeParam doMergeParam) => 
            Ok(await _dataMergeService.DoMerge(doMergeParam));
        [HttpGet("GetDataHistory")]
        public IActionResult GetDataHistory([FromQuery]DataHistoryVm searchValue) => Ok(_moveService.GetDataHistory(searchValue));

        [HttpGet("GetDataHistoryFields")]
        public IActionResult GetDataHistoryFields(int historyId) => Ok(_moveService.GetDataHistoryFields(historyId));

        #region Move

        [HttpGet("GetMovePersonSearch")]
        public IActionResult GetMovePersonSearch([FromQuery]RecordsDataVm searchValue) => Ok(_moveService.GetMovePersonSearch(searchValue));

        [HttpPost("DoMove")]
        public async Task<IActionResult> DoMove(DoMoveParam objParam) => Ok(await _moveService.DoMove(objParam));

        [HttpGet("GetPersonInOut")]
        public IActionResult GetPersonInOut(int inmateId, DateTime? dateIn = null, DateTime? releaseOut = null) => 
            Ok(_bookingService.GetIncarcerationAndBookings(inmateId, false, false, dateIn, releaseOut));

        #endregion

        #region Seal

        [HttpGet("GetSealPersonSearch")]
        public IActionResult GetSealPersonSearch([FromQuery]RecordsDataVm searchValue) => Ok(_sealService.GetPersonSeal(searchValue));


        [HttpPost("DoSeal")]
        public IActionResult PostDoSeal(DoSeal doSeal) => Ok(_sealService.DoSeal(doSeal));

        [HttpGet("SealReason")]
        public IActionResult GetSealReason() => Ok(_sealService.LoadSealLookUp());

        [HttpGet("SealHistory")]
        public IActionResult SealHistory([FromQuery]DataHistoryVm searchValue) => Ok(_sealService.SealHistory(searchValue));

        [HttpPost("DoUnSeal")]
        public IActionResult DoUnSeal(DoSeal doUnSeal) => Ok(_sealService.DoUnSeal(doUnSeal));

        #endregion

    }
}
