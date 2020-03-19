using GenerateTables.Models;
using Microsoft.AspNetCore.Http;
using ServerAPI.Utilities;
using ServerAPI.ViewModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace ServerAPI.Services
{
    public class DataSealService : IDataSealService
    {
        private readonly AAtims _context;
        private readonly ICommonService _commonService;
        private readonly IMoveService _moveService;
        private readonly int _personnelId;

        public DataSealService(AAtims context, ICommonService commonService, IMoveService moveService, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _commonService = commonService;
            _moveService = moveService;
            _personnelId = Convert.ToInt32(httpContextAccessor.HttpContext.User.FindFirst("personnelId")?.Value);
        }

        public List<RecordsDataVm> GetPersonSeal(RecordsDataVm searchValue) //Testing -In Progress
        {

            List<RecordsDataVm> lstRecordsDataVms = _context.Inmate.Where(x =>
                    (string.IsNullOrEmpty(searchValue.PersonLastName) ||
                     x.Person.PersonLastName.StartsWith(searchValue.PersonLastName)) &&
                    (string.IsNullOrEmpty(searchValue.PersonFirstName) ||
                     x.Person.PersonFirstName.StartsWith(searchValue.PersonFirstName)) &&
                    (string.IsNullOrEmpty(searchValue.PersonMiddleName) ||
                     x.Person.PersonMiddleName.StartsWith(searchValue.PersonMiddleName)) &&
                    (!searchValue.PersonDob.HasValue || x.Person.PersonDob == searchValue.PersonDob) &&
                    (string.IsNullOrEmpty(searchValue.Fbi) || x.Person.PersonFbiNo.StartsWith(searchValue.Fbi)) &&
                    (string.IsNullOrEmpty(searchValue.AlienNo) ||
                     x.Person.PersonAlienNo.StartsWith(searchValue.AlienNo)) &&
                    (string.IsNullOrEmpty(searchValue.Ssn) || x.Person.PersonSsn.StartsWith(searchValue.Ssn)) &&
                    (string.IsNullOrEmpty(searchValue.AfisNumber) ||
                     x.Person.AfisNumber.StartsWith(searchValue.AfisNumber)) &&
                    (string.IsNullOrEmpty(searchValue.InmateNumber) ||
                     x.InmateNumber.StartsWith(searchValue.InmateNumber)) &&
                    (!searchValue.InmateActive || x.InmateActive == 1) &&
                    (string.IsNullOrEmpty(searchValue.Cii) ||
                     x.Person.PersonCii.StartsWith(searchValue.Cii)))
                .Select(x => new RecordsDataVm
                {
                    InmateId = x.InmateId,
                    InmateNumber = x.InmateNumber,
                    InmateActive = x.InmateActive == 1,
                    SiteInmateNo = x.InmateSiteNumber,
                    PersonId = x.PersonId,
                    PersonDuplicateId = x.Person.PersonDuplicateId,
                    PersonLastName = x.Person.PersonLastName,
                    PersonFirstName = x.Person.PersonFirstName,
                    PersonMiddleName = x.Person.PersonMiddleName,
                    PersonDob = x.Person.PersonDob,
                    Dln = x.Person.PersonDlNumber,
                    Ssn = x.Person.PersonSsn,
                    Cii = x.Person.PersonCii,
                    Fbi = x.Person.PersonFbiNo,
                    AlienNo = x.Person.PersonAlienNo,
                    AfisNumber = x.Person.AfisNumber
                }).ToList();

            if (searchValue.IsInclAka)
            {
                lstRecordsDataVms.AddRange(_context.Aka.Where(x =>
                        x.PersonId > 0 &&
                        (string.IsNullOrEmpty(searchValue.PersonLastName) ||
                         x.AkaLastName.StartsWith(searchValue.PersonLastName)
                        ) &&
                        (string.IsNullOrEmpty(searchValue.PersonFirstName) ||
                         x.AkaFirstName.StartsWith(searchValue
                             .PersonFirstName)) &&
                        (string.IsNullOrEmpty(searchValue
                             .PersonMiddleName) ||
                         x.AkaMiddleName.StartsWith(searchValue
                             .PersonMiddleName)) &&
                        (!searchValue.PersonDob.HasValue ||
                         x.AkaDob == searchValue.PersonDob) &&
                        (string.IsNullOrEmpty(searchValue.Fbi) ||
                         x.AkaFbi.StartsWith(searchValue.Fbi)) &&
                        (string.IsNullOrEmpty(searchValue.AlienNo) ||
                         x.AkaAlienNo.StartsWith(searchValue.AlienNo)) &&
                        (string.IsNullOrEmpty(searchValue.Ssn) ||
                         x.AkaSsn.StartsWith(searchValue.Ssn)) &&
                        (string.IsNullOrEmpty(searchValue.AfisNumber) ||
                         x.AkaAfisNumber.StartsWith(searchValue.AfisNumber)) &&
                    (string.IsNullOrEmpty(searchValue.Cii) ||
                     x.AkaCii.StartsWith(searchValue.Cii)))
                    .Select(x => new RecordsDataVm
                    {
                        PersonId = x.PersonId ?? 0,
                        AkaId = x.AkaId,
                        PersonLastName = x.AkaLastName,
                        PersonFirstName = x.AkaFirstName,
                        PersonMiddleName = x.AkaMiddleName,
                        PersonDob = x.AkaDob,
                        Dln = x.AkaDl,
                        Ssn = x.AkaSsn,
                        Cii = x.AkaCii,
                        Fbi = x.AkaFbi,
                        AlienNo = x.AkaAlienNo,
                        AfisNumber = x.AkaAfisNumber
                    }).ToList());
            }

            if (searchValue.IsInclAka)
            {
                lstRecordsDataVms = lstRecordsDataVms.Distinct().ToList();
            }

            lstRecordsDataVms = lstRecordsDataVms.OrderBy(a => a.PersonId).ThenBy(a => a.AkaId).ToList();

            lstRecordsDataVms = lstRecordsDataVms.Count > 0 && searchValue.Results > 0 ? lstRecordsDataVms.Take(searchValue.Results).ToList() : lstRecordsDataVms;

            return lstRecordsDataVms;
        }

        public string DoSeal(DoSeal doSeal) //Testing - In progress
        {
            SqlConnection connection = new SqlConnection(Startup.ConnectionString);
            connection.Open();
            SqlCommand command = new SqlCommand("AO_DoSeal", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            //Params
            command.Parameters.AddWithValue("@p_ErrorMessage", string.Empty);
            command.Parameters["@p_ErrorMessage"].Direction = ParameterDirection.Output;
            command.Parameters.AddWithValue("@p_StatusCode", 0);
            command.Parameters["@p_StatusCode"].Direction = ParameterDirection.Output;
            command.Parameters.AddWithValue("@p_SealType", doSeal.SealType);
            command.Parameters.AddWithValue("@p_InmateID ", doSeal.InmateId);
            command.Parameters.AddWithValue("@p_PersonID", doSeal.PersonId);
            command.Parameters.AddWithValue("@p_PersonnelID", _personnelId);
            command.Parameters.AddWithValue("@p_DataTitle", doSeal.DataTitle);
            command.Parameters.AddWithValue("@p_DataReason", doSeal.DataReason);
            command.Parameters.AddWithValue("@p_DataNote", doSeal.DataNote);
            command.Parameters.AddWithValue("@p_WarrantID", doSeal.WarrantId);
            command.Parameters.AddWithValue("@p_CrimeID", doSeal.CrimeId);
            command.Parameters.AddWithValue("@p_CrimeForceID", doSeal.CrimeForceId);
            command.Parameters.AddWithValue("@p_ArrestID", doSeal.ArrestId);
            command.Parameters.AddWithValue("@p_IncarcerationID", doSeal.IncarcerationId);
            command.Parameters.AddWithValue("@p_IncArrestXrefID", doSeal.IncArrestXrefId);
            command.ExecuteNonQuery();
            string statusCode = command.Parameters["@p_StatusCode"].Value.ToString();
            connection.Close();
            return statusCode;
        }

        public List<KeyValuePair<int, string>> LoadSealLookUp() =>
            _commonService.GetLookupKeyValuePairs(LookupConstants.SEALREASON);

        private List<DataHistoryVm> GetSeal(DataHistoryVm searchValue)
        {
            //Get Person Seal details using filter
            List<PersonSeal> lstPersonSeal = _context.PersonSeal.Where(w =>
                    (string.IsNullOrEmpty(searchValue.PersonLastName) || (!string.IsNullOrEmpty(w.PersonLastName) &&
                     w.PersonLastName.ToUpper().StartsWith(searchValue.PersonLastName.ToUpper()))) &&
                    (string.IsNullOrEmpty(searchValue.PersonFirstName) || (!string.IsNullOrEmpty(w.PersonFirstName) &&
                     w.PersonFirstName.ToUpper().StartsWith(searchValue.PersonFirstName.ToUpper()))) &&
                    (string.IsNullOrEmpty(searchValue.PersonMiddleName) || (!string.IsNullOrEmpty(w.PersonMiddleName) &&
                     w.PersonMiddleName.ToUpper().StartsWith(searchValue.PersonMiddleName.ToUpper()))) &&
                    (!searchValue.PersonDob.HasValue || w.PersonDob == searchValue.PersonDob)).ToList();

            List<int?> personIds = lstPersonSeal.Select(s => s.PersonId).ToList();

            //Get Inmate details using filter
            List<Inmate> lstInmateInfo = _context.Inmate.Where(w =>
                        personIds.Contains(w.PersonId) && (string.IsNullOrEmpty(searchValue.InmateNumber) ||
                        w.InmateNumberSealed.ToUpper().StartsWith(searchValue.InmateNumber.ToUpper()))).ToList();

            List<DataHistoryVm> lstSeal = _moveService.GetDataInfo(searchValue, personIds, true);

            List<int> inmateIds = lstInmateInfo.Select(s => s.InmateId).ToList();

            lstSeal = lstSeal.Where(w => inmateIds.Contains(w.DataInmateId ?? 0) || inmateIds.Contains(w.KeepInmateId ?? 0)).ToList();

            lstSeal.ForEach(item =>
            {
                //Assign Data Person Info
                item.DataPersonInfo =
                    AssignInmateAndPersonSealInfo(item.DataPersonId, item.DataInmateId, lstPersonSeal, lstInmateInfo);

                //Assign Keep Person Info
                item.KeepPersonInfo =
                    AssignInmateAndPersonSealInfo(item.KeepPersonId, item.KeepInmateId, lstPersonSeal, lstInmateInfo);
            });

            return lstSeal;
        }

        private PersonInfoVm AssignInmateAndPersonSealInfo(int? personId, int? inmateId,
            List<PersonSeal> lstPersonSeal, List<Inmate> lstInmateInfo)
        {
            PersonSeal personSealInfo = lstPersonSeal.SingleOrDefault(i => i.PersonId == personId);
            Inmate inmateInfo = lstInmateInfo.FirstOrDefault(s => !inmateId.HasValue || s.InmateId == inmateId);

            PersonInfoVm personInfoVm = new PersonInfoVm
            {
                PersonId = personSealInfo?.PersonId ?? 0,
                PersonLastName = personSealInfo?.PersonLastName,
                PersonFirstName = personSealInfo?.PersonFirstName,
                PersonMiddleName = personSealInfo?.PersonMiddleName,
                PersonSuffix = personSealInfo?.PersonSuffix,
                PersonDob = personSealInfo?.PersonDob,
                InmateId = inmateInfo?.InmateId,
                InmateNumber = inmateInfo?.InmateNumber
            };

            return personInfoVm;
        }

        public List<DataHistoryVm> SealHistory(DataHistoryVm searchValue)
        {
            List<DataHistoryVm> lstDataHistory = _moveService.GetDataHistory(searchValue);
            List<DataHistoryVm> lstSealHistory = GetSeal(searchValue);
            lstDataHistory.AddRange(lstSealHistory);
            return lstDataHistory.OrderByDescending(o => o.DataHistoryId).ToList();
        }
        public string DoUnSeal(DoSeal doUnSeal)  //Testing - In progress
        {
            SqlConnection connection = new SqlConnection(Startup.ConnectionString);
            connection.Open();
            SqlCommand command = new SqlCommand("AO_DoUnSeal", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            //Params         
            command.Parameters.AddWithValue("@p_ErrorMessage", string.Empty);
            command.Parameters["@p_ErrorMessage"].Direction = ParameterDirection.Output;
            command.Parameters.AddWithValue("@p_StatusCode", 0);
            command.Parameters["@p_StatusCode"].Direction = ParameterDirection.Output;
            command.Parameters.AddWithValue("@p_SealType", doUnSeal.SealType);
            command.Parameters.AddWithValue("@p_InmateID ", doUnSeal.InmateId);
            command.Parameters.AddWithValue("@p_PersonID", doUnSeal.PersonId);
            command.Parameters.AddWithValue("@p_PersonnelID", doUnSeal.PersonnelId);
            command.Parameters.AddWithValue("@p_WarrantID", doUnSeal.WarrantId);
            command.Parameters.AddWithValue("@p_CrimeID", doUnSeal.CrimeId);
            command.Parameters.AddWithValue("@p_CrimeForceID", doUnSeal.CrimeForceId);
            command.Parameters.AddWithValue("@p_ArrestID", doUnSeal.ArrestId);
            command.Parameters.AddWithValue("@p_IncarcerationID", doUnSeal.IncarcerationId);
            command.Parameters.AddWithValue("@p_IncArrestXrefID", doUnSeal.IncArrestXrefId);
            command.Parameters.AddWithValue("@p_DataHistoryID", doUnSeal.DataHistoryId);
            command.ExecuteNonQuery();
            string statusCode = command.Parameters["@p_StatusCode"].Value.ToString();
            connection.Close();
            return statusCode;
        }
    }
}
