using System.IO;
using System.Linq;
using System.Net;
using Newtonsoft.Json;
using System;
using GenerateTables.Models;
using ServerAPI.ViewModels;

namespace ServerAPI.Services
{
    public class FulcrumService : IFulcrumService
    {
        private readonly AAtims _context;
        private const string BIOSERVER = "http://192.168.250.191/FBF/rpc/api/";
        private const string TAXONOMY = "78248:0:0:0:0:0:0:0"; //This will need to be removed and requested from each client (2 clients should not use the same taxonomy)
        private const string TEMPLATETYPE = "MMStandard";

        public FulcrumService(AAtims context)
        {
            _context = context;
        }

        //ENROLL
        public Object Enroll(FulcrumEnrollRequest value)
        {
            FulcrumGeneralizeRequest generalizeParams = new FulcrumGeneralizeRequest
            {
                biolocation = value.biolocation,
                templates = value.templates
            };
            string generalizeTemplate = GeneralizeFulcrum(generalizeParams);

            string taxonomy = TAXONOMY; 
            FulcrumEnrollRequestParams[] enrollParams = {
                new FulcrumEnrollRequestParams {
                    personId = value.personId,
                    biolocation = value.biolocation,
                    taxonomy = taxonomy,
                    templates = new[] {
                        new FulcrumEnrollRequestTemplate {
                            type = TEMPLATETYPE,
                            template = generalizeTemplate
                        }
                    }
                }
            };

            bool success = EnrollFulcrum(enrollParams);

            return success ? new { status = 0 } : new { status = 1 };
        }

        private string GeneralizeFulcrum(FulcrumGeneralizeRequest value)
        {
            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(BIOSERVER + "template/generalize");
            httpWebRequest.ContentType = "application/json; charset=utf-8";
            httpWebRequest.Method = "POST";
            using (StreamWriter streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                streamWriter.Write(JsonConvert.SerializeObject(value));
            }
            
            HttpWebResponse httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            using (StreamReader streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                string response = streamReader.ReadToEnd();
                string fulcrumResponse = JsonConvert.DeserializeObject<FulcrumGeneralizeResponse>(response).result;

                return fulcrumResponse;
            }
            
        }

        private bool EnrollFulcrum(FulcrumEnrollRequestParams[] value)
        {
            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(BIOSERVER + "biometric/enroll");
            httpWebRequest.ContentType = "application/json; charset=utf-8";
            httpWebRequest.Method = "POST";
            using (StreamWriter streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                streamWriter.Write(JsonConvert.SerializeObject(value));
            }
            
            HttpWebResponse httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            using (StreamReader streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                FulcrumEnrollResponse[] response = JsonConvert.DeserializeObject<FulcrumEnrollResponse[]>(streamReader.ReadToEnd());
                if (response[0].status == "Added")
                {
                    return true;
                }
            }
            
            return false;
        }

        //IDENTIFY
        public Object GetInmateNumber(FulcrumIdentifyRequest value)
        {
            string taxonomy = TAXONOMY; 
            value.taxonomy = taxonomy;
            FulcrumIdentifyRequest[] identifyParam = new FulcrumIdentifyRequest[]
            {
                value
            };
            int? personId = IdentifyFulcrum(identifyParam);
            if (personId != null)
            {
                string inmateNumber = _context.Inmate.Where(x => x.PersonId == personId).Select(s => s.InmateNumber).FirstOrDefault();
                return new {personId, inmateNumber, status = 0 };
            }
            return new { status = 1, error = "Person Not Found" };
        }

        private int? IdentifyFulcrum(FulcrumIdentifyRequest[] value)
        {
            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(BIOSERVER + "biometric/identify");
            httpWebRequest.ContentType = "application/json; charset=utf-8";
            httpWebRequest.Method = "POST";
            using (StreamWriter streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                streamWriter.Write(JsonConvert.SerializeObject(value));
            }

            HttpWebResponse httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            using (StreamReader streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                string response = streamReader.ReadToEnd();
                FulcrumIdentifyResponse[] fulcrumResponse = JsonConvert.DeserializeObject<FulcrumIdentifyResponse[]>(response);
                int? personId = null;
                if (fulcrumResponse[0].personId != null) {
                    personId = int.Parse(fulcrumResponse[0].personId);
                }
                return personId;
            }

        }

        //VERIFY
        public object Verify(FulcrumVerifyRequest value)
        {
            string taxonomy = TAXONOMY;
            value.taxonomy = taxonomy;
            return VerifyFulcrum(value) ? new { status = 0 } : new { status = 1 };
        }

        private bool VerifyFulcrum(FulcrumVerifyRequest value)
        {
            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(BIOSERVER + "biometric/verify");
            httpWebRequest.ContentType = "application/json; charset=utf-8";
            httpWebRequest.Method = "POST";
            using (StreamWriter streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                streamWriter.Write(JsonConvert.SerializeObject(value));
            }
            
            HttpWebResponse httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            using (StreamReader streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                string response = streamReader.ReadToEnd();
                FulcrumVerifyResponse fulcrumResponse = JsonConvert.DeserializeObject<FulcrumVerifyResponse>(response);
                return fulcrumResponse.result;
            }
            
        }

        //DELETE
        public object Delete(FulcrumDeleteRequest value)
        {
            string taxonomy = TAXONOMY;
            value.taxonomy = taxonomy;
            value.biolocation = "UnknownFinger";
            return DeleteFulcrum(value) ? new { status = 0 } : new {status = 1};
        }

        private bool DeleteFulcrum(FulcrumDeleteRequest value)
        {
            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(BIOSERVER + "biometric/delete");
            httpWebRequest.ContentType = "application/json; charset=utf-8";
            httpWebRequest.Method = "POST";
            using (StreamWriter streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                streamWriter.Write(JsonConvert.SerializeObject(value));
            }

            HttpWebResponse httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            using (StreamReader streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                string response = streamReader.ReadToEnd();
                FulcrumVerifyResponse fulcrumResponse = JsonConvert.DeserializeObject<FulcrumVerifyResponse>(response);
                return fulcrumResponse.result;
            }
        }
    }
}